using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unitter
{
    public class LocTime 
    {
        public Vector3 loc;
        public DateTime ts;
    }
    public interface IEventMsgLoc : IEventSystemHandler
    {
        void loc(LocTime msg);
    }
}