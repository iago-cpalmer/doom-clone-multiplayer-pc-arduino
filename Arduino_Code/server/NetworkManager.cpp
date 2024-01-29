#include "Ethernet.h"
#include "NetworkManager.h"

#define MAX_CHAR_BUFFER_SIZE 64
NetworkManager* NetworkManager::Instance = nullptr;
byte mac[] = { 0xDE, 0xAD, 0xBE, 0xEF, 0xFE, 0xFF };
byte* data = new byte[MAX_CHAR_BUFFER_SIZE];
IPAddress serverIp(192, 168, 1, 126);
EthernetServer server(25565);

NetworkManager::NetworkManager() {
  if (NetworkManager::Instance == nullptr) {
    NetworkManager::Instance = this;
  } else {
    return;
  }
}

long NetworkManager::getMaxClients() {
  return MAX_PLAYERS;
}

void NetworkManager::init() {
  Ethernet.begin(mac, serverIp);
  server.begin();
  Serial.println();
  Serial.print("Server connected at: ");
  Serial.println(Ethernet.localIP());
  // Empty all previous messages sent to server (there shouldn't be any, just in case)
  server.flush();
  // Initialize pending messages array
  for (int i = 0; i < MSG_QUEUE_MAX_SIZE; i++) {
    pendingMsg[i] = malloc(sizeof(PendingMessage));
    pendingMsg[i]->uuid = -1;
    pendingMsg[i]->msg = new char[MAX_CHAR_BUFFER_SIZE];
    pendingMsg[i]->length = -1;
  }
  // Initialize clients array
  for (int i = 0; i < MAX_PLAYERS; i++) {
    clients[i].uuid = -1;
  }
}

// Delete a message from "queue" of msg with ack required
void NetworkManager::deletePendingMsg(byte index) {
  index--;
  pendingMsg[index]->uuid = -1;
  pendingMsg[index]->length = -1;
}
// Gets the index of the first free slot in the "queue" of messages to ack
byte NetworkManager::firstFreeSlot() {
  for (byte i = 0; i < MSG_QUEUE_MAX_SIZE; i++) {
    if (isNull(pendingMsg[i])) {
      return i + 1;
    }
  }
  return 0;
}
// Check if a PendingMessage is null (empty)
bool NetworkManager::isNull(PendingMessage* msg) {
  return msg->uuid == -1 && msg->length == -1;
}
// Attempts to add a player to the client array
// Returns the uuid of the new client or -1 if server is full
long NetworkManager::addPlayer(EthernetClient client) {
  long uuid = firstFreeUUID();
  if (uuid == -1) {
    return -1;
  }
  // Player can be added
  clients[uuid].client = client;
  clients[uuid].uuid = uuid;
  return uuid;
}
// Returns the first free slot/uuid for a client instance
long NetworkManager::firstFreeUUID() {
  for (int i = 0; i < MAX_PLAYERS; i++) {
    // Check whether that uuid is -1 or the client is not connected
    if (clients[i].uuid == -1 || clients[i].client.connected() == false) {
      return i;
    }
  }
  return -1;
}
// Removes a Client/Player instance
void NetworkManager::removePlayer(MyClient* client) {
  client->uuid = -1;
  client->entityUid = -1;
}

MyClient* NetworkManager::getClient(long uuid) {
  return &clients[uuid];
}
// Resend all pending messages that require ack
void NetworkManager::resendPendingMsgs() {
  for (int i = 0; i < MSG_QUEUE_MAX_SIZE; i++) {
    if (isNull(pendingMsg[i])==false) {
      // Send message to all clients
      if (pendingMsg[i]->uuid == -1) {
        for (int i = 0; i < MAX_PLAYERS; i++) {
        if (clients[i].uuid != -1) {
          clients[pendingMsg[i]->uuid].client.write(pendingMsg[i]->msg, pendingMsg[i]->length);
        }
      }
      } else if (clients[pendingMsg[i]->uuid].uuid != -1) {
        // Send message to a specific client
        clients[pendingMsg[i]->uuid].client.write(pendingMsg[i]->msg, pendingMsg[i]->length);
      }
    }
  }
}
// Sends a message or adds it to the pending message "queue"
void NetworkManager::sendMessage(long uuid, byte* msg, int length, bool ack, bool freeMsg) {
  if (uuid!=-1 && clients[uuid].uuid == -1) {
    return;
  }
  // Check if it requires ack
  if (!ack) {
    msg[1] = 0;  // ensure is no ack
    if (uuid == -1) {
      // Send it to all clients
      for (int i = 0; i < MAX_PLAYERS; i++) {
        if (clients[i].uuid != -1) {
          clients[uuid].client.write(msg, length);
        }
      }

    } else {
      // Send it to the specified client
      clients[uuid].client.write(msg, length);
    }
    return;
  }
  // It does requiere ack
  byte slot = firstFreeSlot();
  if (slot == 0) {
    return;
  }
  pendingMsg[slot - 1]->uuid = uuid;
  memcpy(pendingMsg[slot-1]->msg, msg, length);
  pendingMsg[slot - 1]->length = length;
  // set ack id message
  pendingMsg[slot - 1]->msg[1] = slot;
}
// Sends message to all connected clients except the specified uuid in arguments. If uuid=-1, message send to all
void NetworkManager::sendMessageExcept(long uuid, byte* data, int length, bool ack, bool freeMsg) {
  for (int i = 0; i < MAX_PLAYERS; i++) {
    if (i != uuid && clients[i].uuid!=-1) {
      sendMessage(i, data, length, ack, freeMsg);
    }
  }
}
// Send an instantiation of object message to all except to the specified client
void NetworkManager::sendInstantiationOfObject(long uuid, Entity* entity) {
  data[0] = HEADER_INSTANTIATE_OBJECT;
  serializeEntity(data, entity, 2);
  sendMessageExcept(uuid, data, 43, true, false);
}
// Send an despawn entity message to all except to the specified client
void NetworkManager::sendDespawnEntity(long uuid, long uid) {
  data[0] = HEADER_DESPAWN_ENTITY;
  serializeLong(data, uid, 2);
  sendMessageExcept(uuid, data, 6, true, false);
}
// Send an instantiation of object message to a specified client
void NetworkManager::sendInstantiationOfObjectToClient(long uuid, Entity* entity) {
  data[0] = HEADER_INSTANTIATE_OBJECT;
  serializeEntity(data, entity, 2);
  sendMessage(uuid, data, 43, true, false);
}
// Send an update entity message to all except to the specified client
void NetworkManager::sendUpdateOfObject(long uuid, Entity* entity) {
  data[0] = HEADER_UPDATE_OBJECT;
  serializeEntity(data, entity, 2);
  sendMessageExcept(uuid, data, 43, false, false);
}
// Send a "strict" update entity message to all except to the specified client
// Strict update: clients will only ignore updates of their own player.
// An strict update will not be ignored by the client
void NetworkManager::sendStrictUpdateOfObject(long uuid, Entity* entity) {
  data[0] = HEADER_STRICT_UPDATE;
  serializeEntity(data, entity, 2);
  sendMessage(uuid, data, 43, true, false);
}
// Send a notification of death to a specific client
void NetworkManager::sendNotifyDeath(long uuid, Entity* entity) {
  data[0] = HEADER_NOTIFY_DEATH;
  serializeEntity(data, entity, 2);
  sendMessage(uuid, data, 43, true, false);
}

EthernetServer* NetworkManager::getServer() {
  return &server;
}
