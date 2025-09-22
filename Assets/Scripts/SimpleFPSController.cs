using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Cámara")]
    public Transform playerCamera;
    public float mouseSensitivity = 200f;

    [Header("Raycast")]
    public float rayDistance = 20f;
    public LayerMask hitLayers = ~0;
    public Color rayColor = Color.red;

    private CharacterController controller;
    private float verticalVelocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MoverJugador();
        RotarCamara();

        if (Input.GetMouseButtonDown(0))
            ShootRay();
    }

    void MoverJugador()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0) verticalVelocity = -2f;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    void RotarCamara()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void ShootRay()
    {
        Vector3 origin = playerCamera.position;
        Vector3 direction = playerCamera.forward;

        Debug.DrawRay(origin, direction * rayDistance, rayColor, 1f);

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, rayDistance, hitLayers, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Golpeó: " + hit.collider.gameObject.name);
        }
        else
        {
            Debug.Log("No golpeó nada");
        }
    }
}
