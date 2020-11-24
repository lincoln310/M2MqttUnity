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
        private M2MqttUnityClient mqttClient;
        
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
                store: GlobalState.store(),
                child: new WidgetsApp(
                    home: new BrokerPage(),
                    pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (BuildContext context, Animation<float> animation,
                                Animation<float> secondaryAnimation) => builder(context)
                        )));
        }
    }
}