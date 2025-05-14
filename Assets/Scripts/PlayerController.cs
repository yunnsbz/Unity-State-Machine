using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private CharacterController controller;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S

        moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}
