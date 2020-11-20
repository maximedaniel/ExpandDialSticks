
           
            // SPEED MAP
            
            /*const int numLevel = 13;
            const int pulseLevels [numLevel+1] = {0, 46, 69, 92, 115, 138, 161, 184, 207, 230, 253, 276, 299, 322};
            int speedLevelFactor =  MAX_SPEED/(float)numLevel;
            //float avgSpeedRate =  avgSpeed/MAX_SPEED;
            int pulseLevel = pulseLevels[(int)floorf(avgSpeed/(float)speedLevelFactor)];
            
            //int velocityPulse = (int) (avgSpeedRate  * (float)RANGE_PULSE);
            servoPulse = (servoDirection == DIR_CCW) ? IDLE_PULSE + pulseLevel : IDLE_PULSE - pulseLevel;*/
            /*
#include "Dialstick.h"
#include <Wire.h>

#define I2C_BUS 8
#define NUM_DIALSTICK 1
#define MAX_DIALSTICK 6
#define STATE_SIZE 7


const float RESOLUTION = 0.1;
unsigned long DELAY = 500;

unsigned long prevCallMillis = 0; 
unsigned long currentCallMillis = 0;
unsigned long prevMoveMillis = 0; 
unsigned long currentMoveMillis = 0;
unsigned long MOVE_DELAY = 10000;

// PIN ARRAYS
const int pinAxisXs [MAX_DIALSTICK] = {A11, A10, A9, A8, A7, A6};
const int pinAxisYs [MAX_DIALSTICK] = {A5, A4, A3, A2, A1, A0};
const int pinButtons [MAX_DIALSTICK] = {2, 3, 4, 5, 6, 7};

const int pinRotationAs [MAX_DIALSTICK] = {53, 51, 49, 47, 45, 43};
const int pinRotationBs [MAX_DIALSTICK] = {52, 50, 48, 46, 44, 42};

const int pinPositionAs [MAX_DIALSTICK] = {41, 39, 37, 35, 33, 31};
const int pinPositionBs [MAX_DIALSTICK] = {40, 38, 36, 34, 32, 30};

const int pinServos [MAX_DIALSTICK] = {8, 9, 10, 11, 12, 13};

const int pinSwitchs [MAX_DIALSTICK] = {22, 24, 26, 27, 25, 23};



// DIALSTICK ARRAY
Rotary* dials;
Rotary* encoders;
Servo* motors;
Dialstick* dialsticks;
    
// STATE ARRAYS
volatile int** states;

// INTERRUPTION TIMER

volatile unsigned long prevInterruptMillis; 
volatile unsigned long currentInterruptMillis;


void setup() {
  Wire.begin(I2C_BUS);
  // TWAR = (adress << 1) | 1;  // enable listening on broadcast messages
  Wire.onRequest(requestEvent); // register event
  Serial.begin(9600); 

  // MEMORY ALLOCATION
  dials = (Rotary*)malloc(NUM_DIALSTICK * sizeof(Rotary));
  encoders = (Rotary*)malloc(NUM_DIALSTICK * sizeof(Rotary));
  motors = (Servo*)malloc(NUM_DIALSTICK * sizeof(Servo));
  dialsticks = (Dialstick*)malloc(NUM_DIALSTICK * sizeof(Dialstick));
  states = (volatile int**)malloc(NUM_DIALSTICK * sizeof(volatile int *));
    for(int i = 0; i < NUM_DIALSTICK; i++) 
      states[i] = (volatile int*)malloc(STATE_SIZE * sizeof(volatile int));

 // INSTANTIATE DIALSTICK OBJECTS
  for(int i = 0; i < NUM_DIALSTICK; i++){
    dials[i] = Rotary(pinRotationAs[i], pinRotationBs[i]);
    encoders[i] = Rotary(pinPositionAs[i], pinPositionBs[i]);
    motors[i] = Servo();
    dialsticks[i] = Dialstick(i, pinAxisXs[i], pinAxisYs[i], pinButtons[i], pinRotationAs[i], pinRotationBs[i], pinServos[i], pinPositionAs[i], pinPositionBs[i], pinSwitchs[i], RESOLUTION, dials[i], encoders[i], motors[i]); 
    dialsticks[i].init();
  }
  
  // DIALSTICK INTERRUPTION ATTACHMENT
  if(NUM_DIALSTICK < 2){
    attachInterrupt(digitalPinToInterrupt(pinRotationAs[0]), rotate0, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinRotationBs[0]), rotate0, CHANGE);
    attachInterrupt(digitalPinToInterrupt(pinPositionAs[0]), encode0, CHANGE); 
    attachInterrupt(digitalPinToInterrupt(pinPositionBs[0]), encode0, CHANGE);
  }
  else if(NUM_DIALSTICK < 3){
  attachInterrupt(digitalPinToInterrupt(pinRotationAs[1]), rotate1, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinRotationBs[1]), rotate1, CHANGE);
  attachInterrupt(digitalPinToInterrupt(pinPositionAs[1]), encode1, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinPositionBs[1]), encode1, CHANGE);
  }
  else if(NUM_DIALSTICK < 4){
  attachInterrupt(digitalPinToInterrupt(pinRotationAs[2]), rotate2, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinRotationBs[2]), rotate2, CHANGE);
  attachInterrupt(digitalPinToInterrupt(pinPositionAs[2]), encode2, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinPositionBs[2]), encode2, CHANGE);
  }
  else if(NUM_DIALSTICK < 5){
  attachInterrupt(digitalPinToInterrupt(pinRotationAs[3]), rotate3, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinRotationBs[3]), rotate3, CHANGE);
  attachInterrupt(digitalPinToInterrupt(pinPositionAs[3]), encode3, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinPositionBs[3]), encode3, CHANGE);
  }
  else if(NUM_DIALSTICK < 6){
  attachInterrupt(digitalPinToInterrupt(pinRotationAs[4]), rotate4, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinRotationBs[4]), rotate4, CHANGE);
  attachInterrupt(digitalPinToInterrupt(pinPositionAs[4]), encode4, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinPositionBs[4]), encode4, CHANGE);
  }
  else if(NUM_DIALSTICK < 7){
  attachInterrupt(digitalPinToInterrupt(pinRotationAs[5]), rotate5, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinRotationBs[5]), rotate5, CHANGE);
  attachInterrupt(digitalPinToInterrupt(pinPositionAs[5]), encode5, CHANGE); 
  attachInterrupt(digitalPinToInterrupt(pinPositionBs[5]), encode5, CHANGE);
  }
  
 // RESET DIALSTICK POSITION
  for(int i = 0; i < NUM_DIALSTICK; i++){
    dialsticks[i].reset();
  }
}

void loop() {
  
  for(int i = 0; i < NUM_DIALSTICK; i++) dialsticks[i].process();
  
  currentCallMillis = currentMoveMillis = millis();
  if (currentCallMillis - prevCallMillis >=  DELAY)
  {   
     prevCallMillis = currentCallMillis;
     displayStates();
  }
}

void displayStates(){
  for(int i = 0; i < NUM_DIALSTICK; i++){
     dialsticks[i].getState(states[i], STATE_SIZE);
     Serial.print("DIALSTICK");
     Serial.print(i);
     Serial.print(" ");
     for (int j = 0; j < STATE_SIZE; j++){
      Serial.print(states[i][j]);
      Serial.print(" ");
     }
     Serial.println();
  }
  
}
void requestEvent() {
  for(int i = 0; i < NUM_DIALSTICK; i++){
     dialsticks[i].getState(states[i], STATE_SIZE);
     Wire.write((byte*)states[i], sizeof(STATE_SIZE * sizeof *states[i]));
  }
}

void rotate0(){
  dialsticks[0].rotate();
}

void encode0(){
  dialsticks[0].encode();
}

void rotate1(){
  dialsticks[1].rotate();
}

void encode1(){
  dialsticks[1].encode();
}

void rotate2(){
  dialsticks[2].rotate();
}

void encode2(){
  dialsticks[2].encode();
}

void rotate3(){
  dialsticks[3].rotate();
}

void encode3(){
  dialsticks[3].encode();
}

void rotate4(){
  dialsticks[4].rotate();
}

void encode4(){
  dialsticks[4].encode();
}

void rotate5(){
  dialsticks[5].rotate();
}

void encode5(){
  dialsticks[5].encode();
}*/
