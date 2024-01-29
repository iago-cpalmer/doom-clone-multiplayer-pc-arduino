#include "Entities.h"
// Entity instance to avoid allocating new memory during runtime
Entity* entityPool;
const long NULL_UID = -1;
void initEntities() {
  entityPool = CreateEntity(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
}

void pushEntityToPool(Entity* entity) {
  entity->uid = NULL_UID;
}

Entity* popEntityFromPool() {
  return entityPool;
}

void setValuesToEntity(Entity* entity, byte type, float x, float y, float z, float xr, float yr, float zr, float xs, float ys, float zs) {
  entity->type = type;
  entity->position->x = x;
  entity->position->y = y;
  entity->position->z = z;
  entity->rotation->x = xr;
  entity->rotation->y = yr;
  entity->rotation->z = zr;
  entity->scale->y = ys;
  entity->scale->x = xs;
  entity->scale->z = zs;
}

Entity* CreateEntity(byte type, float x, float y, float z, float xr, float yr, float zr, float xs, float ys, float zs) {
  Entity* entity = (Entity*)malloc(sizeof(Entity));
  entity->uid = NULL_UID;
  entity->type = type;
  entity->position = (Vector3f*)malloc(sizeof(Vector3f));
  entity->position->x = x;
  entity->position->y = y;
  entity->position->z = z;
  entity->rotation = (Vector3f*)malloc(sizeof(Vector3f));
  entity->rotation->x = xr;
  entity->rotation->y = yr;
  entity->rotation->z = zr;
  entity->scale = (Vector3f*)malloc(sizeof(Vector3f));
  entity->scale->y = ys;
  entity->scale->x = xs;
  entity->scale->z = zs;
  return entity;
}
/*
void FreeEntity(Entity* entity) {
  if (entity->position != nullptr) {
    free(entity->position);
  }
  if (entity->rotation != nullptr) {
    free(entity->rotation);
  }
  if (entity->scale != nullptr) {
    free(entity->scale);
  }
  free(entity);
}*/