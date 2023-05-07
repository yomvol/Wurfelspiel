using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class candleFlame_script : MonoBehaviour {

    public Camera sceneCamera;

    // Use this for initialization
    void Awake () {
        sceneCamera = Camera.allCameras[0];
    }

    void LateUpdate()  {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, sceneCamera.transform.eulerAngles.y + 90, transform.eulerAngles.z);
    }
}
