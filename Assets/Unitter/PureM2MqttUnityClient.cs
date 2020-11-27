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
    public class PureM2MqttUnityClient //: MonoBehaviour
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
        public bool autoConnect = true;
        [Tooltip("UserName for the MQTT broker. Keep blank if no user name is required.")]
        public string mqttUserName = null;
        [Tooltip("Password for the MQTT broker. Keep blank if no password is required.")]
        public string mqttPassword = null;
        
        /// <summary>
        /// Wrapped MQTT client
        /// </summary>
        protected MqttClient client;

        /// <summary>
        /// Event fired when a connection is successfully established
        /// </summary>
        public event Action ConnectionSucceeded;
        /// <summary>
        /// Event fired when failing to connect
        /// </summary>
        public event Action ConnectionFailed;

        /// <summary>
        /// Connect to the broker using current settings.
        /// </summary>
        public virtual void Connect()
        {
            try
            {
                if (client == null)
                {
                    client = new MqttClient(brokerAddress, brokerPort, isEncrypted, null, null, isEncrypted ? MqttSslProtocols.SSLv3 : MqttSslProtocols.None);
                }
                client.Settings.TimeoutOnConnection = timeoutOnConnection;
                string clientId = Guid.NewGuid().ToString();
                client.Connect(clientId, mqttUserName, mqttPassword);
                if (client.IsConnected)
                {
                    client.ConnectionClosed += OnMqttConnectionClosed;
                    // register to message received 
                    client.MqttMsgPublishReceived += OnMqttMessageReceived; 
                    OnConnected();
                }
                else
                {
                    OnConnectionFailed("CONNECTION FAILED!");
                }
            }
            catch (Exception e)
            {
                client = null;
                Debug.LogErrorFormat("Failed to connect to {0}:{1}:\n{2}", brokerAddress, brokerPort, e.ToString());
                OnConnectionFailed(e.Message);
            }
        }

        /// <summary>
        /// Disconnect from the broker, if connected.
        /// </summary>
        public virtual void Disconnect()
        {
            if (client != null)
            {
                CloseConnection();
                OnDisconnected();
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

            SubscribeTopics();

            if (ConnectionSucceeded != null)
            {
                ConnectionSucceeded();
            }
        }

        /// <summary>
        /// Override this method to take some actions if the connection failed.
        /// </summary>
        protected virtual void OnConnectionFailed(string errorMessage)
        {
            Debug.LogWarning("Connection failed.");
            if (ConnectionFailed != null)
            {
                ConnectionFailed();
            }
        }

        /// <summary>
        /// Override this method to subscribe to MQTT topics.
        /// </summary>
        protected virtual void SubscribeTopics()
        {
        }

        /// <summary>
        /// Override this method to unsubscribe to MQTT topics (they should be the same you subscribed to with SubscribeTopics() ).
        /// </summary>
        protected virtual void UnsubscribeTopics()
        {
        }

        /// <summary>
        /// Disconnect before the application quits.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            CloseConnection();
        }

        /// <summary>
        /// Override this method to take some actions when disconnected.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        
        protected virtual void OnMqttConnectionClosed(object sender, EventArgs e)
        {
            // Set unexpected connection closed only if connected (avoid event handling in case of controlled disconnection)
            Fx.SleepThread(connectionDelay);
            Connect();
        }
        
        /// <summary>
        /// Override this method for each received message you need to process.
        /// </summary>
        protected virtual void DecodeMessage(string topic, byte[] message)
        {
            Debug.LogFormat("Message received on topic: {0}", topic);
        }
        
        private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs msg)
        {
            DecodeMessage(msg.Topic, msg.Message);
        }

        private void CloseConnection()
        {
            if (client != null)
            {
                client.ConnectionClosed -= OnMqttConnectionClosed;
                if (client.IsConnected)
                {
                    UnsubscribeTopics();
                    client.Disconnect();
                }
                client.MqttMsgPublishReceived -= OnMqttMessageReceived;
                client = null;
            }
        }
    }
}
