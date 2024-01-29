#ifndef SERIALIZER_H
#define SERIALIZER_H

#include <Arduino.h>
#include "Vector3.h"
#include "Entities.h"
union FloatUnion{
  float floatValue;
  byte byteArray[4];
};

union LongUnion{
  long longValue;
  byte byteArray[4];
};
// Function declarations
void initSerializer();
byte* serializeFloat(float x);
void serializeFloat(byte* bytes, float x, int startIndex);
float deserializeFloat(byte* bytes, int startIndex);


void serializeLongReversed(byte* bytes, long x, int startIndex);
void serializeLong(byte* bytes, long x, int startIndex);
byte* serializeLong(long x);
long deserializeLong(byte* bytes, int startIndex);
long deserializeLongLittleEndian(byte* bytes, int startIndex);


void serializeVector3(byte* bytes, Vector3f* x, int startIndex);
byte* serializeVector3(Vector3f* x);
void deserializeVector3(Vector3f* v, byte* bytes, int startIndex);

void serializeEntity(byte* bytes, Entity* x, int startIndex);
Entity* deserializeEntity(byte* bytes, int startIndex);
#endif