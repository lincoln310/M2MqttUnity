/*
The MIT License (MIT)

Copyright (c) 2018 Giovanni Paolo Vigano'

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Unitter;
using Unity.UIWidgets.Redux;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

/// <summary>
/// Adaptation for Unity of the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// modified to run on UWP (also tested on Microsoft HoloLens).
/// </summary>
namespace M2MqttUnity
{
    /// <summary>
    /// Generic MonoBehavior wrapping a MQTT client, using a double buffer to postpone message processing in the main thread. 
    /// </summary>
    public class M2MqttClient 
    {
        [Header("MQTT broker configuration")]
        [Tooltip("IP address or URL of the host running the broker")]
        public string brokerAddress = "localhost";
        [Tooltip("Port where the broker accepts connections")]
        public int brokerPort = 1883;
        [Tooltip("Use encrypted connection")]
        public bool isEncrypted = false;
        [Header("Connection parameters")]
        [Tooltip("Connection to the broker is delayed by the the given milliseconds")]
        public int connectionDelay = 500;
        [Tooltip("Connection timeout in milliseconds")]
        public int timeoutOnConnection = MqttSettings.MQTT_CONNECT_TIMEOUT;
        [Tooltip("Connect on startup")]
        public bool autoConnect = false;
        public bool autoReconn = true;
        [Tooltip("UserName for the MQTT broker. Keep blank if no user name is required.")]
        public string mqttUserName = null;
        [Tooltip("Password for the MQTT broker. Keep blank if no password is required.")]
        public string mqttPassword = null;
        
        /// <summary>
        /// Wrapped MQTT client
        /// </summary>
        [NonSerialized]
        protected MqttClient client;

        protected string clientId;
        [NonSerialized]
        protected bool forceDisConn = false;

        Thread reconnThread = null;

        private Dictionary<string, byte> topics
        {
            get
            {
                if(_topics == null)
                    _topics = new Dictionary<string, byte>();
                return _topics;
            }
        }

        private Dictionary<int, string[]> msgId2Topics
        {
            get
            {
                if(_msgId2Topics == null)
                    _msgId2Topics = new Dictionary<int, string[]>();
                return _msgId2Topics;
            }
        }

        private Dictionary<string, byte> _topics = null;
        private Dictionary<int, string[]> _msgId2Topics = null;
        
        /// <summary>
        /// Connect to the broker using current settings.
        /// </summary>
        public virtual void Connect()
        {
            if (client == null || !client.IsConnected)
            {
                DoConnect();
            }
        }

        /// <summary>
        /// Disconnect from the broker, if connected.
        /// </summary>
        public virtual void Disconnect()
        {
            autoConnect = false;
            forceDisConn = true;
            if (client != null)
            {
                if (client.IsConnected)
                {
                    UnsubscribeTopics(topics.Keys.ToArray());
                    client.Disconnect();
                }
                client.MqttMsgPublishReceived -= OnMqttMessageReceived;
            }
        }

        /// <summary>
        /// Override this method to take some actions before connection (e.g. display a message)
        /// </summary>
        protected virtual void OnConnecting()
        {
            Debug.LogFormat("Connecting to broker on {0}:{1}...\n", brokerAddress, brokerPort.ToString());
        }

        /// <summary>
        /// Override this method to take some actions if the connection succeeded.
        /// </summary>
        protected virtual void OnConnected()
        {
            Debug.LogFormat("Connected to {0}:{1}...\n", brokerAddress, brokerPort.ToString());
            if (reconnThread != null)
                reconnThread = null;
            SubscribeTopics(topics.Keys.ToArray(), topics.Values.ToArray());
        }

        /// <summary>
        /// Override this method to take some actions if the connection failed.
        /// </summary>
        protected virtual void OnConnectionFailed(string errorMessage)
        {
            Debug.LogWarning("Connection failed.");
        }

        /// <summary>
        /// Override this method to subscribe to MQTT topics.
        /// </summary>
        public void registTopic(string topic, byte qos)
        {
            topics[topic] = qos;
            if(client != null && client.IsConnected)
                SubscribeTopics(new string[]{topic}, new byte[]{qos});
        }

        public void unregistTopic(string topic)
        {
            topics.Remove(topic);
            if(client != null && client.IsConnected)
                UnsubscribeTopics(new string[]{topic});
        }

        void SubscribeTopics(string[] topics, byte[] qoes)
        {
            int msgId = client.Subscribe(topics, qoes);
            msgId2Topics[msgId] = topics;
        }

        /// <summary>
        /// Override this method to unsubscribe to MQTT topics (they should be the same you subscribed to with SubscribeTopics() ).
        /// </summary>
        void UnsubscribeTopics(string[] topics)
        {
            int msgId = client.Unsubscribe(topics);
            msgId2Topics[msgId] = topics;
        }

        /// <summary>
        /// Override this method for each received message you need to process.
        /// </summary>
        protected virtual void DecodeMessage(string topic, byte[] message)
        {
            Debug.LogFormat("Message received on topic: {0}", topic);
        }

        /// <summary>
        /// Override this method to take some actions when disconnected.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        private void OnTopicSubed(object sender, MqttMsgSubscribedEventArgs e)
        {
            string[] topics = msgId2Topics[e.MessageId];
            Debug.Log($"topic subed: {string.Join(";", topics)}");
            msgId2Topics.Remove(e.MessageId);
            OnTopicStateChange(topics, true);
        }
        
        private void OnTopicUnsubed(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            string[] topics = msgId2Topics[e.MessageId];
            Debug.Log($"topic unsubed: {string.Join(";", topics)}");
            msgId2Topics.Remove(e.MessageId);
            OnTopicStateChange(topics, false);
        }

        protected virtual void OnTopicStateChange(string[] topics, bool subed)
        {
        }

        private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs msg)
        {
            DecodeMessage(msg.Topic, msg.Message);
        }

        private IEnumerable<WaitForSeconds> reconn()
        {
            while (client.IsConnected != true)
            {
                DoConnect();
                yield return new WaitForSeconds(1);
            }

        }


        private void OnMqttConnectionClosed(object sender, EventArgs e)
        {
            if (forceDisConn && autoReconn)
            {
                client.ConnectionClosed -= OnMqttConnectionClosed;
                client = null;
                OnDisconnected();
            }
            else
            {
                Debug.Log("Connect lost, trying to reconnect.");
                if (reconnThread == null)
                {
                    reconnThread = new Thread(() => reconn());
                    reconnThread.Start();
                }
            }

            // Set unexpected connection closed only if connected (avoid event handling in case of controlled disconnection)
        }

        /// <summary>
        /// Connects to the broker using the current settings.
        /// </summary>
        /// <returns>The execution is done in a coroutine.</returns>
        private void DoConnect()
        {
            // create client instance 
            if (client != null && client.IsConnected)
                return ;
            try
            {
                if (client == null)
                {
                    client = new MqttClient(brokerAddress, brokerPort, isEncrypted, null, null, isEncrypted ? MqttSslProtocols.SSLv3 : MqttSslProtocols.None);
                    client.ConnectionClosed += OnMqttConnectionClosed;
                    client.MqttMsgSubscribed += OnTopicSubed;
                    client.MqttMsgUnsubscribed += OnTopicUnsubed;
                    // register to message received 
                    client.MqttMsgPublishReceived += OnMqttMessageReceived;
                    //System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate();
                    //client = new MqttClient(brokerAddress, brokerPort, isEncrypted, cert, null, MqttSslProtocols.TLSv1_0, MyRemoteCertificateValidationCallback);
                    client.Settings.TimeoutOnConnection = timeoutOnConnection;
                }
                if (this.clientId == null)
                    this.clientId = Guid.NewGuid().ToString();
                client.Connect(this.clientId, mqttUserName, mqttPassword);
                if (client.IsConnected)
                    OnConnected();
                else
                    OnConnectionFailed("CONNECTION FAILED!"); 
            }
            catch (Exception e)
            {
                client = null;
                Debug.LogErrorFormat("Failed to connect to {0}:{1}:\n{2}", brokerAddress, brokerPort, e.ToString());
                OnConnectionFailed(e.Message);
                return ;
            }
            OnConnecting();
        }

        protected static bool isMatch(string topic, string pattern)
        {
            var realTopicRegex = pattern.Replace(@"/", @"\/")
                .Replace("+", @"[a-zA-Z0-9 _.-]*")
                .Replace("#", @"[a-zA-Z0-9 \/_#+.-]*");
            var regex = new Regex(realTopicRegex);
            return regex.IsMatch(topic);
        }

    }
}
