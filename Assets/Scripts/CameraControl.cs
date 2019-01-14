using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    public static CameraControl instance = null;

    public Camera mainCamera;
    public Camera topCamera;
    private Camera currentCamera;

    // Use this for initialization
    void Start()
    {
        if (instance == null)
            instance = this;
        mainCamera.enabled = true;
        topCamera.enabled = false;
        currentCamera = mainCamera;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 50.0f;
        var y = 0.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 50.0f;

        if (Input.GetKey(KeyCode.Q)) y = 50.0f * Time.deltaTime;
        else if (Input.GetKey(KeyCode.E)) y = -50.0f * Time.deltaTime;

        currentCamera.transform.Translate(x, y, z);
    }

    public void SwitchCamera()
    {
        mainCamera.enabled = !mainCamera.enabled;
        topCamera.enabled = !topCamera.enabled;
        currentCamera = mainCamera.enabled ? mainCamera : topCamera;
        Debug.Log("Camera switched!");
    }
}