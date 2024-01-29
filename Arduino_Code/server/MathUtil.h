#ifndef MATH_UTIL_H
#define MATH_UTIL_H
#include "math.h"
#include <Arduino.h>
// Class to provide some utils

// Convert long to floats and vice versa
union {
  float floatValue;
  long longValue;
} LongFloatUnion;

// Constant to convert degrees to radians
const float DEGREES_TO_RADIANS = M_PI / 180;

void initMathUtil();

// Utility functions to get the sin and cos of a specific angle (in degrees) that are pre-computed
float mSin(float angle);
float mCos(float angle);

float longToFloat(long x);
long floatToLong(float x);

#endif