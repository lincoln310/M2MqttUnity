using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using M2MqttUnity;
using Microsoft.Win32;
using Unity.UIWidgets;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;
using Component = UnityEngine.Component;

namespace Unitter
{
    public class MsgModel
    {
        public string clientId { get; }
        public string topic { get; }
        public string datetime { get; }
        public string message { get; }

        public MsgModel(string topic, string clientId, string datetime, string msg)
        {
            this.topic = topic;
            this.clientId = clientId;
            this.datetime = datetime;
            this.message = msg;
        }
    };

    public class TopicModel
    {
        private List<MsgModel> msgModels = new List<MsgModel>();
        public bool connected = false;
        private MsgModel empty;
        public string topic { get; }
        public string brokerName { get; }
        private int maxCnt;
        public bool scroll { get; set; }= false;


        public TopicModel(string brokerName, string topic, int maxCnt = 100)
        {
            this.brokerName = brokerName;
            this.topic = topic;
            this.maxCnt = maxCnt;
            empty = new MsgModel(topic, "", "", "");
        }

        public void stateSwitch(bool todo)
        {
            this.connected = todo;
            Debug.Log($"topic state: {connected}");
        }

        public MsgModel latest()
        {
            return this.msgModels.Count > 0 ? this.msgModels[this.msgModels.Count - 1] : empty;
        }

        public int count()
        {
            return this.msgModels.Count;
        }

        public MsgModel msgOfIdx(int idx)
        {
            return this.msgModels[idx];
        }

        public void add(MsgModel model)
        {
            this.msgModels.Add(model);
        }

        public void clear()
        {
            this.msgModels.Clear();
        }

        public static TopicModel dummy(string brokerName, string topic)
        {
            TopicModel ret = new TopicModel(brokerName, topic);
            var msgs = new List<MsgModel>()
            {
                new MsgModel(
                    topic,
                    "Laurent",
                    "20:18",
                    "How about meeting tomorrow?"
                ),
                new MsgModel(
                    topic,
                    "Tracy",
                    "19:22",
                    "I love that idea, it's great!"
                ),
                new MsgModel(
                    topic,
                    "Claire",
                    "14:34",
                    "I wasn't aware of that. Let me check"
                ),
                new MsgModel(
                    topic,
                    "Joe",
                    "11:05",
                    "Flutter just release 1.0 officially. Should I go for it?"
                ),
                new MsgModel(
                    topic,
                    "Mark",
                    "09:46",
                    "It totally makes sense to get some extra day-off."
                ),
                new MsgModel(
                    topic,
                    "Williams",
                    "08:15",
                    "It has been re-scheduled to next Saturday 7.30pm"
                )
            };
            foreach (var tmp in msgs)
            {
                ret.add(tmp);
            }

            return ret;
        }
    }

    class MqttClient : M2MqttUnityClient
    {
        public BrokerModel brokerModel;

        public void init(string host, int port, BrokerModel brokerModel)
        {
            base.brokerAddress = host;
            base.brokerPort = port;
            this.brokerModel = brokerModel;
            
            base.ConnectionSucceeded += onConnSuc;
            base.ConnectionFailed += onConnBreak;
        }

        public void Subscribe(string topic, int qos)
        {
            client.Subscribe(new string[] {topic}, new byte[] {(byte) qos});
        }

        public void Unsubscribe(string topic)
        {
            client.Unsubscribe(new string[] {topic});
        }

        protected override void SubscribeTopics()
        {
            if (brokerModel == null)
            {
                Debug.Log("no broker found");
                return;
            }

            byte[] qoes = new byte[brokerModel.allTopices.Count];
            for (int i = 0; i < brokerModel.allTopices.Count; i++)
                qoes[i] = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE;
            client.Subscribe(brokerModel.allTopices.Keys.ToArray(), qoes);
        }

        protected override void UnsubscribeTopics()
        {
            if (brokerModel == null)
                return;
            client.Unsubscribe(brokerModel.allTopices.Keys.ToArray());
        }
        
        protected override void DecodeMessage(string topic, byte[] message)
        {
            base.DecodeMessage(topic, message);
            if (brokerModel == null)
                return;
            string msg = System.Text.Encoding.Default.GetString(message);
            MsgModel msgModel = new MsgModel(topic, "C", DateTime.Now.ToLocalTime().ToShortTimeString(), msg);
            brokerModel.addMsg(brokerModel.name, topic, msgModel);
            GlobalState.changed = true;
            // GlobalState.store.dispatcher.dispatch<TopicModel>((GlobalState.DlgAction) delegate { return GlobalState.store.getState(); });
        }

        void onConnSuc()
        {
            Debug.Log("on conn suc: ${name}");
            if (brokerModel != null)
                brokerModel.connected = true;
            else
                Debug.Log("no broker find");
                    
            GlobalState.changed = true;
        }

        void onConnBreak()
        {
            if (brokerModel != null)
                brokerModel.connected = false;
            else
                Debug.Log("no broker find");
            GlobalState.changed = true;
        }
        
        public static MqttClient getMqttClient(BrokerModel model)
        {
            List<MqttClient> clients = Component.FindObjectsOfType<MqttClient>().ToList();
            MqttClient client = clients.Find(x => x.brokerModel.name == model.name);
            if (client == null)
            {
                int portIdx = model.host.IndexOf(':');
                Debug.Log($"create mqtt client: {model.host}, {portIdx}");
                string brokerAddress = model.host.Substring(0, portIdx);
                int brokerPort = Int32.Parse(model.host.Substring(portIdx + 1));
                
                GameObject owner = GameObject.Find("PanelCfg");
                client = owner.AddComponent<MqttClient>();
                // client.name = name;
                client.init(brokerAddress, brokerPort, model);
                Debug.Log("broker mqttClient inited");
            }

            return client;
        }
    }
    
    public class BrokerModel
    {
        public class AddTopic : BaseAction
        {
            public string brokerName;
            public TopicModel topicModel;

            public override GlobalState Do(GlobalState state)
            {
                state.mqttModel.model(brokerName).add(topicModel);
                return state;
            }
        }

        private MqttClient mqttClient;

        MqttClient getMqttClient()
        {
            if (mqttClient == null)
                mqttClient = MqttClient.getMqttClient(this);
            return mqttClient;
        }

        public BrokerModel(string host, string name = null)
        {
            this.host = host;
            if (null == name || name.Trim().Length == 0)
                this.name = host;
            else
                this.name = name;
        }

        // DataTable msgs = new DataTable();
        public string host { get; }
        public string name { get; }

        // public PureM2MqttUnityClient mqttClient;
        public bool connected { get; set; } = false;

        public Dictionary<string, TopicModel> allTopices = new Dictionary<string, TopicModel>();

        // public BuildContext ctx { get; set; } = null;

        public TopicModel GetTopicModelByIdx(int idx)
        {
            if (allTopices.Count <= idx)
                return null;
            return allTopices.ElementAt(idx).Value;
        }

        public void add(TopicModel model)
        {
            this.allTopices.Add(model.topic, model);
            Debug.Log($"{model.brokerName} add topic {model.brokerName}");
            if (this.connected)
                getMqttClient().Subscribe(model.topic, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE);
            // notifyListeners();
        }


        public void stateSwitch(bool todo)
        {
            if (todo)
                getMqttClient().Connect();
            else
                getMqttClient().Disconnect();
            Debug.Log($"broker state: {connected}");
        }

        public void remove(string topic)
        {
            this.allTopices.Remove(topic);
            if (this.connected)
                getMqttClient().Unsubscribe(topic);
        }


        public void addMsg(string brokerName, string topic, MsgModel msgModel)
        {
            if (!this.allTopices.ContainsKey(topic))
                this.add(new TopicModel(brokerName, topic));
            TopicModel topicModel = this.allTopices[topic];
            topicModel.add(msgModel);
        }

        public static BrokerModel dummy(string host, string brokerName)
        {
            BrokerModel ret = new BrokerModel(host, brokerName);
            ret.add(TopicModel.dummy(brokerName, "testTopic1"));
            ret.add(TopicModel.dummy(brokerName, "testTopic2"));
            return ret;
        }
    }


    [Serializable]
    public class MqttModel
    {
        protected Dictionary<string, BrokerModel> allBrokers = new Dictionary<string, BrokerModel>();

        public int count()
        {
            return this.allBrokers.Count;
        }

        public void add(BrokerModel model)
        {
            this.allBrokers[model.name] = model;
        }

        public void remove(string modelName)
        {
            this.allBrokers.Remove(modelName);
        }

        public BrokerModel model(string modelName)
        {
            Debug.Log(modelName);
            return this.allBrokers[modelName];
        }

        public BrokerModel GetBrokerModelByIdx(int idx)
        {
            if (allBrokers.Count <= idx)
                return null;
            return allBrokers.ElementAt(idx).Value;
        }

        public static MqttModel dummy()
        {
            Debug.Log("host dummy");
            MqttModel ret = new MqttModel();
            ret.allBrokers = new Dictionary<string, BrokerModel>()
            {
                {"broker1", BrokerModel.dummy("mqtt.locawave.com:5683", "broker1")},
                {"broker2", BrokerModel.dummy("testHost2:1883", "broker2")}
            };
            return ret;
        }
    }
}