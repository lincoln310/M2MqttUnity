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
            var lv = new StoreConnector<GlobalState, GlobalState>(
                pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state,
                builder: (ctx, globalState, dispatcher) =>
                {
                    var ret = ListView.builder(
                        itemCount: globalState.count(),
                        itemBuilder: (context, index) => list(context, globalState, index, dispatcher)
                    );
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child: new Text("broker列表")),
                            // backgroundColor: Colors.black,
                            actions: new List<Widget>()
                            {
                                new FlatButton(
                                    textColor: Colors.white,
                                    child: new Text("地图"),
                                    onPressed: () => { SceneManager.LoadScene("map"); },
                                    shape: new RoundedRectangleBorder(side: new BorderSide(color: Colors.white))
                                ),
                                new FlatButton(
                                    textColor: Colors.white,
                                    child: new Text("添加"),
                                    onPressed: () =>
                                    {
                                        Navigator.push(ctx,
                                            new MaterialPageRoute(
                                                builder: (context) => new DialogAddHost()
                                            ));
                                    },
                                    shape: new RoundedRectangleBorder(side: new BorderSide(color: Colors.white))
                                ),
                            }
                        ),
                        body: ret
                    );
                }
            );

            return lv;
        }

        ListTile list(BuildContext context, GlobalState state, int index, Dispatcher dispatcher)
        {
            return list(context, state, state.GetBrokerModelByIdx(index), dispatcher);
        }

        ListTile list(BuildContext context, GlobalState state, BrokerModel _model, Dispatcher dispatcher)
        {
            string l = _model.host[0].ToString().ToUpper();
            Debug.Log($"in host list,  {_model.host}, {state.count()}");
            return new ListTile(
                title: new Text(_model.host),
                leading: new CircleAvatar(
                    child: new Text(l)
                ),
                subtitle: new Text(_model.allTopices.Count.ToString()),
                trailing: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>()
                    {
                        new IconButton(
                            icon: new Icon(Icons.delete, color: Colors.blue, size: 16),
                            iconSize : 48),
                        new FlatButton(
                            textColor: Colors.white,
                            child: new CircleAvatar(child: new Text("删除")),
                            onPressed: () => { dispatcher.dispatch(new BaseReducer()); }
                        ),
                        new FlatButton(
                            textColor: Colors.white,
                            child: new CircleAvatar(child: new Text("连接")),
                            onPressed: () =>
                            {
                                Debug.Log("should connect host");
                                Navigator.push(context, new MaterialPageRoute(
                                        builder: (ctx) => new TopicPage(_model.host) //_model.msgWidget
                                    )
                                );
                            })
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
                                            globalState.add(new BrokerModel(tec.text));
                                            Debug.Log($"{globalState.count()}, {tec.text}");
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
            var lv = new StoreConnector<GlobalState, GlobalState>(
                pure: true, // 这个参数不知道干嘛用的
                converter: (state) => state,
                builder: (ctx, globalState, dispatcher) => { return inputButton(ctx, globalState, dispatcher); }
            );

            return lv;
        }
    }
}