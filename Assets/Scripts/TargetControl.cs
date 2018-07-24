using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 50.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 50.0f;
        transform.Translate(x, 0, z);
    }
}
