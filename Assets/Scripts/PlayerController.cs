using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput;
    public CharacterController controller;
    public Animator animator;
    public Transform cameraTransform;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotateSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.4f;

    [Header("Animator nombres")]
    public string paramMoveX = "MoveX";
    public string paramMoveY = "MoveY";
    public string paramJump = "Jump";
    public string paramEmote = "Emote";
    public string paramGrounded = "Grounded";

    public float smoothTime = 0.08f;
    private Vector2 currentAnim = Vector2.zero;
    private Vector2 animVel = Vector2.zero;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction emoteAction;
    private Vector2 moveInput = Vector2.zero;
    private float verticalVelocity = 0f;

    void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void OnEnable()
    {
        if (playerInput == null) return;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        emoteAction = playerInput.actions["Emote"];

        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null)
        {
            jumpAction.Enable();
            jumpAction.performed += OnJump;
        }
        if (emoteAction != null)
        {
            emoteAction.Enable();
            emoteAction.performed += OnEmote;
        }
    }

    void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.Disable();
        }
        if (emoteAction != null)
        {
            emoteAction.performed -= OnEmote;
            emoteAction.Disable();
        }
    }

    void Update()
    {
        if (moveAction != null)
            moveInput = moveAction.ReadValue<Vector2>();
        else
            moveInput = Vector2.zero;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 worldMove = (camForward * moveInput.y + camRight * moveInput.x);
        Vector3 worldMoveFlat = worldMove;
        worldMoveFlat.y = 0f;
        Vector3 moveDir = worldMoveFlat.sqrMagnitude > 0.001f ? worldMoveFlat.normalized : Vector3.zero;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;
            animator.SetBool(paramGrounded, true);
        }
        else
        {
            animator.SetBool(paramGrounded, false);
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 motion = moveDir * moveSpeed;
        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);
        Vector3 localMove = transform.InverseTransformDirection(moveDir);
        float targetX = Mathf.Clamp(localMove.x, -1f, 1f);
        float targetY = Mathf.Clamp(localMove.z, -1f, 1f);

        currentAnim.x = Mathf.SmoothDamp(currentAnim.x, targetX, ref animVel.x, smoothTime);
        currentAnim.y = Mathf.SmoothDamp(currentAnim.y, targetY, ref animVel.y, smoothTime);

        animator.SetFloat(paramMoveX, currentAnim.x);
        animator.SetFloat(paramMoveY, currentAnim.y);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger(paramJump);
        }
    }

    private void OnEmote(InputAction.CallbackContext ctx)
    {
        if (controller.isGrounded)
        {
            animator.SetTrigger(paramEmote);
        }
    }
}
