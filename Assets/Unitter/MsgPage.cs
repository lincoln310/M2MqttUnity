using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine.Windows.Speech;

namespace Unitter
{
    public class MsgWidget : StatelessWidget
    {
        private string broker = null;
        private string topic = null;

        public MsgWidget(string host, string topic, Key key = null) : base(key)
        {
            this.broker = host;
            this.topic = topic;
        }

        public override Widget build(BuildContext context)
        {
            return GetWidget();
            // return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }

        Widget GetWidget()
        {
            Widget widget = new StoreConnector<GlobalState, TopicModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.model(this.broker).allTopices[this.topic]; },
                builder: (ctx, topicModel, dispatcher) =>
                {
                    ListView body = ListView.builder(
                        itemCount: topicModel.count(),
                        itemBuilder: (context, index) => tile(context, topicModel.msgOfIdx(index))
                    );
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child:new Text("topic消息")),
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
                                    child: new Text("清空"),
                                    onPressed: () =>
                                    {
                                        topicModel.clear();
                                        dispatcher.dispatch(new BaseReducer());
                                    },
                                    shape: new RoundedRectangleBorder(side: new BorderSide(color: Colors.white))
                                )
                            }
                        ),
                        body: body
                    );
                }
            );

            return widget;
        }

        ListTile tile(BuildContext context, MsgModel _model)
        {
            string l = _model.clientId[0].ToString().ToUpper();
            return new ListTile(
                leading: new CircleAvatar(
                    child: new Text(l)
                    // backgroundImage: new NetworkImage(_model.avatarUrl),
                ),
                title: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>()
                    {
                        new Text(_model.clientId),
                        new SizedBox(width: 16.0f),
                        new Text(_model.datetime)
                    }
                ),
                subtitle: new Text(_model.message)
            );
        }
    }
}