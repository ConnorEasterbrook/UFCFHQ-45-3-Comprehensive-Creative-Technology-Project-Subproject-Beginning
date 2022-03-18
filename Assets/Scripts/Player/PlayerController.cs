using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PortalObject
{
    // Character Variables
    private Rigidbody playerRigidbody;
    private CapsuleCollider playerCollider;

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
    private float yCollisionBounds = 0.0f; // Variable used in raycast to check if grounded
    private float lastGroundedTime = 0.0f; // Keep track of when last grounded
    private Vector3 velocity;
    private Vector3 currentVelocity;

    // Spherical Variables
    [HideInInspector] public bool sphericalMovement;
    [HideInInspector] public GameObject planetGameObject;
    private Quaternion panRotation;
    private Quaternion playerToPlanetRotation;
    private GameObject playerChild; // Child object of the player

    // Wall-Walk Variables
    [HideInInspector] public bool wallWalk; // Enable wall walking
    [HideInInspector] public float gravityRotationSpeed = 10.0f; // How quickly should the player rotate
    [HideInInspector] public float wallWalkDetection = 1.5f; // How long should the raycast be
    private Vector3 groundDirection; // What direction is the ground
    private LayerMask groundLayers; // What layers are floor objects set as

    // Inner-Sphere Variables
    [HideInInspector] public bool insideSphere;

    // Shooting Variables
    public bool enableShooting;
    public Rigidbody projectileRigidbody;
    public float projectileSpeed = 20;

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
        yCollisionBounds = playerCollider.bounds.extents.y;

        // Set direction of the ground
        groundDirection = transform.position;

        // Set the layer for ground object. Used for wall walking
        groundLayers = LayerMask.GetMask ("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraMovement();
        UpdateDefaultMovement();
        
        if (sphericalMovement && planetGameObject != null) UpdateSphericalRotation();
        if (wallWalk) UpdateWallWalk();

        if (insideSphere) UpdateInsideSphere();
        
        if (enableShooting && projectileRigidbody != null) UpdateShooting();
    }

    private void FixedUpdate()
    {
        FixedGravity();
        FixedCameraMovement();

        // Apply movement to rigidbody based on calculations
		Vector3 localMove = transform.TransformDirection (velocity); // Final calculation
		playerRigidbody.MovePosition (playerRigidbody.position + localMove * Time.fixedDeltaTime); // Movement call
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

    private void UpdateDefaultMovement()
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

        if (CheckCeilingCollision())
        {
            fallingVelocity = -1.0f;
        }

        // Set velocity to match the recorded movement from previous movement sections
        velocity = new Vector3 (velocity.x, fallingVelocity, velocity.z); // This is used in FixedUpdate() to move the player
    }

    private void FixedGravity()
    {
        playerRigidbody.AddForce (-transform.up * playerRigidbody.mass * gravityForce);

        // Establish falling speed. Increase as the falling duration grows
        fallingVelocity -= gravityForce * Time.deltaTime;

        // Check for jump input and if true, check that the character isn't jumping or falling. Then jump
        if (Input.GetKey (KeyCode.Space) && CheckGrounded())
        {
            fallingVelocity = jumpForce;
        }
        else if (CheckGrounded()) // If there is collision below the player (ground)
        {
            lastGroundedTime = Time.time; // Set lastGroundedTime to the current time
            fallingVelocity = 0; // Stop fallingVelocity
        }
    }

    // Boolean function that uses a raycast to see if there is ground within a superficial amount of the collider bounds.
    private bool CheckGrounded()
    {
        return Physics.Raycast (transform.position, -transform.up, yCollisionBounds + 0.1f);
    }

    // Bool to check ceiling collision with capsule raycast for accurate detection
    private bool CheckCeilingCollision()
    {
        return Physics.Raycast (transform.position, transform.up, yCollisionBounds + 0.1f);
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

        // Rotate the player
        playerRigidbody.MoveRotation (playerToPlanetRotation);
    }

    private void UpdateWallWalk()
    {
        Vector3 setGroundDirection = SurfaceAngle();
        setGroundDirection = new Vector3 (Mathf.Round (setGroundDirection.x), Mathf.Round (setGroundDirection.y), Mathf.Round (setGroundDirection.z));
        groundDirection = Vector3.Lerp (groundDirection, setGroundDirection, gravityRotationSpeed * Time.deltaTime);

        Quaternion wallRotation = Quaternion.FromToRotation (transform.up, groundDirection) * transform.rotation;

        playerRigidbody.MoveRotation (wallRotation);
    }

    // Check to see the angle of the object to climb
    // NOTE: See if a raycast can be drawn in a movement direction for more accurate ray hits
    Vector3 SurfaceAngle()
    {
        Vector3 hitDirection = transform.up;

        // Front raycast
        RaycastHit rayFront;
        Physics.Raycast (playerChild.transform.position, playerChild.transform.forward, out rayFront, wallWalkDetection, groundLayers);
        if (rayFront.transform != null)
        {
            hitDirection += rayFront.normal;
        }

        // Down raycast
        RaycastHit rayDown;
        Physics.Raycast (playerChild.transform.position, -playerChild.transform.up, out rayDown, wallWalkDetection, groundLayers);
        if (rayDown.transform != null)
        {
            hitDirection += rayDown.normal;
        }

        // Back raycast
        RaycastHit rayBack;
        Physics.Raycast (playerChild.transform.position, -playerChild.transform.forward, out rayBack, wallWalkDetection, groundLayers);
        if (rayBack.transform != null)
        {
            hitDirection += rayBack.normal;
        }

        return hitDirection.normalized;
    }

    private void UpdateInsideSphere()
    {
        Vector4 playerPos = new Vector4(transform.position.x, transform.position.y, transform.position.z, 0);
        Shader.SetGlobalVector("_PlayerPos", playerPos);
    }

    private void UpdateShooting()
    {
        if (Input.GetMouseButtonDown (0))
        {
            // Get Vector3 of desired spawn location of the projectile
            Vector3 projectilePosition = new Vector3 (playerCamera.transform.position.x, playerCamera.transform.position.y - 0.15f, playerCamera.transform.position.z);

            // Get Quaternion to ensure the projectile travels in the direction the player is facing
            Quaternion projectileRotation = Quaternion.Euler (transform.rotation.x, playerChild.transform.localRotation.y, transform.rotation.z);

            // Instantiate the projectile with the above variables
            Rigidbody projectile = Instantiate (projectileRigidbody, projectilePosition, projectileRotation) as Rigidbody;

            // Disable gravity so we can apply our own
            projectile.useGravity = false;

            // Add a sphere collider to allow the projectile to be deleted upon impact
            SphereCollider sphereCollider = projectile.gameObject.AddComponent <SphereCollider>();
            sphereCollider.isTrigger = true;

            // Add the projectile script to the projectile
            projectile.gameObject.AddComponent <ProjectileScript>();

            // Add forward momentum to launch the projectile
            projectile.AddForce (playerCamera.transform.forward * (projectileSpeed * 20));
        }

    }

    // Function that handles player teleportation within portals
    public override void Teleport (Transform inPortal, Transform outPortal, Vector3 teleportPosition, Quaternion teleportRotation) 
    {
        Vector3 eulerRotation = teleportRotation.eulerAngles; // Create a Vector3 of quaternion for transform calculation

        // Calculate the shortest distance between player's camera rotation and desired teleportation rotation
        float shortestDistance = Mathf.DeltaAngle (cameraPanSmooth, eulerRotation.y); 

        cameraPan += shortestDistance; // Establish correct rotation for left & right camera movement

        cameraPanSmooth += shortestDistance; // Correct Yaw Smooth with calculated shortest distance to allow for continuity

        playerChild.transform.localEulerAngles = Vector3.up * cameraPanSmooth;  // Set player rotation to correct rotation. It should match the implied direction before entering portals

        playerRigidbody.transform.position = teleportPosition; // Set player position to the calculated teleport location

        velocity = outPortal.TransformVector (inPortal.InverseTransformVector (-velocity)); // Move player off the dotProduct (mid point) of the portal and match velocity of entering

        // playerRigidbody.AddForce (playerCamera.transform.forward * walkSpeed);
        Physics.SyncTransforms (); // Apply transform changes to the physics engine
    }
}
