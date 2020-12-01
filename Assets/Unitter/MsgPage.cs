using System;
using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

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

        ScrollController _scrollController = new ScrollController();

        public override Widget build(BuildContext context)
        {
            return new StoreConnector<GlobalState, TopicModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.mqttModel.model(this.broker).allTopices[this.topic]; },
                builder: (ctx1, topicModel, dispatcher) =>
                {
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child: new Text("topic消息")),
                            leading: new IconButton(
                                icon: new Icon(Icons.arrow_back),
                                onPressed: () => { Navigator.pop(ctx1); }
                            ),
                            actions: new List<Widget>()
                            {
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
                        body: ListView.builder(
                            controller: _scrollController,
                            shrinkWrap: true,
                            itemCount: topicModel.count(),
                            itemBuilder: (ctx2, index) => tile(ctx2, topicModel.msgOfIdx(
                                topicModel.count() > 0 ? topicModel.count() - 1 - index : index))
                        )
                    );
                }
            );
        }

        ListTile tile(BuildContext context, MsgModel _model)
        {
            string l = _model.clientId[0].ToString().ToUpper();
            return new ListTile(
                leading: new CircleAvatar(
                    child: new Text(l)
                ),
                title: new Text($"{_model.topic}, {_model.datetime}"),
                subtitle: new Text(_model.message)
            );
        }
    }
}