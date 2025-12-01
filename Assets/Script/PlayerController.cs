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

    private PlayerStance playerstance;
    Rigidbody rb;

    private void Awake()
    {
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
    }

    private void OnDestroy()
    {
        Input.OnMoveInput -= Move;
        Input.SprintInput -= Sprint;
        Input.OnjumpInput -= jump;
        Input.OnClimbInput -= StartClimb;
        Input.OnCancellClimb -= cancellClimb;

    }

    private void Update()
    {
        CheckGrounded();
        CheckStep();
    }

    private void Move (Vector2 AxisDirection)
    {
        Vector3 MovementDirection = Vector3.zero;
        bool isPlayerStanding = playerstance == PlayerStance.Stand;
        bool isPlayerClimbing = playerstance == PlayerStance.Climb;
        if (isPlayerStanding)
        {
            if (AxisDirection.magnitude >= 01)
            {
                float rotationAngle = Mathf.Atan2(AxisDirection.x, AxisDirection.y) * Mathf.Rad2Deg;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref rotationSmoothVelocity, rotationSmooth);
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                MovementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                rb.AddForce(MovementDirection * speed * Time.deltaTime);
            }
        }
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = AxisDirection.x * transform.right;
            Vector3 vertical = AxisDirection.y * transform.up;
            Vector3 movementDirection = horizontal + vertical;
            rb.AddForce(movementDirection * speed * Time.deltaTime);
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
            if (speed > sprintSpeed)
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
        }
    }

    private void CheckGrounded ()
    {
        IsGrounded = Physics.CheckSphere(GroundDetector.position, detectorRadius, GroundLayer);
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
            Vector3 offset = (transform.forward * climboffset.z) + (transform.up * climboffset.y);
            transform.position = hit.point - offset;
            playerstance = PlayerStance.Climb;
            rb.useGravity = false;
            speed = ClimbSpeed;
        }
    }

    private void cancellClimb ()
    {
        if (playerstance == PlayerStance.Climb)
        {
            playerstance = PlayerStance.Stand;
            rb.useGravity = true;
            transform.position -= transform.forward;
            speed = walkspeed;
        }
    }
}
