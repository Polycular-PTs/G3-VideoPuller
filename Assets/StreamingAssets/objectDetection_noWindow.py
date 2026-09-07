from ultralytics import YOLO
import cv2
import time
from collections import defaultdict
from deepface import DeepFace
import socket
from pygrabber.dshow_graph import FilterGraph
import os

CAPTURE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "captures"))
os.makedirs(CAPTURE_DIR, exist_ok=True)
TARGET_SIZE = (500, 500)

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

def save_smart_crop_object(frame, results, filename, last_face_pos):
    h, w, _ = frame.shape
    center_x, center_y = None, None
    crop_size = None

    person_boxes = []
    for box in results[0].boxes:
        if int(box.cls[0]) == 0 and float(box.conf[0]) > 0.3:
            xyxy = box.xyxy[0].cpu().numpy().astype(int)
            area = (xyxy[2] - xyxy[0]) * (xyxy[3] - xyxy[1])
            person_boxes.append((area, xyxy))

    if person_boxes:
        person_boxes.sort(key=lambda b: b[0], reverse=True) 
        _, xyxy = person_boxes[0]
        
        box_w = xyxy[2] - xyxy[0]
        box_h = xyxy[3] - xyxy[1]
        
        center_x = (xyxy[0] + xyxy[2]) // 2
        center_y = xyxy[1] + int(box_h * 0.22) 
        crop_size = int(max(box_w * 1.5, box_h * 0.55))

    if center_x is None:
        if last_face_pos is not None:
            center_x, center_y, crop_size = last_face_pos
        else:
            center_x, center_y = w // 2, int(h * 0.35)
            crop_size = int(min(w, h) * 0.5)

    crop_size = max(250, min(int(crop_size), min(w, h)))
    half = crop_size // 2

    x1 = int(center_x - half)
    x2 = int(center_x + half)
    y1 = int(center_y - half)
    y2 = int(center_y + half)

    if x1 < 0:
        x2 += abs(x1)
        x1 = 0
    if x2 > w:
        x1 -= (x2 - w)
        x2 = w
    if y1 < 0:
        y2 += abs(y1)
        y1 = 0
    if y2 > h:
        y1 -= (y2 - h)
        y2 = h

    x1, x2 = max(0, x1), min(w, x2)
    y1, y2 = max(0, y1), min(h, y2)

    crop = frame[y1:y2, x1:x2]
    crop_resized = cv2.resize(crop, TARGET_SIZE, interpolation=cv2.INTER_AREA)

    filepath = os.path.join(CAPTURE_DIR, filename)
    cv2.imwrite(filepath, crop_resized)
    print(f"[PhotoSaved] Saved uniform object crop to {filepath}")

    return (center_x, center_y, crop_size)


model = YOLO("yolo11n.pt")

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind(("127.0.0.1", 5005))
sock.listen(1)

print("Object/Emotion Detection Server running on port 5005...")

while True:
    print("Waiting for Unity to connect...")
    conn, addr = sock.accept()
    print(f"Connected to Unity at {addr}")
    conn.setblocking(False)

    cam_index = get_obs_camera_index()
    cap = cv2.VideoCapture(cam_index, cv2.CAP_DSHOW)

    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1080)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1920)
    
    max_objects = defaultdict(int)
    start_time = time.time()
    last_face_pos = None
    debug_mode = False # <-- Auch hier der Default-Status
    
    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break
                
            results = model(frame, verbose=False)
            annotated_frame = results[0].plot()

            # TCP Befehle auslesen
            try:
                data = conn.recv(1024).decode('utf-8')
                if data:
                    for line in data.strip().split('\n'):
                        if line.startswith("CAPTURE:"):
                            filename = line.split("CAPTURE:")[1]
                            last_face_pos = save_smart_crop_object(frame, results, filename, last_face_pos)
                        elif line == "DEBUG:ON":
                            print("[Debug Mode] Enabled")
                            debug_mode = True
                        elif line == "DEBUG:OFF":
                            print("[Debug Mode] Disabled")
                            debug_mode = False
                            cv2.destroyAllWindows() # Fenster sofort zumachen
            except BlockingIOError:
                pass

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
                            
                            cv2.putText(annotated_frame, dominant_emotion, (x1, y1 - 10),
                                        cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 0), 2, cv2.LINE_AA)
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

            # Rendern nur, wenn gewünscht
            if debug_mode:
                cv2.imshow("Object & Emotion Detection", annotated_frame)
                cv2.waitKey(1)

    except (ConnectionAbortedError, ConnectionResetError, socket.error):
        print("Unity disconnected from Object Server.")
    finally:
        cap.release()
        cv2.destroyAllWindows()
        conn.close()
        print("Webcam and windows released. Ready for next connection.")