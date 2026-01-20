from ultralytics import YOLO
import cv2
import time
from collections import defaultdict
from deepface import DeepFace
import numpy as np
import socket

#TCP socket Verbindung
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind(("localhost", 5005)) 
sock.listen(1)
conn, addr = sock.accept()


model = YOLO("yolo11n.pt") 

#webcam aufmachen
cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)

    

max_objects = defaultdict(int)   #für maximale Anzahl eines Objekts die in allen Frames der letzten Sekunde waren, also als Extremwert. Wird jede Sekunde resettet, deshalb außerhalb vom Loop

start_time = time.time()

while True:
    while cap.isOpened():
        ret, frame = cap.read() #die cap.read() Funktion gibt einen bool (ret Variable) zurück, mit der Info ob ein frame gecaptured wurde oder nicht und natürlich den frame selbst als numpy array (frame Variable)
            

        #YOLOV11 Frame Analyse
        results = model(frame)
        annotated_frame = results[0].plot() #.plot() ist quasi eine draw Funktion von Ultralytics, damit man nicht selber bounding boxes & Tags zeichnen muss.
                                            #es werden die YOLO Text Ergebnisse für diesen Frame genommen und eine Kopie des Bildes mit allen darauf gezeichneten annotations erstellt

        
        frame_counts = defaultdict(int) #alle Objekte die im Aktuellen Frame erkannt wurden als Liste

        
        person_emotions = [] #hier kommen weiter unten jeden Frame die erkannten Emotionen rein. Wird nicht vorher deklariert, weil sich die Listen sonst nur ansammeln

        for result in results:
            boxes = result.boxes
            for box in boxes:
                cls = int(box.cls[0])
                object_name = model.names[cls] #name des erkannten class index. YOLO hat circa 75 Objekte die erkannt werden können. über der box soll aber nicht z.B. 34 stehen sondern der Name des Objektes mit dem class index 34, z.B. keyboard
                frame_counts[object_name] += 1 #sonst sind es bei einem Objekt 0, bei zwei Objekten 1 und so weiter


                #wenn das erkannte Objekt eine Person ist, wird der (gecroppte) frame in den DeepFace codeblock weitergschickt für die Emotionserkennung

                if object_name == "person":
                    #crop code mit ChatGPT, weil die KI viel beser mathematisch denken kann als ich ;)

                    xyxy = box.xyxy[0].cpu().numpy().astype(int)
                    x1, y1, x2, y2 = xyxy

                    x1, y1 = max(0, x1), max(0, y1)
                    x2, y2 = min(frame.shape[1], x2), min(frame.shape[0], y2)

                    # Crop face region
                    person_crop = frame[y1:y2, x1:x2]

                    #DeepFace Emotionsanalyse
                    try:
                        emotion = DeepFace.analyze(img_path=person_crop, actions=['emotion'], enforce_detection=False, detector_backend='skip')
                        dominant_emotion = emotion[0]['dominant_emotion']
                        person_emotions.append(dominant_emotion)

                        #Emotionstext über der Box hinzufügen (auch ChatGPT)
                        cv2.putText(
                            annotated_frame,
                            dominant_emotion,
                            (x1, y1 - 10),
                            cv2.FONT_HERSHEY_SIMPLEX,
                            0.6,
                            (255, 255, 0),
                            2,
                            cv2.LINE_AA
                        )
                    except Exception as e:
                        pass


        #max_objects updaten, indem geschaut wird, ob in dem jetzigen frame mehr von einem Objekt sind als in der max_objects für dieses Objekt bis jetzt drinn sind. Wenn ja wird der Objektslot überschrieben.
        #so steht am Ende der Sekunde immer nur der höchste Wert für jedes Objekt drinn.
        for obj, count in frame_counts.items():
            if count > max_objects[obj]:
                max_objects[obj] = count


        #sekündliche zusammenfassung die an Unity über TCP gesendet wird
        current_time = time.time()
        if current_time - start_time >= 1.0:
            if max_objects:
                summary = [f"{count}x_{name}" for name, count in max_objects.items()]
                summary_text = "Objects seen in last second: " + ", ".join(summary)

                #Emotionen hinzufügen
                if person_emotions:
                    emotion_counts = defaultdict(int)
                    for emo in person_emotions:
                        emotion_counts[emo] += 1
                    emotion_summary = [f"{count}x_{emo}" for emo, count in emotion_counts.items()]
                    
                    try:
                        conn.sendall((summary_text + ", Emotions seen:" + " ".join(emotion_summary) + "\n").encode('utf-8'))

                    except Exception:

                        try:
                            cap.release()
                            print("releasing webcam")
                        except:
                            pass
                        
                        try:
                            conn.close()
                        except:
                            pass

                        

                else:
                    try:
                        conn.sendall((summary_text + "\n").encode('utf-8'))
                    except Exception:
                        try:
                            cap.release()
                            print("releasing webcam")
                            conn.close()
                        except:
                            pass

                        

            max_objects.clear() #Die max_objects werden wieder gecleart damit beim nächsten "Sekunden-Loop" wieder von neuem gezählt werden kann
            start_time = current_time

    while not cap.isOpened():
        # Wait for Unity to reconnect
        if current_time > 0.5:
            print("not Opened")
        sock.listen(1)
        conn, addr = sock.accept()
        try:
            cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
            print("reopening cam")
        except Exception as e:
            print(e)


cap.release()
cv2.destroyAllWindows()
