#include "GameManager.h"
#define MAX_ENTITIES 20
Entity* entities[MAX_ENTITIES];
const Vector3f VECTOR_ZERO = { 0, 0, 0 };
const Vector3f VECTOR_UP = { 0, 1, 0 };
Vector3f* forward = nullptr;
Vector3f* right = nullptr;
Vector3f* direction = nullptr;
EntityType entityTypes[] = {
  { 3, 240.0, &updateMainPlayer, 1.0f,30.0f },        // main player
  { 0, 0.0, nullptr ,1.0f, 30.0f},                   // other player 
  { 0, 0.0, nullptr, 1.0f, 1.0f},                     // enemy one
  { 0, 0.0, nullptr, 0.25f, 0.25f },                  // bullet
  { 0, 0.0, nullptr, 1.0f, 1.0f},                    // ammo power-up
  { 0, 0.0, nullptr, 1.0f, 1.0f}                     // health power-up
};

// Some variables for main player
const int BULLETS_PER_SECOND = 2;
const float TIME_BETWEEN_BULLETS = 1.0 / BULLETS_PER_SECOND;
float timerToShoot = 0.0f;
// Update
const int TICKS_PER_SECOND = 120;
const float TIME_PER_TICK = 1.0 / TICKS_PER_SECOND;
float timerForTick = 0;

// Network tick
const int NETWORK_TICKS_PER_SECOND = 20;
const float TIME_PER_NETWORK_TICK = 1.0 / NETWORK_TICKS_PER_SECOND;
float timerForNetworkTick = 0;

const float TIME_FOR_ACK = 0.25f; 
float timerForAck = 0;

float DeltaTime = 1;
void initGameManager() {
  // Initialize entities array
  for (int i = 0; i < MAX_ENTITIES; i++) {
    entities[i] = CreateEntity(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
  }
  if(forward==nullptr) {
    forward = malloc(sizeof(Vector3f));
  }
  if(right==nullptr) {
    right = malloc(sizeof(Vector3f));
  }
  if(direction==nullptr) {
    direction = malloc(sizeof(Vector3f));
  }
}

Entity* getEntity(long uid) {
  if (isNull(entities[uid])) {
    return nullptr;
  }
  return entities[uid];
}

int getMaxEntities() {
  return MAX_ENTITIES;
}

bool isNull(Entity* entity) {
  return (entity->uid < 0 || entity->uid > MAX_ENTITIES);
}
// Update entity values received from server
void updateEntityValues(Entity* entity) {
  long uid = entity->uid;
  if (!isNull(entities[uid])) {
    entities[uid]->uid = uid;
    *entities[uid]->position = *entity->position;
    *entities[uid]->rotation = *entity->rotation;
    *entities[uid]->scale = *entity->scale;
  }
}

void instantiateEntity(Entity* entity) {
  long uid = entity->uid;
  entities[uid]->type = entity->type;
  *entities[uid]->position = *entity->position;
  *entities[uid]->rotation = *entity->rotation;
  *entities[uid]->scale = *entity->scale;
  entities[uid]->uid = uid;
  // Send to pc the instantiation
  NetworkManager::Instance->sendMqttInstantiationOfObject(NetworkManager::Instance->pairTopicSend, entities[entity->uid]);
}

long getFirstFreeUID() {
  for (int i = 0; i < MAX_ENTITIES; i++) {
    if (isNull(entities[i])) {
      return i;
    }
  }
  return -1;
}

void destroyEntity(long uid) {
  entities[uid]->uid = -1;
}
// Updates the staet of the game
void updateGameState() {
  timerForTick += DeltaTime;
  if (timerForTick < TIME_PER_TICK) {
    return;
  }
  timerForTick = 0;
  for (int i = 0; i < MAX_ENTITIES; i++) {
    if (!isNull(entities[i]) && entityTypes[entities[i]->type].updateFunction != nullptr) {
      // call that entity type update function
      entityTypes[entities[i]->type].updateFunction(entities[i]);
    }
  }
}
// Send state to pc and server
void sendState() {
  timerForNetworkTick += DeltaTime;
  timerForAck+=DeltaTime;
  if (timerForNetworkTick < TIME_PER_NETWORK_TICK) {
    return;
  }
  timerForNetworkTick = 0;
  // Send player state to server
  if (NetworkManager::Instance->getOwnUID() != -1) {
    NetworkManager::Instance->sendServerUpdateOfObject(entities[NetworkManager::Instance->getOwnUID()]);
  }

  for (int i = 0; i < MAX_ENTITIES; i++) {
    if (!isNull(entities[i])) {
      // send state of current entity to pc
      NetworkManager::Instance->sendMqttUpdateOfObject(NetworkManager::Instance->pairTopicSend, entities[i]);
    }
  }
  
  if(timerForAck < TIME_FOR_ACK) {
    return;
  }
  timerForAck=0;
  NetworkManager::Instance->resendPendingMsgs();
}
// Returns whether a point specified by x,z is inside a box specificied by lx, lz, rx, rz
bool isInsideBBox(float x, float z, float lx, float lz, float rx, float rz) {
  return x >= lx && z >= lz && x <= rx && z <= rz;
}

bool isMovingEntityCollidingWithMap(Entity* entity, Vector3f* direction) {
  float x = entity->position->x + direction->x;
  float y = entity->position->y + direction->y;
  float z = entity->position->z + direction->z;
  // Check if it's colliding with floor or ceiling
  if (y <= 0 || y >= 50) {
    return true;
  }
  // Check collision with all walls
  float* walls = getWallAddress(0);
  for (int i = 0; i < N_WALLS; i++) {
    if (isInsideBBox(x, z, walls[i * 4 + 0], walls[i * 4 + 1], walls[i * 4 + 2], walls[i * 4 + 3])) {
      return true;
    }
  }
  return false;
}
// Update player
void updateMainPlayer(Entity* entity) {
  entity->rotation->x = getPitch();
  entity->rotation->y = getYaw();
  GetForwardVector(entity);
  GetRightVector3f();
  // Process input and move accordingly
  direction->x = 0;
  direction->y = 0;
  direction->z = 0;
  if (isKeyDown(MOVE_UP)) {
    direction->x -= forward->x;
    direction->z -= forward->z;
  }
  if (isKeyDown(MOVE_DOWN)) {
    direction->x += forward->x;
    direction->z += forward->z;
  }
  if (isKeyDown(MOVE_LEFT)) {
    direction->x += right->x;
    direction->z += right->z;
  }
  if (isKeyDown(MOVE_RIGHT)) {
    direction->x -= right->x;
    direction->z -= right->z;
  }
  // Check if it's colliding with map
  if (isMovingEntityCollidingWithMap(entity, direction) == false) {
    entity->position->x += direction->x * entityTypes[entity->type].speed * DeltaTime;
    entity->position->y += direction->y * entityTypes[entity->type].speed * DeltaTime;
    entity->position->z += direction->z * entityTypes[entity->type].speed * DeltaTime;
  }
  // Shooting action
  timerToShoot+=DeltaTime;
  float ammo = entity->scale->z;
  if (isMouseButtonDown(LEFT_CLICK) && ammo>0 && timerToShoot>=TIME_BETWEEN_BULLETS) {
    timerToShoot=0;
    Entity* bullet = popEntityFromPool();
    setValuesToEntity(bullet, 
    PLAYER_BULLET_ENTITY_TYPE,
    entity->position->x-forward->x*15, 
    entity->position->y-forward->y, 
    entity->position->z-forward->z*15, 
    entity->rotation->x, 
    entity->rotation->y, 
    longToFloat(NetworkManager::Instance->getOwnUID()), 
    0, 
    0, 
    0);
    NetworkManager::Instance->sendShotToServer(bullet);
    ammo--;
    entity->scale->z = ammo;
  }
}

// Computes the forward vector of an entity
void GetForwardVector(Entity* entity) {
  float yaw = entity->rotation->y;    // Convert to radians
  float pitch = entity->rotation->x;  // Convert to radians

  forward->x = mCos(yaw) * mCos(pitch);
  forward->y = mSin(pitch);
  forward->z = mSin(yaw) * mCos(pitch);
  forward->normalize();  // Optional, for unit vector
}
// Computes the right vector
void GetRightVector3f() {
  *right = *forward % VECTOR_UP;
  right->normalize();  // Optional, for unit vector
}

void setDeltaTime(float f) {
  DeltaTime = f;
}
float getDeltaTime() {
  return DeltaTime;
}
