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

    float _jumpVelocity;
    bool _isGrounded;

    Vector3 _hipTargetPos;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _rigidbody = GetComponent<Rigidbody>();
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

        _controller.Move(_speed * Time.deltaTime * transform.forward);
        _controller.Move(_jumpVelocity * Time.deltaTime * Vector3.up);

        _jumpVelocity -= _gravity * Time.deltaTime;
        print(_controller.velocity);
        print(_jumpVelocity);

        _legBounce = Mathf.Clamp01(_legBounce + Time.deltaTime * (_isGrounded ? _bounceIncreaseRate : -_bounceDecreaseRate));
    }

    void Bounce()
    {
        var left = _anim.GetIKPosition(AvatarIKGoal.LeftFoot);
        var right = _anim.GetIKPosition(AvatarIKGoal.RightFoot);

        var leftRay = new Ray(left + _hipHeight / 2 * Vector3.up, Vector3.down);
        var rightRay = new Ray(right + _hipHeight / 2 * Vector3.up, Vector3.down);

        Physics.Raycast(leftRay, out var leftHit, _hipHeight, _groundLayerMask);
        Physics.Raycast(rightRay, out var rightHit, _hipHeight, _groundLayerMask);

        var between = (leftHit.point + rightHit.point) / 2;

        var diff = between.y - transform.position.y;
        if (diff > 0)
        {
            _controller.Move(diff * _legBounce * Vector3.up);
        }

        print($"leftFoot: {leftHit.collider != null}, rightFoot:{rightHit.collider != null}");

        var hip = _anim.GetBoneTransform(HumanBodyBones.Hips).position.y - between.y;
        print(hip);
        if (_jumpVelocity <= 0 && hip < _hipHeight)
        {
            _isGrounded = true;
            _anim.SetBool("IsGrounded", true);
            _jumpVelocity = Mathf.Lerp(_jumpVelocity, 0, _legBounce);
        }
        else
        {
            _isGrounded = false;
            _anim.SetBool("IsGrounded", false);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Bounce();
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
            _jumpVelocity = (_jumpHeight + _gravity * _jumpTime * _jumpTime / 2) / _jumpTime;
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
