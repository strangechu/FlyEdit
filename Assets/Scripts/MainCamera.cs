using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Boid boid = GetComponent<FlockManager>().getBoid(0);
        if (boid)
        {
            //Camera.main.transform.position = boid.transform.position - Camera.main.transform.forward * 10 + Camera.main.transform.up * 0;
            //Camera.main.transform.forward = boid.transform.forward;
        }
	}
}
