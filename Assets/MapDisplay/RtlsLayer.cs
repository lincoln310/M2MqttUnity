using System;
using System.Collections;
using System.Collections.Generic;
using Unitter;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class RtlsLayer : MonoBehaviour
{
    enum DevType
    {
        NONE     = 0,
        ANTCNT   = 1,
        ANCHOR   = 8,
        TAG      = 9,
        CTRLDEV  = 10
    }
    class DevLoc
    {//{"origId":"6044","devId":"2207","devType":"TAG","loc":[996.639343,471.371185,0],"mid":"empty","ac":"NaN","ts":"13828607695122"}
        public UInt32 origId;
        public UInt32 devId;
        public DevType devType;
        public float[] loc;
        public string mid;
        public string ac;
        public long ts;
    }

    public static GameObject icosphere ;
    static Dictionary<UInt32, GameObject> devMoveables = new Dictionary<uint, GameObject>();

    static List<string> msges = new List<string>();

    static public void loc(string msg)
    {
        lock (msges)
        {
            msges.Add(msg);
        }
    }

    private void Update()
    {
        if(icosphere == null)
            icosphere = Resources.Load("Icosphere") as GameObject;
        lock(msges){
        foreach (var msg in msges)
        {
            DevLoc devLoc = JsonUtility.FromJson(msg, typeof(DevLoc)) as DevLoc;
            if (devLoc == null)
                continue;
            float[] loc = devLoc.loc;
            if (float.IsNaN(loc[0]) || float.IsNaN(loc[1]) || float.IsNaN(loc[2]))
                continue;
            if (loc[2] < 0.1)
                loc[2] = 100;
            LocTime lt = new LocTime
            {
                loc = new Vector3(loc[0], loc[2], loc[1]),
                ts = DateTime.FromBinary(devLoc.ts)
            };
            if (devMoveables.ContainsKey(devLoc.devId) != true)
            {
                GameObject go = GameObject.Instantiate(icosphere, lt.loc, new Quaternion());
                if(go != null)
                    devMoveables[devLoc.devId] = go;
            }

            if (devMoveables.ContainsKey(devLoc.devId))
            {
                GameObject go = devMoveables[devLoc.devId];
                if(go != null)
                    ExecuteEvents.Execute<IEventMsgLoc>(go, null, (handler, data) => { handler.loc(lt); });
            }
        }}
    }
}
