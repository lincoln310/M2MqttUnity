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
    
    public enum DevType
    {
        NONE     = 0,
        ANTCNT   = 1,
        ANCHOR   = 8,
        TAG      = 9,
        CTRLDEV  = 10
    }
    public class DevLoc
    {//{"origId":"6044","devId":"2207","devType":"TAG","loc":[996.639343,471.371185,0],"mid":"empty","ac":"NaN","ts":"13828607695122"}
        public UInt32 origId;
        public UInt32 devId;
        public DevType devType;
        public float[] loc;
        public string mid;
        public string ac;
        public long ts;
    }
    
    public interface IEventMsgLoc : IEventSystemHandler
    {
        void loc(DevLoc msg);
    }
}