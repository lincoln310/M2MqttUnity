using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;
using DialogUtils = Unity.UIWidgets.material.DialogUtils;

namespace Unitter
{
    public class TopicPage : StatelessWidget
    {
        private string brokerName = null;

        public TopicPage(string brokerName, Key key = null) : base(key)
        {
            this.brokerName = brokerName;
        }

        public override Widget build(BuildContext context)
        {
            Widget widget = GetWidget();
            return widget;
            // return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }

        static public Widget TurnOnOff(BrokerModel brokerModel, Dispatcher dispatcher)
        {
            return new Switch(
                value: brokerModel.connected,
                onChanged: (newValue) =>
                {
                    dispatcher.dispatch((Action) delegate()
                    {
                        brokerModel.stateSwitch();
                    });

                });
        }

        void Refresh(BuildContext ctx, Dispatcher dispatcher, object obj)
        {
            Promise.Delayed(TimeSpan.FromSeconds(0.1f)).Then(
                () =>
                {
                    dispatcher.dispatch((Action) delegate
                    { });
                    //dosomethings
                    Refresh(ctx, dispatcher, obj);
                }
            );
        }
        
        Widget GetWidget()
        {
            
            Widget body = new StoreConnector<GlobalState, BrokerModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.model.model(this.brokerName); },
                builder: (ctx, brokerModel, dispatcher) =>
                {
                    brokerModel.ctx = ctx;
                    var lv = ListView.builder(
                        itemCount: brokerModel.allTopices.Count,
                        itemBuilder: (context, index) => list(context, brokerModel, index, dispatcher)
                    );
                    
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child: new Text("topic消息")),
                            // automaticallyImplyLeading: false,
                            leading: new IconButton(
                                icon: new Icon(Icons.arrow_back),
                                onPressed: () => { Navigator.pop(ctx); }),
                            actions: new List<Widget>()
                            {
                                TurnOnOff(brokerModel, dispatcher),
                                new IconButton(
                                    // textColor: Colors.white,
                                    // shape: new RoundedRectangleBorder(side: new BorderSide(color: Colors.white)),
                                    icon: new Icon(Icons.add),
                                    onPressed: () =>
                                    {
                                        Navigator.push(ctx, new MaterialPageRoute(
                                            builder: (context) => new DialogSubTopic(this.brokerName)
                                        ));
                                    }
                                ),
                            }
                        ),
                        body: lv
                    );
                }
            );

            return body;
        }

        ListTile list(BuildContext context, BrokerModel brokerModel, int index, Dispatcher dispatcher)
        {
            return list(context, brokerModel, brokerModel.GetTopicModelByIdx(index), dispatcher);
        }

        ListTile list(BuildContext context, BrokerModel brokerModel, TopicModel topicModel, Dispatcher dispatcher)
        {
            Refresh(context, dispatcher, brokerModel);
            string l = topicModel.topic[0].ToString().ToUpper();
            Debug.Log($"in topic list,  {topicModel.topic}, {topicModel.count()}");
            return new ListTile(
                title: new Text(topicModel.topic),
                leading: new CircleAvatar(child: new Text(l)),
                subtitle: new Text(topicModel.count().ToString()),
                trailing: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>()
                    {
                        new IconButton(
                            icon: new Icon(Icons.delete),
                            onPressed: () => 
                            {
                                brokerModel.remove(topicModel.topic);
                                dispatcher.dispatch(brokerModel);
                            }),
                        new Switch(
                            value: topicModel.connected.isNotEmpty(),
                            onChanged: (newValue) =>
                            {
                                dispatcher.dispatch((Action) delegate
                                {
                                    topicModel.stateSwitch();
                                });
                            }),
                        new IconButton(
                            icon: new Icon(Icons.arrow_right),
                            onPressed: () => { Navigator.push(context, new MaterialPageRoute(
                                (ctx) => new MsgWidget(brokerModel.name, topicModel.topic)
                            )); })
                    })
            );
        }
    }

    class DialogSubTopic : StatelessWidget
    {
        private string brokerName = null;

        public DialogSubTopic(string brokerName, Key key = null) : base(key)
        {
            this.brokerName = brokerName;
        }

        public override Widget build(BuildContext context)
        {
            return GetWidget();
            // return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }

        Widget inputButton(BuildContext ctx, BrokerModel brokerModel, Dispatcher dispatcher)
        {
            TextEditingController tec = new TextEditingController();
            return new Center(
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                    children: new List<Widget>()
                    {
                        new TextField(
                            controller: tec,
                            autofocus: false,
                            decoration: new InputDecoration(
                                labelText: "mqtt topic, 支持+#通配符",
                                prefixIcon: new Icon(Icons.network_cell)
                            )),
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                            children: new List<Widget>()
                            {
                                new GestureDetector(
                                    onTap: () =>
                                    {
                                        if (tec.text.Trim().Length == 0)
                                        {
                                            DialogUtils.showDialog(context: ctx, true,
                                                (context) =>
                                                    new AlertDialog(title: new Text("请输入topic")));
                                        }
                                        else
                                        {
                                            dispatcher.dispatch((Action) delegate 
                                            {
                                                brokerModel.add(new TopicModel(this.brokerName, tec.text));
                                            });
                                            Navigator.pop(ctx);
                                        }
                                    },
                                    child: new Text("订阅")
                                ),
                                new GestureDetector(
                                    onTap: () => { Navigator.pop(ctx); },
                                    child: new Text("取消")
                                ),
                            })
                    }));
        }

        Widget GetWidget()
        {
            var lv = new StoreConnector<GlobalState, BrokerModel>(
                pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state.model.model(this.brokerName),
                builder: (ctx, brokerModel, dispatcher) => { return inputButton(ctx, brokerModel, dispatcher); }
            );

            return lv;
        }
    }
}