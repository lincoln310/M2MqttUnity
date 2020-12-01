using System;
using System.IO;
using System.Text;
using FullSerializer;
using Unity.UIWidgets;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unitter
{
    [Serializable]
    public class GlobalState 
    {
        public static BuildContext context { get; set; }
        public MqttModel mqttModel { get; }
        public GlobalState(MqttModel model)
        {
            this.mqttModel = model;
            Debug.Log("new state");
        }

        public static bool changed { get; set; } = false;
        static readonly string config = $"config.json";
        static readonly fsSerializer _serializer = new fsSerializer();
        
        public static Store<GlobalState> store = new Store<GlobalState>(
            reducer: Reducer,
            initialState: new GlobalState(load()),
            ReduxLogging.create<GlobalState>()
        );

        public void save()
        {
            GlobalState.save(this.mqttModel);
        }
        static void save(MqttModel model)
        {
            try
            {
                fsData data;
                _serializer.TrySerialize(typeof(MqttModel), model, out data).AssertSuccessWithoutWarnings();

                // emit the data via JSON
                string json = fsJsonPrinter.CompressedJson(data);
                File.WriteAllBytes(config, Encoding.UTF8.GetBytes(json));
                Debug.Log($"write model to : {config}");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        static MqttModel load()
        {
            Debug.Log($"config : {config}");
            string json = "";
            if (File.Exists(config))
            {
                byte[] all = File.ReadAllBytes(config);
                json = Encoding.UTF8.GetString(all);
            }

            MqttModel model = new MqttModel();
            if (json.Trim().Length != 0)
            {
                try
                {
                    // step 1: parse the JSON data
                    fsData data = fsJsonParser.Parse(json);

                    // step 2: deserialize the data
                    object deserialized = null;
                    _serializer.TryDeserialize(data, typeof(MqttModel), ref deserialized).AssertSuccessWithoutWarnings();

                    model = deserialized as MqttModel;
                    // model = JsonConvert.DeserializeObject(json) as MqttModel;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            return model;
        }

        public delegate GlobalState DlgAction(GlobalState obj);
        public static GlobalState Reducer(GlobalState state, object action)
        {
            // if (action is BaseAction)
            // {
            //     var baseReducer = action as BaseAction;
            //     baseReducer.Do();
            //     return state;
            // } 
            if (action is DlgAction)
            {
                var act = action as DlgAction;
                return act(state);
            }
            if(action is Action)
            {
                var act = action as Action;
                act();
                return state;
            }
            else
                return state;
        }
    }
    public abstract class BaseAction
    {
        public BaseAction() { }
    
        public abstract GlobalState Do(GlobalState state);

        //use :
        // BaseAction c = new BaseAction((Action)delegate()
        // {
        //     // insert code here
        // });
    }
}