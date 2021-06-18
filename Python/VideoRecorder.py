import numpy as np
import cv2
import pythoncom
import threading
import time
import datetime
import paho.mqtt.client as mqtt

class VideoRecorder(threading.Thread):

  def __init__(self, debug=False, path='video'):
    threading.Thread.__init__(self)
    self._stop_event = threading.Event()
    self.debug = debug
    self.path = path
    self.name = type(self).__name__
    self.isStopped = False
    
  def stop(self):
        self._stop_event.set()
        self.isStopped = True

  def stopped(self):
        return self._stop_event.is_set()

  def run(self):
    pythoncom.CoInitialize()
    try:
        cameraPort = 0
        cap = cv2.VideoCapture(cameraPort)
        if cap.isOpened():
          width  = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))   # float `width`
          height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))  # float `height`
          # Define the codec and create VideoWriter object
          fourcc = cv2.VideoWriter_fourcc(*'XVID')
          timestamp = datetime.datetime.now().isoformat().replace(':','.')
          videoPath = self.path+'/'+timestamp+'_video.avi'
          print("["+ self.name +"] recording camera" + str(cameraPort) + "(" + str(width) + ", "+ str(height) + ") at " + videoPath)
          out = cv2.VideoWriter(videoPath, fourcc, 20.0, (width,height))
          while(cap.isOpened() and not self.isStopped):
              #print(self.isStopped)
              ret, frame = cap.read()
              if ret == True:
                  # write the flipped frame
                  out.write(frame)
          # Release everything if job is finished
          cap.release()
          out.release()
        else:
          print("["+ self.name +"] could not open camera.")
    except Exception as e:
      print("["+ self.name +"] ", e)
    finally:
      pythoncom.CoUninitialize()


def on_connect(client, userdata, flags, rc):
    print("Connected with result code "+str(rc))
    client.subscribe("VideoRecorder")

def on_message(client, userdata, msg):
    try:
      command = str(msg.payload.decode("utf-8"))
      if command == 'start':
        videoRecorder.start()
        print("["+ videoRecorder.name +"] Start.")
      elif command == 'stop':
        videoRecorder.stop()
        print("["+ videoRecorder.name +"] Stop.")
      elif command == 'exit':
        client.disconnect()
        print("["+ videoRecorder.name +"] Exit.")
      else :
        print("["+ videoRecorder.name +"] Unknown payload.")
    except Exception as e:
        print(e)

videoRecorder = VideoRecorder()
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect("127.0.0.1", 1883, 60)

# mosquitto
# python34 .\VideoRecorder.py
# mosquitto_pub -h 127.0.0.1 -p 1883 -m "exit" -t videoRecorder
client.loop_forever()
