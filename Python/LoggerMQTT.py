import paho.mqtt.client as mqtt

MQTT_VIDEO_RECORDER = "VIDEO_RECORDER"
MQTT_EMPATICA_RECORDER  ="EMPATICA_RECORDER"
MQTT_SYSTEM_RECORDER  ="SYSTEM_RECORDER"

def on_connect(client, userdata, flags, rc):
    print("Connected with result code "+str(rc))
    client.subscribe(MQTT_VIDEO_RECORDER)
    client.subscribe(MQTT_EMPATICA_RECORDER)
    client.subscribe(MQTT_SYSTEM_RECORDER)

def on_message(client, userdata, msg):
    if msg.topic == MQTT_VIDEO_RECORDER:
    elif msg.topic == MQTT_VIDEO_RECORDER:
    elif msg.topic == MQTT_VIDEO_RECORDER:
    else:
        print("Unknown topic: " +msg.topic)
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

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect("127.0.0.1", 1883, 60)

# mosquitto
# python34 .\LoggerMQTT.py
# mosquitto_pub -h 127.0.0.1 -p 1883 -m "exit" -t videoRecorder
client.loop_forever()