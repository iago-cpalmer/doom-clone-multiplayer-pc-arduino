#include "Serializer.h"
#include "Vector3.h"
#include "NetworkManager.h"
#include "Entities.h"
#include "GameManager.h"
#include "MathUtil.h"
#define N_LIGHTS 4
uint8_t led_pins[] = { 21, 20, 19, 18 };
uint8_t potentiometer_pin = A13;

byte* dataBuffer = new byte[64];
NetworkManager* networkManager = nullptr;
long prevTime = 0;
void processMessages();

void setup() {
  Serial.begin(115200);
  delay(1000);
  // Prepare timer
 SREG = (SREG & 0b01111111); // Disable interruptions
 TIMSK2 = TIMSK2|0b00000001; // Enable interruption by overflow
 TCCR2B = 0b00000111; //7812.5Hz
 SREG = (SREG & 0b01111111) | 0b10000000; // Enable interruptions
 
  // Pins
  for (int i = 0; i < N_LIGHTS; i++) {
    pinMode(led_pins[i], OUTPUT);
    digitalWrite(led_pins[i], LOW);
  }

  initMathUtil();
  initEntities();
  initSerializer();
  initGameManager();

  new NetworkManager();
  networkManager = NetworkManager::Instance;
  networkManager->init();
}
// Handle timer interruption
ISR(TIMER2_OVF_vect) {
  float power = analogRead(potentiometer_pin);
  changeBulletSpeed(power / 1000);
}
void loop() {
  // Create ammo powerup in map
  Entity* entity = popEntityFromPool();
  setValuesToEntity(entity, AMMO_POWERUP_ENTITY_TYPE, 40, 10, 400, 0, 90, longToFloat(-1), 1, 1, 1);
  instantiateNewEntity(entity, -1);

  // Heart power up
  setValuesToEntity(entity, HEALTH_POWERUP_ENTITY_TYPE, 400, 10, 40, 0, 90, longToFloat(-1), 1, 1, 1);
  instantiateNewEntity(entity, -1);

  while (true) {
    // Save delta time
    long elapsed = (millis() - prevTime);
    setDeltaTime(elapsed / 1000.0f);
    prevTime = millis();
    // Main game loop
    processMessages();
    updateGameState();
    sendState();
  }
}
void processMessages() {
  // Process join requests (if any)
  EthernetClient newClient = networkManager->getServer()->accept();
  if (newClient) {
    Serial.println("Client requested to join");
    // Client trying to join
    long uuid = networkManager->addPlayer(newClient);
    //uuid=-1;
    if (uuid == -1) {
      // Cannot join, notify
      Serial.println("Client REJECTED");
      dataBuffer[0] = networkManager->HEADER_REJECTED_JOIN_REQUEST;
      dataBuffer[1] = 0;
      newClient.write(dataBuffer, 2);
      newClient.stop();
    } else {
      // get uid of instance
      Entity* entity = popEntityFromPool();
      setValuesToEntity(entity, 1, 5, 30, 21, 0, 90, getEntityType(1)->maxHealth - 1, getEntityType(1)->xscale, getEntityType(1)->yscale, 20);
      long uid = instantiateNewEntity(entity, uuid);  // Send instantiation to all clients except new client
      Serial.println(uid);
      entity->uid = uid;
      networkManager->getClient(uuid)->entityUid = uid;
      dataBuffer[0] = networkManager->HEADER_CONFIRMED_JOIN_REQUEST;
      serializeLong(dataBuffer, uuid, 2);
      serializeEntity(dataBuffer, entity, 6);
      networkManager->sendMessage(uuid, dataBuffer, 47, true, false);  // Send message to client
      // Send all the rest of entities to the client
      sendAllEntitiesToClient(uuid);

      Serial.print("Client joined with uuid: ");
      Serial.println(uuid);
      // Change light
      digitalWrite(led_pins[uuid], HIGH);
    }
  }

  // Handle client messages
  for (int i = 0; i < networkManager->getMaxClients(); i++) {
    // process that client's data
    if (networkManager->getClient(i)->uuid != -1 && networkManager->getClient(i)->client.connected() == true) {
      processClientMessages(&networkManager->getClient(i)->client);
    }
  }

  // Process desconnections of clients
  for (int i = 0; i < networkManager->getMaxClients(); i++) {
    // process that client's data
    if (networkManager->getClient(i)->uuid != -1 && networkManager->getClient(i)->client.connected() == false) {
      // Change light
      digitalWrite(led_pins[networkManager->getClient(i)->uuid], LOW);

      Serial.print("Disconneting client with uuid: ");
      Serial.println(networkManager->getClient(i)->uuid);
      // Disconnect client and remove from list
      networkManager->getClient(i)->client.stop();
      destroyEntity(networkManager->getClient(i)->entityUid);
      networkManager->removePlayer(networkManager->getClient(i));
    }
  }
}

void processClientMessages(EthernetClient* client) {
  // Check if the client sent a message
  if (client->available()) {
    byte header;  // Reads the first byte. Header
    byte ack;     // Gets if ack required
    client->readBytes(&header, 1);
    client->readBytes(&ack, 1);
    if (header != networkManager->HEADER_ACK && ack > 0) {
      // Send back the ACK
      dataBuffer[0] = networkManager->HEADER_ACK;
      dataBuffer[1] = ack;
      client->write(dataBuffer, 2);
    }
    if (header == networkManager->HEADER_UPDATE_OBJECT) {
      // Message of update of entity received
      client->readBytes(dataBuffer, 41);
      Entity* entity = deserializeEntity(dataBuffer, 0);
      updateEntityValues(entity);
    } else if (header == networkManager->HEADER_ACK) {
      // Received ack, process it
      networkManager->deletePendingMsg(ack);
    } else if (header == networkManager->HEADER_PLAYER_SHOT) {
      // A player has shot.
      // Read entity info. sent by client (the bullet)
      client->readBytes(dataBuffer, 41);
      Entity* entity = deserializeEntity(dataBuffer, 0);
      entity->type = PLAYER_BULLET_ENTITY_TYPE;
      // Instantiate new entity of type bullet
      entity->scale->x = getEntityType(entity->type)->xscale;
      entity->scale->y = getEntityType(entity->type)->yscale;
      instantiateNewEntity(entity, -1);
    }
  }
}
