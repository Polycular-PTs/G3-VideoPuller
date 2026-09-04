from ultralytics import YOLO
import cv2
import numpy as np
import time
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

while True:
    print("Waiting for Unity to connect...")
    conn, addr = sock.accept()
    print(f"Connected to Unity at {addr}")

    cam_index = get_obs_camera_index()
    cap = cv2.VideoCapture(cam_index, cv2.CAP_DSHOW)

    cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1080)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 1920)
    
    previous_y = None
    start_time = time.time()
    action_counts = {"waving": 0, "jumping": 0}

    try:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            results = model(frame, verbose=False)
            annotated = results[0].plot()

            wave_state = "none"
            jump_state = "none"

            if results[0].keypoints is not None and len(results[0].keypoints) > 0:
                for pose in results[0].keypoints:
                    keypoints = pose.xy[0].cpu().numpy()
                    
                    if len(keypoints) >= 17:
                        # Keypoints auf das Bild zeichnen
                        for i, (x, y) in enumerate(keypoints):
                            if x > 0 and y > 0: # Nur zeichnen wenn erkannt
                                name = keypoint_names[i] if i < len(keypoint_names) else f"kp_{i}"
                                cv2.putText(
                                    annotated, f"{i}:{name}", (int(x) + 5, int(y) - 5),
                                    cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 255), 1, cv2.LINE_AA
                                )
                                cv2.circle(annotated, (int(x), int(y)), 3, (0, 255, 255), -1)

                        left_wrist_y = keypoints[9][1]
                        right_wrist_y = keypoints[10][1]
                        left_shoulder_y = keypoints[5][1]

                        if (left_wrist_y > 0 and left_wrist_y < left_shoulder_y) or (right_wrist_y > 0 and right_wrist_y < left_shoulder_y):
                            wave_state = "waving"

                        avg_ankle_y = np.mean([keypoints[15][1], keypoints[16][1]])
                        if previous_y is not None:
                            dy = previous_y - avg_ankle_y
                            if dy > 10:
                                jump_state = "jumping"

                        previous_y = avg_ankle_y

            if wave_state == "waving":
                action_counts["waving"] += 1
            if jump_state == "jumping":
                action_counts["jumping"] += 1
                
            # Actions ins Bild schreiben
            cv2.putText(annotated, f"Wave: {wave_state}", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 255, 0), 2)
            cv2.putText(annotated, f"Jump: {jump_state}", (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 0.9, (0, 255, 0), 2)

            current_time = time.time()
            if current_time - start_time >= 1.0:
                summary_list = [f"{count}x_{action}" for action, count in action_counts.items() if count > 0]
                
                if summary_list:
                    summary_text = ", ".join(summary_list)
                    conn.sendall((summary_text + "\n").encode("utf-8"))

                action_counts = {"waving": 0, "jumping": 0}
                start_time = current_time

            # Feed im Fenster anzeigen
            cv2.imshow("Pose Detection", annotated)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break 

    except (ConnectionAbortedError, ConnectionResetError, socket.error):
        print("Unity disconnected from Pose Server.")
    finally:
        cap.release()
        cv2.destroyAllWindows() # Schließt das Fenster bei Disconnect
        conn.close()
        print("Webcam and windows released. Ready for next connection.")