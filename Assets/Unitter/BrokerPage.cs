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
using UnityEngine.SceneManagement;
using DialogUtils = Unity.UIWidgets.material.DialogUtils;

namespace Unitter
{
    public class BrokerPage : StatelessWidget
    {
        public BrokerPage(Key key = null) : base(key)
        {
        }

        public override Widget build(BuildContext context)
        {
            return GetWidget();
            // return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }

        Widget GetWidget()
        {
            var lv = new StoreConnector<GlobalState, MqttModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state.model,
                builder: (ctx, model, dispatcher) =>
                {
                    var ret = ListView.builder(
                        itemCount: model.count(),
                        itemBuilder: (context, index) => list(context, model, index, dispatcher)
                    );
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child: new Text("broker列表")),
                            actions: new List<Widget>()
                            {
                                new IconButton(
                                    icon: new Icon(Icons.map),
                                    onPressed: () => { SceneManager.LoadScene("map"); }
                                    ),
                                new IconButton(
                                    icon: new Icon(Icons.add),
                                    onPressed: () => { Navigator.push(ctx, new MaterialPageRoute(
                                                builder: (context) => new DialogAddHost()
                                            )); }),
                            }
                        ),
                        body: ret
                    );
                }
            );

            return lv;
        }

        ListTile list(BuildContext context, MqttModel model, int index, Dispatcher dispatcher)
        {
            return list(context, model, model.GetBrokerModelByIdx(index), dispatcher);
        }

        ListTile list(BuildContext context, MqttModel mqttModel, BrokerModel brokerModel, Dispatcher dispatcher)
        {
            string l = brokerModel.host[0].ToString().ToUpper();
            Debug.Log($"in host list,  {brokerModel.host}, {mqttModel.count()}");
            return new ListTile(
                title: new Text(brokerModel.host),
                leading: new CircleAvatar(child: new Text(l)),
                subtitle: new Text(brokerModel.allTopices.Count.ToString()),
                trailing: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>()
                    {
                        new IconButton(
                            icon: new Icon(Icons.delete),
                            onPressed: () =>
                            {
                                mqttModel.remove(brokerModel.host);
                                dispatcher.dispatch(new BaseReducer());
                            }
                            ),
                        new Switch(
                            value: brokerModel.connected.isNotEmpty(),
                            onChanged: (newValue) =>
                            {
                                Debug.Log($"in broker switch {newValue}");
                                brokerModel.stateSwitch();
                                dispatcher.dispatch(new BaseReducer());
                            }),
                        new IconButton(
                            icon: new Icon(Icons.arrow_right),
                            onPressed: () => { Navigator.push(context, new MaterialPageRoute(
                                (ctx) => new TopicPage(brokerModel.host)
                                )); })
                    }),
                onTap: () =>
                {
                    AlertDialog dialog = new AlertDialog(title: new Text("host information"));
                    DialogUtils.showDialog(context, true, (ctx) => dialog);
                }
            );
        }
    }

    public class DialogAddHost : StatelessWidget
    {
        public DialogAddHost(Key key = null) : base(key)
        {
        }

        public override Widget build(BuildContext context)
        {
            return GetWidget();
            // return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }

        Widget inputButton(BuildContext ctx, MqttModel mqttModel, Dispatcher dispatcher)
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
                                labelText: "mqtt服务器地址",
                                hintText: "tcp://host:port",
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
                                                    new AlertDialog(title: new Text("no mqtt host input")));
                                        }
                                        else
                                        {
                                            mqttModel.add(new BrokerModel(tec.text));
                                            Debug.Log($"{mqttModel.count()}, {tec.text}");
                                            dispatcher.dispatch(new BaseReducer());
                                            Navigator.pop(ctx);
                                        }
                                    },
                                    child: new Text("添加")
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
            var lv = new StoreConnector<GlobalState, MqttModel>(
                pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state.model,
                builder: (ctx, mqttModel, dispatcher) => { return inputButton(ctx, mqttModel, dispatcher); }
            );

            return lv;
        }
    }
}