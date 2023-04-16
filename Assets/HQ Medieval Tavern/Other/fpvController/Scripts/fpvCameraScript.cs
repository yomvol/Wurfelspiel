using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class fpvCameraScript : MonoBehaviour {

    private float x;
    private float y;
    public float speedX;
    public float speedY;


    GameObject character;

    // Use this for initialization
    void Start () {
        character = this.transform.parent.gameObject;
	}

    // Update is called once per frame


    void Update() {

    }



    void LateUpdate() {
        y = Input.GetAxis("Mouse X") * speedX * Time.deltaTime;
        x = Input.GetAxis("Mouse Y") * speedY * Time.deltaTime;

        if (transform.eulerAngles.x - x > 180) {
            x = Mathf.Clamp(transform.eulerAngles.x - x - 360, -90, 90);
        } else {
            x = Mathf.Clamp(transform.eulerAngles.x - x, -90, 90);
        }

        transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, 0);

        character.transform.eulerAngles = character.transform.eulerAngles - new Vector3(0, y * -1, 0);
    }

}
