#include "HardwareSerial.h"
#include "Arduino.h"
#include "Serializer.h"
#include "MathUtil.h"
// Serializing and deserializing floats
FloatUnion floatUnion;
// Serializing and deserializing longs
LongUnion longUnion;

// Initializer
void initSerializer() {
  floatUnion.floatValue = 0.0;
  longUnion.longValue = 0;
}
//********************************************
// Serialize and deserialize float methods
//********************************************
byte* serializeFloat(float x) {
  byte* bytes = new byte[4];
  floatUnion.floatValue = x;
  for (int i = 0; i < sizeof(float); i++) {
    bytes[i] = floatUnion.byteArray[i];
  }
  return bytes;
}
void serializeFloat(byte* bytes, float x, int startIndex) {
  floatUnion.floatValue = x;
  for (int i = 0; i < sizeof(float); i++) {
    bytes[i + startIndex] = floatUnion.byteArray[i];
  }
}
float deserializeFloat(byte* bytes, int startIndex) {
  for (int i = 0; i < sizeof(float); i++) {
    floatUnion.byteArray[i] = bytes[i + startIndex];
  }
  return floatUnion.floatValue;
}
//********************************************
// Serialize and deserialize long methods
//********************************************
void serializeLong(byte* bytes, long x, int startIndex) {

  longUnion.longValue = x;
  for (int i = 0; i < sizeof(long); i++) {
    bytes[i + startIndex] = longUnion.byteArray[i];
  }
}
byte* serializeLong(long x) {
  byte* bytes = new byte[4];
  longUnion.longValue = x;
  for (int i = 0; i < sizeof(long); i++) {
    bytes[i] = longUnion.byteArray[i];
  }
  return bytes;
}
long deserializeLong(byte* bytes, int startIndex) {
  long x = 0;
  for (int i = 0; i < sizeof(long); i++) {
    longUnion.byteArray[i] = bytes[i + startIndex];
  }
  return longUnion.longValue;
}
long deserializeLongLittleEndian(byte* bytes, int startIndex) {
  long x = 0;
  for (int i = 0; i < sizeof(long); i++) {
    longUnion.byteArray[sizeof(long)-1-i] = bytes[i + startIndex];
  }
  return longUnion.longValue;
}

//********************************************
// Serialize and deserialize Vector3f methods
//********************************************
void serializeVector3(byte* bytes, Vector3f* x, int startIndex) {
  serializeFloat(bytes, x->x, 0 + startIndex);
  serializeFloat(bytes, x->y, 4 + startIndex);
  serializeFloat(bytes, x->z, 8 + startIndex);
}
byte* serializeVector3(Vector3f* x) {
  byte* bytes = new byte[sizeof(float) * 3];
  serializeFloat(bytes, x->x, 0);
  serializeFloat(bytes, x->y, 4);
  serializeFloat(bytes, x->z, 8);
  return bytes;
}
void deserializeVector3(Vector3f* v, byte* bytes, int startIndex) {
  v->x = deserializeFloat(bytes, startIndex);
  v->y = deserializeFloat(bytes, startIndex + 4);
  v->z = deserializeFloat(bytes, startIndex + 8);
}
//********************************************
// Serialize and deserialize Entities methods
//********************************************
void serializeEntity(byte* bytes, Entity* x, int startIndex) {
  serializeLong(bytes, x->uid, startIndex);
  bytes[startIndex + 4] = x->type;
  serializeVector3(bytes, x->position, startIndex + 5);
  serializeVector3(bytes, x->rotation, startIndex + 17);
  serializeVector3(bytes, x->scale, startIndex + 29);  
}
Entity* deserializeEntity(byte* bytes, int startIndex) {
  Entity* entity = popEntityFromPool();
  entity->uid = deserializeLongLittleEndian(bytes, startIndex);
  entity->type = bytes[startIndex + 4];
  deserializeVector3(entity->position, bytes, 5 + startIndex);
  deserializeVector3(entity->rotation, bytes, 17 + startIndex);
  deserializeVector3(entity->scale, bytes, 29 + startIndex);
  return entity;
}

void deserializeEntity(Entity* entity, byte* bytes, int startIndex) {
  long uid = deserializeLongLittleEndian(bytes, startIndex);
  entity->uid = uid;
  entity->type = bytes[startIndex + 4];
  deserializeVector3(entity->position, bytes, 5 + startIndex);
  deserializeVector3(entity->rotation, bytes, 17 + startIndex);
  deserializeVector3(entity->scale, bytes, 29 + startIndex);
}

