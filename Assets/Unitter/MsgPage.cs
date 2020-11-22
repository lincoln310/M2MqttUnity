using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
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
            return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }

        Widget GetWidget()
        {
            Widget body = new StoreConnector<GlobalState, TopicModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.allBrokers[this.broker].getModelByTopic(this.topic); },
                builder: (ctx, topicModel, dispatcher) =>
                {
                    return ListView.builder(
                        itemCount: topicModel.msgModels.Count,
                        itemBuilder: (context, index) => tile(context, topicModel.msgModels[index])
                    );
                }
            );

            return body; 
        }
        
        ListTile tile(BuildContext context, MsgModel _model)
        {
            return new ListTile(
                leading: new CircleAvatar(
                    // backgroundImage: new NetworkImage(_model.avatarUrl),
                ),
                title: new Row(
                    children: new List<Widget>()
                    {
                        new Text(_model.clientId),
                        new SizedBox(
                            width: 16.0f
                        ),
                        new Text(
                            _model.datetime
                        )
                    }
                ),
                subtitle: new Text(_model.message),
                trailing: new Icon(
                    Icons.arrow_forward_ios
                )
            );
        }
    }
}