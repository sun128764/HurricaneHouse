// parameters
const unsigned int NetworkID = 5001; //
const unsigned int BoardID = 10; //
const unsigned int BoardType = 4; // 1.Coordinatorn (cellular/gps/main), 2. Anemometer, 3. Humidity., 4. regular

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

unsigned int Pressure[10];
int PressureIndex = 0;
int reading;
int number;
bool lock;
unsigned long LastMillis;
unsigned long StartTime;
unsigned long CurrentMillis;
const unsigned long LMask = 255;
const unsigned int PMask = 255;
byte SerBuf[31];

void ForwardData() {
  if (Serial2.available()) {
    while (Serial2.available()) {
      Serial.write( Serial2.read());
    }
  }
}

void SendSample() {
  analogReadResolution(8);
  SerBuf[0] = BoardID;
  SerBuf[1] = BoardType;
  SerBuf[2] = analogRead(A0);//Temperature
  SerBuf[3] = analogRead(A1);//Battery
  //SerBuf[4] = analogRead(A3);//Extention A3
  SerBuf[6] = analogRead(A4);// Expansion A4
  //Mills() to Byte[]
  SerBuf[7] = (StartTime >> 24) & LMask;
  SerBuf[8] = (StartTime >> 16) & LMask;
  SerBuf[9] = (StartTime >> 8) & LMask;
  SerBuf[10] = (StartTime) & LMask;
  int i = 11;
  for (int j = 0; j < 10; j++) {
    SerBuf[i++] = (Pressure[j] >> 8) & PMask;
    SerBuf[i++] = (Pressure[j]) & PMask;
  }
  analogReadResolution(12);
  int windSpeed = analogRead(A3);//Extention A3
  SerBuf[4] = windSpeed >> 8 & LMask;
  SerBuf[5] = windSpeed & LMask;
  Serial2.write(255);
  Serial2.write(SerBuf, 31);
  analogReadResolution(16);
  Serial.println(StartTime);
}

void setup() {
  delay(2000);
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
  if (BoardType == 2) {
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
  Serial2.print("ATCN\r" ); //Exit command mode.
  while (Serial2.available()) {
    Serial.write( Serial2.read() ); // display response
  }
  number = 0;
  LastMillis = millis();
  lock = true;
}

void loop() { // while true
  if (BoardType == 1) {
    ForwardData();
  }
  CurrentMillis = millis();
  if ((CurrentMillis - LastMillis) >= 100 && !lock) {
    lock = true;
    StartTime = CurrentMillis;
  }
  if (number < 4096 && lock) {
    reading += analogRead(A5);
    number++;
  }
  if (number >= 4096 && lock) {
    Pressure[PressureIndex++] = reading >> 12;
    reading = 0;
    lock = false;
    number = 0;
    LastMillis = (CurrentMillis/100)*100; //Round
  }

  if (PressureIndex > 9) {
    PressureIndex = 0;
    SendSample();
  }
  // XCTU https://www.digi.com/products/embedded-systems/digi-xbee/digi-xbee-tools/xctu#productsupport-utilities
}
