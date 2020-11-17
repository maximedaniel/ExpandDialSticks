/*
  Morse.cpp - Library for flashing Morse code.
  Created by David A. Mellis, November 2, 2007.
  Released into the public domain.
*/

#include "Arduino.h"
#include "Rotary.h"
#include "Dialstick.h"

#define MIN_VAL_FROM 0
#define MAX_VAL_FROM 1023
#define MIN_VAL_TO -100
#define MAX_VAL_TO 100
#define DEBUG true

#define STATE_SIZE 7
// Values returned by 'process'
// No complete step yet.
#define DIR_NONE 0x0
// Clockwise step.
#define DIR_CW 0x10
// Counter-clockwise step.
#define DIR_CCW 0x20



const int MAX_CW_PULSE = 700;
const int MIN_CW_PULSE = 1450;
const int IDLE_PULSE = 1500;
const int MIN_CCW_PULSE = 1550;
const int MAX_CCW_PULSE = 2300;
const int RANGE_PULSE = 750;//750;
const int DEAD_BAND_PULSE = 90;

const int MIN_SPEED = 1; // unit/s
const int MAX_SPEED = 40; // unit/s

const int MIN_POSITION = 0;
const int MAX_POSITION = 50;

const int RESET_DURATION = 10; //s


Dialstick::Dialstick()
{
  _id = -1;
  // PIN
  _pinAxisX = 0;
  _pinAxisY = 0;
  _pinButton = 0;
  _pinRotationA = 0;
  _pinRotationB = 0;
  _pinServo = 0;
  _pinPositionA = 0;
  _pinPositionB = 0;
  _pinSwitch = 0;

  // VARIABLE
  _resolution = 0;
  _animated = 0;
  _index = 0;
  _direction = 0;
  _velocity = 0;
  _servoPulse = IDLE_PULSE;
  _velocityArray = 0;
  _velocityArraySize = 0;
  _prevMillis = 0; 
  _currentMillis = 0;
  _dial = NULL;
  _encoder = NULL;
  _motor = NULL;
  
  // STATE
  _xAxis = 0;
  _yAxis = 0;
  _button = 0;
  _rotation = 0;
  _position = MAX_POSITION;
  _switch = 0;
}

Dialstick::Dialstick(int id, int pinAxisX, int pinAxisY, int pinButton, int pinRotationA, int pinRotationB, int pinServo, int pinPositionA, int pinPositionB, int pinSwitch, float resolution, Rotary dial, Rotary encoder, Servo motor)
{
  _id = id;
  // PIN
  _pinAxisX = pinAxisX;
  _pinAxisY = pinAxisY;
  _pinButton = pinButton;
  _pinRotationA = pinRotationA;
  _pinRotationB = pinRotationB;
  _pinServo = pinServo;
  _pinPositionA = pinPositionA;
  _pinPositionB = pinPositionB;
  _pinSwitch = pinSwitch;

  // VARIABLE
  _resolution = resolution;
  _animated = 0;
  _index = 0;
  _direction = 0;
  _velocity = 0;
  _servoPulse = IDLE_PULSE;
  _velocityArray = 0;
  _velocityArraySize = 0;
  _prevMillis = 0; 
  _currentMillis = 0;
  _dial = &dial;
  _encoder = &encoder;
  _motor = &motor;
  
  // STATE
  _xAxis = 0;
  _yAxis = 0;
  _button = 0;
  _rotation = 0;
  _position = MAX_POSITION;
  _switch = 0;
  
}

void Dialstick::init()
{
  // JOYSTICK
  pinMode(_pinAxisX, INPUT);
  pinMode(_pinAxisY, INPUT);
  pinMode(_pinButton, INPUT);
  pinMode(_pinButton, INPUT_PULLUP);
  
  // DIAL
  pinMode(_pinRotationA, INPUT);
  pinMode(_pinRotationA, INPUT_PULLUP);
  pinMode(_pinRotationB, INPUT);
  pinMode(_pinRotationB, INPUT_PULLUP);
  
  
  // SERVOMOTOR
  // pinMode(_pinServo, OUTPUT);
  
  // POSITION
  pinMode(_pinPositionA, INPUT);
  pinMode(_pinPositionA, INPUT_PULLUP);
  pinMode(_pinPositionB, INPUT);
  pinMode(_pinPositionB, INPUT_PULLUP);
  
  
  // ENDSTOP
  pinMode(_pinSwitch, INPUT);
  pinMode(_pinSwitch, INPUT_PULLUP);
}


void Dialstick::process()
{
  _currentMillis = millis();  //get the current "time" (actually the number of milliseconds since the program started) program started)
  
  if (_currentMillis - _prevMillis >=  _resolution * 1000)  //test whether the period has elapsed
  {   
     _prevMillis = _currentMillis;
     
    // JOYSTICK
    _xAxis = (int)map(analogRead(_pinAxisX), MIN_VAL_FROM, MAX_VAL_FROM,  MIN_VAL_TO, MAX_VAL_TO);
    _yAxis = (int)map(analogRead(_pinAxisY), MIN_VAL_FROM, MAX_VAL_FROM,  MIN_VAL_TO, MAX_VAL_TO);
    _button = !digitalRead(_pinButton);
  
    // SWITCH
    _switch = !digitalRead(_pinSwitch);
    
    // SERVO MOTOR
    if(_animated){
        String flag = "";
        // RESET DONE
        if(_direction == DIR_CW && _switch){
           flag = " RESET DONE";
           _velocity = 0;
           _servoPulse = IDLE_PULSE;
           /*_motor -> writeMicroseconds(_servoPulse); 
           delayMicroseconds(_servoPulse);
           _motor -> detach();*/
           _animated = _position = _targetPosition = 0;
        }
        // TARGET REACHED
        else if(
          (_direction == DIR_CW  && _position <= _targetPosition)  || 
          (_direction == DIR_CCW  && _position >= _targetPosition)
        ){
           flag = " TARGET REACHED";
           _velocity = 0;
           _servoPulse = IDLE_PULSE;
           /*_motor -> writeMicroseconds(_servoPulse); 
           delayMicroseconds(_servoPulse);*/
           _animated = 0;
        }
        // REACHING TARGET POSITION
        else if(_index < _velocityArraySize){
           flag = " REACHING...";
          _velocity = (int) (constrain(_velocityArray[_index]/(float)MAX_SPEED, 0, 1)  * (float)RANGE_PULSE);
          _servoPulse =  (_direction == DIR_CW) ? IDLE_PULSE - _velocity : IDLE_PULSE + _velocity;
          /*_motor -> writeMicroseconds(_servoPulse);
           delayMicroseconds(_servoPulse);*/
          _index++;
      
        } else { // REACHING TARGET POSITION
          flag = " STILL REACHING...";
          _velocity = DEAD_BAND_PULSE;
          _servoPulse = (_direction == DIR_CW) ? IDLE_PULSE - _velocity : IDLE_PULSE + _velocity;
          /*_motor -> writeMicroseconds(_servoPulse);
           delayMicroseconds(_servoPulse);*/
      }
       /*if( DEBUG ){
         noInterrupts();
         Serial.print(" [DIALSTICK") && Serial.print(_id);
         Serial.print("] position: ") && Serial.print(_position);
         Serial.print(" target: ") && Serial.print(_targetPosition);
         Serial.print(" animated: ") && Serial.print(_animated);
         Serial.print(" pulse: ") && Serial.print(_servoPulse);
         Serial.print(" flag: ") && Serial.println(flag);
         Serial.println();
         interrupts();
       }*/
   }
 }
}

bool Dialstick::getState(volatile int* stateArray, int stateSize)
{
  
  if(stateSize != STATE_SIZE){
    /*noInterrupts();
    Serial.print(" [DIALSTICK") && Serial.print(_id) && Serial.print("] WRONG STATE SIZE");
    interrupts();*/
    return false;
  }
  
  stateArray[0] = _xAxis;
  stateArray[1] = _yAxis;
  stateArray[2] = _button;
  stateArray[3] = _rotation;
  stateArray[4] = _position;
  stateArray[5] = _switch;
  stateArray[6] = _animated;
  return true;
}


int* Dialstick::getState()
{
  int* state =  (int*)malloc(STATE_SIZE * sizeof *state);
  if(state) {
    state[0] = _xAxis;
    state[1] = _yAxis;
    state[2] = _button;
    state[3] = _rotation;
    state[4] = _position;
    state[5] = _switch;
    state[6] = _animated;
  }
  return state;
}


bool Dialstick::reset()
{
  if(_animated) return false;
  _targetPosition = 0;
  _direction = DIR_CW;
  _index = 0;
  _velocityArraySize =  0;
  free(_velocityArray);
  //_motor -> attach(_pinServo);
  _animated = 1;
  return true;
  
}

bool Dialstick::moveTo(int targetPosition, float durationTime)
{
  if(_animated) {
    /*noInterrupts();
    Serial.print("[DIALSTICK") && Serial.print(_id) && Serial.println("] ANIMATION ALREADY IN PROGRESS");
    interrupts();*/
    return false;
  }
  if(targetPosition < MIN_POSITION || targetPosition > MAX_POSITION) {
   /* noInterrupts();
    Serial.print("[DIALSTICK") && Serial.print(_id) && Serial.println("] OUTSIDE POSITION BOUNDARIES");
    interrupts();*/
    return false;
  }
  if(_position == targetPosition) {
    /*noInterrupts();
    Serial.print("[DIALSTICK") && Serial.print(_id) && Serial.println("] ALREADY AT TARGET POSITION");
    interrupts();*/
    return false;
  }
  _targetPosition = targetPosition;
  if(_targetPosition < _position) _direction = DIR_CW;
  else _direction = DIR_CCW;
  
  int speedSize = (int)(durationTime/_resolution);
  float speedArray [speedSize];
  trapezoidalMotion(_position, _targetPosition, durationTime, _resolution, speedArray, speedSize); 
  _index = 0;
  _velocityArraySize = speedSize;
  free(_velocityArray);
  _velocityArray = (float*)malloc(_velocityArraySize * sizeof(float));
  for(int i = 0; i < _velocityArraySize; i++) _velocityArray[i] = speedArray[i];
  //_motor -> attach(_pinServo);
  _animated = 1;
  return true;
  
}

void Dialstick::trapezoidalMotion(int startPos, int endPos, float durationTime, float deltaTime, float speedArray [], int speedSize){

  int distance = abs(endPos - startPos);
  float avgSpeed = distance/(float)durationTime;
  float maxSpeed = 1.5 * avgSpeed;
  float phaseDuration = durationTime/3.;
  float accel = maxSpeed / (float)phaseDuration;
  
  int i = 0;
  float t = 0;
  float currSpeed = 0;
  float currPos = 0;

  float distanceDone = 0;
  
  if(avgSpeed <= MIN_SPEED){
    /*noInterrupts();
    Serial.print("[DIALSTICK") && Serial.print(_id) && Serial.println("] AVERAGE SPEED LOWER THAN MINIMUM SPEED");
    interrupts();*/
    for ( i = 0 , t = deltaTime; i < speedSize; i++, t += deltaTime) speedArray[i] = MIN_SPEED;
    speedArray[speedSize - 1] = 0;
  }
  else if(avgSpeed >= MAX_SPEED){
    /*noInterrupts();
    Serial.print("[DIALSTICK") && Serial.print(_id) && Serial.println("] AVERAGE SPEED GREATER THAN MAXIMUM SPEED");
    interrupts();*/
    for ( i = 0 , t = deltaTime; i < speedSize; i++, t += deltaTime) speedArray[i] = MAX_SPEED;
    speedArray[speedSize - 1] = 0;
  }
  else for ( i = 0 , t = deltaTime; i < speedSize; i++, t += deltaTime){
      if(t < phaseDuration){
        currPos += currSpeed * deltaTime;
        currSpeed = roundf(constrain(maxSpeed * (t/phaseDuration), 0, maxSpeed));
        speedArray[i] = currSpeed;
        } 
        else if (t < durationTime - phaseDuration){
        currPos += currSpeed * deltaTime;
        currSpeed = roundf(maxSpeed); 
        speedArray[i] = currSpeed;
       
      } else if (t < durationTime + deltaTime){
        currPos += currSpeed * deltaTime;
        currSpeed =  roundf(constrain(maxSpeed * ((durationTime - t)/phaseDuration), 0, maxSpeed));
        speedArray[i] = currSpeed;
      }
  }
}


volatile void Dialstick::rotate()
{
  unsigned char result = _dial->process();
  if (result == DIR_CW) {
    _rotation++;
  } else if (result == DIR_CCW) {
    _rotation--;
  } 
  Serial.println(_rotation);
}

volatile void Dialstick::encode()
{
  unsigned char result = _encoder->process();
  if (result == DIR_CW) {
    _position++;
  } else if (result == DIR_CCW) {
    _position--;
  }
}
