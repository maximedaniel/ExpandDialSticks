import numpy as np
import cv2
import pythoncom
import threading
import os
import datetime
import paho.mqtt.client as mqtt

class SystemRecorder():

  def __init__(self, debug=False):
    self.debug = debug
    self.root = os.path.dirname(os.path.abspath(__file__))
    self.path = os.path.join( self.root, "Logs")
    if not os.path.isdir(self.path):
        os.makedirs(self.path)
    self.name = type(self).__name__
    self.f = None
    self.isStopped = True
  
  def stopped(self):
        return self.isStopped

  def start(self):
    try:
        self.isStopped = False
        timestamp = datetime.datetime.utcnow().isoformat().replace(':','.')
        logPath = os.path.join(self.path, timestamp+'_log.txt')
        self.f = open(logPath, "w")
    except Exception as e:
        print("["+ self.name + "] " + str(e))

  def stop(self):
    try: 
      self.f.close()
      self.isStopped = True
    except Exception as e:
        print("["+ self.name + "] " + str(e))
        
  def write(self, msg):
    try: 
        if not self.f:
          self.start()
        timestamp = datetime.datetime.utcnow().isoformat()
        self.f.write(timestamp + "|" + msg + "\n")
    except Exception as e:
        print("["+ self.name + "] " + str(e))

