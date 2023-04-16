using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class closeDoor : MonoBehaviour {


    public bool isOpening = false;
    public bool isClosing = false;

    // Use this for initialization
    void Start () {

	}


    void OnTriggerExit(Collider col)
    {
        if (isClosing == false)
        {
            isClosing = true;
            isOpening = false;
            StartCoroutine(RotateMe(0.5f));
        }
    }

    IEnumerator RotateMe(float inTime)
    {
        var fromAngle = transform.GetChild(0).localRotation;
        var toAngle = Quaternion.Euler(new Vector3(0, 0, 0));
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            if (isOpening)
                break;

            transform.GetChild(0).localRotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null;
        }
        isClosing = false;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
