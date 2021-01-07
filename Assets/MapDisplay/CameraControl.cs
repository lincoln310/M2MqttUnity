
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class CameraControl : MonoBehaviour {
    public GameObject mainCamera;//获取摄像机
    public GameObject terrain;//获取地图
    public float rollAngle=45;//摄像机倾斜角度
    public float height=800f;//摄像机和地图的高度差
    public float moveSpeed = 1.5f;//摄像机移动速度系数
    public float maxRangeX;//摄像机x坐标变化范围
    public float minRangeX;
    public float maxRangeZ;//摄像机z坐标变化范围
    public float minRangeZ;
    public float maxRangeY = 1575f;//缩放最大高度
    public float minRangeY = 30f;//缩放最小高度
    public float zoomSpeed = 5f;//缩放速度系数
    public float rotateAngle = 0;
    public float rotateSpeed=100f;//旋转速度系数
    Vector3 cameraPos;//摄像机临时坐标
    public float sizeX;//地图尺寸x
    public float sizeZ;//地图尺寸z
    public float rate = 1;
    void Start ()
    {
        mainCamera = gameObject;
        terrain = GameObject.Find("mainTerrian");
        //获取地图尺寸
        sizeX = terrain.GetComponent<TerrainCollider>().bounds.size.x;
        sizeZ = terrain.GetComponent<TerrainCollider>().bounds.size.z;
        // Debug.Log(terrain.transform.localScale);
        // Debug.Log(terrain.transform.localPosition);
        // Debug.Log(terrain.transform.position);
        maxRangeX = terrain.transform.position.x + 1.25f * sizeX;
        minRangeX = terrain.transform.position.x + -0.25f * sizeX;
        maxRangeZ = terrain.transform.position.z + 1.25f * sizeZ;
        minRangeZ = terrain.transform.position.z + -0.25f * sizeZ;
        //设置摄像机位置
        cameraPos = mainCamera.transform.position;
        cameraPos.x = terrain.transform.position.x +  sizeX/ 2 ;
        cameraPos.y = terrain.transform.position.y + height;
        cameraPos.z = terrain.transform.position.z +  sizeZ/ 2 ;
        // Vector3 eulerAngles = new Vector3(rollAngle, 0, 0);
        // mainCamera.transform.position = cameraPos;
        // mainCamera.transform.eulerAngles = eulerAngles;
        maxRangeY = sizeX;
        Debug.Log("地图尺寸为：" + sizeX + "X" + sizeZ);
        Debug.Log("当前位置" + cameraPos);
        Debug.Log("当前角度" + mainCamera.transform.eulerAngles);
    }
	void Update ()
    {
        height = cameraPos.y;
        rate = height / minRangeY/3;
        //更新摄像机坐标
        MoveCamera();
        // RotateCamera();
        ZoomCamera();
        // LimitRange();
        mainCamera.transform.position = cameraPos;
    }
    void MoveCamera() {
        
        Vector3 moveDirectionZ = transform.forward;
        moveDirectionZ.z = moveDirectionZ.y;
        moveDirectionZ.y = 0;
        Vector3 moveDirectionX = transform.right;
        moveDirectionX.y = 0;
        
        //按下shift加速移动摄像机
        moveSpeed = rate;
        if (Input.GetMouseButton(0))
        {
            var dx = Input.GetAxis("Mouse X");
            var dy = Input.GetAxis("Mouse Y");
            cameraPos += -1 * dx / Time.deltaTime * 0.5f * moveSpeed * moveDirectionX;
            cameraPos += dy / Time.deltaTime * 0.5f * moveSpeed * moveDirectionZ;
        }
    }
    void ZoomCamera()
    {
        float tmpZoomSpd = zoomSpeed * rate * 0.5f;
        //滚动鼠标滑轮缩放摄像机
        if (Input.GetAxis("Mouse ScrollWheel") > 0) {
            // if (cameraPos.y < minRangeY)
                // return;
            Vector3 moveDirectionZ = transform.forward;
            cameraPos += moveDirectionZ * tmpZoomSpd;
        }
        if (Input.GetAxis("Mouse ScrollWheel") <0) {
            // if (cameraPos.y > maxRangeY)
                // return;
            Vector3 moveDirectionZ = -transform.forward;
            cameraPos += moveDirectionZ * tmpZoomSpd;
        }
    }
    void RotateCamera() {
        //按下鼠标右键旋转摄像机
        if (Input.GetMouseButton(1)) {
            rotateAngle = Input.GetAxis("Mouse X")*rotateSpeed*Time.deltaTime * 3;
            mainCamera.transform.Rotate(0,rotateAngle,0,Space.World);
            rotateAngle = Input.GetAxis("Mouse Y")*rotateSpeed*Time.deltaTime * 3;
            mainCamera.transform.Rotate(-rotateAngle,0,0,Space.World);
        }
    }
    void LimitRange() {
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