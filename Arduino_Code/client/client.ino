#include "Serializer.h"
#include "Vector3.h"
#include "InputHandler.h"
#include "NetworkManager.h"
#include "Entities.h"
#include "GameManager.h"
#include "MathUtil.h"
// Constants for pins (for unique identifiers to get a unique mac address)
const uint8_t N_ID_PINS = 4;
const uint8_t identifierPins[] = { 31, 32, 33, 34 };
unsigned long prevTime = 0;
byte* dataBuffer = new byte[64];
NetworkManager* networkManager = nullptr;
const char topic_board_request[] = "/doomclone/boardrequests";  // Topic to send request
const char topic_board_confirm[] = "/doomclone/boardconfirm";   // Topic to send confirmation to a pc
const char topic_confirmation[] = "/doomclone/pcconfirm";       // Topic to subscribe to wait for confirmation
char pairTopicReceive[32] = "/doomclone/";
char pairTopicSend[32] = "/doomclone/";
char boardIdentifier[] = "boardx\0";
bool gotResponseFromServer = false;
// Functions
int setupId();
void pair();
void callbackMqtt(char* topic, byte* payload, unsigned int length);
void processServerMessages();
/******************************************************************************/
/** SETUP *********************************************************************/
/******************************************************************************/
int setupId() {
  // Set up board id
  uint8_t boardId = 0;
  for (int i = 0; i < N_ID_PINS; i++) {
    pinMode(identifierPins[i], INPUT);
    uint8_t state = digitalRead(identifierPins[i]);
    boardId = (boardId << 1) + state;
  }
  return boardId;
}
void setup() {
  Serial.begin(115200);
  delay(1000);
  Serial.println("Initializing...");
  initEntities();
  initGameManager();
  initMathUtil();
  initSerializer();
  new NetworkManager();
  networkManager = NetworkManager::Instance;
  networkManager->topic_board_confirm = topic_board_confirm;
  networkManager->topic_board_request = topic_board_request;
  networkManager->topic_confirmation = topic_confirmation;
  networkManager->pairTopicReceive = pairTopicReceive;
  networkManager->pairTopicSend = pairTopicSend;
  networkManager->boardIdentifier = boardIdentifier;
  networkManager->init(setupId());
  networkManager->mqttClient.setCallback(callbackMqtt);
  Serial.println("Initialization done!");
}


/******************************************************************************/
/** LOOP **********************************************************************/
/******************************************************************************/
void loop() {
  // Pair with computer
  pair();
  // Connect to server
  Serial.println("Connecting to server...");
  networkManager->connectTCP();
  // Main loop
  while (true) {
    long elapsed = (millis() - prevTime);
    setDeltaTime(elapsed / 1000.0f);
    prevTime = millis();
    if(networkManager->paired==false) {
      pair();
      return;
    }
    // Call regularly to maintain connection with the server
    // and process incoming messages
    networkManager->loopMqtt();
    // Main loop
    processServerMessages();
    updateGameState();
    sendState();
  }
}

void processServerMessages() {
  EthernetClient* tcpClient = networkManager->getTCPClient();
  if (tcpClient->available() >= 2) {
    // There is a message from the server
    byte header;  // Reads the first byte. Header
    byte ack;     // Gets if ack required
    tcpClient->readBytes(&header, 1);
    tcpClient->readBytes(&ack, 1);

    if (header != networkManager->HEADER_ACK && ack > 0) {
      // Ack required, send ack
      dataBuffer[0] = networkManager->HEADER_ACK;
      dataBuffer[1] = ack;
      tcpClient->write(dataBuffer, 2);
    }
    if (header == networkManager->HEADER_ACK) {
      // Received ack, process it
      networkManager->deletePendingMsg(ack);
    } else if (header == networkManager->HEADER_INSTANTIATE_OBJECT) {
      // An object must be instantiated, read data (41B in total)
      tcpClient->readBytes(dataBuffer, 41);
      Entity* entity = deserializeEntity(dataBuffer, 0);
      if (entity->uid != networkManager->getOwnUID()) {
        instantiateEntity(entity);
      }
    } else if (header == networkManager->HEADER_UPDATE_OBJECT) {
      // An object must be updated, read data (41B in total)
      tcpClient->readBytes(dataBuffer, 41);
      Entity* entity = deserializeEntity(dataBuffer, 0);
      if (entity->uid != networkManager->getOwnUID()) {
        updateEntityValues(entity);
      }
    } else if (header == networkManager->HEADER_CONFIRMED_JOIN_REQUEST) {
      // Confirmed the join request, store uuid and entity
      tcpClient->readBytes(dataBuffer, 45);
      if (!gotResponseFromServer) {
        gotResponseFromServer = true;
        long uuid = deserializeLong(dataBuffer, 0);

        Serial.print("JOIN REQUEST ACCEPTED, own client uuid: ");
        Serial.println(uuid);

        long uid = deserializeLong(dataBuffer, 4);
        Entity* entity = deserializeEntity(dataBuffer, 4);
        entity->type = 0;
        networkManager->setOwnUUID(uuid);
        networkManager->setOwnUID(entity->uid);
        instantiateEntity(entity);
      }

    } else if (header == networkManager->HEADER_REJECTED_JOIN_REQUEST) {
      Serial.println("CLIENT GOT REJECTED. Diconnecting...");
      gotResponseFromServer = true;
      // Cannot join the server, disconnect
      tcpClient->stop();
    } else if (header == networkManager->HEADER_DESPAWN_ENTITY) {
      tcpClient->readBytes(dataBuffer, 4);
      // ENTITY MUST BE UNSPAWNED
      long uid = deserializeLong(dataBuffer, 0);
      destroyEntity(uid);
      // Send message to pc to notify despawn
      dataBuffer[0] = networkManager->HEADER_DESPAWN_ENTITY;
      serializeLongReversed(dataBuffer, uid, 2);
      networkManager->sendMqttMessage(pairTopicSend, dataBuffer, 6, true, false);
    } else if (header == networkManager->HEADER_NOTIFY_DEATH) {
      // PLAYER HAS DIED
      tcpClient->readBytes(dataBuffer, 41);
      Entity* entity = deserializeEntity(dataBuffer, 0);
      updateEntityValues(entity);
      // Notify pc
      dataBuffer[0] = networkManager->HEADER_NOTIFY_DEATH;
      networkManager->sendMqttMessage(pairTopicSend, dataBuffer, 2, true, false);
    } else if(header == networkManager->HEADER_STRICT_UPDATE) {
      // Strict update received
      tcpClient->readBytes(dataBuffer, 41);
      Entity* entity = deserializeEntity(dataBuffer, 0);
      updateEntityValues(entity);
    }
  } else {
    // Check if client has been disconnected from server
    if (networkManager->tcpClient->connected() == false) {
      Serial.println("Client has been disconnected from the server. Please, reset in order to attempt connection again...");
      initGameManager();
    }
  }
}

void pair() {
  networkManager->connectMqtt();
  networkManager->subscribeMqtt(topic_confirmation);
  Serial.println("Trying to pair with a pc.");
  while (networkManager->paired == false) {
    // TODO: WHILE ATTEMPTING TO CONNECT TO, RGB LIGHT WILL BE BLINKING IN GREEN
    // Try to pair, send request
    networkManager->sendMqtt(topic_board_request, boardIdentifier);

    networkManager->loopMqtt();
    delay(100);
  }
}


// Callback function for mqtt messages (from game engine in pc)
void callbackMqtt(char* topic, byte* payload, unsigned int length) {
  if (strncmp(topic, networkManager->topic_confirmation, sizeof(topic)) == 0 && networkManager->paired == false) {
    // Get arduino id that got confirmation
    int i = 0;
    while (payload[i] != '_') {
      i++;
    }
    // Check if it's the same
    if (strncmp(payload, networkManager->boardIdentifier, i) == 0) {
      // Got confirmation from pc
      networkManager->paired = true;
      // Send confirmation
      strcat(networkManager->pairTopicReceive, payload);
      networkManager->sendMqtt(networkManager->topic_board_confirm, networkManager->pairTopicReceive);
      strcpy(networkManager->pairTopicSend, networkManager->pairTopicReceive);
      strcat(networkManager->pairTopicSend, "/pc");
      strcat(networkManager->pairTopicReceive, "/board");
      // Subscribe to new topic for this specific pair
      networkManager->mqttClient.subscribe(networkManager->pairTopicReceive);
      
      if(gotResponseFromServer) {
        // Send all entities instances
        networkManager->sendMqttAllInstances();
      }
    }
  } else if (strncmp(topic, networkManager->pairTopicReceive, sizeof(topic)) == 0) {
    if (payload[0] == networkManager->HEADER_INPUT) {
      deserializeInput(payload, 2);
    } else if (payload[0] == networkManager->HEADER_ACK) {
      byte ackedMsg = payload[1];
      networkManager->deletePendingMsg(ackedMsg);
    } else if (payload[0] == networkManager->HEADER_DISCONNECT_PC) {
      // pc has been disconnected
      networkManager->unsubscribe(networkManager->pairTopicReceive);
      networkManager->paired = false;
      strcpy(networkManager->pairTopicSend, "/doomclone/");
      strcpy(networkManager->pairTopicReceive, "/doomclone/");
      networkManager->resetPendingMessages();
    }
  }
}
