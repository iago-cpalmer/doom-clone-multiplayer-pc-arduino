#include "GameManager.h"
#define MAX_ENTITIES 20
const float ENEMY_SHOOTING_RANGE = 30.0f;
const float TIME_BETWEEN_SHOTS = 1.0f;  // one second
float timerForEnemyToShoot = 0.0f;
Entity* entities[MAX_ENTITIES];
const Vector3f VECTOR_ZERO = { 0, 0, 0 };
const Vector3f VECTOR_UP = { 0, 1, 0 };
const float SCALE_FACTOR = 10;
EntityType entityTypes[] = {
  { 3, 60.0, nullptr, 1.0f, 15.0f },                          // main player -- THIS ONE IS USELESS IN SERVER-SIDE
  { 3, 60.0, &updatePlayer, 1.0f, 3.0f },                     // Player.
  { 2, 60.0, nullptr/* &updateEnemyOne*/, 1.0f, 1.0f },       // Enemy (was not implented at the end)
  { 2, 480.0, &updateBullet, 0.25f, 0.25f },                  // Bullet
  { 1, 240.0, &updateAmmoPwup, 1.0f, 1.0f},
  { 1, 240.0, &updateHealthPwup, 1.0f, 1.0f}
};
// Possible spawn positions for players
Vector3f spawnPositions[] = {
  {20,30,20},
  {480,30,480},
  {20,30,480},
  {480,30,20}
};
// Vectors that will be used during game loops to avoid allocating new memory during runtime
Vector3f* forward;
Vector3f* right;
Vector3f* direction;
float speedMultiplierBullet = 1.0f;
// Update
const int TICKS_PER_SECOND = 120;
const float TIME_PER_TICK = 1.0 / TICKS_PER_SECOND;
float timerForTick = 0;

// Network tick
const int NETWORK_TICKS_PER_SECOND = 20;
const float TIME_PER_NETWORK_TICK = 1.0 / NETWORK_TICKS_PER_SECOND;
float timerForNetworkTick = 0;

// Ack ticks
const float TIME_FOR_ACK = 0.25f; 
float timerForAck = 0;

float DeltaTime = 1;
void initGameManager() {
  for (int i = 0; i < MAX_ENTITIES; i++) {
    entities[i] = CreateEntity(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
  }
  forward = malloc(sizeof(Vector3f));
  right = malloc(sizeof(Vector3f));
  direction = malloc(sizeof(Vector3f));
}

EntityType* getEntityType(byte id) {
  return &entityTypes[id];
}
Vector3f* getSpawnPosition(long i) {
  return &spawnPositions[i];
}
void changeBulletSpeed(float multiplier) {
  speedMultiplierBullet = multiplier;
}

Entity* getEntity(long uid) {
  if (isNull(entities[uid])) {
    return nullptr;
  }
  return entities[uid];
}

bool isNull(Entity* entity) {
  return (entity->uid == -1);
}
// Sends all entities states (if not null) to the clients
void sendAllEntitiesToClient(long uuid) {
  for (int i = 0; i < MAX_ENTITIES; i++) {
    if (!isNull(entities[i])) {
      NetworkManager::Instance->sendInstantiationOfObjectToClient(uuid, entities[i]);
    }
  }
}
// Updates a specific entity values. Mainly used for players from clients
void updateEntityValues(Entity* entity) {
  long uid = entity->uid;
  if (!isNull(entities[uid])) {
    *entities[uid]->position = *entity->position;
    *entities[uid]->rotation = *entity->rotation;
    *entities[uid]->scale = *entity->scale;
  }
}
// Instantiates a new entity with the first possible uid
long instantiateNewEntity(Entity* entity, long uuid) {
  long uid = getFirstFreeUID();
  entities[uid]->type = entity->type;
  *entities[uid]->position = *entity->position;
  *entities[uid]->rotation = *entity->rotation;
  *entities[uid]->scale = *entity->scale;
  entities[uid]->uid = uid;
  // Send instantiation to client(s)
  NetworkManager::Instance->sendInstantiationOfObject(uuid, entities[uid]);
  return entities[uid]->uid;
}
// Gets the first UID/ slot in entities array
long getFirstFreeUID() {
  for (int i = 0; i < MAX_ENTITIES; i++) {
    if (isNull(entities[i])) {
      return i;
    }
  }
  return -1;
}
// Destroys/ Despawns an entity
void destroyEntity(long uid) {
  // Gets if its a specific client entity (the player). In that case, we want to
  // avoid sending a message with ack to that client because it's disconnected
  long uuid = getEntityOfClient(uid);
  // Notify clients
  NetworkManager::Instance->sendDespawnEntity(uuid,uid);
  entities[uid]->uid = -1;
}
// Update game state
void updateGameState() {
  timerForTick += DeltaTime;
  if (timerForTick < TIME_PER_TICK) {
    return;
  }
  timerForTick = 0;
  timerForEnemyToShoot += DeltaTime;
  // Update all entities calling to their specific update function
  for (int i = 0; i < MAX_ENTITIES; i++) {
    if (isNull(entities[i])==false && entityTypes[entities[i]->type].updateFunction != nullptr) {
      // call that entity type update function
      entityTypes[entities[i]->type].updateFunction(entities[i]);
    }
  }
}
// Send the current state of the game to the clients
void sendState() {
  timerForNetworkTick += DeltaTime;
  if (timerForNetworkTick < TIME_PER_NETWORK_TICK) {
    return;
  }
  timerForNetworkTick = 0;
  for (int i = 0; i < MAX_ENTITIES; i++) {
    if (!isNull(entities[i])) {
      // send state of current entity to clients
      NetworkManager::Instance->sendUpdateOfObject(-1, entities[i]);
    }
  }
  // Resend messages that require an ack
  NetworkManager::Instance->resendPendingMsgs();
}

// Returns whether a point specified by x,z is inside a box specificied by lx, lz, rx, rz
bool isInsideBBox(float x, float z, float lx, float lz, float rx, float rz) {
  return x >= lx && z >= lz && x <= rx && z <= rz;
}
// Returns whether a point specified by x,z is inside a box with a finite height
bool isInside3BBox(float x, float y, float z, float lx, float ly, float lz, float rx, float ry, float rz) {
  return x >= lx && y >=ly && z >= lz && x <= rx && y <= ry && z <= rz;
}
// Returns whether an entity (with a direction of movement) is colliding with any wall of the map
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
// Returns the entity of the specified type that is colliding with the particular entity
Entity* isCollidingWithEntityType(Entity* entity, Vector3f* direction, byte entityType) {
  float x = entity->position->x + direction->x;
  float y = entity->position->y + direction->y;
  float z = entity->position->z + direction->z;
  for (int i = 0; i < MAX_ENTITIES; i++) {
      if (!isNull(entities[i]) && entities[i]->type == entityType) {
        // Check if given point is inside bounding box of entity
        // Compute bounding box of entity to check collision with
        float lbcX = entities[i]->position->x - SCALE_FACTOR*entities[i]->scale->x;
        float lbcY = 0;
        float lbcZ = entities[i]->position->z - SCALE_FACTOR;
        float rtcX = entities[i]->position->x + SCALE_FACTOR*entities[i]->scale->x;
        float rtcY = SCALE_FACTOR*entities[i]->scale->y;
        float rtcZ = entities[i]->position->z + SCALE_FACTOR;
        if(isInside3BBox(x,y,z,lbcX,lbcY,lbcZ,rtcX,rtcY,rtcZ)==true) {
          return entities[i];
        }
      }
  }
  return nullptr;
}
// Gets the entity attached to that specific client instance (its player)
long getEntityOfClient(long uid) {
  for(int i = 0; i < NetworkManager::Instance->getMaxClients(); i++) {
    if(NetworkManager::Instance->getClient(i)->entityUid==uid) {
      return NetworkManager::Instance->getClient(i)->uuid;
    }
  }
  return -1;
}
// Function that updates the player
// It only checks if its health is below 0 (dead)
// If dead, respawns it and notifies the client
void updatePlayer(Entity* entity){
  
  // Check if hp is less or equal to 0
  if(entity->rotation->z<=0) {
    // Player is dead
    // Change position, rotation & scale of player
    entity->position->x = spawnPositions[entity->uid%4].x;
    entity->position->y = spawnPositions[entity->uid%4].y;
    entity->position->z = spawnPositions[entity->uid%4].z;
    entity->rotation->x = 0;
    entity->rotation->y = 0;
    entity->rotation->z = (float) entityTypes[PLAYER_ENTITY_TYPE].maxHealth; // health
    entity->scale->x = entityTypes[PLAYER_ENTITY_TYPE].xscale;
    entity->scale->y = entityTypes[PLAYER_ENTITY_TYPE].yscale;
    entity->scale->z = 20; // Set ammo storage
    // Notify player that is dead with its new entity info (position, rotation & scale)
    long uuid = getEntityOfClient(entity->uid);
    NetworkManager::Instance->sendNotifyDeath(uuid, entity);
  }
}
// Function to update the enemy of type one. 
// Is not being called but it's kept in code just in case if 
// enemy must be added in the future
void updateEnemyOne(Entity* entity) {
  // The enemy will check its health to see if it's gotta be despawned (hp==0)
  // The enemy will select a target (a player, entity of type=1)
  // The enemy will save the target uid in its rotation.z
  // Check if the target uid is still a player or uid is not -1 (in that case, it has been despawned, so the enemy will choose another target)
  // If it's a player, the entity will move towards him until the enemy
  // gets in range and will shoot a projectile in its direction
  // After shooting it, will check for the most near player to change its target
  // If the entity hits a wall with collision, it will stop there and choose a new target

  if (entity->scale->z <= 0) {
    // the health is 0, despawn
    destroyEntity(entity->uid);
  }
  long targetUid = floatToLong(entity->rotation->z);
  if (targetUid < 0 || entities[targetUid]->uid < 0 || entities[targetUid]->type != PLAYER_ENTITY_TYPE) {
    // No target that is a player has been choosen
    // Get nearest player
    float minDistance = 999999;
    targetUid = -1;
    for (int i = 0; i < MAX_ENTITIES; i++) {
      if (!isNull(entities[i]) && entities[i]->type == PLAYER_ENTITY_TYPE) {
        // It's a player
        float currentDist = entity->position->distance_squared(entities[i]->position);
        if (currentDist <= minDistance) {
          targetUid = i;
          minDistance = currentDist;
        }
      }
    }
    if (targetUid == -1) {
      return;
    }
    Serial.print("Min distance: ");
    Serial.println(minDistance);
    // Nearest player found
    entity->rotation->z = longToFloat(targetUid);
    // Calculate rotation to player
    direction->x = (entity->position->x - entities[targetUid]->position->x) / 10;
    direction->y = (entity->position->y - entities[targetUid]->position->y) / 10;
    direction->z = (entity->position->z - entities[targetUid]->position->z) / 10;
    direction->normalize();
    entity->rotation->y = atan2(direction->x, direction->z) * 180 / M_PI;
    // Get angle of rotation for y-axis
    //entity->rotation->y = atan2(direction->x, direction->z) * 180 / M_PI;
  }

  direction->x = (entity->position->x - entities[targetUid]->position->x) / 10;
  direction->y = (entity->position->y - entities[targetUid]->position->y) / 10;
  direction->z = (entity->position->z - entities[targetUid]->position->z) / 10;
  direction->normalize();
  entity->rotation->y = atan2(direction->x, direction->z) * 180 / M_PI;

  /*
  // Check if the entity has a collision with his forward vector
  GetForwardVector(entity);
  forward->x = -forward->x;
  forward->y = -forward->y;
  forward->z = -forward->z;
  if (isMovingEntityCollidingWithMap(entity, forward)) {
    // It's colliding, rotate a bit
    entity->rotation->y = (entity->rotation->y + 15.0f);
    if(entity->rotation->y>=360.0f){
      entity->rotation->y=0.0f;
    }
  } else {
    
    // Not colliding, move forward
    entity->position->x += forward->x * entityTypes[entity->type].speed * DeltaTime;
    entity->position->y += forward->y * entityTypes[entity->type].speed * DeltaTime;
    entity->position->z += forward->z * entityTypes[entity->type].speed * DeltaTime;
    // Check if between player and enemy there is a wall.
    // Calculate rotation to player
    direction->x = -(entity->position->x - entities[targetUid]->position->x);
    direction->y = -(entity->position->y - entities[targetUid]->position->y);
    direction->z = -(entity->position->z - entities[targetUid]->position->z);
    direction->normalize();
    if (isMovingEntityCollidingWithMap(entity, direction)==false) {
      // It's a "free path"
      // Change rotation to look to the player
      entity->rotation->y = atan2(direction->x, direction->y) / DEGREES_TO_RADIANS;
      // Check if target is in its shooting range & if the enemy can shoot because it's not on cd
      if (timerForEnemyToShoot >= TIME_BETWEEN_SHOTS) {
        float currentDist = entity->position->distance_squared(entities[targetUid]->position);
        if (currentDist <= ENEMY_SHOOTING_RANGE) {
          // it's in range, shoot
          timerForEnemyToShoot = 0;
          // move backwards (Enemy shouldn't move when it's in range of player and it has moved previously, reverse it)
          entity->position->x -= forward->x * entityTypes[entity->type].speed * DeltaTime;
          entity->position->y -= forward->y * entityTypes[entity->type].speed * DeltaTime;
          entity->position->z -= forward->z * entityTypes[entity->type].speed * DeltaTime;
        }
      }
    }
  }*/
}

// Function to update the enemy's (type 1) projectile.
// As well as with the enemy, is not used now and it's kept
// just in case for the future
void updateEnemyOneProjectile(Entity* entity) {
  //Serial.println("Updating bullet!");
  // Update the enemy projectile position as it will be simulated here
  GetForwardVector(entity);
  entity->position->x -= forward->x * entityTypes[entity->type].speed * DeltaTime;
  entity->position->y -= forward->y * entityTypes[entity->type].speed * DeltaTime;
  entity->position->z -= forward->z * entityTypes[entity->type].speed * DeltaTime;
  // TODO: Check if it collides with an enemy or a wall.
  // If it collides with a wall, it will despawn
  // If it collides with an enemy, it will remove one of health to that entity & despawn itself
}
// Update of the ammunition power-up. It restores all the ammo of a player
void updateAmmoPwup(Entity* entity) {
  // SPIN
  entity->rotation->y = (entity->rotation->y + entityTypes[entity->type].speed * DeltaTime);
  if(entity->rotation->y>=360.0f) {
    entity->rotation->y = entity->rotation->y-360.0f;
  }
  // Check collision with player
  direction->x = 0;
  direction->y = 0;
  direction->z = 0;
  Entity* collision = isCollidingWithEntityType(entity, direction, PLAYER_ENTITY_TYPE);
  if(collision!=nullptr) {
    // Player has collided with bullet power up
    collision->scale->z = 20;
    long uuid = getEntityOfClient(collision->uid);
    NetworkManager::Instance->sendStrictUpdateOfObject(uuid, collision);
    // Change to new position
    entity->position->x = (float)random(2000,48000)/100.0f;
    entity->position->z = (float)random(2000,48000)/100.0f;
  }
}
// Update of the health power-up. It restores all the hp of a player
void updateHealthPwup(Entity* entity) {
  // Spin
  entity->rotation->y = (entity->rotation->y + entityTypes[entity->type].speed * DeltaTime);
  if(entity->rotation->y>=360.0f) {
    entity->rotation->y = entity->rotation->y-360.0f;
  }
  // Check collision with player
  direction->x = 0;
  direction->y = 0;
  direction->z = 0;
  Entity* collision = isCollidingWithEntityType(entity, direction, PLAYER_ENTITY_TYPE);
  if(collision!=nullptr) {
    // Player has collided with bullet power up
    collision->rotation->z=3;
    long uuid = getEntityOfClient(collision->uid);
    NetworkManager::Instance->sendStrictUpdateOfObject(uuid, collision);
    // Change to new position
    entity->position->x = (float)random(2000,48000)/100.0f;
    entity->position->z = (float)random(2000,48000)/100.0f;
  }
}
// Updates a bullet, it goes straight forward and checks collision with map or with player to despawn itself and damage a player (if one got hit)
void updateBullet(Entity* entity) {
  GetForwardVector(entity);
  entity->position->x -= forward->x * entityTypes[entity->type].speed * DeltaTime * speedMultiplierBullet;
  entity->position->y -= forward->y * entityTypes[entity->type].speed * DeltaTime * speedMultiplierBullet;
  entity->position->z -= forward->z * entityTypes[entity->type].speed * DeltaTime * speedMultiplierBullet;
  direction->x = 0;
  direction->y = 0;
  direction->z = 0;
  // Check collision with wall
  if(isMovingEntityCollidingWithMap(entity, direction)==true){
    // Despawn bullet
    destroyEntity(entity->uid);
    return;
  }
  
  // Check collision with players
  Entity* collision = isCollidingWithEntityType(entity, direction, PLAYER_ENTITY_TYPE);
  //long shooterUid = floatToLong(entity->rotation->z);
  if(collision!=nullptr) {
    // Bullet is colliding with a player
    // Damage player
    collision->rotation->z--; // Hp is stored in rotation.z
    long uuid = getEntityOfClient(collision->uid);
    NetworkManager::Instance->sendStrictUpdateOfObject(uuid, collision);
    // Despawn bullet
    destroyEntity(entity->uid);
    return;
  }
}
// Computes the forward vector of an entity with its rotation
Vector3f* GetForwardVector(Entity* entity) {
  float yaw = entity->rotation->y;    // Convert to radians
  float pitch = entity->rotation->x;  // Convert to radians

  float x = mCos(yaw) * mCos(pitch);
  float y = mSin(pitch);
  float z = mSin(yaw) * mCos(pitch);

  forward->x = x;
  forward->y = y;
  forward->z = z;
  forward->normalize();  // Optional, for unit vector
  return forward;
}
// Computes the right vector
Vector3f* GetRightVector3f() {
  *right = *forward % VECTOR_UP;
  right->normalize();  // Optional, for unit vector
  return right;
}
void setDeltaTime(float f) {
  DeltaTime = f;
}
float getDeltaTime() {
  return DeltaTime;
}
