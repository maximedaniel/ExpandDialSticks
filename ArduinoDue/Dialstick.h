/*
  Morse.h - Library for flashing Morse code.
  Created by David A. Mellis, November 2, 2007.
  Released into the public domain.
*/
#ifndef Dialstick_h
#define Dialstick_h

#include "Arduino.h"
#include "Rotary.h"
#include <Servo.h>

class Dialstick
{
  public: 
    Dialstick();
    Dialstick(int id, int pinAxisX, int pinAxisY, int pinButton, int pinRotationA, int pinRotationB, int pinServo, int pinPositionA, int pinPositionB, int pinSwitch, float resolution, Rotary dial, Rotary encoder, Servo motor);
    void init();
    void process();
    bool getState(volatile int* stateArray, int stateArraySize);
    int* getState();
    bool reset();
    bool moveTo(int targetPosition, float durationTime);
    volatile void encode();
    volatile void rotate();
    
    
  private:
    void trapezoidalMotion(int startPos, int endPos, float durationTime, float deltaTime, float speedArray [], int speedSize);

    int _id;
    // PIN
    int _pinAxisX;
    int _pinAxisY;
    int _pinButton;
    int _pinRotationA;
    int _pinRotationB;
    int _pinServo;
    int _pinPositionA;
    int _pinPositionB;
    int _pinSwitch;
    
    // OBJECT
    Rotary* _dial;
    Rotary* _encoder;
    Servo* _motor;

    // VARIABLE
    float _resolution;
    int _animated;
    int _index;
    int _direction;
    int _velocity;
    int _servoPulse;
    int _targetPosition;
    float* _velocityArray;
    int _velocityArraySize;
    unsigned long _prevMillis; 
    unsigned long _currentMillis;
    
    // STATE 
    int _xAxis;
    int _yAxis;
    int _button;
    volatile int _rotation;
    volatile int _position;
    int _switch;
    
};

#endif
