using System;
using System.Collections;
using System.Collections.Generic;
using Unitter;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class RtlsLayer : MonoBehaviour
{
    

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

    private void Start()
    {
        if(icosphere == null)
            icosphere = Resources.Load("Icosphere") as GameObject;
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
            
            if (devMoveables.ContainsKey(devLoc.devId) != true)
            {
                GameObject go = GameObject.Instantiate(icosphere, new Vector3(0, 0, 0), new Quaternion());
                if (go != null)
                    devMoveables[devLoc.devId] = go;
            }

            if (devMoveables.ContainsKey(devLoc.devId))
            {
                GameObject go = devMoveables[devLoc.devId];
                if(go != null)
                    ExecuteEvents.Execute<IEventMsgLoc>(go, null, (handler, data) =>
                    {
                        handler.loc(devLoc);
                    });
            }
        }}
    }
}
