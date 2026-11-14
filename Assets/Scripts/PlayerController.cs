using System;
using UnityEngine; // For basic unity functionality
using UnityEngine.InputSystem; // For obtaining functions related to the modern Input System
using UnityEngine.TextCore.Text; // Stylizing for text

// If the component isn't provided, give it
[RequireComponent(typeof(CharacterController))]
// Player controller class used to control the player's mouse and keyboard movement
public class PlayerController : MonoBehaviour
{
    // For the player walking speed 
    [SerializeField] private float walkSpeed = 6.0f;
    // For the player sprinting speed
    [SerializeField] private float runSpeed = 10.0f;
    // For the player jump height
    [SerializeField] private float jumpHeight = 5.0f;
    // For the player's gravity
    [SerializeField] private float gravityForce = 20.0f;
    // For the player's mouse sensitivty
    [SerializeField] private float lookSensitivty = 0.2f;
    // To create clamping
    [SerializeField] private float maxLookAngle = 90.0f;
    // Bool for determining if the player pressed button to jump
    [SerializeField] private AudioClip[] footSteps;
    // Amount of time needed per footstep
    private float stepTimer = 0.0f;
    // Minimum movement speed needed to play foostep
    private const float minWalkSpeed = 1.0f;
    // Minimum runnin speed to play footstep
    private const float minRunSpeed = 6.0f;
    // If the jump button was pressed
    bool jumpPressed = false;

    // Camera for player vision
    private Camera mainCamera;
    // For player controller
    private CharacterController characterController; 
    // Reference to player inputs

    // Player walking input
    private InputAction moveInput;
    // Player running input
    private InputAction runInput;
    // Player jump input
    private InputAction jumpInput;

    // Player's current movement speed
    private float currentMovementSpeed = 0.0f;
    // Player's current movement direction 
    private Vector3 moveDirection = Vector3.zero;
    // Player's current look Angle
    private float lookAngle = 0.0f;

    // When the object with this script is created
    private void Start()
    {
        // Within this current parent's child, get its Camera components
        mainCamera = GetComponentInChildren<Camera>();
        // Within this parent, get its CharacterController components
        characterController = GetComponent<CharacterController>();

        // Obtain the movement action for walking
        moveInput = InputSystem.actions.FindAction("Move");
        // Obtain the movement action for sprinting
        runInput = InputSystem.actions.FindAction("Sprint");
        // Obtain the movement action for jumping
        jumpInput = InputSystem.actions.FindAction("Jump");

        // Stop the cursor from flinging around
        Cursor.lockState = CursorLockMode.Locked;
        // Don't let the cursor appear on screen
        Cursor.visible = false;
        // Visual may need to replace this later so the player can see a dot or something
        // Get the current walk speed
        currentMovementSpeed = walkSpeed;

        // If the jumpInput was pressed, handle the jump
        // This is set from the beginning to be used throughout the script
        jumpInput.started += HandleJump;
    }   

    // Updated every frame
    private void Update()
    {
        // For moveVector, read in its constantly updating from moveInput Vector2 value
        Vector2 moveVector = moveInput.ReadValue<Vector2>();

        // For mouseVetor read in its changes constantly updating from the mouse direction
        // For any changes on both the x and y axis for my mouse on the previous frame, read my mouse value
        Vector2 mouseVector = new Vector2(Mouse.current.delta.x.ReadValue(), Mouse.current.delta.y.ReadValue());

        // If the character leaves the ground, don't let the player jump more
        if (!characterController.isGrounded)
            jumpPressed = false;

        // If the running button was pressed, update the speed before moving
        if (runInput.IsPressed())
            currentMovementSpeed = runSpeed;
        // If it wasn't presssed
        else
            currentMovementSpeed = walkSpeed;

        // Once the value is obtained translate the movement through this function
        HandleMovement(moveVector);
        // One the value is obtained for the mouse vector, translate the mouse movement through this function
        HandleLook(mouseVector);
    }

    // Function used to handle player's four cardinal directions and jumping
    private void HandleMovement(Vector2 moveVector)
    {
        // Update vector for forward and right based on transform direction
        // This will be used to control the four cardinal directions for the player because 
        // the values can be turned negative for backward and forward
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // oldY value to be used so it tracks when the player is moving in the air or not
        float oldY = moveDirection.y;

        // Move vector uses x for left and right and y for forward and backwards
        Vector2 newSpeed = new Vector2(moveVector.y * currentMovementSpeed, moveVector.x * currentMovementSpeed);

        // Grab the movement direction based on the left and right values and for the forward and backward values to control
        // the four directions
        moveDirection = (forward * newSpeed.x) + (right* newSpeed.y);

        // If the player jumped and they are grounded
        if (jumpPressed && characterController.isGrounded)
            // Update their y value to include their jump height
            moveDirection.y = jumpHeight;
        else
            // Else keep the old y value
            moveDirection.y = oldY;

        // If the character is not on the ground
        if (!characterController.isGrounded)
            // Decrease their y value gradually as until they hit the ground over time
            moveDirection.y -= gravityForce * Time.deltaTime;
        
        // Now let the characterController receieve this updated movement and multiply it after each frame's time
        characterController.Move(moveDirection * Time.deltaTime);

        // Footstep sound effect timer between each step is subtracted by the deltaTime
        stepTimer -= Time.deltaTime;

        // While the character is on the ground
        // While the character is running either forward/backwards or left/right
        if (characterController.isGrounded && (Math.Abs(newSpeed.x) > minRunSpeed || Math.Abs(newSpeed.y) > minRunSpeed))
        {
            // If the stepTimer reaches 0,
            if (stepTimer <= 0f)
            {
                // Randomly play one of the footstep sound effects every time this is called
                AudioManager.instance.PlayRandomSoundEffecsClip(footSteps, transform, 1);
                // Increment time between each sound
                stepTimer = 0.25f;
            }
        }
        // While the character is walking either forward/backwards or left/right
        else if (characterController.isGrounded && (Math.Abs(newSpeed.x) > minWalkSpeed || Math.Abs(newSpeed.y) > minWalkSpeed))
        {
            if (stepTimer <= 0f)
            {
                // Randomly play one of the footstep sound effects every time this is called
                AudioManager.instance.PlayRandomSoundEffecsClip(footSteps, transform, 1);
                // Increment time between each sound
                stepTimer = 0.40f;
            }
        }
         // Else don't play it
        else
            stepTimer = 0.0f;
    }

    // Change the bool to true if the Input for jump was made
    private void HandleJump(InputAction.CallbackContext _)
    {
        // If this function was called, make sure the InputSystem knows the player jumped
        jumpPressed = true;
    }

    // Change the camera to look in the direction the player is looking or move the entire player depending on the
    // direction
    private void HandleLook(Vector2 mouseVector)
    {
        // Control how fast the camera moves when the player moves up or down
        // It's negative because we need to make sure the camera moves down when the player looks up and vice
        // versa
        lookAngle += -mouseVector.y * lookSensitivty; 
        lookAngle = Mathf.Clamp(lookAngle, -maxLookAngle, maxLookAngle);

        // Once this angle value is obtained, you must update the camera's transform position
        // Only transform the local angle since we just want the child to move and not the entire player character
        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);

        // Modify the player's left and right movement by rotating the entire player for this
        // Change this via lookSensitivity
        transform.rotation *= Quaternion.Euler(0, mouseVector.x * lookSensitivty, 0);


    }

}
