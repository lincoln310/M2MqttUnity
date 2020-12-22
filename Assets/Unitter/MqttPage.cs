using System.Collections;
using System.Collections.Generic;
using M2MqttUnity;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unitter
{
    public class MqttPage : UIWidgetsPanel
    {
        
        protected override void OnEnable()
        {
            var font = Resources.Load<Font>(path: "Font/Material Icons");
            FontManager.instance.addFont(font, "Material Icons");
            base.OnEnable();
        }

        // Start is called before the first frame update
        protected override Widget createWidget()
        {
            return new StoreProvider<GlobalState>(
                store: GlobalState.store,
                child: new WidgetsApp(
                    initialRoute:"/", //名为"/"的路由作为应用的home(首页)
                    // theme: new ThemeData(primarySwatch: Colors.blue),
                    //注册路由表
                    routes: new Dictionary<string, WidgetBuilder>(){
                        {"/", (context) => new BrokerPage()}, //注册首页路由
                        // {"/broker/topics", (context) => new TopicPage()},
                        // {"/broker/topic/msges", (context) => new MsgWidget()},
                    },
                    pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (BuildContext context, Animation<float> animation,
                                Animation<float> secondaryAnimation) => builder(context)
                        )));
        }

        protected override void Update()
        {
            base.Update();
            if (GlobalState.changed)
            {
                Debug.Log("changed backGround");
                GlobalState.changed = false;
                GlobalState.store.dispatcher.dispatch(GlobalState.store);
            }
        }
    }
}