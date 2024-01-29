#include "MathUtil.h"

float sinTable[181];
float cosTable[181];

void initMathUtil() {
    for (int i = 0; i <= 180; i++) {
        float radians = i * M_PI / 180.0;
        sinTable[i] = sin(radians);
        cosTable[i] = cos(radians);
    }
}

float mSin(float angle) {
    angle = fmod(angle + 360, 360);  // Normalize angle to be between 0 and 360
    if (angle > 180) {
        return -sinTable[360 - (int)angle];
    } else {
        return sinTable[(int)angle];
    }
}

float mCos(float angle) {
    angle = fmod(angle + 360, 360);  // Normalize angle to be between 0 and 360
    return cosTable[(int)(angle > 180 ? 360 - angle : angle)];
}

float longToFloat(long x) {
  LongFloatUnion.longValue = x;
  return LongFloatUnion.floatValue;
}
long floatToLong(float x) {
  LongFloatUnion.floatValue = x;
  return LongFloatUnion.longValue;
}

