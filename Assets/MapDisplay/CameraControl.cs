using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject mainCamera; //获取摄像机
    public GameObject terrain = null; //获取地图
    public float rollAngle = 45; //摄像机倾斜角度
    public float height = 800f; //摄像机和地图的高度差
    public float moveSpeed = 1.5f; //摄像机移动速度系数
    public float maxRangeX; //摄像机x坐标变化范围
    public float minRangeX;
    public float maxRangeZ; //摄像机z坐标变化范围
    public float minRangeZ;
    public float maxRangeY = 1575f; //缩放最大高度
    public float minRangeY = 30f; //缩放最小高度
    public float zoomSpeed = 5f; //缩放速度系数
    public Vector3 rotateAngle;
    public float rotateSpeed = 100f; //旋转速度系数
    public Vector3 cameraPos; //摄像机临时坐标
    public Vector3 eulerAngles; //摄像机临时角度
    public Vector3 size; //地图尺寸x
    public float rate = 1;
    public Vector3 initPos;

    void Start()
    {
        mainCamera = gameObject;
        cameraPos = mainCamera.transform.position;
        eulerAngles = mainCamera.transform.eulerAngles;

        if (terrain == null)
        {
            string mapname = "maps/map";
            terrain = GameObject.Instantiate(Resources.Load(mapname), Vector3.zero, new Quaternion()) as GameObject; 
            if (terrain != null)
                Debug.Log($"load map [{mapname}] succ");
            else
                Debug.LogError($"load map [{mapname}] failed");
        }

        //获取地图尺寸
        float[] bound = getMapSize(terrain);
        size = new Vector3(bound[1] - bound[0], bound[3] - bound[2], bound[5] - bound[4]);
        terrain.transform.position = size / 2f;
        // Debug.Log(terrain.transform.localScale);
        // Debug.Log(terrain.transform.localPosition);
        // Debug.Log(terrain.transform.position);
        maxRangeX = terrain.transform.position.x + 1.25f * size.x;
        minRangeX = terrain.transform.position.x + -0.25f * size.x;
        maxRangeZ = terrain.transform.position.z + 1.25f * size.z;
        minRangeZ = terrain.transform.position.z + -0.25f * size.z;
        //设置摄像机位置
        initPos = new Vector3(0, terrain.transform.position.y + height, 0);
        cameraPos = initPos;
        maxRangeY = size.x;
        Debug.Log("地图尺寸为：" + size);
        Debug.Log("当前位置" + cameraPos);
        Debug.Log("当前角度" + mainCamera.transform.eulerAngles);
    }

    float[] getMapSize(GameObject terrain)
    {
        float[] bound = null;
        TerrainCollider[] terrainColliders = terrain.GetComponentsInChildren<TerrainCollider>();
        foreach (var terrianCollider in terrainColliders)
        {
            var tmpBounds = terrianCollider.bounds;
            var tmp = new float[]
            {
                tmpBounds.center[0] - tmpBounds.extents[0],
                tmpBounds.center[0] + tmpBounds.extents[0],
                tmpBounds.center[1] - tmpBounds.extents[1],
                tmpBounds.center[1] + tmpBounds.extents[1],
                tmpBounds.center[2] - tmpBounds.extents[2],
                tmpBounds.center[2] + tmpBounds.extents[2],
            };
            if (bound == null)
            {
                bound = tmp;
                continue;
            }
            for (int i = 0; i < 3; i++)
            {
                int j = 2 * i;
                if (tmp[j] < bound[j])
                    bound[j] = tmp[j];
                if (tmp[j + 1] > bound[j + 1])
                    bound[j + 1] = tmp[j + 1];
            }
        }

        return bound;
    }

    void Update()
    {
        height = cameraPos.y;
        rate = height / minRangeY / 3;
        //更新摄像机坐标
        MoveCamera();
        RotateCamera();
        ZoomCamera();
        // LimitRange();
        mainCamera.transform.position = cameraPos;
    }

    void MoveCamera()
    {
        moveSpeed = rate * 0.2f;
        if (Input.GetMouseButton(0))
        {
            var dx = Input.GetAxis("Mouse X");
            dx *= moveSpeed / Time.deltaTime;
            var dy = Input.GetAxis("Mouse Y");
            dy *= moveSpeed / Time.deltaTime;
            var move = transform.right * -dx + transform.up * -dy;
            cameraPos += move;
        }
    }

    void ZoomCamera()
    {
        //滚动鼠标滑轮缩放摄像机
        Vector3 moveDirectionY = transform.forward;
        var ms = Input.GetAxis("Mouse ScrollWheel");
        float tmpZoomSpd = ms * rate * 5f;
        cameraPos += moveDirectionY * tmpZoomSpd;
    }

    void RotateCamera()
    {
        //按下鼠标右键旋转摄像机
        if (Input.GetMouseButton(1))
        {
            float spd = rotateSpeed * Time.deltaTime * 3;
            var dx = Input.GetAxis("Mouse X") * spd;
            var dy = Input.GetAxis("Mouse Y") * spd;
            transform.Rotate(dy * Vector3.right, Space.Self);
            transform.Rotate(dx * Vector3.up, Space.World);
        }
    }

    void LimitRange()
    {
        //摄像机移动范围限制
        if (cameraPos.x > maxRangeX)
            cameraPos.x = maxRangeX;
        if (cameraPos.x < minRangeX)
            cameraPos.x = minRangeX;
        if (cameraPos.z > maxRangeZ)
            cameraPos.z = maxRangeZ;
        if (cameraPos.z < minRangeZ)
            cameraPos.z = minRangeZ;
    }
}