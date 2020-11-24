using System.Collections.Generic;
using System.Linq;
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
        private string broker = null;

        public TopicPage(string broker, Key key = null) : base(key)
        {
            this.broker = broker;
        }

        public override Widget build(BuildContext context)
        {
            return GetWidget();
            // return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }


        Widget GetWidget()
        {
            Widget body = new StoreConnector<GlobalState, BrokerModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.model(this.broker); },
                builder: (ctx, brokerModel, dispatcher) =>
                {
                    var lv = ListView.builder(
                        itemCount: brokerModel.allTopices.Count,
                        itemBuilder: (context, index) => list(context, brokerModel, index, dispatcher)
                    );
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child:new Text("topic消息")),
                            // automaticallyImplyLeading: false,
                            leading: new FlatButton(
                                textColor: Colors.white,
                                child: new Text("返回"), 
                                onPressed: () => { Navigator.pop(ctx); },
                                shape: new RoundedRectangleBorder(side: new BorderSide(color: Colors.white))
                            ),
                            actions: new List<Widget>()
                            {
                                new FlatButton(
                                    textColor: Colors.white,
                                    child: new Text("订阅"),
                                    onPressed: () =>
                                    {
                                        Debug.Assert(false, "没有实施订阅消息");
                                        Navigator.push(ctx, new MaterialPageRoute(
                                                builder: (context) => new DialogSubTopic(this.broker)
                                            ));
                                    },
                                    shape: new RoundedRectangleBorder(side: new BorderSide(color: Colors.white))
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

        ListTile list(BuildContext context, BrokerModel brokerModel, TopicModel _model, Dispatcher dispatcher)
        {
            string l = _model.topic[0].ToString().ToUpper();
            Debug.Log($"in topic list,  {_model.topic}, {_model.count()}");
            return new ListTile(
                title: new Text(_model.topic),
                leading: new CircleAvatar(child: new Text(l)),
                subtitle: new Text(_model.count().ToString()),
                trailing: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>()
                    {
                        new IconButton(
                            icon: new Icon(Icons.delete, color: Colors.blue, size: 16),
                            iconSize : 48),
                        new FlatButton(
                            textColor: Colors.white,
                            child: new CircleAvatar(child: new Text("取掉订阅")),
                            onPressed: () => { dispatcher.dispatch(new BaseReducer()); }
                        )
                    }),
                onTap: () =>
                {
                    Debug.Log("topic selected, should turn to msges");
                    Navigator.push(context, new MaterialPageRoute(
                            builder: (ctx) => new MsgWidget(brokerModel.host, _model.topic) 
                        )
                    );
                }
            );
        }
    }

    class DialogSubTopic: StatelessWidget
    {
        private string host = null;
        public DialogSubTopic(string host, Key key = null) : base(key)
        {
            this.host = host;
        }

        public override Widget build(BuildContext context)
        {
            return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }

        Widget inputButton(BuildContext ctx, GlobalState globalState, Dispatcher dispatcher)
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
                                        if (tec.text.Length == 0)
                                        {
                                            DialogUtils.showDialog(context: ctx, true,
                                                (context) =>
                                                    new AlertDialog(title: new Text("请输入topic")));
                                        }
                                        else
                                        {
                                            BrokerModel model = globalState.model(this.host);
                                            model.add(new TopicModel(this.host, tec.text));
                                            Debug.Log($"{model.allTopices.Count}, {tec.text}");
                                            dispatcher.dispatch(new BaseReducer());
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
            var lv = new StoreConnector<GlobalState, GlobalState>(
                pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state,
                builder: (ctx, globalState, dispatcher) => { return inputButton(ctx, globalState, dispatcher); }
            );

            return lv;
        }
    }
}