using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace doom_clone
{
    class MqttService
    {
        IMqttClient mqttClient;

        public async Task SubscribeToTopic(String topic)
        {
            Console.WriteLine("Subscribing to: " + topic);
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
        }
        public async Task Start(string brokerIp, String[] topics, Action<string, string> callbackPairing = null, Action<string, byte[]> callback = null)
        {
            MqttFactory factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            MqttClientOptions options = new MqttClientOptionsBuilder()
                .WithWebSocketServer(brokerIp)
                //.WithTcpServer(brokerIp)
                .Build();

            mqttClient.DisconnectedAsync += (async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await mqttClient.ConnectAsync(options, CancellationToken.None);
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });
            mqttClient.ApplicationMessageReceivedAsync += (async e =>
            {
#if DEBUG_MESSAGES
                Console.WriteLine("------------------------------------");
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {e.ApplicationMessage.ConvertPayloadToString()}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
#endif
                e.ApplicationMessage.PayloadFormatIndicator = MQTTnet.Protocol.MqttPayloadFormatIndicator.CharacterData;
                callbackPairing?.Invoke(e.ApplicationMessage.Topic, e.ApplicationMessage.ConvertPayloadToString());
                //byte[] b = e.ApplicationMessage.Payload;

                callback?.Invoke(e.ApplicationMessage.Topic, e.ApplicationMessage.Payload);
                var message = new MqttApplicationMessageBuilder()
                .WithTopic("/hello/world")
                .Build();

                Task.Run(() => mqttClient.PublishAsync(message, CancellationToken.None));


            });
            mqttClient.ConnectingAsync += async e =>
            {
                Console.WriteLine("### CONNECTING WITH SERVER...");
            };

            mqttClient.ConnectedAsync += (async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");
                for (int i = 0; i < topics.Length; i++)
                {
                    try
                    {
                        await SubscribeToTopic(topics[i]);
                        Console.WriteLine($"### SUBSCRIBED to {topics[i]} ###");
                    }
                    catch
                    {
                        Console.WriteLine($"### SUBSCRIBED FAILED to {topics[i]} ###");
                    }
                }

            });

            await mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        public async Task SendMessage(string topic, string payload)
        {
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload + "\0")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                //.WithRetainFlag()
                .Build();

#if DEBUG_MESSAGES
            Console.WriteLine("Publish MQTT message");
            Console.WriteLine($" - Topic: {message.Topic}");
            Console.WriteLine($" - Payload: {message.ConvertPayloadToString()}");
            Console.WriteLine("");
#endif


            await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async Task SendMessage(string topic, byte[] payload)
        {
            /*
            Console.Write("Sending message: ");
            for(int i = 0; i < payload.Length; i++)
            {
                Console.Write(payload[i] + ", ");
            }
            Console.WriteLine("");*/
            //payload[payload.Length - 1] = 0;
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                //.WithRetainFlag()
                .Build();

#if DEBUG_MESSAGES
            Console.WriteLine("Publish MQTT message");
            Console.WriteLine($" - Topic: {message.Topic}");
            Console.WriteLine($" - Payload: {message.ConvertPayloadToString()}");
            Console.WriteLine("");
#endif


            await mqttClient.PublishAsync(message, CancellationToken.None);
        }
    }
}
