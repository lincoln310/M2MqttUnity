using System;
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
            Debug.Log("broker page build");
            return new StoreConnector<GlobalState, MqttModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state.mqttModel,
                builder: (ctx, model, dispatcher) =>
                {
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
                                                builder: (ctx2) => new DialogAddHost()
                                            )); }),
                            }
                        ),
                        body: ListView.builder(
                            itemCount: model.count(),
                            itemBuilder: (ctx2, index) => list(context, model, index, dispatcher)
                        )
                    );
                }
            );
        }

        ListTile list(BuildContext context, MqttModel mqttModel, int index, Dispatcher dispatcher)
        {
            BrokerModel brokerModel = mqttModel.GetBrokerModelByIdx(index);
            string l = brokerModel.name[0].ToString().ToUpper();
            Debug.Log($"in host list, {brokerModel.name}, {brokerModel.host}, {mqttModel.count()}");
            return new ListTile(
                title: new Text(brokerModel.name),
                leading: new CircleAvatar(child: new Text(l)),
                subtitle: new Text($"{brokerModel.host}  {brokerModel.allTopices.Count.ToString()}"),
                trailing: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>()
                    {
                        new IconButton(
                            icon: new Icon(Icons.delete),
                            onPressed: () =>
                            {
                                dispatcher.dispatch((Action) delegate{
                                    mqttModel.remove(brokerModel.name);
                                });
                            }),
                        TopicPage.TurnOnOff(brokerModel, dispatcher),
                        new IconButton(
                            icon: new Icon(Icons.arrow_right),
                            onPressed: () => { Navigator.push(context, new MaterialPageRoute(
                                (ctx) => new TopicPage(brokerModel.name)
                                )); })
                    }),
                onTap: () =>
                {
                    AlertDialog dialog = new AlertDialog(title: new Text("broker information"));
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
            return new StoreConnector<GlobalState, MqttModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state.mqttModel,
                builder: (ctx, mqttModel, dispatcher) => { return inputButton(ctx, mqttModel, dispatcher); }
            );
        }

        Widget inputButton(BuildContext ctx, MqttModel mqttModel, Dispatcher dispatcher)
        {
            TextEditingController brokerHost = new TextEditingController();
            TextEditingController brokerName = new TextEditingController();
            return new Center(
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                    children: new List<Widget>()
                    {
                        new TextField(
                            controller: brokerName,
                            autofocus: false,
                            decoration: new InputDecoration(
                                labelText: "命名",
                                hintText: "地址1",
                                prefixIcon: new Icon(Icons.network_cell)
                            )),
                        new TextField(
                            controller: brokerHost,
                            autofocus: true,
                            decoration: new InputDecoration(
                                labelText: "mqtt服务器地址",
                                hintText: "host:port",
                                prefixIcon: new Icon(Icons.network_cell)
                            )),
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                            children: new List<Widget>()
                            {
                                new IconButton(
                                    icon: new Icon(Icons.done),
                                    onPressed: () =>
                                    {
                                        string host = brokerHost.text.Trim();
                                        if (host.Length == 0 || host.Contains(":") != true)
                                        {
                                            DialogUtils.showDialog(context: ctx, true,
                                                (context) =>
                                                    new AlertDialog(title: new Text("服务器地址没有填写或者格式不对")));
                                        }
                                        else
                                        {
                                            dispatcher.dispatch((Action) delegate
                                            {
                                                mqttModel.add(new BrokerModel(brokerHost.text, brokerName.text));
                                            });
                                            Navigator.pop(ctx);
                                        }
                                    }),
                                new IconButton(
                                    icon: new Icon(Icons.cancel),
                                    onPressed: () => { Navigator.pop(ctx); }
                                ),
                            })
                    }));
        }
    }
}