import numpy as np
import cv2
import pythoncom
import threading
import os
import datetime
import paho.mqtt.client as mqtt
import time

class CameraRecorder(threading.Thread):

  def __init__(self, debug=False):
    threading.Thread.__init__(self)
    self._stop_event = threading.Event()
    self.debug = debug
    self.root = os.path.dirname(os.path.abspath(__file__))
    self.path = os.path.join( self.root, "Videos")
    if not os.path.isdir(self.path):
        os.makedirs(self.path)
    self.name = type(self).__name__
    self.isStopped = True

  def stop(self):
        self._stop_event.set()
        self.isStopped = True

  def stopped(self):
        return self.isStopped

  def run(self):
    #pythoncom.CoInitialize()
    try:
      self.isStopped = False
      cap = None
      cameraPortFound = False
      for cameraPort in range(2):
          #print(cameraPort)
          cap = cv2.VideoCapture(cameraPort + cv2.CAP_DSHOW)
          if cap.isOpened():
            #print("running!")
            cameraPortFound = True
            #cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280) # 640
            #cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720) # 480
            width  = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))   # float `width`
            height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))  # float `height`
            print("width: %i, height: %i" %(width, height))
            # Define the codec and create VideoWriter object
            fourcc = cv2.VideoWriter_fourcc(*'XVID')
            timestamp = datetime.datetime.utcnow().isoformat().replace(':','.')
            videoPath = os.path.join(self.path, timestamp+'_video.avi')
            #print("Starting recording camera" + str(cameraPort) + "(" + str(width) + ", "+ str(height) + ") at " + videoPath)
            out = cv2.VideoWriter(videoPath, fourcc, 20.0, (width,height))
            while(cap.isOpened() and not self.isStopped):
                #print("still running!")  
                ret, frame = cap.read()
                if ret == True:
                    out.write(frame)
            #print("cap.isOpened(): ", cap.isOpened())
            #print("self.isStopped: ", self.isStopped)
            cap.release()
            out.release()
            break
            #print("Ending recording camera" + str(cameraPort) + "(" + str(width) + ", "+ str(height) + ") at " + videoPath)
      if not cameraPortFound:
        print("["+ self.name +"] Could not open camera.")
    except Exception as e:
      print("["+ self.name +"] " + str(e))
    #finally:
    #  pythoncom.CoUninitialize()

  def run1(self):
    try:
      cap = None
      highestResCameraPort = 0
      highestRes = 0
      cameraPortFound = False
      for cameraPort in range(2):
        cap = cv2.VideoCapture(cameraPort + cv2.CAP_DSHOW)
        if cap.isOpened():
          width  = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))   # float `width`
          height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))  # float `height`
          res = width * height
          if res >= highestRes:
              highestResCameraPort = cameraPort
              cameraPortFound = True
          cap.release()

      if cameraPortFound:
        cap = cv2.VideoCapture(highestResCameraPort + cv2.CAP_DSHOW)
        if cap.isOpened():
          width  = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))   # float `width`
          height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))  # float `height`
          # Define the codec and create VideoWriter object
          fourcc = cv2.VideoWriter_fourcc(*'XVID')
          timestamp = datetime.datetime.utcnow().isoformat().replace(':','.')
          videoPath = os.path.join(self.path, timestamp+'_video.avi')
          print("Starting recording camera" + str(cameraPort) + "(" + str(width) + ", "+ str(height) + ") at " + videoPath)
          out = cv2.VideoWriter(videoPath, fourcc, 20.0, (width, height))
          while(cap.isOpened() and not self.isStopped):
              ret, frame = cap.read()
              if ret == True:
                  out.write(frame)
          cap.release()
          out.release()
          print("End recording camera" + str(cameraPort) + "(" + str(width) + ", "+ str(height) + ") at " + videoPath)
      else :
        print("["+ self.name +"] Could not open any camera.")
    except Exception as e:
      print("["+ self.name +"] " + str(e))

cameraRecorder = CameraRecorder()
cameraRecorder.start()
time.sleep(15)
cameraRecorder.stop()