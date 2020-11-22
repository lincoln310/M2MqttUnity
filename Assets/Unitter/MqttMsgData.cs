using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using Microsoft.Win32;
using Unity.UIWidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;

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

    public class TopicModel :ChangeNotifier 
    {
        public List<MsgModel> msgModels { get; set; }
        private MsgModel empty;
        public string topic { get; }
        private int maxCnt;
        
        public TopicModel(string topic, int maxCnt = 100)
        {
            this.topic = topic;
            this.maxCnt = maxCnt;
            empty = new MsgModel(topic, "", "", "");
        }

        public MsgModel latest()
        {
            return msgModels.Count > 0 ? msgModels[msgModels.Count - 1] : empty;
        }

        public void add(MsgModel model)
        {
            
            if(msgModels.Count > maxCnt)
                msgModels.RemoveRange(0, msgModels.Count - maxCnt);
            msgModels.Add(model);
            notifyListeners();
        }


        public static TopicModel dummy(string topic)
        {
            TopicModel ret = new TopicModel(topic);
            ret.msgModels = new List<MsgModel>()
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
            return ret;
        }

    }
    
    
    public class BrokerModel : ChangeNotifier
    {
        // DataTable msgs = new DataTable();
        public string host;
        public Dictionary<string, int> topic2Idx = new Dictionary<string, int>();
        public List<TopicModel> allTopices = new List<TopicModel>();

        public TopicModel getModelByTopic(string topic)
        {
            if (topic2Idx.ContainsKey(topic))
            {
                return allTopices[topic2Idx[topic]];
            }

            return null;
        }
        void add(string topic, TopicModel model = null)
        {
            if(model == null)
                model = new TopicModel(topic);
            this.topic2Idx[topic] = this.allTopices.Count;
            this.allTopices.Add(model);
        }

        public void addMsg(string topic, MsgModel msgModel)
        {
            if (!this.topic2Idx.ContainsKey(topic))
                this.add(topic);
            int idx = this.topic2Idx[topic];
            this.allTopices[idx].add(msgModel);
            notifyListeners();
        }

        public BrokerModel(string host)
        {
            this.host = host;
        }

        public static BrokerModel dummy(string host)
        {
            BrokerModel ret = new BrokerModel(host);
            ret.allTopices.Add(TopicModel.dummy("testTopic1"));
            ret.topic2Idx["testTopic1"] = 0;
            ret.allTopices.Add(TopicModel.dummy("testTopic2"));
            ret.topic2Idx["testTopic2"] = 1;
            return ret;
        }
    }

    public class BaseReducer
    {
        public GlobalState reduce(GlobalState state)
        {
            throw new NotImplementedException();
        }

        // private static List<BaseReducer> _registed = new List<BaseReducer>();
        // public void regist(BaseReducer reducer)
        // {
        //     _registed.Add(reducer);
        // }
    }

    public class GlobalState
    {
        public Dictionary<string, BrokerModel> allBrokers { get; set; } = new Dictionary<string, BrokerModel>()
        {
            {"testHost1", BrokerModel.dummy("testHost1")},
            {"testHost2", BrokerModel.dummy("testHost2")}
        };

        public static Store<GlobalState> store()
        {
            return new Store<GlobalState>(
                reducer: Reducer,
                initialState: new GlobalState()
            );
        }

        public static GlobalState Reducer(GlobalState state, object action)
        {
            var baseReducer = action as BaseReducer;
            return baseReducer.reduce(state);
        }
    }
    

}