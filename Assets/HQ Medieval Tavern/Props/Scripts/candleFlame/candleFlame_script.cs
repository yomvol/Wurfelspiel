﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class candleFlame_script : MonoBehaviour {

    public Camera sceneCamera;

    // Use this for initialization
    void Start () {
        sceneCamera = Camera.main;
    }
	
	// Update is called once per frame
	void Update () {
 
    }

    void LateUpdate()  {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, sceneCamera.transform.eulerAngles.y + 90, transform.eulerAngles.z);
    }
}
