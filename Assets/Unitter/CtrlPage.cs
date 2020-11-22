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
    class AppState
    {
    }

    public class CtrlPage : UIWidgetsPanel
    {
        private M2MqttUnityClient mqttClient;

        // protected override void OnEnable()
        // {
        //     // if you want to use your own font or font icons.
        //     // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"), "font family name");
        //
        //     // load custom font with weight & style. The font weight & style corresponds to fontWeight, fontStyle of
        //     // a TextStyle object
        //     // FontManager.instance.addFont(Resources.Load<Font>(path: "path to your font"), "Roboto", FontWeight.w500,
        //     //    FontStyle.italic);
        //
        //     // add material icons, familyName must be "Material Icons"
        //     // FontManager.instance.addFont(Resources.Load<Font>(path: "path to material icons"), "Material Icons");
        //
        //     base.OnEnable();
        // }

        // Start is called before the first frame update
        protected override Widget createWidget()
        {
            return new StoreProvider<AppState>(
                child: new WidgetsApp(
                    home: new UiMqttClient(),
                    pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                        new PageRouteBuilder(
                            settings: settings,
                            pageBuilder: (BuildContext context, Animation<float> animation,
                                Animation<float> secondaryAnimation) => builder(context)
                        )));
        }

        class UiMqttClient : StatefulWidget
        {
            public UiMqttClient(Key key = null) : base(key)
            {
            }

            public override State createState()
            {
                return new UiMqttClientState();
            }
        }

        class UiMqttClientState : SingleTickerProviderStateMixin<UiMqttClient>
        {
            private TabController _tabController;
            private Dictionary<string, Widget> _tabs;
            private TopicPage topicWidget = new TopicPage("testHost1");

            public override void initState()
            {
                base.initState();
                _tabController = new TabController(vsync: this, length: 4);
                _tabs = new Dictionary<string, Widget>()
                {
                    {"mqtt消息", topicWidget},
                    {"mqtt配置", config()},
                    {"地图显示", new Text("this is tab2")},
                };
            }


            List<Widget> tabs()
            {
                List<Widget> ret = new List<Widget>();
                foreach (string key in _tabs.Keys)
                    ret.Add(new Tab(text: key));
                return ret;
            }

            List<Widget> bodys()
            {
                List<Widget> ret = new List<Widget>();
                foreach (Widget val in _tabs.Values)
                    ret.Add(val);
                return ret;
            }

            ScrollController _scrollController = new ScrollController();

            public override Widget build(BuildContext context)
            {
                return new Scaffold(
                    appBar: new AppBar(
                        // title: new Text("顶部Tab切换"),
                        bottom: new TabBar(
                            tabs: tabs(),
                            controller: _tabController // 记得要带上tabController
                        )
                    ),
                    body: new TabBarView(
                        controller: _tabController,
                        children: bodys()));
            }

            Widget Buttons()
            {
                return new Container(
                    height: 40,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                        children: new List<Widget>()
                        {
                            new GestureDetector(
                                onTap: () => { SceneManager.LoadScene("display"); },
                                child: new Container(
                                    padding: EdgeInsets.fromLTRB(8, 8, 8, 8),
                                    color: Colors.grey,
                                    child: new Text("返回")
                                ))
                        }));
            }

            Widget config()
            {
                return new Column(children: new List<Widget>
                {
                    new Flex(
                        direction: Axis.horizontal,
                        children: new List<Widget>()
                        {
                            new Expanded(
                                child: new Container(child:
                                    new TextField(
                                        autofocus: false,
                                        decoration: new InputDecoration(
                                            labelText: "mqtt服务器地址",
                                            hintText: "tcp://host:port",
                                            prefixIcon: new Icon(Icons.person)
                                        )))),
                            new SizedBox(
                                child: new GestureDetector(
                                    onTap: () => { },
                                    child: new Container(
                                        padding: EdgeInsets.fromLTRB(8, 8, 8, 8),
                                        color: Colors.grey,
                                        child: new Text("连接")
                                    ))),
                            new Divider(color: Colors.white, indent: 8),
                            new SizedBox(
                                child: new GestureDetector(
                                    onTap: () => { },
                                    child: new Container(
                                        padding: EdgeInsets.fromLTRB(8, 8, 8, 8),
                                        color: Colors.grey,
                                        child: new Text("断开")
                                    ))),
                        }),
                    new Flex(
                        direction: Axis.horizontal,
                        children: new List<Widget>()
                        {
                            new Expanded(
                                child: new Container(child:
                                    new TextField(
                                        autofocus: false,
                                        decoration: new InputDecoration(
                                            labelText: "topic",
                                            hintText: "支持通配符：t1/t2/# 或者 t1/+/t3/# 等",
                                            prefixIcon: new Icon(Icons.person)
                                        )))),
                            new SizedBox(
                                child: new GestureDetector(
                                    onTap: () => { },
                                    child: new Container(
                                        padding: EdgeInsets.fromLTRB(8, 8, 8, 8),
                                        color: Colors.grey,
                                        child: new Text("订阅")
                                    ))),
                        }),
                    new Flex(
                        direction: Axis.horizontal,
                        children: new List<Widget>()
                        {
                            new Expanded(
                                child: new Container(child:
                                    new TextField(
                                        autofocus: false,
                                        decoration: new InputDecoration(
                                            labelText: "msg to send",
                                            hintText: "",
                                            prefixIcon: new Icon(Icons.person)
                                        )))),
                            new SizedBox(
                                child: new GestureDetector(
                                    onTap: () => { },
                                    child: new Container(
                                        padding: EdgeInsets.fromLTRB(8, 8, 8, 8),
                                        color: Colors.grey,
                                        child: new Text("测试发送")
                                    ))),
                        })
                });
            }

            // new Expanded(child: 
            //     ListView.builder(
            //         // reverse: true,
            //         shrinkWrap: true,
            //         controller: _scrollController,
            //         itemCount: this.recved.Count,
            //         itemExtent: 20,
            //         itemBuilder: builder
            //     ))
        }
    }
}