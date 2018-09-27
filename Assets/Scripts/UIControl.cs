using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour {

    public static UIControl instance = null;

    public Button switchCameraBtn;

    // Use this for initialization
    void Start () {
        if (instance == null)
            instance = this;
        switchCameraBtn.onClick.AddListener(OnClickSwitchCameraBtn);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnClickSwitchCameraBtn ()
    {
        CameraControl.instance.SwitchCamera();
    }
}
