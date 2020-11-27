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

        public MsgWidget(string brokerName, string topic, Key key = null) : base(key)
        {
            this.broker = brokerName;
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
                pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.model.model(this.broker).allTopices[this.topic]; },
                builder: (ctx, topicModel, dispatcher) =>
                {
                    ListView body = ListView.builder(
                        itemCount: topicModel.count(),
                        itemBuilder: (context, index) => tile(context, topicModel.msgOfIdx(index))
                    );
                    return new Scaffold(
                        appBar: new AppBar(
                            title: new Center(child: new Text("topic消息")),
                            leading: new IconButton(
                                icon: new Icon(Icons.arrow_back),
                                onPressed: () => { Navigator.pop(ctx); }
                            ),
                            actions: new List<Widget>()
                            {
                                new IconButton(
                                    // textColor: Colors.white,
                                    // shape: new RoundedRectangleBorder(side: new BorderSide(color: Colors.white)),
                                    icon: new Icon(Icons.delete),
                                    onPressed: () =>
                                    {
                                        topicModel.clear();
                                        dispatcher.dispatch(topicModel);
                                    }
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