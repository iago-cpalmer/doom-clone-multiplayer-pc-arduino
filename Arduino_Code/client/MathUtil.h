#ifndef MATH_UTIL_H
#define MATH_UTIL_H
#include "math.h"
#include <Arduino.h>
// Some utils

// Convert float to longs and vice versa
union {
  float floatValue;
  long longValue;
} LongFloatUnion;
const float DEGREES_TO_RADIANS = M_PI / 180;

void initMathUtil();
// Return the values of sin and cos of a specific angle (in degrees) from the pre-computed table
float mSin(float angle);
float mCos(float angle);

float longToFloat(long x);
long floatToLong(float x);
#endif