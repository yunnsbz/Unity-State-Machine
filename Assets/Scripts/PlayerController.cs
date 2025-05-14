using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

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

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}
