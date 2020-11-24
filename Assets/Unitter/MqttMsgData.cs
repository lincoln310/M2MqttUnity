using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using Microsoft.Win32;
using Unity.UIWidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;
using UnityEngine;

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

        
        
        // public class MsgReducer : BaseReducer
        // {
        //     protected string topic = null;
        //     protected string host = null;
        //
        //     public MsgReducer(string host, string topic)
        //     {
        //         this.host = host;
        //         this.topic = topic;
        //     }
        // }
        // public class ReducerClear : MsgReducer
        // {
        //     public ReducerClear(string host, string topic) : base(host, topic)
        //     {
        //     }
        //
        //     public override GlobalState reduce(GlobalState state)
        //     {
        //         // state.allBrokers[host].allTopices[topic].clear();
        //         return state;
        //     }
        // }
    };

    public class TopicModel 
    {
        private List<MsgModel> msgModels  = new List<MsgModel>();
        private MsgModel empty;
        public string topic { get; }
        public string host { get; }
        private int maxCnt;

        // public class BrokerReducer : BaseReducer
        // {
        //     protected string topic = null;
        //     protected string host = null;
        //
        //     public BrokerReducer(string host, string topic)
        //     {
        //         this.host = host;
        //         this.topic = topic;
        //     }
        // }
        // public class AddReducer: BrokerReducer{
        //     public AddReducer(string host, string topic) : base(host, topic)
        //     {
        //     }
        //
        //     public override GlobalState reduce(GlobalState state)
        //     {
        //         state.allBrokers[this.host].add(new TopicModel(this.host, this.topic));
        //         return state;
        //     }
        //
        // }
        //
        // public class RemoveReducer : BrokerReducer
        // {
        //     public RemoveReducer(string host, string topic) : base(host, topic)
        //     {
        //     }
        //
        //     public override GlobalState reduce(GlobalState state)
        //     {
        //         state.allBrokers[this.host].remove(this.topic);
        //         return state;
        //     }
        //
        // }
        //
        public TopicModel(string host, string topic, int maxCnt = 100)
        {
            this.host = host;
            this.topic = topic;
            this.maxCnt = maxCnt;
            empty = new MsgModel(topic, "", "", "");
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

        public void clear2Left(int cnt)
        {
            if(this.msgModels.Count > cnt)
                this.msgModels.RemoveRange(0, this.msgModels.Count - cnt);
            // notifyListeners();
        }

        public void add(MsgModel model)
        {
            this.msgModels.Add(model);
            // notifyListeners();
        }

        public void clear()
        {
            this.msgModels.Clear();
            // notifyListeners();
        }
        
        public static TopicModel dummy(string host, string topic)
        {
            TopicModel ret = new TopicModel(host, topic);
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
    
    
    public class BrokerModel 
    {
        // DataTable msgs = new DataTable();
        public string host;
        public Dictionary<string, TopicModel> allTopices = new Dictionary<string, TopicModel>();

        public TopicModel GetTopicModelByIdx(int idx)
        {
            if (allTopices.Count <= idx)
                return null;
            return allTopices.ElementAt(idx).Value;
        }
        public void add(TopicModel model)
        {
            this.allTopices.Add(model.topic, model);
            // notifyListeners();
        }

        public void remove(string topic)
        {
            this.allTopices.Remove(topic);
            // notifyListeners();
        }

        public void addMsg(string host, string topic, MsgModel msgModel)
        {
            if (!this.allTopices.ContainsKey(topic))
                this.add(new TopicModel(host, topic));
            TopicModel topicModel = this.allTopices[topic];
            topicModel.add(msgModel);
            // notifyListeners();
        }

        public BrokerModel(string host)
        {
            this.host = host;
        }

        public static BrokerModel dummy(string host)
        {
            BrokerModel ret = new BrokerModel(host);
            ret.add(TopicModel.dummy(host, "testTopic1"));
            ret.add(TopicModel.dummy(host, "testTopic2"));
            return ret;
        }
    }

    public class BaseReducer
    {
        virtual public GlobalState reduce(GlobalState state)
        {
            return state;
        }

    }

    public class GlobalState
    {
        protected Dictionary<string, BrokerModel> allBrokers = new Dictionary<string, BrokerModel>();

        public int count()
        {
            return this.allBrokers.Count;
        }

        public void add(BrokerModel model)
        {
            this.allBrokers[model.host] = model;
        }

        public void remove(string modelName)
        {
            this.allBrokers.Remove(modelName);
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
        public static GlobalState dummy()
        {
            Debug.Log("host dummy");
            GlobalState ret = new GlobalState();
            ret.allBrokers = new Dictionary<string, BrokerModel>()
            {
                {"testHost1", BrokerModel.dummy("testHost1")},
                {"testHost2", BrokerModel.dummy("testHost2")}
            };
            return ret;
        }

        public GlobalState()
        {
            Debug.Log("new state");
        }

        public static GlobalState state = GlobalState.dummy();

        public static Store<GlobalState> store()
        {
            return new Store<GlobalState>(
                reducer: Reducer,
                initialState: state
            );
        }

        public static GlobalState Reducer(GlobalState state, object action)
        {
            var baseReducer = action as BaseReducer;
            return baseReducer.reduce(state);
        }
    }
    

}