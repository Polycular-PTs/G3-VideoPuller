from ultralytics import YOLO
import cv2
import numpy as np
import time
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

def save_smart_crop_pose(frame, results, filename, last_face_pos):
    h, w, _ = frame.shape
    center_x, center_y = None, None
    crop_size = None

    if results[0].keypoints is not None and len(results[0].keypoints) > 0:
        kp = results[0].keypoints[0].xy[0].cpu().numpy()
        conf = results[0].keypoints[0].conf[0].cpu().numpy()
        
        if conf[0] > 0.3 and kp[0][0] > 0:
            center_x, center_y = kp[0][0], kp[0][1]
        elif conf[1] > 0.3 and conf[2] > 0.3:
            center_x = (kp[1][0] + kp[2][0]) / 2.0
            center_y = (kp[1][1] + kp[2][1]) / 2.0
        elif conf[5] > 0.3 and conf[6] > 0.3:
            center_x = (kp[5][0] + kp[6][0]) / 2.0
            center_y = (kp[5][1] + kp[6][1]) / 2.0 - 60 

        if conf[5] > 0.3 and conf[6] > 0.3:
            shoulder_dist = np.linalg.norm(kp[5] - kp[6])
            crop_size = int(shoulder_dist * 2.2) 
        else:
            crop_size = int(h * 0.35)

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
    print(f"[PhotoSaved] Saved uniform pose crop to {filepath}")
    
    return (center_x, center_y, crop_size)

model = YOLO("yolo11n-pose.pt")

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind(("127.0.0.1", 5006))
sock.listen(1)

print("Pose Detection Server running on port 5006...")

keypoint_names = [
    "nose", "left_eye", "right_eye", "left_ear", "right_ear",
    "left_shoulder", "right_shoulder", "left_elbow", "right_elbow",
    "left_wrist", "right_wrist", "left_hip", "right_hip",
    "left_knee", "right_knee", "left_ankle", "right_ankle"
]

CONF_THRESHOLD = 0.5  

while True:
    print("Waiting for Unity to connect...")
    conn, addr = sock.accept()
    conn.setblocking(False)
    print(f"Connected to Unity at {addr}")

    cam_index = get_obs_camera_index()
    cap = cv2.VideoCapture(cam_index, cv2.CAP_DSHOW)

    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1080)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1920)
    
    previous_hip_y = None
    previous_torso_height = None
    start_time = time.time()
    action_counts = {"waving": 0, "jumping": 0}

    last_face_pos = None
    debug_mode = False # <-- Hier setzen wir den Standardwert

    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break
                
            results = model(frame, verbose=False)
            annotated = results[0].plot()

            # Commands lesen (Nachdem 'results' existiert!)
            try:
                data = conn.recv(1024).decode('utf-8')
                if data:
                    for line in data.strip().split('\n'):
                        if line.startswith("CAPTURE:"):
                            filename = line.split("CAPTURE:")[1]
                            last_face_pos = save_smart_crop_pose(frame, results, filename, last_face_pos)
                        elif line == "DEBUG:ON":
                            print("[Debug Mode] Enabled")
                            debug_mode = True
                        elif line == "DEBUG:OFF":
                            print("[Debug Mode] Disabled")
                            debug_mode = False
                            cv2.destroyAllWindows() # Fenster sofort schließen
            except BlockingIOError:
                pass

            wave_state = "none"
            jump_state = "none"

            if results[0].keypoints is not None and len(results[0].keypoints) > 0:
                for pose in results[0].keypoints:
                    keypoints = pose.xy[0].cpu().numpy()
                    confidences = pose.conf[0].cpu().numpy()
                    
                    if len(keypoints) >= 17:
                        is_valid = lambda idx: confidences[idx] >= CONF_THRESHOLD and keypoints[idx][0] > 0

                        for i, (x, y) in enumerate(keypoints):
                            if is_valid(i):
                                name = keypoint_names[i] if i < len(keypoint_names) else f"kp_{i}"
                                cv2.putText(annotated, f"{i}:{name}", (int(x) + 5, int(y) - 5),
                                            cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 255), 1, cv2.LINE_AA)
                                cv2.circle(annotated, (int(x), int(y)), 3, (0, 255, 255), -1)

                        left_wave = (is_valid(9) and is_valid(5) and keypoints[9][1] < (keypoints[5][1] - 20))
                        right_wave = (is_valid(10) and is_valid(6) and keypoints[10][1] < (keypoints[6][1] - 20))

                        if left_wave or right_wave:
                            wave_state = "waving"

                        if is_valid(11) and is_valid(12):
                            current_hip_y = (keypoints[11][1] + keypoints[12][1]) / 2.0

                            if is_valid(5) and is_valid(6):
                                shoulder_y = (keypoints[5][1] + keypoints[6][1]) / 2.0
                                torso_height = max(50.0, current_hip_y - shoulder_y)
                            else:
                                torso_height = 200.0

                            if previous_hip_y is not None and previous_torso_height is not None:
                                dy = previous_hip_y - current_hip_y  
                                torso_change = (torso_height - previous_torso_height) / previous_torso_height
                                jump_velocity = dy / torso_height

                                if jump_velocity > 0.08 and torso_change > -0.04:
                                    jump_state = "jumping"

                            previous_hip_y = current_hip_y
                            previous_torso_height = torso_height
                        else:
                            previous_hip_y = None
                            previous_torso_height = None

            if wave_state == "waving":
                action_counts["waving"] += 1
            if jump_state == "jumping":
                action_counts["jumping"] += 1
                
            cv2.putText(annotated, f"Wave: {wave_state}", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 255, 0), 2)
            cv2.putText(annotated, f"Jump: {jump_state}", (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 255, 0), 2)

            current_time = time.time()
            if current_time - start_time >= 1.0:
                summary_list = [f"{count}x_{action}" for action, count in action_counts.items() if count > 0]
                summary_text = ", ".join(summary_list) if summary_list else "no actions"
                conn.sendall((summary_text + "\n").encode("utf-8"))

                action_counts = {"waving": 0, "jumping": 0}
                start_time = current_time

            # Nur rendern, wenn Debug Mode aktiv ist
            if debug_mode:
                cv2.imshow("Pose Detection", annotated)
                cv2.waitKey(1)

    except (ConnectionAbortedError, ConnectionResetError, socket.error):
        print("Unity disconnected from Pose Server.")
    finally:
        cap.release()
        cv2.destroyAllWindows()
        conn.close()
        print("Webcam and windows released. Ready for next connection.")