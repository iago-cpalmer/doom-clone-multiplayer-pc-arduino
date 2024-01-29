#ifndef GAME_MANAGER_H
#define GAME_MANAGER_H
#include <Arduino.h>
#include "Vector3.h"
#include "Entities.h"
#include "NetworkManager.h"
#include "InputHandler.h"
#include "MathUtil.h"
#include "Walls.h"
// Constants to get index of entity types
#define MAIN_PLAYER_ENTITY_TYPE 0
#define PLAYER_ENTITY_TYPE 1
#define ENEMY_ENTITY_TYPE 2
#define PLAYER_BULLET_ENTITY_TYPE 3
#define AMMO_POWERUP_ENTITY_TYPE 4
#define HEALTH_POWERUP_ENTITY_TYPE 5

void initGameManager();
bool isNull(Entity* entity);
Entity* getEntity(long uid);
int getMaxEntities();
void updateEntityValues(Entity* entity);
void destroyEntity(long uid);
void instantiateEntity(Entity* entity);
void updateGameState();
void sendState();
long getFirstFreeUID();

bool isInsideBBox(float x, float z, float lx, float lz, float rx, float rz);
bool isMovingEntityCollidingWithMap(Entity* entity, Vector3f* direction);
void updateMainPlayer(Entity* entity);
void updateEnemyOne(Entity* entity);
void updateEnemyOneProjectile(Entity* entity);

void setDeltaTime(float f);
float getDeltaTime();
void GetForwardVector(Entity* entity);
void GetRightVector3f();
#endif
