
#include <HID.h>

void setup() {
  // put your setup code here, to run once:
  // Starts Serial at baud 115200 otherwise HID wont work on Uno/Mega.
  // This is not needed for Leonado/(Pro)Micro but make sure to activate desired USB functions in HID.h
  Serial.begin(SERIAL_HID_BAUD);

  // Sends a clean report to the host. This is important because
  // the 16u2 of the Uno/Mega is not turned off while programming
  // so you want to start with a clean report to avoid strange bugs after reset.
  Mouse.begin();
  Keyboard.begin();
}

const int MaxChars = 12;
char strValue[MaxChars + 1];
int readIndex = 0;
int readingCompleted = 0;

void loop() {

  if (Serial.available() > 0 )
  {
    char ch = Serial.read();
    if (readIndex <  MaxChars) {
      strValue[readIndex++] = ch; // add the ASCII character to the string;
      if (ch == 'z')
      {
        readingCompleted = 1;
        readIndex = 0;
      }
    }
    else
    {
      readIndex = 0;
    }
  }

  if (readingCompleted)
  {
    readingCompleted = 0;
    if (strValue[0] == 'a')
    {
      Mouse.press(MOUSE_LEFT);
    }
    else if (strValue[0] == 'b')
    {
      Mouse.release(MOUSE_LEFT);
    }
    else if (strValue[0] == 'c')
    {
      Mouse.press(MOUSE_RIGHT);
    }
    else if (strValue[0] == 'd')
    {
      Mouse.release(MOUSE_RIGHT);
    }
    else if (strValue[0] == 'e')
    {
      strValue[MaxChars] = 0;
      char xValueStr[6];
      char yValueStr[6];
      for (int i = 0; i < 5; i++)
      {
        xValueStr[i] = strValue[i + 1];
        yValueStr[i] = strValue[i + 6];
      }
      xValueStr[5] = 0;
      yValueStr[5] = 0;

      int xValue = atoi(xValueStr);
      int yValue = atoi(yValueStr);
      // do mouse move with this value
      Mouse.move(xValue, yValue, 0);
    }
  }
}
