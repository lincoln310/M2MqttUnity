using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unitter;


public class MoveCtrl : MonoBehaviour, IEventMsgLoc{

    // private RectTransform rectTransform;
    

    [SerializeField]
    public AnimationCurve animationCurve;  // 动画曲线
    LocTime preLocTime = null;
    public string id;

    void Start()
    {
        // 遍历关键帧数组，打印每一个关键帧的 time 和 value
        for (int i = 0; i < animationCurve.keys.Length; i++)
            Debug.Log(animationCurve.keys[i].time + "       " + animationCurve.keys[i].value);

        time = animationCurve.keys[animationCurve.length - 1].time + 1;
        
        preLocTime = new LocTime();
        preLocTime.loc = transform.position;
        preLocTime.ts = DateTime.Now;
        
    }

    private void Awake()
    {
        List<TextMesh> comps = new List<TextMesh>();
        GetComponentsInChildren(comps);
         
        curLoc = comps.Find(mesh => mesh.name == "Loc");
        name = comps.Find(mesh => mesh.name == "Name");
    }

    private Vector3 preLoc;
    private Vector3 step;
    private float time;
    private TextMesh name;
    private TextMesh curLoc;

    private LocTime newCurLoc = null;
    

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.A))
        {
            LocTime worldPosition = mousePos();
            if(worldPosition != null)
                newLoc(worldPosition);
        }

        if (newCurLoc != null)
        {
            newLoc(newCurLoc);
            newCurLoc = null;
        }
        GetAnimationCurveValue();
        // Awake();
        if(curLoc != null)
            curLoc.text = transform.position.ToString();
        if(id != null)
            name.text = id;
        if (Camera.current != null)
        {
            Vector3 cameraDirection = Camera.current.transform.forward;
            transform.rotation = Quaternion.LookRotation (cameraDirection);
        }
    }
    
    LocTime mousePos()
    {
        GameObject parent = transform.parent.gameObject;
        TerrainCollider terrainCollider = parent.GetComponent<TerrainCollider>();
        List<Camera> comps = new List<Camera>();
        parent.GetComponentsInChildren(comps);
        Ray ray = comps[0].ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        if (terrainCollider.Raycast(ray, out hitData, 1000))
        {
            LocTime lt = new LocTime();
            lt.loc = hitData.point;
            lt.ts = DateTime.Now;
            return lt;
        }
        return null;
    }

    public void newLoc(LocTime lt)
    {
        preLoc = transform.position;
        step = lt.loc - preLoc;
        // Debug.Log($"pre: {preLoc}, mouse: {worldPosition}, step: {step}");
        time = 0;

        animationCurve.RemoveKey(animationCurve.length - 1);
        // Debug.Log($"{animationCurve.length}");

        var dis = (lt.loc - preLocTime.loc).magnitude;
        var dt = (lt.ts - preLocTime.ts).Seconds;
        var slideTs = 2;
        var slideDis = dis / dt * slideTs;
        var slideRate = slideDis / dis;
        // Debug.Log($"slide: {slideTs}, {slideDis}, {slideRate}");
        animationCurve.AddKey(animationCurve.keys[animationCurve.length - 1].time + slideTs, 1 + slideRate);
    }

    private void GetAnimationCurveValue()
    {
        if (time <= animationCurve.keys[animationCurve.length - 1].time)
        {
            time += Time.deltaTime;

            // 根据时间获取动画曲线相应点的 value 
            float value = animationCurve.Evaluate(time);
            var tmp = value * step;
            // pos.x = bloodTextPos.x + value;
            // 设置文字坐标的 Y 值
            transform.position = preLoc + tmp;
            // Debug.Log($"time: {time}, value: {value}, {tmp}, {transform.position}");
        }
    }

    // 代码建立动画曲线
    private AnimationCurve CreateAnimationCurve()
    {
        // 定义关键帧数组
        Keyframe[] ks = new Keyframe[30];

        int i = 0;
        while (i < ks.Length)
        {
            // 给每一个关键帧赋值  time, value
            ks[i] = new Keyframe(i, Mathf.Sin(i));
            i++;
        }

        // 设置前一个点进入该关键帧的切线（也就是设置斜率）
        ks[1].inTangent = 45;

        // 设置从 10 关键帧出去时的切线 （也是设置斜率）
        ks[10].outTangent = 90;

        // 经过关键帧数组实例化动画曲线
        AnimationCurve animaCur = new AnimationCurve(ks);

        //设置动画曲线最后一帧的 循环类型
        animaCur.postWrapMode = WrapMode.Loop;

        //设置动画曲线第一帧的 循环类型
        animaCur.preWrapMode = WrapMode.Once;

        Keyframe k = new Keyframe(31, 2);
        // 添加一个动画帧
        animaCur.AddKey(k);

        // 移除第 10 个关键帧
        animaCur.RemoveKey(10);

        // 设置第 20 个关键帧 的平滑度
        animaCur.SmoothTangents(20, 3);

        // 根据时间获取动画曲线 time 时间点的 value
        float time = 15.5f;
        animaCur.Evaluate(time);

        return animaCur;
    }

    public void loc(DevLoc devLoc)
    {
        id = devLoc.devId.ToString();
        float[] loc = devLoc.loc;
        if (float.IsNaN(loc[0]) || float.IsNaN(loc[1]) || float.IsNaN(loc[2]))
            return;
        if (loc[2] < 0.1)
            loc[2] = 100;
        newCurLoc = new LocTime
        {
            loc = new Vector3(loc[0], loc[2], loc[1]),
            ts = DateTime.FromBinary(devLoc.ts)
        };
    }

    public void setName(string id)
    {
        this.id = id;
    }
}