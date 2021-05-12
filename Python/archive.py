import cv2
import mediapipe as mp
import paho.mqtt.client as mqtt
import threading, queue

mp_drawing = mp.solutions.drawing_utils
mp_hands = mp.solutions.hands

lock = threading.Lock()

image_source = ''

def on_connect(client, userdata, flags, rc):
    print("Connected with result code "+str(rc))
    client.subscribe("LeapMotion")

def on_message(client, userdata, msg):
    try:
        png_as_np = np.frombuffer(msg.payload, dtype=np.uint8)
        lock.acquire()
        image_source = cv2.imdecode(png_as_np, flags=1)
        lock.release()
    except Exception as e:
        print("error while parsing image.")

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect("127.0.0.1", 1883, 60)

client.loop_start()

with mp_hands.Hands(
    min_detection_confidence=0.7,
    min_tracking_confidence=0.7) as hands:
    while True:
        # Flip the image horizontally for a later selfie-view display, and convert
        # the BGR image to RGB.
        lock.acquire()
        image = cv2.cvtColor(cv2.flip(image_source, 1), cv2.COLOR_BGR2RGB)
        lock.release()
        # To improve performance, optionally mark the image as not writeable to
        # pass by reference.
        image.flags.writeable = False
        results = hands.process(image)
        # Draw the hand annotations on the image.
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                mp_drawing.draw_landmarks(
                    image, hand_landmarks, mp_hands.HAND_CONNECTIONS)
        cv2.imshow('MediaPipe Hands', image)
        if cv2.waitKey(5) & 0xFF == 27:
            break