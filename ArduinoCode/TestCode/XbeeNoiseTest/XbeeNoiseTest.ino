
// define XBee
#include <Arduino.h>   // required before wiring_private.h
#include "wiring_private.h" // pinPeripheral() function

// define buffers
#include <stdio.h>
#include <string.h>
#include <inttypes.h>

unsigned int green = 25; unsigned int blue = 26; // Green and blue LEDs' pins
void led(unsigned int color, unsigned int n) {
  for (unsigned int i = 0; i < n; i++) {
    digitalWrite(color, LOW);
    delay(1);
    digitalWrite(color, HIGH);
  }
}

long reading;
int pressure;
int number;
bool lock;
unsigned long LastMillis;
unsigned long CurrentMillis;


void SendSample() {
  Serial.print("1000");
  Serial.print(",");
  Serial.print("1");
  Serial.print(",");
  Serial.print("4");
  Serial.print(",");
  Serial.print(analogRead(A0)); //Temperature
  Serial.print(",");
  Serial.print(analogRead(A1)); //Battery
  Serial.print(",");
  Serial.print(pressure);
  Serial.print(",");
  Serial.print(analogRead(A3)); //Extention A3
  Serial.print(",");
  Serial.println(analogRead(A4)); // Expansion A4
  number = 0;
  LastMillis = CurrentMillis;
  lock = true;
}

void setup() {
  Serial.begin(9600);
  Serial.print("start");
  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A3, INPUT);
  pinMode(A4, INPUT);
  pinMode(A5, INPUT);
  pinMode(green, OUTPUT);
  pinMode(blue, OUTPUT);
  // Default ADC's resolution of the Atmel's ATSAM21 is 10bit (reading range from 0 to 1023).
  analogReadResolution(16);
  //analogReference(AR_EXTERNAL);

  number = 0;
  pressure = 0;
  LastMillis = millis();
  lock = true;
}

void loop() { // while true
  CurrentMillis = millis();
  if ((CurrentMillis - LastMillis) >= 500 && !lock) {
    SendSample();
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

  // XCTU https://www.digi.com/products/embedded-systems/digi-xbee/digi-xbee-tools/xctu#productsupport-utilities
}
