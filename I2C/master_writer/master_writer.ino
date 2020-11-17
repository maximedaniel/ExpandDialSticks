// Wire Master Writer
// by Nicholas Zambetti <http://www.zambetti.com>

// Demonstrates use of the Wire library
// Writes data to an I2C/TWI slave device
// Refer to the "Wire Slave Receiver" example for use with this

// Created 29 March 2006

// This example code is in the public domain.


#include <Wire.h>

#define I2C_BUS 4

#define NO_CMD 0x0
#define GET_CMD 0x01
#define SET_CMD 0x02

#define NONE_ANS -1
#define SUCCESS_ANS 0 
#define DURATION_ERROR_ANS 1
#define ID_ERROR_ANS 2
#define RESET_ERROR_ANS 3
#define POSITION_ERROR_ANS 4

#define NUM_DIALSTICK 1
#define MAX_DIALSTICK 6
#define STATE_SIZE 9

#define SUCCESS_ANS 0
#define BUFFER_LENGTH_ERR_ANS 1
#define NACK_ADDR_ERR_ANS 2
#define NACK_DATA_ERR_ANS 3
#define OTHER_ERR_ANS 4


int requestStatus;

void setup()
{
  Wire.begin(); // join i2c bus (address optional for master)
  Serial.begin(9600);
}


void loop()
{
  Serial.println("SEND GET_CMD");
  // READ COMMAND FROM SERIAL
  Wire.beginTransmission(I2C_BUS);
  Wire.write(GET_CMD);
  requestStatus = Wire.endTransmission(false);  // Condition RESTART
  Serial.print("requestStatus ");
  Serial.println(requestStatus);
  if(requestStatus == SUCCESS_ANS){
    Wire.requestFrom(I2C_BUS, 32, true); // Condition STOP
    if (Wire.available() >= 32) {
      
      Serial.print("X: ");
      // Read X axis
      signed char xAxisValue [MAX_DIALSTICK];
      for(int i = 0; i < MAX_DIALSTICK; i ++){
        xAxisValue[i] = Wire.read();
        Serial.print(xAxisValue[i]);
        Serial.print(" ");
      };
        Serial.println();
      
      Serial.print("Y: ");
      // Read Y axis
      signed char yAxisValue [MAX_DIALSTICK];
      for(int i = 0; i < MAX_DIALSTICK; i ++){
        yAxisValue[i] = Wire.read();
        Serial.print(yAxisValue[i]);
        Serial.print(" ");
      };
      Serial.println();
      
      Serial.print("S: ");
      // Read select Count
      unsigned char selectCountValue [MAX_DIALSTICK];
      for(int i = 0; i < MAX_DIALSTICK; i ++){
        selectCountValue[i] = Wire.read();
        Serial.print(selectCountValue[i]);
        Serial.print(" ");
      };
      Serial.println();
      
      Serial.print("R: ");
      // Read dial
      signed char rotationValue [MAX_DIALSTICK];
      for(int i = 0; i < MAX_DIALSTICK; i ++){
        rotationValue[i] = Wire.read();
        Serial.print(rotationValue[i]);
        Serial.print(" ");
      };
      Serial.println();
      
      Serial.print("P: ");
      // Read encoder
      unsigned char positionValue [MAX_DIALSTICK];
      for(int i = 0; i < MAX_DIALSTICK; i ++){
        positionValue[i] = Wire.read();
        Serial.print(positionValue[i]);
        Serial.print(" ");
      };
      Serial.println();
      
      Serial.print("R: ");
      // Read reaching mode
      byte reachingByte = Wire.read();
      bool reachingCmd [8];
      for (int i = 0; i < 8; i++)  {
        reachingCmd[i] = (reachingByte & (1 << (7-i))) > 0;
        Serial.print(reachingCmd[i]);
        Serial.print(" ");
      }
      Serial.println();
      
      Serial.print("H: ");
      // Read holding mode
      byte holdByte = Wire.read();
      bool holdCmd [8];
      for (int i = 0; i < 8; i++)  {
        holdCmd[i] = (holdByte & (1 << (7-i))) > 0;
        Serial.print(holdCmd[i]);
        Serial.print(" ");
      }
      
      Serial.println();
    }
  }
  
  delay(1000);
  Serial.println("SEND SET_CMD 0 40 1 1");
  Wire.beginTransmission(I2C_BUS);
  Wire.write(SET_CMD);
  signed char positionCmd [MAX_DIALSTICK] = {40, 40, 40, 40, 40, 40};  
  Wire.write((byte*)positionCmd, MAX_DIALSTICK);
  float durationCmd [MAX_DIALSTICK] = {1., 1., 1., 1., 1., 1.};
  Wire.write((byte*)durationCmd, MAX_DIALSTICK * sizeof(float));
  byte holdCmd = 0b11111111;
  Wire.write(holdCmd);
  requestStatus = Wire.endTransmission(false);  // Condition RESTART
  if(requestStatus == SUCCESS_ANS){
    Serial.print("requestStatus ");
    Serial.println(requestStatus);
    Wire.requestFrom(I2C_BUS, MAX_DIALSTICK, true); // Condition STOP
    
    if (Wire.available() >= MAX_DIALSTICK) {
      Serial.print("Answer: ");
      // Read answer
      signed char answerValue [MAX_DIALSTICK];
      for(int i = 0; i < MAX_DIALSTICK; i ++){
        answerValue[i] = Wire.read();
        Serial.print(answerValue[i]);
        Serial.print(" ");
      };
      Serial.println();
    }
  }
  delay(5000);
}
