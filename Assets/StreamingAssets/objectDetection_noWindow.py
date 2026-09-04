from ultralytics import YOLO
import cv2
import time
from collections import defaultdict
from deepface import DeepFace
import socket
from pygrabber.dshow_graph import FilterGraph

def get_obs_camera_index():
    try:
        graph = FilterGraph()
        devices = graph.get_input_devices()
        for index, device_name in enumerate(devices):
            if "OBS Virtual Camera" in device_name:
                print(f"OBS Virtual Camera detected at index {index}.")
                return index
    except Exception as e:
        print(f"Error reading camera names: {e}")
    return 0

model = YOLO("yolo11n.pt")

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind(("127.0.0.1", 5005))
sock.listen(1)

print("Object/Emotion Detection Server running on port 5005...")

while True:
    print("Waiting for Unity to connect...")
    conn, addr = sock.accept()
    print(f"Connected to Unity at {addr}")

    cam_index = get_obs_camera_index()
    cap = cv2.VideoCapture(cam_index, cv2.CAP_DSHOW)

    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1080)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1920)
    
    max_objects = defaultdict(int)
    start_time = time.time()

    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            results = model(frame, verbose=False)
            annotated_frame = results[0].plot() # Zeichnet Bounding Boxes von YOLO
            
            frame_counts = defaultdict(int)
            person_emotions = []

            for result in results:
                for box in result.boxes:
                    cls = int(box.cls[0])
                    object_name = model.names[cls]
                    frame_counts[object_name] += 1

                    if object_name == "person":
                        xyxy = box.xyxy[0].cpu().numpy().astype(int)
                        x1, y1 = max(0, xyxy[0]), max(0, xyxy[1])
                        x2, y2 = min(frame.shape[1], xyxy[2]), min(frame.shape[0], xyxy[3])
                        person_crop = frame[y1:y2, x1:x2]

                        try:
                            emotion = DeepFace.analyze(img_path=person_crop, actions=['emotion'], enforce_detection=False, detector_backend='skip')
                            dominant_emotion = emotion[0]['dominant_emotion']
                            person_emotions.append(dominant_emotion)
                            
                            # Emotion als Text über der Person einblenden
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
                        except:
                            pass

            for obj, count in frame_counts.items():
                if count > max_objects[obj]:
                    max_objects[obj] = count

            current_time = time.time()
            if current_time - start_time >= 1.0:
                summary = [f"{count}x_{name}" for name, count in max_objects.items()]
                summary_text = "Objects seen in last second: " + ", ".join(summary)

                if person_emotions:
                    emotion_counts = defaultdict(int)
                    for emo in person_emotions:
                        emotion_counts[emo] += 1
                    emotion_summary = [f"{count}x_{emo}" for emo, count in emotion_counts.items()]
                    final_msg = summary_text + ", Emotions seen: " + " ".join(emotion_summary) + "\n"
                else:
                    final_msg = summary_text + "\n"

                conn.sendall(final_msg.encode('utf-8'))

                max_objects.clear()
                start_time = current_time

            # Feed im Fenster anzeigen
            cv2.imshow("Object & Emotion Detection", annotated_frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break # Bricht die Frame-Schleife ab und wartet auf neuen Connect

    except (ConnectionAbortedError, ConnectionResetError, socket.error):
        print("Unity disconnected from Object Server.")
    finally:
        cap.release()
        cv2.destroyAllWindows() # Verhindert eingefrorene Geisterfenster
        conn.close()
        print("Webcam and windows released. Ready for next connection.")