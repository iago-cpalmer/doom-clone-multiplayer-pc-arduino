#ifndef INPUT_HANDLER_H
#define INPUT_HANDLER_H
#include <Arduino.h>
#include "Serializer.h"
// Key and mouse button states
const byte KEYSTATE_UP = 0;
const byte KEYSTATE_PRESSED = 1;
const byte KEYSTATE_DOWN = 2;
const byte KEYSTATE_RELEASED = 3;

// Keys
const byte MOVE_LEFT = 0;
const byte MOVE_UP = 2;
const byte MOVE_RIGHT = 4;
const byte MOVE_DOWN = 6;
// Mouse buttons
const byte LEFT_CLICK = 0;
const byte MIDDLE_CLICK = 1;
const byte RIGHT_CLICK = 2;
void deserializeInput(byte* data, int startIndex);
// Keys
bool isKeyDown(byte key);
bool isKeyPressed(byte key);
bool isKeyReleased(byte key);
bool isKeyUp(byte key);
byte getKeyState(byte key);
// Mouse
byte getMouseButtonState(byte button);
bool isMouseButtonDown(byte mouseButton);
bool isMouseButtonPressed(byte mouseButton);
bool isMouseButtonReleased(byte mouseButton);
bool isMouseButtonUp(byte mouseButton);

// Yaw & pitch
float getYaw();
float getPitch();
#endif