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
            return new StoreConnector<GlobalState, BrokerModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.mqttModel.model(this.brokerName); },
                builder: (ctx, brokerModel, dispatcher) =>
                {
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child: new Text("topic列表")),
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
                                            builder: (ctx2) => new DialogSubTopic(this.brokerName)
                                        ));
                                    }
                                ),
                            }
                        ),
                        body: ListView.builder(
                            itemCount: brokerModel.allTopices.Count,
                            itemBuilder: (ctx2, index) => list(context, brokerModel, index, dispatcher)
                        )
                    );
                }
            );
        }

        ListTile list(BuildContext context, BrokerModel brokerModel, int index, Dispatcher dispatcher)
        {
            TopicModel topicModel = brokerModel.GetTopicModelByIdx(index);
            // Refresh(context, dispatcher, brokerModel);
            string l = topicModel.topic[0].ToString().ToUpper();
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
                            value: topicModel.connected,
                            onChanged: (newValue) => topicModel.stateSwitch(!topicModel.connected)),
                        new FlatButton(
                            child: new Text("清空"),
                            onPressed: () =>
                            {
                                topicModel.clear();
                                dispatcher.dispatch(topicModel);
                            }
                        ),
                        new IconButton(
                            icon: new Icon(Icons.arrow_right),
                            onPressed: () => { Navigator.push(context, new MaterialPageRoute(
                                (ctx) => new MsgWidget(brokerModel.name, topicModel.topic)
                            )); })
                    })
            );
        }
        static public Widget TurnOnOff(BrokerModel brokerModel, Dispatcher dispatcher)
        {
            return new Switch(
                value: brokerModel.connected,
                onChanged: (newValue) => brokerModel.stateSwitch(!brokerModel.connected));
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
            return new StoreConnector<GlobalState, BrokerModel>(
                pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state.mqttModel.model(this.brokerName),
                builder: (ctx, brokerModel, dispatcher) => { return inputButton(ctx, brokerModel, dispatcher); }
            );
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
                                new IconButton(
                                    icon: new Icon(Icons.add),
                                    onPressed: () => 
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
                                    }
                                ),
                                new IconButton(
                                    icon: new Icon(Icons.cancel),
                                    onPressed: () => Navigator.pop(ctx)
                                    )
                            })
                    }));
        }
    }
}