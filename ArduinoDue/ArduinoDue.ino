#include "Rotary.h"
#include <Servo.h>
#include <Wire.h>

#define I2C_BUS 0x11
#define I2C_BAUDRATE 200000 //hz


#define NO_CMD 0x0
#define GET_CMD 0x01
#define SET_CMD 0x02

#define NONE_ANS -1
#define SUCCESS_ANS 0 
#define DURATION_ERROR_ANS 1
#define ID_ERROR_ANS 2
#define RESET_ERROR_ANS 3
#define POSITION_ERROR_ANS 4

#define NUM_DIALSTICK 6
#define MAX_DIALSTICK 6
#define STATE_SIZE 9

#define DEBUG 0

// COMMUNICATION PARAMETERS
unsigned char requestedCmd = NO_CMD;
signed char requestAns [MAX_DIALSTICK];

// TIMER PARAMETERS
unsigned long PERIOD_DELAY_MS = 50;
const float PERIOD_DELAY_S = PERIOD_DELAY_MS/(float)1000;
unsigned long prevPeriodMillis [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};

unsigned long prevDisplayMillis = 0; 
unsigned long currentDisplayMillis = 0;
unsigned long DISPLAY_DELAY = 2000;
bool displayMode = false;

// JOYSTICK PARAMETERS
const int MIN_VAL_FROM = 0;
const int MAX_VAL_FROM = 1023;
const int MIN_VAL_TO = -128;
const int MAX_VAL_TO = 127;

const int pinAxisX [MAX_DIALSTICK] = {A5, A4, A3, A2, A1, A0};
const int pinAxisY [MAX_DIALSTICK] = {A11, A10, A9, A8, A7, A6};
const int pinButton [MAX_DIALSTICK] = {2, 3, 4, 5, 6, 7};
volatile signed char xAxisValue [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};
volatile signed char yAxisValue [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};
volatile bool buttonValue [MAX_DIALSTICK] = {false, false, false, false, false, false};
volatile unsigned char selectCount [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};
unsigned long DEBOUNCE_DELAY_MS = 5;
unsigned long prevDebounceMillis [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};
unsigned long currentDebounceMillis [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};


// DIAL PARAMETERS
const int pinRotationA [MAX_DIALSTICK] = {53, 51, 49, 47, 45, 43};
const int pinRotationB [MAX_DIALSTICK] = {52, 50, 48, 46, 44, 42};
Rotary dial [MAX_DIALSTICK] = {
  Rotary(pinRotationA[0], pinRotationB[0]),
  Rotary(pinRotationA[1], pinRotationB[1]),
  Rotary(pinRotationA[2], pinRotationB[2]),
  Rotary(pinRotationA[3], pinRotationB[3]),
  Rotary(pinRotationA[4], pinRotationB[4]),
  Rotary(pinRotationA[5], pinRotationB[5])
};

volatile signed char dialValue [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};

// ENCODER PARAMETERS
const int RESET_POSITION = -1;
const int MIN_POSITION = 0;
const int MAX_POSITION = 49;

const int pinPositionA [MAX_DIALSTICK] = {41, 39, 37, 35, 33, 31};
const int pinPositionB [MAX_DIALSTICK] = {40, 38, 36, 34, 32, 30};
Rotary encoder [MAX_DIALSTICK] = {
  Rotary(pinPositionA[0], pinPositionB[0]),
  Rotary(pinPositionA[1], pinPositionB[1]),
  Rotary(pinPositionA[2], pinPositionB[2]),
  Rotary(pinPositionA[3], pinPositionB[3]),
  Rotary(pinPositionA[4], pinPositionB[4]),
  Rotary(pinPositionA[5], pinPositionB[5])
  };
volatile signed char encoderValue [MAX_DIALSTICK] = {MAX_POSITION, MAX_POSITION, MAX_POSITION, MAX_POSITION, MAX_POSITION, MAX_POSITION};

// SWITCH PARAMETERS
const int pinSwitch [MAX_DIALSTICK] = {22, 24, 26, 27, 25, 23};
volatile bool switchValue [MAX_DIALSTICK] = {false, false, false, false, false, false};

// SERVO PARAMETERS
const int MIN_SPEED = 4; // unit/s
const int MAX_SPEED = 40; // unit/s
const int DEAD_BAND_PULSE = 45;
const int IDLE_PULSE = 1500;
const int MIN_CW_PULSE = IDLE_PULSE - DEAD_BAND_PULSE;
const int MAX_CW_PULSE = 1185;
const int MIN_CCW_PULSE = IDLE_PULSE + DEAD_BAND_PULSE;
const int MAX_CCW_PULSE = 1815;
const int RANGE_PULSE = IDLE_PULSE-MAX_CW_PULSE; //315;
const int MAX_DURATION = MAX_POSITION/(float)MIN_SPEED; // unit/s

const int pinServo [MAX_DIALSTICK] = {8, 9, 10, 11, 12, 13};
Servo servo [MAX_DIALSTICK] = {
  Servo(),
  Servo(),
  Servo(),
  Servo(),
  Servo(),
  Servo()
  };
bool servoIsReaching [MAX_DIALSTICK] = {false, false, false, false, false, false};
unsigned char servoDirection [MAX_DIALSTICK] = {DIR_NONE, DIR_NONE, DIR_NONE, DIR_NONE, DIR_NONE, DIR_NONE};
signed char servoDistance [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};
unsigned short servoPulse [MAX_DIALSTICK] = {IDLE_PULSE, IDLE_PULSE, IDLE_PULSE, IDLE_PULSE, IDLE_PULSE, IDLE_PULSE};
bool servoIsHolding [MAX_DIALSTICK] = {false, false, false, false, false, false};
volatile signed char encoderTargetValue [MAX_DIALSTICK] = {MAX_POSITION, MAX_POSITION, MAX_POSITION, MAX_POSITION, MAX_POSITION, MAX_POSITION};
float speedRate [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};
float duration [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};

// STATE ARRAY
byte state [MAX_DIALSTICK][STATE_SIZE] = {
  {0, 0, 0, 0, 0, 0, 0, 0, 0},
  {0, 0, 0, 0, 0, 0, 0, 0, 0},
  {0, 0, 0, 0, 0, 0, 0, 0, 0},
  {0, 0, 0, 0, 0, 0, 0, 0, 0},
  {0, 0, 0, 0, 0, 0, 0, 0, 0},
  {0, 0, 0, 0, 0, 0, 0, 0, 0},
};
byte stateEmpty [STATE_SIZE] = {0, 0, 0, 0, 0, 0, 0, 0, 0};
byte emptyValue [MAX_DIALSTICK] = {0, 0, 0, 0, 0, 0};
    
 
unsigned long prevTimerMillis; 
unsigned long currentTimerMillis;

// INTERRUPTION TIMER
unsigned long prevInterruptMillis; 
unsigned long currentInterruptMillis;

// INTERRUPTION METHODS
void rotate0(){
  unsigned char result = dial[0].process();
  if (result == DIR_CW) {
    dialValue[0]++;
  } else if (result == DIR_CCW) {
    dialValue[0]--;
  } 
}

void rotate1(){
  unsigned char result = dial[1].process();
  if (result == DIR_CW) {
    dialValue[1]++;
  } else if (result == DIR_CCW) {
    dialValue[1]--;
  } 
}

void rotate2(){
  unsigned char result = dial[2].process();
  if (result == DIR_CW) {
    dialValue[2]++;
  } else if (result == DIR_CCW) {
    dialValue[2]--;
  } 
}

void rotate3(){
  unsigned char result = dial[3].process();
  if (result == DIR_CW) {
    dialValue[3]++;
  } else if (result == DIR_CCW) {
    dialValue[3]--;
  } 
}

void rotate4(){
  unsigned char result = dial[4].process();
  if (result == DIR_CW) {
    dialValue[4]++;
  } else if (result == DIR_CCW) {
    dialValue[4]--;
  } 
}

void rotate5(){
  unsigned char result = dial[5].process();
  if (result == DIR_CW) {
    dialValue[5]++;
  } else if (result == DIR_CCW) {
    dialValue[5]--;
  } 
}

void encode0(){
  unsigned char result = encoder[0].process();
  if (result == DIR_CW) encoderValue[0] = max(0, encoderValue[0]+1);
   else if (result == DIR_CCW) encoderValue[0] = max(0, encoderValue[0]-1);
}

void encode1(){
  unsigned char result = encoder[1].process();
  if (result == DIR_CW) encoderValue[1] = max(0, encoderValue[1]+1);
   else if (result == DIR_CCW) encoderValue[1] = max(0, encoderValue[1]-1);
}

void encode2(){
  unsigned char result = encoder[2].process();
  if (result == DIR_CW) encoderValue[2] = max(0, encoderValue[2]+1);
   else if (result == DIR_CCW) encoderValue[2] = max(0, encoderValue[2]-1);
}

void encode3(){
  unsigned char result = encoder[3].process();
  if (result == DIR_CW) encoderValue[3] = max(0, encoderValue[3]+1);
   else if (result == DIR_CCW) encoderValue[3] = max(0, encoderValue[3]-1);
}

void encode4(){
  unsigned char result = encoder[4].process();
  if (result == DIR_CW) encoderValue[4] = max(0, encoderValue[4]+1);
   else if (result == DIR_CCW) encoderValue[4] = max(0, encoderValue[4]-1);
}

void encode5(){
  unsigned char result = encoder[5].process();
  if (result == DIR_CW) encoderValue[5] = max(0, encoderValue[5]+1);
   else if (result == DIR_CCW) encoderValue[5] = max(0, encoderValue[5]-1);
}

void attachAllInterrupts(){
  
  // INTERUPTION ATTACHMENT
  if(NUM_DIALSTICK > 0){
    attachInterrupt(digitalPinToInterrupt(pinRotationA[0]), rotate0, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinRotationB[0]), rotate0, CHANGE);
  
    attachInterrupt(digitalPinToInterrupt(pinPositionA[0]), encode0, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinPositionB[0]), encode0, CHANGE);
  }
  
  if(NUM_DIALSTICK > 1){
    attachInterrupt(digitalPinToInterrupt(pinRotationA[1]), rotate1, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinRotationB[1]), rotate1, CHANGE);
  
    attachInterrupt(digitalPinToInterrupt(pinPositionA[1]), encode1, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinPositionB[1]), encode1, CHANGE);
  }
  
  if(NUM_DIALSTICK > 2){
    attachInterrupt(digitalPinToInterrupt(pinRotationA[2]), rotate2, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinRotationB[2]), rotate2, CHANGE);
  
    attachInterrupt(digitalPinToInterrupt(pinPositionA[2]), encode2, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinPositionB[2]), encode2, CHANGE);
  }
  
  if(NUM_DIALSTICK > 3){
    attachInterrupt(digitalPinToInterrupt(pinRotationA[3]), rotate3, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinRotationB[3]), rotate3, CHANGE);
  
    attachInterrupt(digitalPinToInterrupt(pinPositionA[3]), encode3, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinPositionB[3]), encode3, CHANGE);
  }
  
  if(NUM_DIALSTICK > 4){
    attachInterrupt(digitalPinToInterrupt(pinRotationA[4]), rotate4, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinRotationB[4]), rotate4, CHANGE);
  
    attachInterrupt(digitalPinToInterrupt(pinPositionA[4]), encode4, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinPositionB[4]), encode4, CHANGE);
  }
  
  if(NUM_DIALSTICK > 5){
    attachInterrupt(digitalPinToInterrupt(pinRotationA[5]), rotate5, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinRotationB[5]), rotate5, CHANGE);
  
    attachInterrupt(digitalPinToInterrupt(pinPositionA[5]), encode5, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinPositionB[5]), encode5, CHANGE);
  }
}


void detachAllInterrupts(){
  
  // INTERUPTION ATTACHMENT
  if(NUM_DIALSTICK > 0){
    detachInterrupt(digitalPinToInterrupt(pinRotationA[0])); 
    detachInterrupt(digitalPinToInterrupt(pinRotationB[0]));
  
    detachInterrupt(digitalPinToInterrupt(pinPositionA[0])); 
    detachInterrupt(digitalPinToInterrupt(pinPositionB[0]));
  }
  
  if(NUM_DIALSTICK > 1){
    detachInterrupt(digitalPinToInterrupt(pinRotationA[1])); 
    detachInterrupt(digitalPinToInterrupt(pinRotationB[1]));
  
    detachInterrupt(digitalPinToInterrupt(pinPositionA[1])); 
    detachInterrupt(digitalPinToInterrupt(pinPositionB[1]));
  }
  
  if(NUM_DIALSTICK > 2){
    detachInterrupt(digitalPinToInterrupt(pinRotationA[2])); 
    detachInterrupt(digitalPinToInterrupt(pinRotationB[2]));
  
    detachInterrupt(digitalPinToInterrupt(pinPositionA[2])); 
    detachInterrupt(digitalPinToInterrupt(pinPositionB[2]));
  }
  
  if(NUM_DIALSTICK > 3){
    detachInterrupt(digitalPinToInterrupt(pinRotationA[3])); 
    detachInterrupt(digitalPinToInterrupt(pinRotationB[3]));
  
    detachInterrupt(digitalPinToInterrupt(pinPositionA[3])); 
    detachInterrupt(digitalPinToInterrupt(pinPositionB[3]));
  }
  
  if(NUM_DIALSTICK > 4){
    detachInterrupt(digitalPinToInterrupt(pinRotationA[4])); 
    detachInterrupt(digitalPinToInterrupt(pinRotationB[4]));
  
    detachInterrupt(digitalPinToInterrupt(pinPositionA[4])); 
    detachInterrupt(digitalPinToInterrupt(pinPositionB[4]));
  }
  
  if(NUM_DIALSTICK > 5){
    detachInterrupt(digitalPinToInterrupt(pinRotationA[5])); 
    detachInterrupt(digitalPinToInterrupt(pinRotationB[5]));
  
    detachInterrupt(digitalPinToInterrupt(pinPositionA[5])); 
    detachInterrupt(digitalPinToInterrupt(pinPositionB[5]));
  }
}

void setup() {

  for (int i = 0; i < NUM_DIALSTICK; i++){
    // JOYSTICK SETUP 
    pinMode(pinAxisX[i], INPUT);
    pinMode(pinAxisY[i], INPUT);
    pinMode(pinButton[i], INPUT);
    pinMode(pinButton[i], INPUT_PULLUP);
  
    // DIAL SETUP
    pinMode(pinRotationA[i], INPUT);
    pinMode(pinRotationA[i], INPUT_PULLUP);
    pinMode(pinRotationB[i], INPUT);
    pinMode(pinRotationB[i], INPUT_PULLUP);
    
    // ENCODER SETUP
    pinMode(pinPositionA[i], INPUT);
    pinMode(pinPositionA[i], INPUT_PULLUP);
    pinMode(pinPositionB[i], INPUT);
    pinMode(pinPositionB[i], INPUT_PULLUP);
  
    // ENDSTOP SETUP
    pinMode(pinSwitch[i], INPUT);
    pinMode(pinSwitch[i], INPUT_PULLUP);
  }
  
  // INTERUPTION ATTACHMENT
  attachAllInterrupts();

  for (int i = 0; i < NUM_DIALSTICK; i++){
    moveTo(i, RESET_POSITION, MAX_DURATION, false);
  }

  // I2C SETUP
  Wire.begin(I2C_BUS);                // join i2c bus with address #8
  Wire.setClock(I2C_BAUDRATE);       
  Wire.onReceive(receiveEvent); // register event
  Wire.onRequest(requestEvent); // register event
  
  
  // SERIAL SETUP
  Serial.begin(9600);
  
}

void loop() {
  for(int i = 0; i < NUM_DIALSTICK; i++){
    // ACQUIRE
    xAxisValue[i] = map(analogRead(pinAxisX[i]), MIN_VAL_FROM, MAX_VAL_FROM,  MIN_VAL_TO, MAX_VAL_TO);
    yAxisValue[i] = map(analogRead(pinAxisY[i]), MIN_VAL_FROM, MAX_VAL_FROM,  MIN_VAL_TO, MAX_VAL_TO);
    
    if (millis() - prevDebounceMillis[i] >=  DEBOUNCE_DELAY_MS) {
      bool nextButtonValue = !digitalRead(pinButton[i]);
      if( buttonValue[i] == 0 && nextButtonValue == 1) selectCount[i]++;
      buttonValue[i] = nextButtonValue;
      prevDebounceMillis[i] = millis();
    }
    
    switchValue[i] = !digitalRead(pinSwitch[i]);
    
    // IF MOTOR IS ACTIVE
    if(servo[i].attached()){
      // ASSESS DISTANCE AND DIRECTION
      signed char distance = encoderTargetValue[i] - encoderValue[i];
      servoDirection[i] = (distance == 0)? DIR_NONE : ( (distance > 0) ? DIR_CCW : DIR_CW);
      servoDistance[i] = abs(distance);
      
      // IF MOVING CLOCKWISED AND ENDSTOP IS REACHED
      if(servoDirection[i] == DIR_CW && switchValue[i]){
        encoderValue[i] = encoderTargetValue[i] = 0; // RESET POSITION VALUE
        servoIsReaching[i] = false;
        servoPulse[i] = IDLE_PULSE; 
        servo[i].writeMicroseconds(servoPulse[i]);  // IDLE MOTOR
        prevPeriodMillis[i] = millis(); // AND WAIT UNTIL NEXT PERIOD TO CONFIRM THAT WE REACHED THE TARGET POSITION
      }
      
      // ELSE IF TARGET IS REACHED AND MOTOR IS NOT IDLE
      else if (servoDirection[i] == DIR_NONE && servoPulse[i] != IDLE_PULSE){
        servoIsReaching[i] = false;
        servoPulse[i] = IDLE_PULSE; 
        servo[i].writeMicroseconds(servoPulse[i]);  // IDLE MOTOR
        prevPeriodMillis[i] = millis(); // AND WAIT UNTIL NEXT PERIOD TO CONFIRM THAT WE CORRECTLY REACHED THE TARGET POSITION
      }
      
      // ELSE IF TARGET IS NOT REACHED OR NEED TO BE CONFIRMED
      else {
        // IF PERIOD IS UP
        if ( millis() - prevPeriodMillis[i] >=  PERIOD_DELAY_MS) {
          // IF TARGET POSITION IS REACHED, MOTOR IS IDLE AND HOLDING MODE IS DISABLED
          if(servoDirection[i] == DIR_NONE && servoPulse[i] == IDLE_PULSE && !servoIsHolding[i]) {
            servoIsReaching[i] = false;
            servo[i].detach(); // UNPLUG MOTOR
          }
          // ELSE REACH TARGET
          else {
            servoIsReaching[i] = true;
            float avgSpeed = min(servoDistance[i]/(float)duration[i], MAX_SPEED);
            float avgSpeedRate =  avgSpeed/MAX_SPEED; 
            unsigned short velocityPulse = (unsigned short) (avgSpeed/(float)MAX_SPEED  * (float)RANGE_PULSE);
            servoPulse[i] = (servoDirection[i] == DIR_CCW) ? IDLE_PULSE + velocityPulse : IDLE_PULSE - velocityPulse;
            duration[i] = max(PERIOD_DELAY_S, duration[i] - PERIOD_DELAY_S);
            /*servoPulse[i] = (servoDirection[i] == DIR_NONE) ? IDLE_PULSE : ((servoDirection[i] == DIR_CCW) ? MAX_CCW_PULSE : MAX_CW_PULSE);
            duration[i] = max(PERIOD_DELAY_S, duration[i] - PERIOD_DELAY_S);*/
            servo[i].writeMicroseconds(servoPulse[i]);
           }
          prevPeriodMillis[i] =  millis();
        }
      }
    }
  }
  
    

    // READ COMMAND FROM SERIAL
    if(Serial.available() > 0)
    {
        String str = Serial.readStringUntil('\n');
        String readCmd = "read";
        int readIndex = str.indexOf(readCmd);
        if(readIndex != -1){
          
          for(int i = 0; i < NUM_DIALSTICK; i++){
            //Serial.print(" id: ");
            Serial.print(i);
            
            Serial.print(" ");
            // Serial.print(" x: ");
            Serial.print(xAxisValue[i]);
            Serial.print(" ");
            //Serial.print(" y: ");
            Serial.print(yAxisValue[i]);
            Serial.print(" ");
            //Serial.print(" sel: ");
            Serial.print(selectCount[i]);
            Serial.print(" ");
            //Serial.print(" rot: ");
            Serial.print(dialValue[i]);
            Serial.print(" ");
            //Serial.print(" pos: ");
            Serial.print(encoderValue[i]);
            Serial.print(" ");
            //Serial.print(" end: ");
            Serial.print(switchValue[i]);
            Serial.print(" ");
            //Serial.print(" reaching: ");
            Serial.print(servoIsReaching[i]);
            Serial.print(" ");
            //Serial.print(" holding: ");
            Serial.print(servoIsHolding[i]);
            Serial.print(" ");
            Serial.println();
          }
          Serial.println();
        }
        
        String displayOnCmd = "displayOn";
        int displayOnIndex = str.indexOf(displayOnCmd);
        if(displayOnIndex != -1){
          displayMode = true;
        }
        
        String displayOffCmd = "displayOff";
        int displayOffIndex= str.indexOf(displayOffCmd);
        if(displayOffIndex != -1){
          displayMode = false;
        }
        
        String writeCmd = "write ";
        int writeIndex = str.indexOf(writeCmd);
        if(writeIndex != -1){
          str = str.substring(writeIndex + writeCmd.length());
          int targetId;
          int targetPosition;
          float targetDuration;
          bool targetIsHolding;
          int endTargetIdIndex = str.indexOf(" ");
          if(endTargetIdIndex != -1){
            targetId = str.substring(0, endTargetIdIndex).toInt();
            str = str.substring(endTargetIdIndex+1);
            int endTargetPositionIndex = str.indexOf(" ");
            if(endTargetPositionIndex != -1){
              targetPosition = str.substring(0, endTargetPositionIndex).toInt();
              str = str.substring(endTargetPositionIndex+1);
              int endtargetDurationIndex = str.indexOf(" ");
              if(endtargetDurationIndex != -1){
                Serial.println(str);
                targetDuration = str.substring(0, endtargetDurationIndex).toFloat();
                Serial.println(targetDuration);
                targetIsHolding = str.substring(endtargetDurationIndex).toInt();
                Serial.println(targetIsHolding);
                Serial.print("MoveTo(");
                Serial.print(targetId);
                Serial.print(", ");
                Serial.print(targetPosition);
                Serial.print(", ");
                Serial.print(targetDuration);
                Serial.print(", ");
                Serial.print(targetIsHolding);
                Serial.println(")");
                moveTo(targetId, targetPosition, targetDuration, targetIsHolding);
              }
            }
          }
        }
     }

    
 
 currentDisplayMillis  = millis();
  if (displayMode && (currentDisplayMillis - prevDisplayMillis >=  DISPLAY_DELAY))
  {
    prevDisplayMillis = currentDisplayMillis;
    for(int i = 0; i < NUM_DIALSTICK; i++){
      //Serial.print(" id: ");
      Serial.print(i);
      
      Serial.print(" ");
      // Serial.print(" x: ");
      Serial.print(xAxisValue[i]);
      Serial.print(" ");
      //Serial.print(" y: ");
      Serial.print(yAxisValue[i]);
      Serial.print(" ");
      //Serial.print(" sel: ");
      Serial.print(selectCount[i]);
      Serial.print(" ");
      //Serial.print(" rot: ");
      Serial.print(dialValue[i]);
      Serial.print(" ");
      //Serial.print(" pos: ");
      Serial.print(encoderValue[i]);
      Serial.print(" ");
      //Serial.print(" end: ");
      Serial.print(switchValue[i]);
      Serial.print(" ");
      //Serial.print(" reaching: ");
      Serial.print(servoIsReaching[i]);
      Serial.print(" ");
      //Serial.print(" holding: ");
      Serial.print(servoIsHolding[i]);
      Serial.print(" ");
      Serial.println();
    }
    Serial.println();
  }
  
}

/*bool reset(int id){
  if(id < 0 || id > NUM_DIALSTICK) return false;
  encoderTargetValue[id] = RESET_POSITION;  
  duration[id] = MAX_DURATION;
  servoIsHolding[id] = false;
  servo[id].attach(pinServo[id]);
  return true;
}*/

signed char moveTo(int id, int toEncoderTargetValue, float durationTime, bool holding){
  
  if(toEncoderTargetValue == 0 && durationTime == 0 && holding == 0) return NONE_ANS;
  if(durationTime < PERIOD_DELAY_S) return DURATION_ERROR_ANS;
  if(id < 0 || id > NUM_DIALSTICK) return ID_ERROR_ANS;
  if(encoderTargetValue[id] == RESET_POSITION) return RESET_ERROR_ANS;
  if(toEncoderTargetValue < RESET_POSITION || toEncoderTargetValue > MAX_POSITION) return POSITION_ERROR_ANS;
  encoderTargetValue[id] = toEncoderTargetValue;
  //if(encoderTargetValue[id] == MIN_POSITION) encoderTargetValue[id] = RESET_POSITION;
  duration[id] = durationTime;
  servoIsHolding[id] = holding;
  servo[id].attach(pinServo[id]);
  return SUCCESS_ANS;
}

bool setHold(int id, bool holding){
  if(id < 0 || id > NUM_DIALSTICK) return false;
  servoIsHolding[id] = holding;
  return true;
}

// function that executes whenever data is received from master
// this function is registered as an event, see setup()
void receiveEvent(int howMany) {
   detachAllInterrupts();
   String str;
   if(Wire.available()){
      char cmd = Wire.read();
      if(cmd == GET_CMD){
        //Serial.println("GET_CMD RECEIVED");
        requestedCmd = GET_CMD;  
     }
     else if (cmd == SET_CMD && Wire.available() >= 31){
        //Serial.println("SET_CMD RECEIVED");
        requestedCmd = SET_CMD;  
        // Read target position
        signed char positionCmd [MAX_DIALSTICK];
        for(int i = 0; i < MAX_DIALSTICK; i ++){
          positionCmd[i] = Wire.read();
          //Serial.print(positionCmd[i]);
          //Serial.print(" ");
        };
         //Serial.println();
        
        // Read duration
        float durationCmd [MAX_DIALSTICK];
        for(int i = 0; i < MAX_DIALSTICK; i ++){
          unsigned char floatAsBytes[4];
          floatAsBytes[0] = Wire.read();
          floatAsBytes[1] = Wire.read();
          floatAsBytes[2] = Wire.read();
          floatAsBytes[3] = Wire.read();
          durationCmd[i] = (*(float*)floatAsBytes);
          //Serial.print(durationCmd[i]);
          //Serial.print(" ");
        };
         //Serial.println();
        
        // Read holding mode
        byte holdByte = Wire.read();
        bool holdCmd [8];
        for (int i = 0; i < 8; i++)  holdCmd[i] = (holdByte & (1 << (7-i))) > 0;
        
        // Run move
        for(int i = 0; i < MAX_DIALSTICK; i ++){
          requestAns [i] = moveTo(i, positionCmd[i], durationCmd[i], holdCmd[i]);
        }
     }else {
        //Serial.println("UNDEFINED CMD RECEIVED");
        requestedCmd = NO_CMD;
     }
     while(Wire.available())Wire.read();
   }
  attachAllInterrupts();
}

void requestEvent(){
  //Serial.print("requestedCmd ");
  //Serial.println(requestedCmd);
  
  detachAllInterrupts();
  
  if(requestedCmd == GET_CMD){
    //Serial.println(" GET CMD ANSWER SENDED");
    byte isReachingValue = 0;
    byte isHoldingValue = 0;
    for(int i = 0; i < MAX_DIALSTICK; i++){
      if(servoIsReaching[i]) isReachingValue |= (byte)(1 << (7 - i));
      if(servoIsHolding[i]) isHoldingValue |= (byte)(1 << (7 - i));
    }    
    Wire.write((byte*)xAxisValue, MAX_DIALSTICK); // 6 bytes
    Wire.write((byte*)yAxisValue, MAX_DIALSTICK); // 6 bytes
    Wire.write((byte*)selectCount, MAX_DIALSTICK); // 6 bytes
    Wire.write((byte*)dialValue, MAX_DIALSTICK); // 6 bytes
    Wire.write((byte*)encoderValue, MAX_DIALSTICK); // 6 bytes
    Wire.write(isReachingValue); // 1 byte
    Wire.write(isHoldingValue); // 1 byte
  }
  else if(requestedCmd == SET_CMD){
    //Serial.println(" GET CMD ANSWER SENDED");
    Wire.write((byte*)requestAns, MAX_DIALSTICK); // 6 bytes
  }
  else if(requestedCmd == NO_CMD){
    //Serial.println("CANNOT SEND ANSWSER TO NONE CMD");
  }
  else {
    //Serial.println("CANNOT SEND ANSWSER TO UNDEFINED CMD");
  }
  attachAllInterrupts();
}
