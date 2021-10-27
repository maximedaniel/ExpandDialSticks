import threading
import os
import datetime
import socket
import time

TCP_IP = "127.0.0.1"
TCP_PORT = 28000
BUFFER_SIZE = 2048
TIME_OUT = 3
DEVICE_ID = "e3cb11"

class EmpaticaRecorder(threading.Thread):

  def __init__(self, debug=False):
    threading.Thread.__init__(self)
    self._stop_event = threading.Event()
    self.debug = debug
    self.root = os.path.dirname(os.path.abspath(__file__))
    self.path = os.path.join( self.root, "Physios")
    if not os.path.isdir(self.path):
        os.makedirs(self.path)
    self.name = type(self).__name__
    self.isStopped = True

  def stop(self):
        self.isStopped = True
        self._stop_event.set()

  def stopped(self):
        return self.isStopped
  
  def launch(self):
    try: 
      if self.isStopped:
        return
      # Create Socket
      s = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
      print("[%s] (1/9) Socket successfully created" %(self.name))

      # Connect Socket
      s.connect((TCP_IP, TCP_PORT))
      print("[%s] (2/9) Socket successfully connected to %s:%s" %(self.name, TCP_IP, TCP_PORT))
      # Set Socket timeout
      s.settimeout(TIME_OUT)

      # Connect to Empatica 4
      connect_req = "device_connect %s\n" %(DEVICE_ID)
      s.send(str.encode(connect_req))
      connect_ans = s.recv(BUFFER_SIZE)
      while b"R device_connect OK\n" not in connect_ans:
        connect_ans = s.recv(BUFFER_SIZE)
      print("[%s] (3/9) Connected to Empatica E4 @%s" %(self.name, DEVICE_ID))

      # PAUSED
      pause_on_req = "pause ON\n"
      s.send(str.encode(pause_on_req))
      pause_on_ans = s.recv(BUFFER_SIZE)
      while b"R pause ON\n" not in pause_on_ans:
        pause_on_ans = s.recv(BUFFER_SIZE)
      print("[%s] (4/9) Paused Empatica E4 @%s" %(self.name, DEVICE_ID))

      # SUBSCRIBED TO BVP
      subscribe_bvp_req = "device_subscribe bvp ON\n"
      s.send(str.encode(subscribe_bvp_req))
      subscribe_bvp_ans = s.recv(BUFFER_SIZE)
      while b"R device_subscribe bvp OK\n" not in subscribe_bvp_ans:
        subscribe_bvp_ans = s.recv(BUFFER_SIZE)
      print("[%s] (5/9) BVP Subscribed to Empatica E4 @%s" %(self.name, DEVICE_ID))

      # SUBSCRIBED TO GSR
      subscribe_gsr_req = "device_subscribe gsr ON\n"
      s.send(str.encode(subscribe_gsr_req))
      subscribe_gsr_ans = s.recv(BUFFER_SIZE)
      while b"R device_subscribe gsr OK\n" not in subscribe_gsr_ans:
        subscribe_gsr_ans = s.recv(BUFFER_SIZE)
      print("[%s] (6/9) GRS Subscribed to Empatica E4 @%s" %(self.name, DEVICE_ID))

      # SUBSCRIBED TO IBI
      subscribe_ibi_req = "device_subscribe ibi ON\n"
      s.send(str.encode(subscribe_ibi_req))
      subscribe_ibi_ans = s.recv(BUFFER_SIZE)
      while b"R device_subscribe ibi OK\n" not in subscribe_ibi_ans:
        subscribe_ibi_ans = s.recv(BUFFER_SIZE)
      print("[%s] (7/9) IBI Subscribed to Empatica E4 @%s" %(self.name, DEVICE_ID))

      # SUBSCRIBED TO TMP
      subscribe_tmp_req = "device_subscribe tmp ON\n"
      s.send(str.encode(subscribe_tmp_req))
      subscribe_tmp_ans = s.recv(BUFFER_SIZE)
      while b"R device_subscribe tmp OK\n" not in subscribe_tmp_ans:
        subscribe_tmp_ans = s.recv(BUFFER_SIZE)
      print("[%s] (8/9) TMP Subscribed to Empatica E4 @%s" %(self.name, DEVICE_ID))

      # CREATE LOG FILE
      try:
        timestamp = datetime.datetime.utcnow().isoformat().replace(':','.')
        logPath = os.path.join(self.path, timestamp+'_physios.txt')
        f = open(logPath, "w")
        # UNPAUSED
        pause_off_req = "pause OFF\n"
        s.send(str.encode(pause_off_req))
        pause_off_ans = s.recv(BUFFER_SIZE)
        while b"R pause OFF\n" not in pause_off_ans:
          pause_off_ans = s.recv(BUFFER_SIZE)
        print("[%s] (9/9) Unpaused Empatica E4 @%s" %(self.name, DEVICE_ID))
        # LOG WHILE NOT STOPPED
        while not self.isStopped:
          data_ans = s.recv(BUFFER_SIZE)
          dt = datetime.datetime.utcnow().isoformat()
          data = list(filter(None, data_ans.decode().split("\n")))
          for datum in data:
            f.write("%s|%s" %(dt, datum))
        # CLOSE FILE
        f.close()
      except Exception as e:
        print ("[%s] %s" %(self.name, str(e)))

      # PAUSED
      pause_on_req = "pause ON\n"
      s.send(str.encode(pause_on_req))
      pause_on_ans = s.recv(BUFFER_SIZE)
      while b"R pause ON\n" not in pause_on_ans:
        pause_on_ans = s.recv(BUFFER_SIZE)
      print("Paused Empatica E4 @%s" %(DEVICE_ID))
      # DISCONNECTED
      disconnect_req = "device_disconnect %s\n" %(DEVICE_ID)
      s.send(str.encode(disconnect_req))
      disconnect_ans = s.recv(BUFFER_SIZE)
      while  b"R device_disconnect OK\n" not in disconnect_ans:
        disconnect_ans = s.recv(BUFFER_SIZE)
      print("Disconnected from Empatica E4 @%s" %(DEVICE_ID))
      # CLOSE SOCKET
      s.close()
    except Exception as err: 
      print ("[%s] %s" %(self.name, err))
      time.sleep(3)
      self.launch()

  def run(self):
      self.isStopped = False
      self.launch()