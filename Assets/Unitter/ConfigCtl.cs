using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Component = UnityEngine.Component;

public class ConfigCtl : MonoBehaviour
{
    private bool cfgActive = false;

    private Button yourButton;

    // Start is called before the first frame update
    void Start()
    {
        Button btn = GetComponent<Button>();
        Debug.Log(btn.name);
        btn.onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClick()
    {
        cfgActive = !cfgActive;
        SceneManager.LoadScene("ui");
        Scene scene = SceneManager.GetSceneByName("ui");
        Debug.Log("clicked bt");
    }
}
