using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapDisplay
{
    public class ToCfg : MonoBehaviour
    {
        public GameObject panel;
        public GameObject camera;
        public void toCfg()
        {
            DontDestroyOnLoad(panel);
            panel.SetActive(true);
        }

        public void resetCameraAngle()
        {
            var ang = Vector3.zero;
            ang.x = 60;
            camera.transform.eulerAngles = ang;
        }
    }
}