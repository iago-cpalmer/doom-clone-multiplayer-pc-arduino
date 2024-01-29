#ifndef ENTITIES_H
#define ENTITIES_H
#include "Vector3.h"
#include <Arduino.h>
struct Entity {
  long uid; 
  byte type;
  Vector3f* position;
  Vector3f* rotation;
  Vector3f* scale;
};

Entity* CreateEntity(byte type, float x, float y, float z, float xr, float yr, float zr, float xs, float ys, float zs);
Entity* popEntityFromPool();
void pushEntityToPool(Entity* entity);
void initEntities();
void setValuesToEntity(Entity* entity, byte type,  float x, float y, float z, float xr, float yr, float zr, float xs, float ys, float zs);

// EntityTypes
struct EntityType {
  byte maxHealth;
  float speed;
  void (*updateFunction)(Entity*);
  float xscale;
  float yscale;
};


#endif