using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraArrayController : MonoBehaviour
{
    // Horizontal movement speed
    public float walkingSpeed = 7.5f;
    // Speed of rotation
    public float lookSpeed = 2.0f;
    // Lock how far you can move in one motion
    public float lookXLimit = 45.0f;
    // Vertical movement speed
    public float verticalSpeed = 5.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    void Start()
    {
        //Get the character controller
        characterController = GetComponent<CharacterController>();

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Calculate velocity based on the directional inputs of the player
        float curSpeedX = canMove ? walkingSpeed * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? walkingSpeed * Input.GetAxis("Horizontal") : 0;
        float curSpeedZ = canMove ? verticalSpeed * Input.GetAxis("Jump") : 0;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = curSpeedZ;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            //transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}