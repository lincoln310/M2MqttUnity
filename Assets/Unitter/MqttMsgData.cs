using System;
using System.Collections.Generic;
using System.Linq;
using M2MqttUnity;
using UnityEngine;

namespace Unitter
{
    public class MsgModel
    {
        public enum MsgType : ushort
        {
            IN = 0,
            OUT 
        }
        public string clientId { get; }
        public string topic { get; }

        public string datetime { get; }
        public string message { get; }
        
        public MsgType msgType { get; }

        public MsgModel(string topic, string clientId, string datetime, string msg, MsgType msgType = MsgType.IN)
        {
            this.topic = topic;
            this.clientId = clientId;
            this.datetime = datetime;
            this.message = msg;
            this.msgType = msgType;
        }
    };

    // [JsonConverter(typeof(TopicModel.Converter))]
    [Serializable]
    public class TopicModel
    {
        private List<MsgModel> msgModels
        {
            get
            {
                if (_msgModels == null)
                    _msgModels = new List<MsgModel>();
                return _msgModels;
            }
            set => _msgModels = value;
        }

        private List<MsgModel> _msgModels = new List<MsgModel>();
        
        [NonSerialized] public bool connected = false;
        private MsgModel empty;

        private BrokerModel brokerModel
        {
            get
            {
                if (_brokerModel == null)
                    _brokerModel = GlobalState.store.getState().mqttModel.model(brokerName);
                return _brokerModel;
            }
            set { _brokerModel = value; }
        }

        [SerializeField] public string topic = "";
        [SerializeField] public string brokerName = "";
        [SerializeField] private int maxCnt;
        private BrokerModel _brokerModel;

        public TopicModel(string brokerName, string topic, int maxCnt = 100)
        {
            this.brokerName = brokerName;
            this.topic = topic;
            this.maxCnt = maxCnt;
            empty = new MsgModel(topic, "", "", "");
        }

        public void stateSwitch(bool todo)
        {
            Debug.Log($"topic state: {connected}");
            if (todo)
                brokerModel.registTopic(topic, 0);
            else
                brokerModel.unregistTopic(topic);
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
            if (model.msgType == MsgModel.MsgType.OUT)
            {
                brokerModel.send(model.topic,
                System.Text.Encoding.Default.GetBytes(model.message));
            }
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

    [Serializable]
    public class BrokerModel : M2MqttClient
    {
        //M2MqttClient
        public void initMqttClient(string host, int port)
        {
            base.brokerAddress = host;
            base.brokerPort = port;
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            base.DecodeMessage(topic, message);
            string msg = System.Text.Encoding.Default.GetString(message);
            MsgModel msgModel = new MsgModel(topic, "recv", DateTime.Now.ToLocalTime().ToString(), msg);
            addMsg(name, topic, msgModel);
            // GlobalState.store.dispatcher.dispatch<TopicModel>((GlobalState.DlgAction) delegate { return GlobalState.store.getState(); });
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            state = true;
            GlobalState.changed = true;
        }

        protected override void OnConnectionFailed(string errorMessage)
        {
            base.OnConnectionFailed(errorMessage);
            state = false;
            GlobalState.changed = true;
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            state = false;
            GlobalState.changed = true;
        }

        protected override void OnTopicStateChange(string[] topics, bool subed)
        {
            base.OnTopicStateChange(topics, subed);
            foreach (var topic in topics)
                allTopices[topic].connected = subed;
            GlobalState.changed = true;
        }

        public void send(string topic, byte[] msg)
        {
            client.Publish(topic, msg); 
        }

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

        public BrokerModel(string host, string name = null)
        {
            this.host = host;
            if (null == name || name.Trim().Length == 0)
                this.name = host;
            else
                this.name = name;

            int portIdx = host.IndexOf(':');
            Debug.Log($"create mqtt client: {host}, {portIdx}");
            string brokerAddress = host.Substring(0, portIdx);
            int brokerPort = Int32.Parse(host.Substring(portIdx + 1));

            // client.name = name;
            initMqttClient(brokerAddress, brokerPort);
            Debug.Log("broker mqttClient inited");
        }

        [SerializeField] public string host = "";
        [SerializeField] public string name = "";

        [SerializeField]
        public Dictionary<string, TopicModel> allTopices
        {
            get
            {
                if(_allTopices == null)
                    _allTopices = new Dictionary<string, TopicModel>();
                return _allTopices;
            }
            set => _allTopices = value;
        } 
        private Dictionary<string, TopicModel> _allTopices = null;
        private bool state = false;

        public bool connected()
        {
            return state;
        }

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
            GlobalState.store.getState().save();
        }

        public void stateSwitch(bool todo)
        {
            if (todo)
                base.Connect();
            else
                base.Disconnect();
            Debug.Log($"broker state: {state}");
        }

        public void remove(string topic)
        {
            this.allTopices.Remove(topic);
            GlobalState.store.getState().save();
            if (this.state)
                base.unregistTopic(topic);
        }

        public void addMsg(string brokerName, string topic, MsgModel msgModel)
        {
            bool added = false;
            foreach (var pair in allTopices)
            {
                if (M2MqttClient.isMatch(topic, pair.Key))
                {
                    pair.Value.add(msgModel);
                    added = true;
                }
            }

            if (!added)
            {
                if (!this.allTopices.ContainsKey(topic))
                {
                    TopicModel model = new TopicModel(brokerName, topic);
                    model.connected = true;
                    base.registTopic(topic, 0);
                    this.add(model);
                }

                TopicModel topicModel = this.allTopices[topic];
                topicModel.add(msgModel);
            }
            GlobalState.changed = true;
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

        [SerializeField]
        protected Dictionary<string, BrokerModel> allBrokers
        {
            get
            {
                if(_allBrokers == null)
                    _allBrokers = new Dictionary<string, BrokerModel>();
                return _allBrokers;
            }
            set => _allBrokers = value;
        }
        Dictionary<string, BrokerModel> _allBrokers = null;
        public int count()
        {
            return this.allBrokers.Count;
        }

        public void add(BrokerModel model)
        {
            this.allBrokers[model.name] = model;
            GlobalState.store.getState().save();
        }

        public void remove(string modelName)
        {
            this.allBrokers.Remove(modelName);
            GlobalState.store.getState().save();
        }

        public BrokerModel model(string modelName)
        {
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
            ret._allBrokers = new Dictionary<string, BrokerModel>()
            {
                {"broker1", BrokerModel.dummy("mqtt.locawave.com:5683", "broker1")},
                {"broker2", BrokerModel.dummy("testHost2:1883", "broker2")}
            };
            return ret;
        }
    }
}