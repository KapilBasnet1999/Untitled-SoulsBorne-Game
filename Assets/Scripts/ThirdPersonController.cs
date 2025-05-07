using UnityEngine;

    public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.0f;
    public float sprintSpeed = 5.335f;
    [Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;
    public float speedChangeRate = 10.0f;
    public float airControl = 0.5f;
    public float animationSmoothTime = 0.1f;

    [Header("Jumping")]
    public float jumpHeight = 1.2f;
    public float gravity = -15.0f;
    public float jumpTimeout = 0.5f;
    public float fallTimeout = 0.15f;

    [Header("Ground Check")]
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;
    public LayerMask groundLayers;
    public bool Grounded { get; private set; }

    [Header("Camera")]
    public GameObject cinemachineCameraTarget;
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;

    [Header("Audio Clips")]
    public AudioClip[] footstepSounds;
    public AudioClip landingSound;

    private AudioSource _audioSource;
    private CharacterController _controller;
    private GameObject _mainCamera;
    private Animator _animator;
    private float _speed;
    private float _animationBlend;
    private float _targetRotation;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    // Added for double jump
    private int jumpCount = 0;
    private int maxJumps = 2;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        TryGetComponent(out _animator);

        AssignAnimationIDs();
        _jumpTimeoutDelta = jumpTimeout;
        _fallTimeoutDelta = fallTimeout;
    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnFootstep()
    {
        if (footstepSounds.Length > 0)
        {
            int index = Random.Range(0, footstepSounds.Length);
            _audioSource.PlayOneShot(footstepSounds[index]);
        }
    }

    private void OnLand()
    {
        if (landingSound != null)
            _audioSource.PlayOneShot(landingSound);
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        bool sphereCheck = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        float rayLength = groundedRadius + 0.1f;
        bool rayCheck = Physics.Raycast(transform.position, Vector3.down, rayLength, groundLayers, QueryTriggerInteraction.Ignore);

        Grounded = sphereCheck || rayCheck;

        if (_animator != null)
            _animator.SetBool(_animIDGrounded, Grounded);
    }

    private void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift);

        float inputMagnitude = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);
        float targetSpeed = sprint ? sprintSpeed : walkSpeed;
        targetSpeed *= inputMagnitude;

        if (horizontal == 0 && vertical == 0) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;

        float currentRate = (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            ? speedChangeRate : speedChangeRate * 2f;

        if (!Grounded)
        {
            currentRate *= airControl;
            targetSpeed *= airControl;
        }

        _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * currentRate);
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * animationSmoothTime);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        Vector3 inputDirection = new Vector3(horizontal, 0.0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        if (_animator != null)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }
    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = fallTimeout;

            if (_animator != null)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            if (_verticalVelocity < 0.0f)
                _verticalVelocity = -2f;

            jumpCount = 0;

            if (Input.GetKeyDown(KeyCode.Space) && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpCount++;
                if (_animator != null)
                    _animator.SetBool(_animIDJump, true);
            }

            if (_jumpTimeoutDelta >= 0.0f)
                _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = jumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
                _fallTimeoutDelta -= Time.deltaTime;
            else if (_animator != null)
                _animator.SetBool(_animIDFreeFall, true);

            if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpCount++;
                if (_animator != null)
                    _animator.SetBool(_animIDJump, true);
            }
        }

        if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity += gravity * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = Grounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }
}

