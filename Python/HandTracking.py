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

#calibrated = False
#toCalibrate = False

# imageWidth = 814
# imageHeight = 458
# rows = 5
# columns = 6
# expandialsticks = []

# originSquare = np.array([[289,179],[530,179],[585,415], [239,415]], np.int32)

# for i in range(rows):
#     expandialsticks.append([])
#     for j in range (columns):
#         expandialsticks[i].append((i * 10, j * 10))

# def calibrate(src_img):
#     print("Calibrating...")
#     thresholding = False
#     img = src_img
#     # Median Blur
#     #img = cv2.medianBlur(img, 3)
#     # Gaussian Blur
#     #img = cv2.GaussianBlur(img, (3,3), 0)
#     kernel = np.ones((5,5), np.uint8)
#     # Erosion
#     #img = cv2.erode(src_img, kernel, iterations=1)
#     # Dilatation
#     #img = cv2.dilate(img, kernel, iterations=1)
#     # Opening
#     img = cv2.morphologyEx(img, cv2.MORPH_OPEN, kernel, iterations=1)
#     # kernel & convolution
#     kernel = np.array([[-1,-1,-1], [-1,9,-1], [-1,-1,-1]])
#     img = cv2.filter2D(img, -1, kernel)
    
#     gray = cv2.cvtColor(img,cv2.COLOR_BGR2GRAY)
#     if thresholding:
#         mostBrightness = np.argmax(np.bincount(gray.flatten()))
#         print("most brigthness:", mostBrightness)
#         ret,img = cv2.threshold(img,mostBrightness,255,cv2.THRESH_TOZERO)
#         meanBrightness = np.mean(img)
#         print("mean brigthness:", meanBrightness)
#         ret,img = cv2.threshold(img,meanBrightness,255,cv2.THRESH_TOZERO)

#     gray = np.float32(gray)
#     dst = cv2.cornerHarris(gray,2,3,0.04)
#     #result is dilated for marking the corners, not important
#     dst = cv2.dilate(dst,None)
#     # Threshold for an optimal value, it may vary depending on the image.
#     #img[dst>0.04*dst.max()]=[0,0,255]

#     # CHESSBOARD TRACKING
#     # termination criteria
#     criteria = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)
#     # prepare object points, like (0,0,0), (1,0,0), (2,0,0) ....,(6,5,0)
#     objp = np.zeros((6*8,3), np.float32)
#     objp[:,:2] = np.mgrid[0:8,0:6].T.reshape(-1,2)
#     # Arrays to store object points and image points from all the images.
#     objpoints = [] # 3d point in real world space
#     imgpoints = [] # 2d points in image plane.
#     # Find the chess board corners
#     ret, corners = cv2.findChessboardCorners(img, (8,6),None)
#     # If found, add object points, image points (after refining them)
#     if ret == True:
#         print('chess found !')
#         objpoints.append(objp)
#         corners2 = cv2.cornerSubPix(gray,corners,(11,11),(-1,-1),criteria)
#         imgpoints.append(corners2)
#         # Draw and display the corners
#         img = cv2.drawChessboardCorners(img, (8,6), corners2,ret)
#         cv2.imwrite("calibration.png", img)
#     else:
#         print('chess not found !')
#     cv2.imshow('MediaPipe Hands', np.hstack([src_img, img]))


#     #cv2.imshow('dst', np.hstack([src_img, img])) #np.hstack([img,sharpen, dst]))
#     #  if cv2.waitKey(0) & 0xff == 27:
#     #   cv2.destroyAllWindows()
#     return ret


def on_connect(client, userdata, flags, rc):
    print("Connected with result code "+str(rc))
    client.subscribe("LeapMotion")

def on_message(client, userdata, msg):
    try:
        global q, calibrated, toCalibrate
        png_as_np = np.frombuffer(msg.payload, dtype=np.uint8)
        np_to_img = cv2.imdecode(png_as_np, flags=1)
        # if not calibrated:
        #     #height, width, channels = np_to_img.shape
        #     #print(height,  width, channels)
        #     #cv2.imwrite("calibration.png", np_to_img)
        #     toCalibrate = True
        q.put(np_to_img)
    except Exception as e:
        print(e)

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect("127.0.0.1", 1883, 60)

client.loop_start()

with mp_hands.Hands(
    min_detection_confidence=0.5,
    min_tracking_confidence=0.4) as hands:
    while True:
        while not q.empty():
            img = q.get()
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
        if cv2.waitKey(5) & 0xFF == 27:
            break

