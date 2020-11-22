using System.Collections.Generic;
using System.Linq;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;
using UnityEngine;

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
            // Store<GlobalState> store = GlobalState.store();
            return new StoreProvider<GlobalState>(GlobalState.store(), GetWidget());
        }


        Widget GetWidget()
        {
            Widget body = new StoreConnector<GlobalState, BrokerModel>(
                // pure: true, // 这个参数不知道干嘛用的
                converter: (state) => { return state.allBrokers[this.broker]; },
                builder: (ctx, brokerModel, dispatcher) =>
                {
                    return ListView.builder(
                        itemCount: brokerModel.allTopices.Count,
                        itemBuilder: (context, index) => list(context, brokerModel, index)
                    );
                }
            );

            return body;
            // return new Scaffold(
            // body: body
            // );
        }

        ListTile list(BuildContext context, BrokerModel brokerModel, int index)
        {
            return list(context, brokerModel, brokerModel.allTopices[index]);
        }

        ListTile list(BuildContext context, BrokerModel brokerModel, TopicModel _model)
        {
            string l = _model.topic[0].ToString().ToUpper();
            Debug.Log("in topic list, " + _model.topic);
            return new ListTile(
                leading: new CircleAvatar(
                    child: new Text(l)
                    // backgroundImage: new NetworkImage(_model.avatarUrl),
                ),
                title: new Text(_model.topic),
                subtitle: new Text(_model.msgModels.Count.ToString()),
                trailing: new Icon(Icons.delete, size: 14),
                onTap: () =>
                {
                    Debug.Log("topic selected, should turn to msges");
                    Navigator.push(
                        context,
                        new MaterialPageRoute(
                            builder: (ctx) => new MsgWidget(brokerModel.host, _model.topic) //_model.msgWidget
                        )
                    );
                }
            );
        }
    }
}