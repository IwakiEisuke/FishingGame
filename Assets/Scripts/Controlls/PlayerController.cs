using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float _maxSpeed = 3f;
    [SerializeField] float _animationSpeed = 1f;
    [SerializeField, Range(0, 1)] float _moveDirectionSpeed = 0.2f;
    [SerializeField, Range(0, 1)] float _rotationSpeed = 0.2f;
    [Header("Jump")]
    [SerializeField] float _jumpHeight = 1f;
    [SerializeField] float _gravity = 9.8f;
    [SerializeField] LayerMask _groundLayerMask;
    [Header("IK")]
    [SerializeField, Range(0, 1)] float _legBounce = 0.5f;
    [SerializeField, Range(0.01f, 5f)] float _bounceDecreaseRate = 0.2f;
    [SerializeField, Range(0.01f, 5f)] float _bounceIncreaseRate = 1.5f;
    [SerializeField] float _hipHeight = 1f;
    [SerializeField] float _dampTime = 0.05f; // アニメーション遷移

    Vector2 _rawInput;
    Vector2 _input;
    Animator _anim;
    Rigidbody _rigidbody;
    CharacterController _controller;

    float _speed;

    float _upperVelocity;
    bool _isGrounded;
    bool _isStanding;

    Vector3 _hipTargetPos;

    Vector3 _defaultCenter;
    float _defaultHeight;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _rigidbody = GetComponent<Rigidbody>();
        _defaultCenter = _controller.center;
        _defaultHeight = _controller.height;
    }

    void Update()
    {
        _input = Vector2.MoveTowards(_input, _rawInput, _moveDirectionSpeed);
        _anim.speed = _animationSpeed;

        _speed = _input.magnitude * _maxSpeed;

        _anim.SetFloat("InputMagnitude", _input.magnitude, _dampTime, Time.deltaTime);
        _anim.SetFloat("Speed", _speed, _dampTime, Time.deltaTime);

        if (_input != Vector2.zero)
        {
            var cameraLook = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
            var moveDir = Quaternion.LookRotation(cameraLook) * new Vector3(_input.x, 0, _input.y);

            transform.forward = Vector3.Slerp(transform.forward, moveDir, _rotationSpeed);
        }

        _controller.Move(_speed * _animationSpeed * Time.deltaTime * transform.forward);
        _controller.Move(_upperVelocity * Time.deltaTime * Vector3.up);

        _upperVelocity -= _gravity * Time.deltaTime;
        print(_controller.velocity);
        print(_upperVelocity);

        _legBounce = Mathf.Clamp01(_legBounce + Time.deltaTime * (_isGrounded ? _bounceIncreaseRate : -_bounceDecreaseRate));
    }

    private void OnAnimatorIK(int layerIndex)
    {
        var left = _anim.GetIKPosition(AvatarIKGoal.LeftFoot);
        var right = _anim.GetIKPosition(AvatarIKGoal.RightFoot);

        var leftRay = new Ray(left + _hipHeight / 2 * Vector3.up, Vector3.down);
        var rightRay = new Ray(right + _hipHeight / 2 * Vector3.up, Vector3.down);

        Physics.Raycast(leftRay, out var leftHit, _hipHeight, _groundLayerMask);
        Physics.Raycast(rightRay, out var rightHit, _hipHeight, _groundLayerMask);

        var yOffset = Mathf.Abs(leftHit.point.y - rightHit.point.y);
        _controller.center = Vector3.Lerp(_controller.center, _defaultCenter + Vector3.up * yOffset / 2, Time.deltaTime);
        _controller.height = _defaultHeight - yOffset;

        if (_controller.isGrounded)
        {
            _anim.SetBool("IsGrounded", true);
            _upperVelocity = Mathf.Lerp(_upperVelocity, 0, _legBounce);
        }
        else
        {
            _anim.SetBool("IsGrounded", false);
        }
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
            _upperVelocity = (_jumpHeight + _gravity * _jumpTime * _jumpTime / 2) / _jumpTime;
            _isGrounded = false;
            _anim.SetTrigger("Jump");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _hipHeight, 0.05f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _hipHeight);
    }
}
