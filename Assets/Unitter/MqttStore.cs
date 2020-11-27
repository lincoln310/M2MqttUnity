using System;
using System.Collections.Generic;
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
        
        public static Store<GlobalState> store = new Store<GlobalState>(
            reducer: Reducer,
            initialState: new GlobalState(MqttModel.dummy()),
            ReduxLogging.create<GlobalState>()
        );

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
                return null;
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