using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace Unitter
{
    public class MsgWidget : StatelessWidget
    {
        private string broker = null;
        private string topic = null;

        public MsgWidget(string brokerName, string topic, Key key = null) : base(key)
        {
            this.broker = brokerName;
            this.topic = topic;
        }

        public MsgWidget()
        {
        }

        ScrollController _scrollController = new ScrollController();

        public override Widget build(BuildContext context)
        {
            List<string> args= ModalRoute.of(context).settings.arguments as List<string>;
            this.broker = args[0];
            this.topic = args[1];
            return new StoreConnector<GlobalState, TopicModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.mqttModel.model(this.broker).allTopices[this.topic]; },
                builder: (ctx1, topicModel, dispatcher) =>
                {
                    TextEditingController textEditingController = new TextEditingController();
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child: new Text("topic消息")),
                            leading: new IconButton(
                                icon: new Icon(Icons.arrow_back),
                                onPressed: () => { Navigator.pop(ctx1); }
                            ),
                            actions: new List<Widget>()
                            {
                                GlobalState.btn2map(),
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
                                )
                            }
                        ),
                        body: new Column(children: new List<Widget>()
                        {
                            new Expanded(child: ListView.builder(
                                controller: _scrollController,
                                shrinkWrap: true,
                                itemCount: topicModel.count(),
                                itemBuilder: (ctx2, index) => tile(ctx2, topicModel.msgOfIdx(
                                    topicModel.count() > 0 ? topicModel.count() - 1 - index : index))
                            )),
                            new Row(
                                children: new List<Widget>()
                                {
                                    new IconButton(icon: new Icon(Icons.keyboard)),
                                    // Text input
                                    new Flexible(
                                        child: new Container(
                                            child: new TextField(
                                                controller: textEditingController,
                                                decoration: InputDecoration.collapsed(
                                                    hintText: "Type a message",
                                                    hintStyle: new TextStyle(color: Colors.grey)
                                                )))),
                                    // Send Message Button
                                    new IconButton(icon: new Icon(Icons.send), color: Colors.grey,
                                        onPressed: () =>
                                        {
                                            topicModel.add(new MsgModel(
                                                topicModel.topic, "send",
                                                DateTime.Now.ToLocalTime().ToString(),
                                                textEditingController.text,
                                                MsgModel.MsgType.OUT));
                                        })
                                }
                            )
                        })
                    );
                }
            );
        }

        Widget tile(BuildContext context, MsgModel _model)
        {
            string l = _model.clientId[0].ToString().ToUpper();
            var icon = new Container(child:new CircleAvatar(child: new Text(l)), padding: EdgeInsets.only(left: 10.0f, right: 10.0f));
            MainAxisAlignment alignment = _model.msgType == MsgModel.MsgType.IN
                ? MainAxisAlignment.start
                : MainAxisAlignment.end;
            CrossAxisAlignment crossAxisAlignment = _model.msgType == MsgModel.MsgType.IN
                ? CrossAxisAlignment.start
                : CrossAxisAlignment.end;
            TextAlign textAlign = _model.msgType == MsgModel.MsgType.IN
                ? TextAlign.left
                : TextAlign.right;
            var content = new Container(child:
                new Column(children: new List<Widget>()
                    {
                        new Text($"{_model.datetime} , {_model.topic}"),
                        new Text($"{_model.message}", maxLines: 2,
                            style: new TextStyle(fontSize: 24.0f))
                    },
                    crossAxisAlignment: crossAxisAlignment
                ));
            List<Widget> tile = new List<Widget>()
            {
                icon,
                new Expanded(child: content, flex: 8),
                new Expanded(flex: 2, child: new Container())
            };
            if (_model.msgType == MsgModel.MsgType.OUT)
                tile.Reverse();
            var ret = new GestureDetector(child: new Container(child: new Row(
                        mainAxisAlignment: alignment,
                        children: tile
                    ),
                    padding: EdgeInsets.fromLTRB(15.0f, 10.0f, 15.0f, 10.0f)
                ),
                onTap: () =>
                {
                    Navigator.push(context, new MaterialPageRoute(
                        (ctx) => msgDetail(ctx, _model)
                    ));
                });
            return ret;
        }

        Widget msgDetail(BuildContext ctx, MsgModel msgModel)
        {
            return new Scaffold(
                appBar: new AppBar(
                    title: new Center(child: new Text("消息内容")),
                    leading: new IconButton(
                        icon: new Icon(Icons.arrow_back),
                        onPressed: () => { Navigator.pop(ctx); }
                    )),
                body: new Column(children: new List<Widget>()
                    {
                        new Text($"topic: {msgModel.topic}"),
                        new Divider(),
                        new Text($"time: {msgModel.datetime}"),
                        new Divider(),
                        new Text($"msg: {msgModel.message}")
                    },
                    crossAxisAlignment: CrossAxisAlignment.center,
                    mainAxisAlignment: MainAxisAlignment.center));
        }
    }
}