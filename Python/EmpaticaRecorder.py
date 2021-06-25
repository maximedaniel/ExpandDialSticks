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

  def run(self):
    try: 
      self.isStopped = False
      # Create Socket
      s = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
      #print("Socket successfully created")
      # Connect Socket
      s.connect((TCP_IP, TCP_PORT))
      #print("Socket successfully connected to %s:%s" %(TCP_IP, TCP_PORT))
      # Set Socket timeout
      s.settimeout(TIME_OUT)
      # Connect to Empatica 4
      connect_req = "device_connect %s\n" %(DEVICE_ID)
      s.send(str.encode(connect_req))
      connect_ans = s.recv(BUFFER_SIZE)
      assert connect_ans == b"R device_connect OK\n", connect_ans.decode().replace("\n", "")
      #print("Connected to Empatica E4 @%s" %(DEVICE_ID))
      # PAUSED
      pause_on_req = "pause ON\n"
      s.send(str.encode(pause_on_req))
      pause_on_ans = s.recv(BUFFER_SIZE)
      while b"R pause ON\n" not in pause_on_ans:
        pause_on_ans = s.recv(BUFFER_SIZE)
      assert pause_on_ans == b"R pause ON\n", pause_on_ans.decode().replace("\n", "")
      #print("Paused Empatica E4 @%s" %(DEVICE_ID))
      # SUBSCRIBED TO BVP
      subscribe_bvp_req = "device_subscribe bvp ON\n"
      s.send(str.encode(subscribe_bvp_req))
      subscribe_bvp_ans = s.recv(BUFFER_SIZE)
      assert subscribe_bvp_ans == b"R device_subscribe bvp OK\n", subscribe_bvp_ans.decode().replace("\n", "")
      #print("BVP Subscribed to Empatica E4 @%s" %(DEVICE_ID))
      # SUBSCRIBED TO GSR
      subscribe_gsr_req = "device_subscribe gsr ON\n"
      s.send(str.encode(subscribe_gsr_req))
      subscribe_gsr_ans = s.recv(BUFFER_SIZE)
      assert subscribe_gsr_ans == b"R device_subscribe gsr OK\n", subscribe_gsr_ans.decode().replace("\n", "")
      #print("GRS Subscribed to Empatica E4 @%s" %(DEVICE_ID))
      # SUBSCRIBED TO IBI
      subscribe_ibi_req = "device_subscribe ibi ON\n"
      s.send(str.encode(subscribe_ibi_req))
      subscribe_ibi_ans = s.recv(BUFFER_SIZE)
      assert subscribe_ibi_ans == b"R device_subscribe ibi OK\n", subscribe_ibi_ans.decode().replace("\n", "")
      #print("IBI Subscribed to Empatica E4 @%s" %(DEVICE_ID))
      # SUBSCRIBED TO TMP
      subscribe_tmp_req = "device_subscribe tmp ON\n"
      s.send(str.encode(subscribe_tmp_req))
      subscribe_tmp_ans = s.recv(BUFFER_SIZE)
      assert subscribe_tmp_ans == b"R device_subscribe tmp OK\n", subscribe_tmp_ans.decode().replace("\n", "")
      #print("TMP Subscribed to Empatica E4 @%s" %(DEVICE_ID))
      # CREATE LOG FILE
      try:
        timestamp = datetime.datetime.utcnow().isoformat().replace(':','.')
        logPath = os.path.join(self.path, timestamp+'_physios.txt')
        f = open(logPath, "w")
        # UNPAUSED
        pause_off_req = "pause OFF\n"
        s.send(str.encode(pause_off_req))
        pause_off_ans = s.recv(BUFFER_SIZE)
        assert pause_off_ans == b"R pause OFF\n", pause_off_ans.decode().replace("\n", "")
        # LOG WHILE NOT STOPPED
        while not self.isStopped:
          data_ans = s.recv(BUFFER_SIZE)
          dt = datetime.datetime.utcnow().isoformat()
          data = list(filter(None, data_ans.decode().split("\n")))
          for datum in data:
            f.write("%s|%s\n" %(dt, datum))
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
      assert pause_on_ans == b"R pause ON\n", pause_on_ans.decode().replace("\n", "")
      #print("Paused Empatica E4 @%s" %(DEVICE_ID))
      # DISCONNECTED
      disconnect_req = "device_disconnect %s\n" %(DEVICE_ID)
      s.send(str.encode(disconnect_req))
      disconnect_ans = s.recv(BUFFER_SIZE)
      assert disconnect_ans == b"R device_disconnect OK\n", disconnect_ans.decode().replace("\n", "")
      #print("Disconnected from Empatica E4 @%s" %(DEVICE_ID))
      # CLOSE SOCKET
      s.close()
    except Exception as err: 
      print ("[%s] %s" %(self.name, err))
      exit()
