using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float _maxSpeed = 3f;
    [SerializeField] float _animationSpeed = 1f;
    [SerializeField, Range(0, 1)] float _moveDirectionSpeed = 0.2f;
    [SerializeField, Range(0, 1)] float _moveDamping = 0.2f;
    [SerializeField, Range(0, 10)] float _moveStartDamp = 0.2f;
    [SerializeField, Range(0, 1)] float _rotationSpeed = 0.2f;
    [Header("Jump")]
    [SerializeField] float _jumpHeight = 1f;
    [SerializeField] float _gravity = 9.8f;
    [SerializeField] LayerMask _groundLayerMask;
    [Header("Animation")]
    [SerializeField] float _dampTime = 0.05f; // アニメーション遷移
    [SerializeField, Range(0, 1)] float _legBounce = 0.5f;
    [SerializeField, Range(0.01f, 5f)] float _bounceDecreaseRate = 0.2f;
    [SerializeField, Range(0.01f, 5f)] float _bounceIncreaseRate = 1.5f;

    Vector2 _rawInput;
    Vector2 _input;
    Animator _anim;
    Rigidbody _rb;
    CharacterController _controller;

    float _speed;
    float _upwardVelocity;

    public Vector3 Velocity { get; private set; }
    public float HorizontalVelocityMagnitude => Mathf.Sqrt(Velocity.x * Velocity.x + Velocity.z * Velocity.z);

    void Start()
    {
        _anim = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (_rawInput.magnitude > 0.1f)
        {
            _moveDamping += Time.deltaTime * _moveStartDamp;
        }
        else
        {
            _moveDamping -= Time.deltaTime * _moveStartDamp;
        }
        _moveDamping = Mathf.Clamp01(_moveDamping);

        _input = Vector2.MoveTowards(_input, _rawInput, _moveDirectionSpeed);
        _anim.speed = _animationSpeed;

        _speed = _input.magnitude * _maxSpeed * _moveDamping;

        _anim.SetFloat("InputMagnitude", _input.magnitude, _dampTime, Time.deltaTime);
        _anim.SetFloat("Speed", _speed, _dampTime, Time.deltaTime);

        var cameraLook = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        var moveDir = cameraLook * new Vector3(_input.x, 0, _input.y);

        if (_input != Vector2.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, _rotationSpeed);
        }

        var horizontalVelocity = _speed * _animationSpeed * moveDir;
        _controller.Move(horizontalVelocity * Time.deltaTime);
        _controller.Move(_upwardVelocity * Time.deltaTime * Vector3.up);

        if (_controller.isGrounded)
        {
            _anim.SetBool("IsGrounded", true);
            _upwardVelocity = Mathf.Lerp(_upwardVelocity, 0, _legBounce);
        }
        else
        {
            _anim.SetBool("IsGrounded", false);
        }

        _upwardVelocity -= _gravity * Time.deltaTime;
        _legBounce = Mathf.Clamp01(_legBounce + Time.deltaTime * (_controller.isGrounded ? _bounceIncreaseRate : -_bounceDecreaseRate));

        Velocity = horizontalVelocity + _upwardVelocity * Vector3.up;
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label($"_controller.velocity: {_controller.velocity}");
        GUILayout.Label($"_upperVelocity: {_upwardVelocity}");
        GUILayout.Label($"isGrounded: {_controller.isGrounded}");
        GUILayout.EndVertical();
    }

    void OnMove(InputValue value)
    {
        _rawInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            var _jumpTime = Mathf.Sqrt(2 * _jumpHeight / _gravity);
            _upwardVelocity = (_jumpHeight + _gravity * _jumpTime * _jumpTime / 2) / _jumpTime;
            _anim.Play("Jump");
        }
    }
}
