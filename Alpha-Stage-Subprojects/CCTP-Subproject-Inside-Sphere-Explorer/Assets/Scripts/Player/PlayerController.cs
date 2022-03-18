using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Character Variables
    private Rigidbody playerRigidbody;
    private Collider playerCollider;

    // Camera Variables
    Camera playerCamera;

    [Header ("Camera Movement")]
    [Tooltip ("Set whether the Cursor is hidden or shown.")]
    public bool lockCursor = false;
    public float mouseSensitivity = 10.0f;

    [Header ("Camera Controls")]
    [Tooltip ("Length of camera movement smoothing. Lower values = sharper stops. 0.1f offers a realistic feel.")]
    [Range (0.0f, 0.4f)]
    [SerializeField] private float cameraRotationSmoothTime = 0.1f;
    private float cameraPan; // Looking left and right
    private float cameraPanSmooth; // For smoothing the pan movement
    private float cameraPanSmoothVelocity; // Pan smoothing speed
    private float cameraTilt; // Looking up and down
    private float cameraTiltSmooth; // For smoothing the tilt movement
    private float cameraTiltSmoothVelocity; // Tilt smoothing speed
    [Tooltip ("Control the camera tilt range. X = Up. Y = Down. +-40 = A good range.")]
    [SerializeField] private Vector2 cameraTiltRange = new Vector2 (-40.0f, 40.0f); // Control how far player can look (up, down)

    // Movement Variables
    private Vector3 moveDirection;

    [Header ("Player Movement")]
    [Tooltip ("Walking speed. 5.0f feels good for shooter-like movement.")]
    public float walkSpeed = 5.0f;
    [Tooltip ("Sprinting speed. Usually 1.5x faster than walking speed for smooth movement change.")]
    public float sprintSpeed = 7.5f;
    [Tooltip ("Jump height. 10.0f feels good for arcade-like jump.")]
    private float currentSpeed; // For determining our speed in code
    [Tooltip ("Smooths player movement. Lower values = sharper stops. 0.1f feels cinematic.")]
    [Range (0.0f, 0.4f)]
    public float movementSmoothTime = 0.1f;
    [Tooltip ("Jump height. 7.5f feels good for arcade-like jumping (10.0f gravity). 10.0 for realistic jumping (20.0f gravity)")]
    public float jumpForce = 10.0f;
    [Tooltip ("Amount of gravity. 10.0f feels good for arcade-like gravity. 20.0f for realistic gravity.")]
    public float gravityForce = 20.0f;
    private float fallingVelocity = 0.0f; // Keep track of falling speed
    private float distanceToGround = 0.0f; // Variable used in raycast to check if grounded
    private float lastGroundedTime = 0.0f; // Keep track of when last grounded
    private bool jumping = false; // Keep track of player jumping
    private Vector3 velocity;
    private Vector3 currentVelocity;
    [Space(10)] public bool sphericalMovement = false;

    //Spherical Variables
    [HideInInspector] public GameObject planetGameObject;
    private Quaternion panRotation;
    private Quaternion playerToPlanetRotation;
    private GameObject playerChild;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        playerChild = transform.GetChild(0).gameObject;

        // Assign controller variable to the Character Controller & relevant collider
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        // Assign playerCamera as the main camera
        playerCamera = Camera.main;

        // If we want to lock the cursor then lock and hide it
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {  
        // Establish playerCamera's variable values
        cameraPan = transform.eulerAngles.y;
        cameraPanSmooth = cameraPan;
        cameraTilt = playerCamera.transform.localEulerAngles.x;
        cameraTiltSmooth = cameraTilt;

        // Give currentSpeed variable a value
        currentSpeed = walkSpeed;

        // Set rigidbody values
        distanceToGround = playerCollider.bounds.extents.y;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraMovement();
        DefaultMovement();
        
        if (sphericalMovement) UpdateSphericalRotation();
    }

    private void FixedUpdate()
    {
        FixedCameraMovement();

        // Apply movement to rigidbody based on calculations
		Vector3 localMove = transform.TransformDirection (velocity); // Final calculation
		playerRigidbody.MovePosition (playerRigidbody.position + localMove * Time.deltaTime); // Movement call
    }

    private void UpdateCameraMovement()
    {
        // Get mouse movement
        float mouseX = Input.GetAxisRaw ("Mouse X");
        float mouseY = Input.GetAxisRaw ("Mouse Y");

        // Stop camera from swinging down on game start
        float mouseMagnitude = Mathf.Sqrt (mouseX * mouseX + mouseY * mouseY);
        if (mouseMagnitude > 5) 
        {
            mouseX = 0;
            mouseY = 0;
        }

        cameraPan += mouseX * mouseSensitivity; // Move camera left & right
        cameraTilt -= mouseY * mouseSensitivity; // Move camera up & down

        // Clamp the camera pitch so that the there is a limit when looking up & down
        cameraTilt = Mathf.Clamp (cameraTilt, cameraTiltRange.x, cameraTiltRange.y);

        // Smooth camera movement
        cameraTiltSmooth = Mathf.SmoothDampAngle 
        (
            cameraTiltSmooth, 
            cameraTilt, 
            ref cameraTiltSmoothVelocity, 
            cameraRotationSmoothTime
        );
        cameraPanSmooth = Mathf.SmoothDampAngle 
        (
            cameraPanSmooth, 
            cameraPan, 
            ref cameraPanSmoothVelocity, 
            cameraRotationSmoothTime
        );

        // Get cameraPanSmooth float to work with rigidbody rotation by making it a Quaternion
        panRotation = Quaternion.Euler(0.0f, 1.0f * cameraPanSmooth, 0.0f);
    }

    private void FixedCameraMovement() 
    {
        // Horizontal camera movement. Uses the rigidbody to rotate.
        // playerRigidbody.rotation = panRotation;
        playerChild.transform.localRotation = panRotation;

        // Vertical camera movement. Uses Camera transform to rotate.
        playerCamera.transform.localEulerAngles = Vector3.right * cameraTiltSmooth;
    }

    private void DefaultMovement()
    {
        // Create a new Vector2 variable that takes in our movement inputs
        Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

        // Normalize the Vector2 input variable and make it a Vector3. Then multiply it with the set walk speed.
        Vector3 inputDirection = new Vector3 (input.x, 0, input.y).normalized;
        Vector3 inputMove = inputDirection * walkSpeed;

        // If sprint key is pressed then make our current speed equal to sprintSpeed
        float speedOffset = 2; // Speed offset required to keep values tidy (Stop speed from being 0.5f, for example.)
        if (Input.GetKey (KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed / speedOffset;
        }
        else
        {
            currentSpeed = walkSpeed / speedOffset;
        }

        // Get and convert the direction of childObject for correct movement direction
        float facingDirection = playerChild.transform.localEulerAngles.y;
        Quaternion facingDirectionEuler = Quaternion.Euler (0.0f, facingDirection, 0.0f);

        // Create a new Vector3 that takes in childObject direction, input movement and current speed to then use in a movement smoothing calculation
        Vector3 targetVelocity = facingDirectionEuler * inputMove * currentSpeed; 
        velocity = Vector3.SmoothDamp 
        (
            velocity, 
            targetVelocity, 
            ref currentVelocity, 
            movementSmoothTime
        ); // ref currentVelocity because function needs to set a currentVelocity

        // Establish falling speed. Increase as the falling duration grows
        fallingVelocity -= gravityForce * Time.deltaTime;

        // Set velocity to match the recorded movement from previous movement sections
        velocity = new Vector3 (velocity.x, fallingVelocity, velocity.z); // This is used in FixedUpdate() to move the player

        // Check for jump input and if true, check that the character isn't jumping or falling. Then jump
        if (Input.GetKey (KeyCode.Space))
        {
            float sinceLastGrounded = Time.time - lastGroundedTime;

            if (CheckGrounded() || (!jumping && lastGroundedTime < 0.15f))
            {
                jumping = true;
                fallingVelocity = jumpForce;
            }
        }
        else if (CheckGrounded()) // If there is collision below the player (ground)
        {
            jumping = false; // Set jumping to false
            lastGroundedTime = Time.time; // Set lastGroundedTime to the current time
            fallingVelocity = 0; // Stop fallingVelocity
        }
    }

    // Boolean function that uses a raycast to see if there is ground within a superficial amount of the collider bounds.
    private bool CheckGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, distanceToGround + 0.1f);
    }

    private void UpdateSphericalRotation()
    {
        // Get player's current "up" && get the centre of the planetGameObject
        Vector3 localUp = playerRigidbody.transform.up;
        Vector3 targetDirection = (playerRigidbody.position - planetGameObject.transform.position).normalized;

        // Create a smooth movement of adusting player rotation to match axis with the planetGameObject
        playerToPlanetRotation = Quaternion.Slerp 
        (
            transform.rotation, // Get player's current rotation
            Quaternion.FromToRotation (localUp, targetDirection) * playerRigidbody.rotation, // Compare current position to planet pos
            cameraRotationSmoothTime * 2 // Speed of rotation
        );

        
        float facingDirection = playerChild.transform.rotation.eulerAngles.y;
        Quaternion facingDirectionEuler = Quaternion.Euler (0.0f, facingDirection, 0.0f);

        Quaternion rotatePlayer = playerToPlanetRotation * facingDirectionEuler;

        // Rotate the player
        playerRigidbody.MoveRotation (playerToPlanetRotation);
    }

    private void OnDrawGizmos()
    {
		Gizmos.color = Color.blue;
        if (sphericalMovement) Gizmos.DrawLine (transform.position, planetGameObject.transform.position);
    }
}
