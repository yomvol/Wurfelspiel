using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openDoorRight : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}


    void OnTriggerEnter(Collider col)
    {
        if (transform.parent.gameObject.GetComponent<closeDoor>().isOpening == false)
        {
            transform.parent.gameObject.GetComponent<closeDoor>().isOpening = true;
            transform.parent.gameObject.GetComponent<closeDoor>().isClosing = false;
            StartCoroutine(RotateMe(0.5f));
        }
    }


    IEnumerator RotateMe(float inTime)
    {
        var fromAngle = transform.parent.GetChild(0).localRotation;
        var toAngle = Quaternion.Euler(new Vector3(0, 90, 0));
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            if (transform.parent.gameObject.GetComponent<closeDoor>().isClosing)
                break;

            transform.parent.GetChild(0).localRotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
    }



    // Update is called once per frame
    void Update () {
		
	}
}
