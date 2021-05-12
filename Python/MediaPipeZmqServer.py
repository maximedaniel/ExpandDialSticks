#
#   Hello World server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import time
import zmq

import cv2
import mediapipe as mp
import numpy as np
import paho.mqtt.client as mqtt
import threading, queue

mp_drawing = mp.solutions.drawing_utils
mp_hands = mp.solutions.hands

q = queue.Queue()

image_source = None
cv2.startWindowThread()
cv2.namedWindow("MediaPipe Hands")

# def on_connect(client, userdata, flags, rc):
#     print("Connected with result code "+str(rc))
#     client.subscribe("LeapMotion")

# def on_message(client, userdata, msg):
#     try:
#         global q, calibrated, toCalibrate
#         png_as_np = np.frombuffer(msg.payload, dtype=np.uint8)
#         np_to_img = cv2.imdecode(png_as_np, flags=1)
#         # if not calibrated:
#         #     #height, width, channels = np_to_img.shape
#         #     #print(height,  width, channels)
#         #     #cv2.imwrite("calibration.png", np_to_img)
#         #     toCalibrate = True
#         q.put(np_to_img)
#     except Exception as e:
#         print(e)

# client = mqtt.Client()
# client.on_connect = on_connect
# client.on_message = on_message

# client.connect("127.0.0.1", 1883, 60)

# client.loop_start()


# Start ZMQ Server

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

with mp_hands.Hands(
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5) as hands:
    while True:
        #  Wait for next request from client
        message = socket.recv()
        print("Received request: %s" % message)
        #  Do some 'work'
        png_as_np = np.frombuffer(message, dtype=np.uint8)
        np_to_img = cv2.imdecode(png_as_np, flags=1)
        # Flip the image horizontally for a later selfie-view display, and convert
        image = cv2.cvtColor(cv2.flip(img, 1), cv2.COLOR_BGR2RGB)
        # To improve performance, optionally mark the image as not writeable to
        # # pass by reference.
        image.flags.writeable = False
        results = hands.process(image)
        # Draw the hand annotations on the image.
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                mp_drawing.draw_landmarks(image, hand_landmarks, mp_hands.HAND_CONNECTIONS)
        cv2.imshow('MediaPipe Hands', image)
        #  Send reply back to client
        socket.send(b"World")
        if cv2.waitKey(5) & 0xFF == 27:
            break



