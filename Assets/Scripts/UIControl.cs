using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour {

    public static UIControl instance = null;

    public Button switchCameraBtn;
    public Button optimizeBtn;

    // Use this for initialization
    void Start () {
        if (instance == null)
            instance = this;
        switchCameraBtn.onClick.AddListener(OnClickSwitchCameraBtn);
        optimizeBtn.onClick.AddListener(OnClickOptimizeBtn);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnClickSwitchCameraBtn ()
    {
        CameraControl.instance.SwitchCamera();
    }

    void OnClickOptimizeBtn()
    {
        TraceReader.instance.Optimize();
        //try
        //{
        //    print("Optimization start.");
        //    Process myProcess = new Process();
        //    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
        //    myProcess.StartInfo.CreateNoWindow = false;
        //    myProcess.StartInfo.UseShellExecute = false;
        //    string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "BirdOpti.exe");
        //    myProcess.StartInfo.FileName = filePath;
        //    myProcess.StartInfo.Arguments = filePath;
        //    myProcess.EnableRaisingEvents = true;
        //    myProcess.Start();
        //    myProcess.WaitForExit();
        //    int ExitCode = myProcess.ExitCode;
        //}
        //catch (Exception e)
        //{
        //    print(e);
        //}
        //print("Optimization end.");
    }
}
