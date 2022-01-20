import sys
import paho.mqtt.client as mqtt
from smbus2 import SMBus, i2c_msg
from bitarray import bitarray
import time
import struct
import json
from colorama import init, Fore, Back, Style
init()

# debug utils
def verbose(msg=""):
    print(msg)
def info(msg = "", topic = " INFO "):
    print("%s%s%s%s %s" % (Back.WHITE, Fore.BLACK, topic, Style.RESET_ALL, msg))
def warn(msg = "", topic = " WARNING "):
    print("%s%s%s%s %s" % (Back.YELLOW, Fore.BLACK, topic, Style.RESET_ALL, msg))
def err(msg = "", topic = " ERROR "):
    print("%s%s%s%s %s" % (Back.RED, Fore.BLACK, topic, Style.RESET_ALL, msg))
def ok(msg = "", topic = " O ", nbSpace = 0):
    print("%*s%s%s%s %s" % (nbSpace, Back.GREEN, Fore.BLACK, topic, Style.RESET_ALL, msg))
def notOk(msg = "", topic = " X ", nbSpace = 0):
    print("%*s%s%s%s %s" % (nbSpace, Back.RED, Fore.BLACK, topic, Style.RESET_ALL, msg))

# constants and variables
I2C_DELAY_MILLIS = 200
NO_CMD = 0x00
GET_CMD = 0x01
SET_CMD = 0x02

BUFFER_SIZE = 32

MQTT_TOPIC = "ExpanDialSticks"
MQTT_BAD_VALUES = "bad values"
MQTT_WRONG_LENGTH = "wrong length"
MQTT_MISSING_KEY = "missing key"
MQTT_UNKNOWN_CMD = "unknown command"
MQTT_VALUE_ERROR = "json value error"
MQTT_SUCCESS = "success"
SMBUS_IO_ERROR = "I/O error"
NONE_ANS = -1
SUCCESS_ANS = 0
DURATION_ERROR_ANS = 1
ID_ERROR_ANS = 2
RESET_ERROR_ANS = 3
POSITION_ERROR_ANS = 4

NUM_DIALSTICK = 6
MAX_DIALSTICK = 6
STATE_SIZE = 9

SUCCESS_ANS = 0
BUFFER_LENGTH_ERR_ANS = 1
NACK_ADDR_ERR_ANS = 2
NACK_DATA_ERR_ANS = 3
OTHER_ERR_ANS = 4

I2C_SLAVE_ADDRESS = [0x10, 0x11, 0x12, 0x13, 0x14]


mqtt_verbose = False
driver_verbose = True
request = {}
backup = {}
state = {}
state['xAxisValue'] = [0 for i in range(MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS))]
state['yAxisValue'] = [0 for i in range(MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS))]
state['selectCountValue'] = [0 for i in range(MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS))]
state['rotationValue'] = [0 for i in range(MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS))]
state['positionValue'] = [0 for i in range(MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS))]
state['reachingValue'] = [0 for i in range(MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS))]
state['holdingValue'] = [0 for i in range(MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS))]

    


def on_connect(client, userdata, flags, rc):
    print("Connected with result code "+str(rc))
    client.subscribe(MQTT_TOPIC)

def on_message(client, userdata, msg):
    global request, backup, mqtt_verbose, driver_verbose
    str_payload = str(msg.payload)
    if mqtt_verbose:
        verbose(msg.topic+" "+ str_payload)
    if msg.topic == MQTT_TOPIC:
        if 'mqtt verbose on' in str_payload:
            info("mqtt verbose on")
            mqtt_verbose = True
            return
        if 'mqtt verbose off' in str_payload:
            info("mqtt verbose off")
            mqtt_verbose = False
            return
        if 'driver verbose on' in str_payload:
            info("mqtt driver on")
            driver_verbose = True
            return
        if 'driver verbose off' in str_payload:
            info("mqtt driver off")
            driver_verbose = False
            return
        try: 
            msg_dict = json.loads(msg.payload.decode("utf-8"))
            if 'ANS' not in msg_dict:
                    if 'GET' not in msg_dict and 'SET' not in msg_dict:
                            client.publish(MQTT_TOPIC, json.dumps({'ANS':{'status':MQTT_UNKNOWN_CMD, 'content':{}}}))
                            return
                            
                    if 'SET' in msg_dict:
                        if 'position' not in msg_dict['SET'] or 'duration' not in msg_dict['SET'] or 'holding' not in msg_dict['SET']:
                            client.publish(MQTT_TOPIC, json.dumps({'ANS':{'status':MQTT_MISSING_KEY, 'content':{}}}))
                            return
                            
                        elif len(msg_dict['SET']['position']) != MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS) or len(msg_dict['SET']['duration']) != MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS)  or len(msg_dict['SET']['holding']) != MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS):
                            client.publish(MQTT_TOPIC, json.dumps({'ANS':{'status':MQTT_WRONG_LENGTH, 'content':{}}}))
                            return
                            
                        elif (
                        sum([isinstance(val, int) for val in msg_dict['SET']['position']]) != MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS)
                        or sum([(isinstance(val, float) or isinstance(val, int)) for val in msg_dict['SET']['duration']]) != MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS)
                        or sum([(isinstance(val, int) and (val == 0 or val == 1 )) for val in msg_dict['SET']['holding']]) != MAX_DIALSTICK * len(I2C_SLAVE_ADDRESS)
                        ):
                            client.publish(MQTT_TOPIC, json.dumps({'ANS':{'status':MQTT_BAD_VALUES, 'content':{}}}))
                            return
                            
                    if request == {}: # no conccurent command
                        if backup != {}: # there is a backup command
                            if driver_verbose:
                                info("BACKUP SET request sended");
                            request = backup
                        else :
                            request = msg_dict
                    else: # concurrent command
                        if 'SET' in msg_dict: # give priority to SET Command
                                if backup != {}: # backup command already exists
                                    if driver_verbose:
                                        warn("Previous BACKUP SET request replaced : %s" %(backup))
                                backup = msg_dict
                                if driver_verbose:
                                    info("BACKUP SET request saved")
                        
                        
        except ValueError as e:
            client.publish(MQTT_TOPIC, json.dumps({'ANS':{'status':MQTT_VALUE_ERROR, 'content':{}}}))
            return

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect("localhost", 1883, 60)
client.loop_start()

#prevMillis = 0

status = 0

with SMBus(1) as bus:
    # READ
    while True :
        #mosquitto_pub -h localhost -t "ExpanDialSticks" -m '{"GET":[]}'
        if request == {} and backup != {}:
            if driver_verbose:
                info("BACKUP SET request used");
            request = backup
            backup = {}
        if 'GET' in request:
            status = MQTT_SUCCESS
            for i, slaveAddress in enumerate(I2C_SLAVE_ADDRESS):
                try : 
                    write = i2c_msg.write(slaveAddress, [GET_CMD])
                    read = i2c_msg.read(slaveAddress, BUFFER_SIZE)
                    bus.i2c_rdwr(write, read)
                    
                    k = 0
                    # X Axis Bytes
                    for j in range(MAX_DIALSTICK):
                        state['xAxisValue'][i*MAX_DIALSTICK + j] = int.from_bytes(read.buf[k], signed= 'true', byteorder='big')
                        k+=1
                    
                    # Y Axis Bytes
                    for j in range(MAX_DIALSTICK):
                        state['yAxisValue'][i*MAX_DIALSTICK + j] = int.from_bytes(read.buf[k], signed= 'true', byteorder='big')
                        k+=1
                    
                    # Select Count Bytes
                    for j in range(MAX_DIALSTICK):
                        state['selectCountValue'][i*MAX_DIALSTICK + j] = int.from_bytes(read.buf[k], signed= 'false', byteorder='big')
                        k+=1
                    
                    # Rotation Bytes
                    for j in range(MAX_DIALSTICK):
                        state['rotationValue'][i*MAX_DIALSTICK + j] = int.from_bytes(read.buf[k], signed= 'true', byteorder='big')
                        k+=1
                        
                    # Position Bytes
                    for j in range(MAX_DIALSTICK):
                        state['positionValue'][i*MAX_DIALSTICK + j] = int.from_bytes(read.buf[k], signed= 'true', byteorder='big')
                        k+=1
                        
                    # Reaching Byte
                    reachingByte = read.buf[k][0]
                    k+=1
                    for j in range (MAX_DIALSTICK):
                        state['reachingValue'][i*MAX_DIALSTICK + j] = int(bool(reachingByte & (1 << (7- j))))
                    
                    # Holding Byte
                    holdingByte = read.buf[k][0]
                    k+=1
                    for j in range (MAX_DIALSTICK):
                        state['holdingValue'][i*MAX_DIALSTICK + j] = int(bool(holdingByte & (1 << (7- j))))
                    
                except OSError as err:
                    print(slaveAddress, err)
                    status = SMBUS_IO_ERROR
                    
            request["ANS"] = {'status': status, 'content': state} 
            client.publish(MQTT_TOPIC, json.dumps(request))
            request = {}
            
        #mosquitto_pub -h localhost -t "ExpanDialSticks" -m '{"SET":{"position":[1, 15, 20, 40, 5, 0,1, 15, 20, 40, 5, 0,1, 15, 20, 40, 5, 0,1, 15, 20, 40, 5, 0,1, 15, 20, 40, 5, 0,1, 15, 20, 40, 5, 0], "duration": [1.5, 1, 2, 2.5, 1.9, 2.1,1.5, 1, 2, 2.5, 1.9, 2.1,1.5, 1, 2, 2.5, 1.9, 2.1,1.5, 1, 2, 2.5, 1.9, 2.1,1.5, 1, 2, 2.5, 1.9, 2.1], "holding":[0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1]}}'

        #mosquitto_pub -h localhost -t "ExpanDialSticks" -m '{"SET":{"position":[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], "duration": [1.5, 1, 2, 2.5, 1.9, 2.1,1.5, 1, 2, 2.5, 1.9, 2.1,1.5, 1, 2, 2.5, 1.9, 2.1,1.5, 1, 2, 2.5, 1.9, 2.1,1.5, 1, 2, 2.5, 1.9, 2.1], "holding":[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]}}'

        if 'SET' in request:
            ans = []
            status = MQTT_SUCCESS
            for i, slaveAddress in enumerate(I2C_SLAVE_ADDRESS):
                positionCmd = []
                durationCmd = []
                holdCmd = 0b00000000
                for j in range(MAX_DIALSTICK):
                    positionCmd += list(struct.pack("b", request["SET"]["position"][i*MAX_DIALSTICK + j])) # integer
                    durationCmd += list(struct.pack("f", request["SET"]["duration"][i*MAX_DIALSTICK + j])) # float
                    holdCmd |= (request["SET"]["holding"][i*MAX_DIALSTICK + j] << (7- j)) # bool
                
                cmd = [SET_CMD] + positionCmd + durationCmd + [holdCmd]
                currAns = [NONE_ANS for j in range(MAX_DIALSTICK)]
                try : 
                    write = i2c_msg.write(slaveAddress, cmd)
                    read = i2c_msg.read(slaveAddress, MAX_DIALSTICK)
                    bus.i2c_rdwr(write, read)
                    for j in range(read.len):
                        currAns [j] = int.from_bytes(read.buf[j], signed= 'true', byteorder='big')
                except OSError as err:
                    #print(slaveAddress, err)
                    status = SMBUS_IO_ERROR
                ans += currAns
            request["ANS"] = {'status': status, 'content': ans} 
            client.publish(MQTT_TOPIC, json.dumps(request))
            request = {}
