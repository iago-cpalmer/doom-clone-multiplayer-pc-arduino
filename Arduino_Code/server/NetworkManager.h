#ifndef NETWORK_MANAGER_H
#define NETWORK_MANAGER_H

#include <Arduino.h>
#include <Ethernet.h>
#include "Entities.h"
#include "Vector3.h"
#include "Serializer.h"
#include "MathUtil.h"

// Struct to save Client instance
struct MyClient {
  EthernetClient client;
  long uuid;
  long entityUid;
};

// Pending message struct (for messages with ack required)
struct PendingMessage {
  long uuid;
  byte* msg;
  int length;
};

class NetworkManager {
public:
  static NetworkManager* Instance;

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
  const byte HEADER_STRICT_UPDATE = 12;

  const byte MAX_PLAYERS = 4;
  MyClient* clients = new MyClient[MAX_PLAYERS];
  const byte MSG_QUEUE_MAX_SIZE = 20;
  PendingMessage* pendingMsg[20];
  NetworkManager();
  void init();
  // Handle acks
  void deletePendingMsg(byte index);
  byte firstFreeSlot();
  void resendPendingMsgs();
  bool isNull(PendingMessage* msg);

  // Handle clients
  long addPlayer(EthernetClient client);
  long firstFreeUUID();
  void removePlayer(MyClient* client);
  MyClient* getClient(long uuid);
  long getMaxClients();
  
  // Send messages to clients
  void sendMessage(long uuid, byte* msg, int length, bool ack, bool freeMsg);
  void sendMessageExcept(long uuid, byte* data, int length, bool ack, bool freeMsg);
  void sendInstantiationOfObject(long uuid, Entity* entity);
  void sendInstantiationOfObjectToClient(long uuid, Entity* entity);
  void sendUpdateOfObject(long uuid, Entity* entity);
  void sendStrictUpdateOfObject(long uuid, Entity* entity);
  void sendNotifyDeath(long uuid, Entity* entity);
  void sendDespawnEntity(long uuid, long uid);
  EthernetServer* getServer();
};

#endif