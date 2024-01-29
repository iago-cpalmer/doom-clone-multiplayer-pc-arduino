#ifndef GAME_MANAGER_H
#define GAME_MANAGER_H
#include <Arduino.h>
#include "Vector3.h"
#include "Entities.h"
#include "NetworkManager.h"
#include "MathUtil.h"
#include "Walls.h"
// Constants to get the index of the entity type
#define MAIN_PLAYER_ENTITY_TYPE 0
#define PLAYER_ENTITY_TYPE 1
#define ENEMY_ENTITY_TYPE 2
#define PLAYER_BULLET_ENTITY_TYPE 3
#define AMMO_POWERUP_ENTITY_TYPE 4
#define HEALTH_POWERUP_ENTITY_TYPE 5

void changeBulletSpeed(float multiplier);
void initGameManager();
bool isNull(Entity* entity);
EntityType* getEntityType(byte id);
Vector3f* getSpawnPosition(long i);
Entity* getEntity(long uid);
void updateEntityValues(Entity* entity);
void destroyEntity(long uid);
long instantiateNewEntity(Entity* entity, long uuid);
void updateGameState();
void sendState();
void sendAllEntitiesToClient(long uuid);
long getFirstFreeUID();
long getEntityOfClient(long uid);
bool isInsideBBox(float x, float z, float lx, float lz, float rx, float rz);
bool isInside3BBox(float x, float y, float z, float lx, float ly, float lz, float rx, float ry, float rz);
Entity* isCollidingWithEntityType(Entity* entity, Vector3f* direction, byte entityType);
bool isMovingEntityCollidingWithMap(Entity* entity, Vector3f* direction);
void updatePlayer(Entity* entity);
void updateEnemyOne(Entity* entity);
void updateEnemyOneProjectile(Entity* entity);
void updateBullet(Entity* entity);
void updateAmmoPwup(Entity* entity);
void updateHealthPwup(Entity* entity);
void setDeltaTime(float f);
float getDeltaTime();
Vector3f* GetForwardVector(Entity* entity);
Vector3f* GetRightVector3f();
#endif