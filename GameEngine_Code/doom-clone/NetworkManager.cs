using doom_clone.GameObjects;
using doom_clone.Utils;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace doom_clone
{   
    // Class to handle all messages from the mqtt service.
    // Will update the state of the NetworkObjects in GameManager
    class NetworkManager
    {
        public static NetworkManager Instance { get { return _instance; } }
        private static NetworkManager _instance;
        // Dictionary of type (of GameObject (e.g.: 0 - Main player, 1 - Other player, 2 - Enemy...) with a prefab
        private ConcurrentDictionary<int, GameObject> _gameObjectTypes;
        private const String brokerWS = "broker.hivemq.com:8000/mqtt";
        private const String brokerTCP = "broker.hivemq.com";
        private String[] TOPICS_SUBSCRIBE = { "/doomclone/boardrequests", "/doomclone/boardconfirm" };
        private const String TOPIC_CONFIRM = "/doomclone/pcconfirm";
        private String topicToReceive;
        private String topicToSend;
        private bool _paired = false;
        private bool _connected = false;
        private MqttService _mqttService;
        private String myId;

        // Header of messages constants
        private const byte HEADER_SEND_INPUT = 1;   // Header to send input to board
        private const byte HEADER_INSTANTIATE_OBJECT = 2; // Request to instantiate an object.  HEADER(CHAR - 1)ACK(1)_UID(INT-4)_POS(VEC3 - 12)_ROT(VEC3 - 12)_SCALE(VEC3 - 12)_TYPE(BYTE - 1)
        private const byte HEADER_ACK = 3;   // Header to send ACK
        private const byte HEADER_UPDATE_OBJECT = 4;   // Header to receive update of an object, HEADER(1),ACK(1),UID(4),POS(12),ROT(12),SCALE(12)
        private const byte HEADER_NOTIFY_DEATH = 9;
        private const byte HEADER_DESPAWN_ENTITY = 10;
        private const byte HEADER_DISCONNECT = 11;
        // Network ticks
        private const int TICKS_PER_SECOND = 10;
        private const float TIME_PER_TICK = 1.0f/TICKS_PER_SECOND;
        private float timerCountForTick = 0.0f;

        public NetworkManager(String id)
        {
            if(_instance==null)
            {
                _instance = this;
            } else
            {
                return;
            }
            myId = id;
            _gameObjectTypes = new ConcurrentDictionary<int, GameObject>();
            _gameObjectTypes.TryAdd(0, Prefabs.MAIN_PLAYER_PREFAB);
            _gameObjectTypes.TryAdd(1, Prefabs.PLAYER_PREFAB);
            _gameObjectTypes.TryAdd(2, Prefabs.PLAYER_PREFAB);
            _gameObjectTypes.TryAdd(3, Prefabs.BULLET_PREFAB);
            _gameObjectTypes.TryAdd(4, Prefabs.BULLET_POWERUP_PREFAB);
            _gameObjectTypes.TryAdd(5, Prefabs.HEALTH_POWERUP_PREFAB);
            _mqttService = new MqttService();
            Init();

        }
        public async Task Init()
        {
            await _mqttService.Start(brokerWS, TOPICS_SUBSCRIBE, HandlePairingMessage, HandleMessage);
        }

        public void Update()
        {
            // Check if a network tick should be executed
            timerCountForTick += Window.DeltaTime;
            if (timerCountForTick >= TIME_PER_TICK)
            {
                // Tick
                Tick();
                timerCountForTick = 0;
            }
        }
        private void Tick()
        {
            SendInputState();
        }

        public void SendInputState()
        {
            byte[] data = new byte[15]; // Header: 1B, ACK, Mouse State: 1B, KeyStates: 4B, Pitch: 4B, Yaw: 4B = 14B
            data[0] = HEADER_SEND_INPUT;
            data[1] = 0;
            data[2] = InputHandler.MouseState;
            Serializer.Serialize(ref data, InputHandler.KeyStates, 3);
            Serializer.Serialize(ref data, GameManager.Instance.GetCurrentActiveCamera().Pitch, 7);
            Serializer.Serialize(ref data, GameManager.Instance.GetCurrentActiveCamera().Yaw, 11);
            _mqttService.SendMessage(topicToSend, data);
        }

        public void SendDisconnection()
        {
            byte[] data = new byte[2];
            data[0] = HEADER_DISCONNECT;
            data[1] = 0;
            _mqttService.SendMessage(topicToSend, data);
        }

        public void HandlePairingMessage(String topic, String msg)
        {
            if (topic == TOPICS_SUBSCRIBE[0] && !_connected)
            {
                Console.WriteLine("Subscription request received from: " + msg);
                Console.WriteLine("Sending confirmation...");
                _mqttService.SendMessage(TOPIC_CONFIRM, msg + "_" + myId);
                _connected = true;
            }
            else if (topic == TOPICS_SUBSCRIBE[1] && !_paired)
            {
                if (msg.Contains(myId))
                {
                    Console.WriteLine("CONFIRMED Subscription in: " + msg);
                    _paired = true;
                    topicToReceive = msg + "/pc";
                    _mqttService.SubscribeToTopic(topicToReceive);
                    topicToSend = msg + "/board";
                }
            }
        }

        public void HandleMessage(String topic, byte[] msg)
        {
            if (topic == topicToReceive)
            {
                // We received a message from the board updating the state of a game object
                if (msg[1]>0)
                {
                    byte x = msg[1];
                    // Message to ack
                    byte[] ackMsg = { HEADER_ACK, x};
                    // Send ack
                    _mqttService.SendMessage(topicToSend, ackMsg);
                }
                if(msg[0] == HEADER_INSTANTIATE_OBJECT)
                {
                    int uid = Serializer.DeserializeInt(msg, 2);
                    byte type = msg[6];
                    Vector3 pos = Serializer.DeserializeVector3(msg, 7);
                    Vector3 rot = new Vector3(Serializer.DeserializeVector2(msg, 19));
                    Vector3 sca = new Vector3(Serializer.DeserializeVector2(msg, 27));
                    
                    GameObject gameObject = _gameObjectTypes[type].Clone();
                    GameManager.Instance.RegisterNetworkObject(uid, gameObject, "InGame");
                    GameManager.Instance.UpdateNetworkObject(uid, pos, rot, sca);
                    if (type == 0)
                    {
                        ((InGameHandler)GameManager.Instance.GetScene("InGame").FindGameObject("InGameHandler")).SetPlayer(gameObject);
                    }
                } else if(msg[0] == HEADER_UPDATE_OBJECT)
                {
                    int uid = Serializer.DeserializeInt(msg, 2);
                    byte type = msg[6];
                    Vector3 pos = Serializer.DeserializeVector3(msg, 7);
                    Vector3 rot = Serializer.DeserializeVector3(msg, 19);
                    Vector3 sca = Serializer.DeserializeVector3(msg, 31);
                    GameManager.Instance.UpdateNetworkObject(uid, pos, rot, sca);
                } else if(msg[0] == HEADER_DESPAWN_ENTITY)
                {
                    int uid = Serializer.DeserializeInt(msg, 2);
                    GameManager.Instance.UnregisterNetworkObject(uid);
                } else if(msg[0] == HEADER_NOTIFY_DEATH)
                {
                    GameManager.Instance.SetActiveScene("DeathScene");
                }
            } 
            
        }
    }
}
