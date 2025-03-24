using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{

    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;

    [Header("General")]
    public float rotationSpeed;
    public LayerMask whatIsGround;
    public float playerHeight;

    public PlayerMovement playerMovement;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        PlayerCameraRotation();
        AlignPlayer();
    }

    private void PlayerCameraRotation()
    {
        // rotate orientation 
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        // rotate player object
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Input.GetKey(playerMovement.leftKey)) horizontalInput = -1f;
        if (Input.GetKey(playerMovement.rightKey)) horizontalInput = 1f;
        if (Input.GetKey(playerMovement.forwardKey)) verticalInput = 1f;
        if (Input.GetKey(playerMovement.backwardKey)) verticalInput = -1f;

        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }

    private void AlignPlayer()
    {
        RaycastHit hit;
        if (Physics.Raycast(orientation.position, Vector3.down, out hit, playerHeight * 0.5f + 0.2f))
        {
            Quaternion RotToGround = Quaternion.FromToRotation(playerObj.up, hit.normal);
            playerObj.rotation = Quaternion.Slerp(playerObj.rotation, RotToGround * playerObj.rotation, 10);
        }
    }
}
