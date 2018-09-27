using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public static CameraControl instance = null;

    public Camera mainCamera;
    public Camera topCamera;

    // Use this for initialization
    void Start()
    {
        if (instance == null)
            instance = this;
        mainCamera.enabled = true;
        topCamera.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchCamera()
    {
        mainCamera.enabled = !mainCamera.enabled;
        topCamera.enabled = !topCamera.enabled;
        Debug.Log("Camera switched!");
    }
}