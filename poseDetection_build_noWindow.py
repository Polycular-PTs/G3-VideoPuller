from ultralytics import YOLO
import cv2
import numpy as np
import time
import socket

# YOLO pose model
model = YOLO("yolo11n-pose.pt")

# TCP socket for Unity
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind(("127.0.0.1", 5006))
sock.listen(1)

print("Waiting for Unity connection on port 5006...")
conn, addr = sock.accept()
print("Connected to Unity:", addr)

cap = cv2.VideoCapture(0)
if not cap.isOpened():
    print("Error: could not open webcam")
    exit()

previous_y = None

# Track actions per second
start_time = time.time()
action_counts = {
    "waving": 0,
    "jumping": 0
}

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # Run pose detection (no visualization)
    results = model(frame, verbose=False)

    wave_state = "none"
    jump_state = "none"

    for pose in results[0].keypoints:
        keypoints = pose.xy[0].cpu().numpy()

        left_wrist_y = keypoints[9][1]
        right_wrist_y = keypoints[10][1]
        left_shoulder_y = keypoints[5][1]

        # --- WAVE DETECTION ---
        if left_wrist_y < left_shoulder_y or right_wrist_y < left_shoulder_y:
            wave_state = "waving"

        # --- JUMP DETECTION ---
        avg_ankle_y = np.mean([keypoints[15][1], keypoints[16][1]])
        if previous_y is not None:
            dy = previous_y - avg_ankle_y
            if dy > 10:
                jump_state = "jumping"

        previous_y = avg_ankle_y

    # Count detections
    if wave_state == "waving":
        action_counts["waving"] += 1
    if jump_state == "jumping":
        action_counts["jumping"] += 1

    # --- SEND SUMMARY EVERY SECOND ---
    current_time = time.time()
    if current_time - start_time >= 1.0:

        summary_list = [
            f"{count}x_{action}"
            for action, count in action_counts.items()
            if count > 0
        ]

        summary_text = ", ".join(summary_list)

        try:
            conn.sendall((summary_text + "\n").encode("utf-8"))

        except Exception as e:
            print("Unity disconnected – waiting for reconnect:", e)

            try:
                cap.release()
                print("releasing webcam")
                conn.close()
            except:
                pass

            sock.listen(1)
            conn, addr = sock.accept()
            cap = cv2.VideoCapture(0, cv2.CAP_DSHOW)
            print("Reconnected to Unity:", addr)
            

        # Reset counters
        action_counts = {"waving": 0, "jumping": 0}
        start_time = current_time

cap.release()
