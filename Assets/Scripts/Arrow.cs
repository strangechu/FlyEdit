using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {
    Camera mainCamera;
    public GameObject arrow;
    public float speed = 100.0f;

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetAxis("Mouse X") != 0)
        {
            this.transform.Rotate(0, Input.GetAxis("Mouse X") * speed, 0);
        }
        if (Input.GetAxis("Mouse Y") != 0)
        {
            this.transform.Rotate(-Input.GetAxis("Mouse Y") * speed, 0, 0);
        }
        if (Input.GetMouseButton(0))
        {
            this.transform.position += this.transform.forward * speed;
        }
        if (Input.GetMouseButton(1))
        {
            this.transform.position -= this.transform.forward * speed;
        }
    }

    public void UpdatePosition()
    {
        arrow.transform.position = mainCamera.transform.position + mainCamera.transform.forward * 2;
        arrow.transform.forward = mainCamera.transform.forward;
    }
}
