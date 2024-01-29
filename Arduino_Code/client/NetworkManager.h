#ifndef NETWORK_MANAGER_H
#define NETWORK_MANAGER_H

#define MSG_QUEUE_MAX_SIZE 20

#include <Arduino.h>
#include <Ethernet.h>
#include <PubSubClient.h>
#include "Entities.h"
#include "Vector3.h"
#include "Serializer.h"
#include "InputHandler.h"
#include "GameManager.h"

// Pending message struct
struct PendingMessage {
public:
  char* topic;
  byte* msg;
  int length;
};

class NetworkManager {
public:
  static NetworkManager* Instance;
  // Headers of messages
  const byte HEADER_INPUT = 1;
  const byte HEADER_INSTANTIATE_OBJECT = 2;
  const byte HEADER_ACK = 3;
  const byte HEADER_UPDATE_OBJECT = 4;
  const byte HEADER_CONFIRMED_JOIN_REQUEST = 5;
  const byte HEADER_REJECTED_JOIN_REQUEST = 6;
  const byte HEADER_REQUEST_JOIN_TO_SERVER = 7;
  const byte HEADER_PLAYER_SHOT = 8;
  const byte HEADER_NOTIFY_DEATH = 9;
  const byte HEADER_DESPAWN_ENTITY = 10;
  const byte HEADER_DISCONNECT_PC = 11;
  
  const byte HEADER_STRICT_UPDATE = 12;
  
  const char* SERVER_PENDING_MSG = "server";
  const char* broker_name = "broker.hivemq.com";
  const unsigned int broker_port = 1883;
  const uint16_t serverPort = 25565;
  bool paired;
  EthernetClient* tcpClient;
  EthernetClient ethClient;
  PubSubClient mqttClient;
  IPAddress myIp;
  long myUID;
  long myUUID = -1;
  PendingMessage* pendingMsg[MSG_QUEUE_MAX_SIZE];
  NetworkManager();
  void init(uint8_t boardId);
  // Handle acks
  void deletePendingMsg(byte index);
  byte firstFreeSlot();
  void resendPendingMsgs();
  void resetPendingMessages();
  bool isNull(PendingMessage* msg);
  // Send MQTT messages
  void sendMqttMessage(char* topic, byte* msg, int length, bool ack, bool freeMsg);
  void sendMqtt(const char* topic, const char* x);
  void sendMqttBytes(const char* topic, byte* x, int length);
  void sendMqttInstantiationOfObject(char* topic, Entity* entity);
  void sendMqttUpdateOfObject(char* topic, Entity* entity);
  void sendMqttAllInstances();
  // Send messages to server
  void sendServerMessage(byte* msg, int length, bool ack, bool freeMsg);
  void sendServerInstantiationOfObject(Entity* entity);
  void sendServerUpdateOfObject(Entity* entity);
  void sendShotToServer(Entity* entity);
  // Function to connect to mqtt
  void connectMqtt();
  void subscribeMqtt(char* topic);
  void loopMqtt();
  void unsubscribe(char* topic);

  // Function to connect to server with tcp
  void connectTCP();
  void setOwnUID(long uid);
  long getOwnUID();

  void setOwnUUID(long uuid);
  long getOwnUUID();
  EthernetClient* getTCPClient();
  char* topic_board_request;
  char* topic_board_confirm;
  char* topic_confirmation;
  char* pairTopicReceive;
  char* pairTopicSend;
  char* boardIdentifier;
  byte* mac;
};

#endif
