// define XBee
#include <Arduino.h>   // required before wiring_private.h
#include "wiring_private.h" // pinPeripheral() function
#include <XBee.h>

// define buffers
#include <stdio.h>
#include <string.h>
#include <inttypes.h>

long reading;
int pressure;
int number;
bool lock;
unsigned long LastMillis;
unsigned long CurrentMillis;

void setup() {
  Serial.begin(9600);
  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A3, INPUT);
  pinMode(A4, INPUT);
  pinMode(A5, INPUT);
  // Default ADC's resolution of the Atmel's ATSAM21 is 10bit (reading range from 0 to 1023).
  analogReadResolution(16);
  number = 0;
  pressure = 0;
  LastMillis = millis();
  lock = true;
}


void loop() { // while true
  CurrentMillis = millis();
  if ((CurrentMillis - LastMillis) >= 500 && !lock) {
    Serial.print("T:");
    Serial.print(analogRead(A0));
    Serial.print("B:");
    Serial.print(analogRead(A1));
    Serial.print("P:");
    Serial.print(pressure);
    Serial.print("W:");
    Serial.print(analogRead(A3));
    Serial.print("H:");
    Serial.println(analogRead(A4));
    number = 0;
    LastMillis = CurrentMillis;
    lock = true;
  }
  if (number < 1024 && lock) {
    reading += analogRead(A5);
    number++;
    delay(1);
  }
  if (number >= 512 && lock) {
    lock = false;
    pressure = reading >> 9;
    reading = 0;
    
  }

  // debug
  //for (int j=0;j<a0i+1;j++){ Serial.printf(  "%03d" , a0[j]  ); }
  //Serial.println();

  // XCTU https://www.digi.com/products/embedded-systems/digi-xbee/digi-xbee-tools/xctu#productsupport-utilities

}
