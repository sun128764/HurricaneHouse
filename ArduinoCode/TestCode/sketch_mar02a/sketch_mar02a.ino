// parameters
const unsigned int NetworkID = 5001; //
const unsigned int BoardID = 1; //
const unsigned int BoardType = 1; // 1.Coordinatorn (cellular/gps/main), 2. Anemometer, 3. Humidity., 4. regular

const unsigned int Fs = 50; // sample reading per second (per sensor)
const unsigned int nSensors = 5;
const unsigned int nPaylad = 40; // for each transaction (per sensor)

int loop_time = 1000 / Fs; // in milli-second

// define XBee
#include <Arduino.h>   // required before wiring_private.h
#include "wiring_private.h" // pinPeripheral() function
#include <XBee.h>
//#define MAX_FRAME_DATA_SIZE 104 // bytes
unsigned int XBeeRX = 3; // D3
unsigned int XBeeTX = 4; // D4
//    D6: Clear to send
//    D5: RX signal ignal strength of last transmission
//    D7: energy control
Uart Serial2 (&sercom2, XBeeRX, XBeeTX, SERCOM_RX_PAD_1, UART_TX_PAD_0);
void SERCOM2_Handler() {
  Serial2.IrqHandler();
}
XBee xbee = XBee();

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

void ForwardData() {
  if (Serial2.available()) {
    while (Serial2.available()) {
      Serial.write( Serial2.read());
    }

  }
}

void SendSample() {
  Serial2.print(NetworkID);
  Serial2.print(",");
  Serial2.print(BoardID);
  Serial2.print(",");
  Serial2.print(BoardType);
  Serial2.print(",");
  Serial2.print(analogRead(A0)); //Temperature
  Serial2.print(",");
  Serial2.print(analogRead(A1)); //Battery
  Serial2.print(",");
  Serial2.print(pressure);
  Serial2.print(",");
  Serial2.print(analogRead(A3)); //Extention A3
  Serial2.print(",");
  Serial2.println(analogRead(A4)); // Expansion A4
  number = 0;
  LastMillis = CurrentMillis;
  lock = true;
}

void setup() {
  Serial.begin(9600);
  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A3, INPUT);
  pinMode(A4, INPUT);
  pinMode(A5, INPUT);
  pinMode(green, OUTPUT);
  pinMode(blue, OUTPUT);
  // Default ADC's resolution of the Atmel's ATSAM21 is 10bit (reading range from 0 to 1023).
  analogReadResolution(16);

  Serial2.begin(9600);
  pinPeripheral(XBeeRX, PIO_SERCOM_ALT);
  pinPeripheral(XBeeTX, PIO_SERCOM_ALT);
  xbee.setSerial(Serial2);
  delay(1100);
  Serial2.print("+++"); // start attention (AT) commands
  delay(1100);
  Serial2.printf("ATID %d\r", NetworkID ); // network PAN id
  delay(100);
  Serial2.printf("ATNI %d\r", BoardID ); // node id
  delay(100);
  if (BoardType == 1) {
    Serial2.print("ATCE 1\r"); // set cooridnator device
  } else {
    Serial2.print("ATCE 0\r"); // set router/endpoint device
  }
  delay(100);
  // DOTO: add encryption and check max payload available
  Serial2.print("ATEE 1\r" ); // enable encryption (secure communication)
  delay(100);
  Serial2.print("ATKY AAAAABBBBBCCCCCDDDDDEEEEEFFFFF12\r" ); // set encryption key 32 Hex digits
  delay(100);
  Serial2.print("ATNP\r" ); // read the max payload of a packet. (determined from encryption and type of coommunication)
  delay(100);
  while (Serial2.available()) {
    Serial.write( Serial2.read() ); // display response
  }
  number = 0;
  pressure = 0;
  LastMillis = millis();
  lock = true;
}

void loop() { // while true

  if (BoardType == 1) {
    ForwardData();
    delay(10);
  }
  else {
    CurrentMillis = millis();
    if ((CurrentMillis - LastMillis) >= 500 && !lock) {
      SendSample();
      Serial.print("Send");
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
  }
  // XCTU https://www.digi.com/products/embedded-systems/digi-xbee/digi-xbee-tools/xctu#productsupport-utilities
}
