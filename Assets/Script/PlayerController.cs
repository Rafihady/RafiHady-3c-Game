using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float walkspeed;
    float rotationSmooth = 0.1f;
    private float rotationSmoothVelocity;
    [SerializeField] InputManager Input;

    float speed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float sprintTransition;

    [SerializeField] float jumpForce;
    bool IsGrounded;
    [SerializeField] LayerMask GroundLayer;
    [SerializeField] Transform GroundDetector;
    [SerializeField] float detectorRadius;

    [SerializeField] float Stepforce;
    [SerializeField] Vector3 upperStepOffsett;
    [SerializeField] float StepCheckerDistance;

    [SerializeField] Transform climbDetector;
    [SerializeField] float climbCheckDistance;
    [SerializeField] LayerMask climbLayer;
    [SerializeField] Vector3 climboffset;
    [SerializeField] float ClimbSpeed;

    [SerializeField] float CrouchSpedd;

    private CapsuleCollider PlayerCollider;
    private Animator animator;

    [SerializeField] Transform cameraTransform;
    [SerializeField] private CameraManager cameraManager;

    [SerializeField] private float GlideSpeed;
    [SerializeField] private float AirDrag;
    [SerializeField] Vector3 GlideRotationSpeed;
    [SerializeField] float minGlideRotationX;
    [SerializeField] float maxGlideRotationX;

    private bool Ispunching;
    private int combo;
    [SerializeField] private float ResetComboInterval;
    private Coroutine ComboReset;

    [SerializeField] private Transform HitDetector;
    [SerializeField] private LayerMask HitLayer;
    [SerializeField] private float HitDetectorRadius;

    [SerializeField] private PlayerAudioManager playerAudioManager;

    private PlayerStance playerstance;
    Rigidbody rb;

    private void Awake()
    {
        hideandlockCursor();
        animator = GetComponent<Animator>();
        PlayerCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        speed = walkspeed;
        playerstance = PlayerStance.Stand;
    }
    void Start()
    {
        Input.OnMoveInput += Move;
        Input.SprintInput += Sprint;
        Input.OnjumpInput += jump;
        Input.OnClimbInput += StartClimb;
        Input.OnCancellClimb += cancellClimb;
        cameraManager.OnChangePerpective += ChangePerspective;
        Input.OnCrouching += Crouch;
        Input.OnGliding += StartGlide;
        Input.OnCancellGlide += CancellGlide;
        Input.OnPunchInput += Punch;
    }

    private void OnDestroy()
    {
        Input.OnMoveInput -= Move;
        Input.SprintInput -= Sprint;
        Input.OnjumpInput -= jump;
        Input.OnClimbInput -= StartClimb;
        Input.OnCancellClimb -= cancellClimb;
        cameraManager.OnChangePerpective -= ChangePerspective;
        Input.OnCrouching -= Crouch;
        Input.OnGliding -= StartGlide;
        Input.OnCancellGlide -= CancellGlide;
        Input.OnPunchInput -= Punch;

    }

    private void Update()
    {
        CheckGrounded();
        CheckStep();
        Glide();
    }

    private void hideandlockCursor ()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Move (Vector2 AxisDirection)
    {
        Vector3 MovementDirection = Vector3.zero;
        bool isPlayerStanding = playerstance == PlayerStance.Stand;
        bool isPlayerClimbing = playerstance == PlayerStance.Climb;
        bool isPlayerCrouching = playerstance == PlayerStance.Crouch;
        bool isPlayerGliding = playerstance == PlayerStance.Glide;
        if ((isPlayerStanding || isPlayerCrouching) && !Ispunching)
        {
            switch (cameraManager._CameraState)
            {
                case CameraState.ThrirdPerson:
                    if (AxisDirection.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(AxisDirection.x, AxisDirection.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmooth);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        MovementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        rb.AddForce(MovementDirection * speed * Time.deltaTime);
                    }
                    break;
                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
                    Vector3 horizontalDirection = AxisDirection.y * transform.forward;
                    Vector3 verticalDirection = AxisDirection.x * transform.right;
                    MovementDirection = verticalDirection + horizontalDirection;
                    rb.AddForce(MovementDirection * speed * Time.deltaTime);
                    break;
                 default:
                    break;
            }
            Vector3 velocity = new Vector3 (rb.velocity.x,0,rb.velocity.z);
            animator.SetFloat("Velocity", velocity.magnitude * AxisDirection.magnitude);
            animator.SetFloat("VelocityX", velocity.magnitude * AxisDirection.x);
            animator.SetFloat("VelocityZ", velocity.magnitude * AxisDirection.y);


        }
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = AxisDirection.x * transform.right;
            Vector3 vertical = AxisDirection.y * transform.up;
            Vector3 movementDirection = horizontal + vertical;
            rb.AddForce(movementDirection * speed * Time.deltaTime);
            Vector3 velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0);
            animator.SetFloat("ClimbVelocityX", velocity.magnitude * AxisDirection.x);
            animator.SetFloat("ClimbVelocityY", velocity.magnitude * AxisDirection.y);

        }
        else if (isPlayerGliding)
        {
            Vector3 rotationDegree = transform.rotation.eulerAngles;
            rotationDegree.x += GlideRotationSpeed.x * AxisDirection.y * Time.deltaTime;
            rotationDegree.x = Mathf.Clamp(rotationDegree.x, minGlideRotationX, maxGlideRotationX);
            rotationDegree.z += GlideRotationSpeed.z * AxisDirection.x * Time.deltaTime;
            rotationDegree.y += GlideRotationSpeed.y * AxisDirection.x * Time.deltaTime;
            transform.rotation = Quaternion.Euler(rotationDegree);
        }

    }

    private void Sprint (bool isSprint)
    {
        if (isSprint)
        {
            if (speed < sprintSpeed)
            {
                speed = speed + sprintTransition * Time.deltaTime;
            }
        }

        else
        {
            if (speed > walkspeed)
            {
                speed = speed - sprintTransition * Time.deltaTime;
            }
        }
    }

    private void jump ()
    {
        if (IsGrounded)
        {
            Vector3 JumpDirection = Vector3.up;
            rb.AddForce(JumpDirection * jumpForce);
            animator.SetTrigger("Jump");
            
        }
    }

    private void CheckGrounded ()
    {
        IsGrounded = Physics.CheckSphere(GroundDetector.position, detectorRadius, GroundLayer);
        animator.SetBool("IsGrounded", IsGrounded);
        if (IsGrounded)
        {
            CancellGlide();
        }
    }

    private void CheckStep ()
    {
        bool ishitLowerStep = Physics.Raycast(GroundDetector.position, transform.forward, StepCheckerDistance);
        bool ishitUpperstep = Physics.Raycast(GroundDetector.position + upperStepOffsett, transform.forward, StepCheckerDistance);
        if (ishitLowerStep && !ishitUpperstep)
        {
            rb.AddForce(0, Stepforce * Time.deltaTime, 0);
        }
    }
    private void StartClimb ()
    {
        bool isInFrontClimbingWall = Physics.Raycast (climbDetector.position, transform.forward, out RaycastHit hit, climbCheckDistance,climbLayer);
        bool isNotClimbing = playerstance != PlayerStance.Climb;
        if (isInFrontClimbingWall && IsGrounded && isNotClimbing)
        {
            PlayerCollider.center = Vector3.up * 1.3f;
            Vector3 offset = (transform.forward * climboffset.z) + (transform.up * climboffset.y);
            transform.position = hit.point - offset;
            playerstance = PlayerStance.Climb;
            rb.useGravity = false;
            speed = ClimbSpeed;
            cameraManager.setFPSClampedCamera(true, transform.rotation.eulerAngles);
            cameraManager.SetTpsFOV(70);
            animator.SetBool("IsClimbing", true);
        }
    }

    private void cancellClimb ()
    {
        if (playerstance == PlayerStance.Climb)
        {
            PlayerCollider.center = Vector3.up * 0.9f;
            playerstance = PlayerStance.Stand;
            rb.useGravity = true;
            transform.position -= transform.forward;
            speed = walkspeed;
            cameraManager.setFPSClampedCamera(false, transform.rotation.eulerAngles);
            cameraManager.SetTpsFOV(40);
            animator.SetBool("IsClimbing", false);


        }
    }
    
    private void ChangePerspective ()
    {
        animator.SetTrigger("ChangePerspective");
    }

    private void Crouch ()
    {
        if (playerstance == PlayerStance.Stand)
        {
            playerstance = PlayerStance.Crouch;
            animator.SetBool("IsCrouch", true);
            PlayerCollider.height = 1.3f;
            PlayerCollider.center = Vector3.up * 0.66f;
            speed = CrouchSpedd;
            
        }

        else if (playerstance == PlayerStance.Crouch)
        {
            playerstance = PlayerStance.Stand;
            animator.SetBool("IsCrouch", false);
            PlayerCollider.height = 1.8f;
            PlayerCollider.center = Vector3.up * 0.9f;
            speed = walkspeed;
        }
    }

    private void Glide ()
    {
        if (playerstance == PlayerStance.Glide)
        {
            Vector3 PlayerRotation = transform.rotation.eulerAngles;
            float lift = PlayerRotation.x;
            Vector3 Upforce = transform.up * (lift * AirDrag);
            Vector3 forwardForce = transform.forward * GlideSpeed;
            Vector3 totalForce = Upforce + forwardForce;
            rb.AddForce(totalForce * Time.deltaTime);
        }
    }

    private void StartGlide ()
    {
        if (playerstance != PlayerStance.Glide && !IsGrounded)
        {
            playerstance = PlayerStance.Glide;
            animator.SetBool("IsGliding", true);
            cameraManager.setFPSClampedCamera(true, transform.rotation.eulerAngles);
            playerAudioManager.PlayGlidingSFX();
        }
    }

    private void CancellGlide ()
    {
        if (playerstance == PlayerStance.Glide)
        {
            playerstance = PlayerStance.Stand;
            animator.SetBool("IsGliding", false);
            cameraManager.setFPSClampedCamera(false, transform.rotation.eulerAngles);
            playerAudioManager.StopGlidingSFX();
        }
    }

    private void Punch ()
    {
        if (!Ispunching && playerstance == PlayerStance.Stand)
        {
            Ispunching = true;
            if (combo < 3)
            {
                combo = combo + 1;
            }
            else
            {
                combo = 1;
            }
        }
        animator.SetInteger("Combo", combo);
        animator.SetTrigger("Punch");
    }
    private void EndPunch ()
    {
        Ispunching = false;
        if (ComboReset != null)
        {
            StopCoroutine(ComboReset);
        }
        ComboReset = StartCoroutine(ResetCombo());
    }

    private IEnumerator ResetCombo ()
    {
        yield return new WaitForSeconds (ResetComboInterval);
        combo = 0;
    }

    private void Hit ()
    {
        Collider[] HitObjects = Physics.OverlapSphere(HitDetector.position, HitDetectorRadius, HitLayer);
        for (int i = 0; i < HitObjects.Length; i++)
        {
            if (HitObjects[i].gameObject != null)
            {
                Destroy(HitObjects[i].gameObject);
            }
        }
    }
}
