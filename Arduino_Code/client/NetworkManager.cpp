#include "PubSubClient.h"
#include "NetworkManager.h"
#include "MathUtil.h"
#define MAX_CHAR_BUFFER_SIZE 64
NetworkManager* NetworkManager::Instance = nullptr;
IPAddress serverIp(192, 168, 1, 126);
byte* data = new byte[MAX_CHAR_BUFFER_SIZE];
NetworkManager::NetworkManager() {
  if (NetworkManager::Instance == nullptr) {
    NetworkManager::Instance = this;
  } else {
    return;
  }
  mac = new byte[6]{ 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0x00 };
}
void NetworkManager::init(uint8_t boardId) {
  boardIdentifier[5] = (char)boardId + '0';
  mac[5] = boardId;
  mqttClient = PubSubClient(ethClient);
  Ethernet.begin(mac);
  mqttClient.setServer(broker_name, broker_port);
  paired = false;
  for (int i = 0; i < MSG_QUEUE_MAX_SIZE; i++) {
    pendingMsg[i] = new PendingMessage();
    pendingMsg[i]->topic;
    pendingMsg[i]->msg = new char[MAX_CHAR_BUFFER_SIZE];
    pendingMsg[i]->length = -1;
  }
  setOwnUID(-1);
  setOwnUUID(-1);
}

void NetworkManager::unsubscribe(char* topic) {
  mqttClient.unsubscribe(topic);
}

// Send a message to the pc through mqtt
void NetworkManager::sendMqttMessage(char* topic, byte* msg, int length, bool ack, bool freeMsg) {
  // Check if it requires ack
  if (!ack) {
    msg[1] = 0;  // ensure is no ack
    sendMqttBytes(topic, msg, length);
    return;
  }
  // It does requiere ack
  byte slot = firstFreeSlot();
  if (slot == 0) {
    return;
  }

  pendingMsg[slot - 1]->topic = topic;
  memcpy(pendingMsg[slot - 1]->msg, msg, length);
  pendingMsg[slot - 1]->length = length;
  // set ack id message
  pendingMsg[slot - 1]->msg[1] = slot;
}
// Delete a pending message from "queue"
void NetworkManager::deletePendingMsg(byte index) {
  index--;
  pendingMsg[index]->length = -1;
}
// Get the first free slot in the "queue" of pending msg's
byte NetworkManager::firstFreeSlot() {
  for (byte i = 1; i < MSG_QUEUE_MAX_SIZE; i++) {
    if (isNull(pendingMsg[i])) {
      return i + 1;
    }
  }
  return 0;
}
// Returns whether a pending message is empty/null
bool NetworkManager::isNull(PendingMessage* msg) {
  return msg->length == -1;
}
// Resets the queue of pending messages
void NetworkManager::resetPendingMessages() {
  for (int i = 1; i < MSG_QUEUE_MAX_SIZE; i++) {
    deletePendingMsg(i);
  }
}
// Resends all pending messages
void NetworkManager::resendPendingMsgs() {
  for (int i = 1; i < MSG_QUEUE_MAX_SIZE; i++) {
    if (!isNull(pendingMsg[i])) {
      if (strncmp(pendingMsg[i]->topic, SERVER_PENDING_MSG, 6) == 0) {
        tcpClient->write(pendingMsg[i]->msg, pendingMsg[i]->length);
      } else {
        // Send to mqtt
        sendMqttBytes(pendingMsg[i]->topic, pendingMsg[i]->msg, pendingMsg[i]->length);
      }
    }
  }
}

// Send mqtt message
void NetworkManager::sendMqtt(const char* topic, const char* x) {
  mqttClient.publish(topic, x);
}
// Send a specific amount of bytes
void NetworkManager::sendMqttBytes(const char* topic, byte* x, int length) {
  mqttClient.publish(topic, x, length);
}
// Send an instantiation message through mqtt
void NetworkManager::sendMqttInstantiationOfObject(char* topic, Entity* entity) {
  data[0] = NetworkManager::Instance->HEADER_INSTANTIATE_OBJECT;
  serializeEntity(data, entity, 2);
  sendMqttMessage(topic, data, 43, true, false);
}
// Send all entities to the pc
void NetworkManager::sendMqttAllInstances() {
  for(int i = 0; i < getMaxEntities(); i++) {
    sendMqttInstantiationOfObject(pairTopicSend, getEntity(i));
  } 
}
// Send an update of object through mqtt
void NetworkManager::sendMqttUpdateOfObject(char* topic, Entity* entity) {
  data[0] = NetworkManager::Instance->HEADER_UPDATE_OBJECT;
  serializeEntity(data, entity, 2);
  sendMqttMessage(topic, data, 43, false, false);
}
// Send a message to the server
void NetworkManager::sendServerMessage(byte* msg, int length, bool ack, bool freeMsg) {
  // Check if it requires ack
  if (!ack) {
    msg[1] = 0;  // ensure is no ack
    if (getOwnUUID() != -1) {
      tcpClient->write(msg, length);
    }
    return;
  }
  // It does requiere ack
  byte slot = firstFreeSlot();
  if (slot == 0) {
    return;
  }
  pendingMsg[slot - 1]->topic = SERVER_PENDING_MSG;
  memcpy(pendingMsg[slot - 1]->msg, msg, length);
  pendingMsg[slot - 1]->length = length;
  pendingMsg[slot - 1]->msg[1] = slot;
}
// Send instantation to server
void NetworkManager::sendServerInstantiationOfObject(Entity* entity) {
  data[0] = NetworkManager::Instance->HEADER_INSTANTIATE_OBJECT;
  serializeEntity(data, entity, 2);
  sendServerMessage(data, 43, true, false);
}
// Send update of object to server
void NetworkManager::sendServerUpdateOfObject(Entity* entity) {
  data[0] = NetworkManager::Instance->HEADER_UPDATE_OBJECT;
  serializeEntity(data, entity, 2);
  sendServerMessage(data, 43, false, false);
}
// Send the shot action to server 
void NetworkManager::sendShotToServer(Entity* entity) {
  data[0] = NetworkManager::Instance->HEADER_PLAYER_SHOT;
  serializeEntity(data, entity, 2);
  sendServerMessage(data, 43, true, false);
}

void NetworkManager::connectMqtt() {
  bool connection_successful;
  // Loop until we're reconnected
  while (!mqttClient.connected()) {
    // TODO: WHILE ATTEMPTING TO CONNECT, RGB LIGHT WILL BE BLINKING IN ORANGE
    Serial.println("Attempting MQTT connection ...");
    // Attempt to connect
    connection_successful = mqttClient.connect(boardIdentifier);
    if (connection_successful) {
      Serial.println("connected");
    } else {
      Serial.print("failed, rc=");
      Serial.print(mqttClient.state());
      Serial.println(" try again in 2 seconds");
      delay(2000);
    }
  }
}
void NetworkManager::connectTCP() {
  int connection_status;
  tcpClient = new EthernetClient();
  while (!tcpClient->connected()) {
    // Attempt to connect
    connection_status = tcpClient->connect(serverIp, serverPort);
    if (connection_status) {
      Serial.println("Connected to server");
    } else {
      Serial.print("failed");
      Serial.println(" try again in 2 seconds");
      delay(2000);
    }
  }
}

void NetworkManager::subscribeMqtt(char* topic) {
  mqttClient.subscribe(topic, 1);
}

void NetworkManager::loopMqtt() {
  mqttClient.loop();
}

void NetworkManager::setOwnUID(long uid) {
  myUID = uid;
}
long NetworkManager::getOwnUID() {
  return myUID;
}

void NetworkManager::setOwnUUID(long uuid) {
  myUUID = uuid;
}
long NetworkManager::getOwnUUID() {
  return myUUID;
}

EthernetClient* NetworkManager::getTCPClient() {
  return tcpClient;
}
