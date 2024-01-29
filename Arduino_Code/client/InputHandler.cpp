#include "InputHandler.h"
#include "MathUtil.h"
// Some variables to store the state of input
long keyStates;
uint8_t mouseState;
float yaw;
float pitch;

// Deserializes input from byte array
void deserializeInput(byte* data, int startIndex) {
  mouseState = data[startIndex];
  keyStates = deserializeLongLittleEndian(data, 1 + startIndex);
  pitch = deserializeFloat(data, 5 + startIndex);
  yaw = deserializeFloat(data, 9 + startIndex);
}
// Returns whether the key is down or not
bool isKeyDown(byte key) {
  return getKeyState(key) == KEYSTATE_DOWN;
}
// Returns whether the key is pressed or not
bool iKeyPressed(byte key) {
  return getKeyState(key) == KEYSTATE_PRESSED;
}
// Returns whether the key is released or not
bool isKeyReleased(byte key) {
  return getKeyState(key) == KEYSTATE_RELEASED;
}
// Returns whether the key is up or not
bool isKeyUp(byte key) {
  return getKeyState(key) == KEYSTATE_UP;
}
// Returns the key state
byte getKeyState(byte key) {
  return (keyStates & (long)(0b11 << ((int)key)))>>(int)key;
}
// Returns the mouse button state
byte getMouseButtonState(byte button) {
  return (mouseState & (0b11 << (button))) >>button;
}
// Returns whether the mouse button is down or not
bool isMouseButtonDown(byte mouseButton) {
  return getMouseButtonState(mouseButton) == KEYSTATE_DOWN;
}
// Returns whether the mouse button is pressed or not
bool isMouseButtonPressed(byte mouseButton) {
  return getMouseButtonState(mouseButton) == KEYSTATE_PRESSED;
}
// Returns whether the mouse button is released or not
bool isMouseButtonReleased(byte mouseButton) {
  return getMouseButtonState(mouseButton) == KEYSTATE_RELEASED;
}
// Returns whether the mouse button is up or not
bool isMouseButtonUp(byte mouseButton) {
  return getMouseButtonState(mouseButton) == KEYSTATE_UP;
}

float getYaw() {
  return yaw;
}
float getPitch() {
  return pitch;
}