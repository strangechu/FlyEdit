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
    public InputField param0Input;
    public InputField param1Input;
    public InputField param2Input;
    public InputField param3Input;
    public InputField param4Input;

    // Use this for initialization
    void Start () {
        if (instance == null)
            instance = this;
        switchCameraBtn.onClick.AddListener(OnClickSwitchCameraBtn);
        optimizeBtn.onClick.AddListener(OnClickOptimizeBtn);

        param3Input.onEndEdit.AddListener(delegate { UpdateParameters(); });
        param4Input.onEndEdit.AddListener(delegate { UpdateParameters(); });
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("space"))
        {
            GetComponent<Canvas>().enabled = !GetComponent<Canvas>().enabled;
        }
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
    public void UpdateParameters()
    {
        TraceReader.instance.SEPERATION_DIST = int.Parse(param3Input.text);
        TraceReader.instance.SEPERATION_WEIGHT= int.Parse(param4Input.text);
        UnityEngine.Debug.Log("Parameter updated!");
    }

}
