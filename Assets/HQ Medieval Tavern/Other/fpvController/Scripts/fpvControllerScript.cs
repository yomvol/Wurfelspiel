using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class fpvControllerScript : MonoBehaviour {

    public float speed = 4.0f;
    public float jumpSpeed = 4.0f;
    public float gravity = 10.0f;


    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.visible = false;
    }

    void Update()
    {
        if (controller.isGrounded) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            if (Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") != 0)
            {
                moveDirection = moveDirection * (speed / 1.41f);
            } else {
                moveDirection = moveDirection * speed;
            }

            if (Input.GetButton("Jump")) {
                moveDirection.y = jumpSpeed;
            }
        }

        // Apply gravity
        moveDirection.y = moveDirection.y - (gravity * Time.deltaTime);

        // Move the controller
        controller.Move(moveDirection * Time.deltaTime);
    }


}

