using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointLight_script : MonoBehaviour {

    // Use this for initialization

    Light thisLight;
    Vector3 basePosition;
    float baseIntensity;
    bool isMoving=false;

    public float minusPosition = -0.075f;
    public float plusPosition = 0.075f;
    public float minusIntensity = -0.2f;
    public float plusIntensity = 0.2f;

	void Start () {
        thisLight = GetComponent<Light>();
        baseIntensity = thisLight.intensity;
        basePosition = transform.position;

    }
	
	// Update is called once per frame
	void Update () {
        if (!isMoving)
        {
            StartCoroutine(changeLight((basePosition + new Vector3(Random.Range(minusPosition, plusPosition), Random.Range(minusPosition, plusPosition), Random.Range(minusPosition, plusPosition))), 
                (baseIntensity + Random.Range(minusIntensity, plusIntensity))));
        }
    }

    IEnumerator changeLight(Vector3 finalPosition, float finalIntensity){
        isMoving = true;
        for (var t = 0f; t < 1; t += Time.deltaTime/0.25f)
        {
            transform.position = transform.position + (finalPosition - transform.position) * Time.deltaTime / 0.25f;
            thisLight.intensity = thisLight.intensity + (finalIntensity - thisLight.intensity) * Time.deltaTime / 0.25f;
            yield return null;
        }
        isMoving = false;
    }

}
