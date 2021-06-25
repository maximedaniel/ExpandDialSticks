# 1. Open and connect E4 Streaming Server
# 2. python34 .\LoggerMQTT.py
# 3. [DEBUG] mosquitto_pub -h 127.0.0.1 -p 1883 -m <CMD> -t <TOPIC>

from CameraRecorder import CameraRecorder
from SystemRecorder import SystemRecorder
from EmpaticaRecorder import EmpaticaRecorder
import paho.mqtt.client as mqtt

EXPANDIALSTICKS_MQTT_ADDRESS = "192.168.0.10"
LOCALHOST_MQTT_ADDRESS = "127.0.0.1"
MQTT_PORT = 1883
MQTT_CAMERA_RECORDER = "CAMERA_RECORDER"
MQTT_EMPATICA_RECORDER  ="EMPATICA_RECORDER"
MQTT_SYSTEM_RECORDER  ="SYSTEM_RECORDER"
MQTT_UNKNOWN_TOPIC  ="UNKNOWN_TOPIC"
CMD_START  ="START"
CMD_STOP ="STOP"
CMD_UNKNOWN  ="UNKNOWN"

def on_connect(client, userdata, flags, rc):
    print("[LoggerMQTT] Connected with result code "+str(rc))
    client.subscribe(MQTT_CAMERA_RECORDER)
    client.subscribe(MQTT_EMPATICA_RECORDER)
    client.subscribe(MQTT_SYSTEM_RECORDER)

def on_disconnect(client, userdata, rc):
    print("[LoggerMQTT] Disconnected with result code "  +str(rc))
    client.connected_flag=False
    client.disconnect_flag=True

    if not cameraRecorder.stopped():
        cameraRecorder.stop()
        cameraRecorder.join()

    if not empaticaRecorder.stopped():
        empaticaRecorder.stop()
        empaticaRecorder.join()

    if not systemRecorder.stopped():
        systemRecorder.stop()
    quit()

def on_message(client, userdata, msg):
    global cameraRecorder
    global empaticaRecorder
    command = str(msg.payload.decode("utf-8"))
    # VideoRecorder Topic
    if msg.topic == MQTT_CAMERA_RECORDER:
      if command == CMD_START:
          print("[LoggerMQTT] %s|%s" %(MQTT_CAMERA_RECORDER, CMD_START))
          if not cameraRecorder.stopped():
              cameraRecorder.stop()
              cameraRecorder.join()
          cameraRecorder = CameraRecorder()
          cameraRecorder.start()

      elif command == CMD_STOP:
          print("[LoggerMQTT] %s|%s" %(MQTT_CAMERA_RECORDER, CMD_STOP))
          if not cameraRecorder.stopped():
              cameraRecorder.stop()
              cameraRecorder.join()

      else:
          print("[LoggerMQTT] %s|%s" %(MQTT_CAMERA_RECORDER, CMD_UNKNOWN))

    elif msg.topic == MQTT_EMPATICA_RECORDER:
      if command == CMD_START:
          print("[LoggerMQTT] %s|%s" %(MQTT_EMPATICA_RECORDER, CMD_START))
          if not empaticaRecorder.stopped():
              empaticaRecorder.stop()
              empaticaRecorder.join()
          empaticaRecorder = EmpaticaRecorder()
          empaticaRecorder.start()

      elif command == CMD_STOP:
          print("[LoggerMQTT] %s|%s" %(MQTT_EMPATICA_RECORDER, CMD_STOP))
          if not empaticaRecorder.stopped():
              empaticaRecorder.stop()
              empaticaRecorder.join()
      else:
          print("[LoggerMQTT] %s|%s" %(MQTT_EMPATICA_RECORDER, CMD_UNKNOWN))


    elif msg.topic == MQTT_SYSTEM_RECORDER:

      if command == CMD_START:
          print("[LoggerMQTT] %s|%s" %(MQTT_SYSTEM_RECORDER, CMD_START))
          if not systemRecorder.stopped():
              systemRecorder.stop()
          systemRecorder.start()

      elif command == CMD_STOP:
          print("[LoggerMQTT] %s|%s" %(MQTT_SYSTEM_RECORDER, CMD_STOP))
          if not systemRecorder.stopped():
              systemRecorder.stop()
      else:
          systemRecorder.write(command)
    else:
      print(MQTT_UNKNOWN_TOPIC)

cameraRecorder = CameraRecorder()
systemRecorder = SystemRecorder()
empaticaRecorder = EmpaticaRecorder()
client = mqtt.Client()
client.on_connect = on_connect
client.on_disconnect = on_disconnect
client.on_message = on_message
try:
    client.connect(EXPANDIALSTICKS_MQTT_ADDRESS, MQTT_PORT, 60)
    print("[LoggerMQTT] Connected to EXPANDIALSTICKS broker @%s:%i" %(EXPANDIALSTICKS_MQTT_ADDRESS, MQTT_PORT))
    client.loop_forever()
except Exception as e1:
    try:
        client.connect(LOCALHOST_MQTT_ADDRESS, MQTT_PORT, 60)
        print("[LoggerMQTT] Connected to LOCALHOST broker @%s:%i" %(LOCALHOST_MQTT_ADDRESS, MQTT_PORT))
        client.loop_forever()
    except Exception as e2:
        print("[LoggerMQTT] %s" %(e2))