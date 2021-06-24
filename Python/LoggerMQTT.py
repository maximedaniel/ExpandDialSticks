from CameraRecorder import CameraRecorder
from SystemRecorder import SystemRecorder
import paho.mqtt.client as mqtt

MQTT_CAMERA_RECORDER = "CAMERA_RECORDER"
MQTT_EMPATICA_RECORDER  ="EMPATICA_RECORDER"
MQTT_SYSTEM_RECORDER  ="SYSTEM_RECORDER"
MQTT_UNKNOWN_TOPIC  ="UNKNOWN_TOPIC"
CMD_START  ="START"
CMD_STOP ="STOP"
CMD_UNKNOWN  ="UNKNOWN"

def on_connect(client, userdata, flags, rc):
    print("Connected with result code "+str(rc))
    client.subscribe(MQTT_CAMERA_RECORDER)
    client.subscribe(MQTT_EMPATICA_RECORDER)
    client.subscribe(MQTT_SYSTEM_RECORDER)

def on_message(client, userdata, msg):
    global cameraRecorder
    command = str(msg.payload.decode("utf-8"))
    # VideoRecorder Topic
    if msg.topic == MQTT_CAMERA_RECORDER:

      if command == CMD_START:
          print(MQTT_CAMERA_RECORDER + " | " + CMD_START)
          if cameraRecorder.is_alive():
              cameraRecorder.stop()
              cameraRecorder.join()
          cameraRecorder = CameraRecorder()
          cameraRecorder.start()

      elif command == CMD_STOP:
          print(MQTT_CAMERA_RECORDER + " | " + CMD_STOP)
          cameraRecorder.stop()

      else:
          print(MQTT_CAMERA_RECORDER + " | " + CMD_UNKNOWN)
    elif msg.topic == MQTT_EMPATICA_RECORDER:

      if command == CMD_START:
          print(MQTT_EMPATICA_RECORDER + " | " + CMD_START)
      elif command == CMD_STOP:
          print(MQTT_EMPATICA_RECORDER + " | " + CMD_STOP)
      else:
          print(MQTT_EMPATICA_RECORDER + " | " + CMD_UNKNOWN)


    elif msg.topic == MQTT_SYSTEM_RECORDER:
      if command == CMD_START:
          print(MQTT_SYSTEM_RECORDER + " | " + CMD_START)
          systemRecorder.start()
      elif command == CMD_STOP:
          print(MQTT_SYSTEM_RECORDER + " | " + CMD_STOP)
          systemRecorder.stop()
      else:
          systemRecorder.write(command)
    else:
      print(MQTT_UNKNOWN_TOPIC)

cameraRecorder = CameraRecorder()
systemRecorder = SystemRecorder()
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect("127.0.0.1", 1883, 60)

# mosquitto
# python34 .\LoggerMQTT.py
# mosquitto_pub -h 127.0.0.1 -p 1883 -m "exit" -t videoRecorder
client.loop_forever()