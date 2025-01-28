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
    [SerializeField] float _dampTime = 0.05f; // �A�j���[�V�����J��

    Vector2 _rawInput;
    Vector2 _input;
    Animator _anim;
    Rigidbody _rb;
    CharacterController _controller;

    float _speed;
    float _upwardVelocity;

    Vector3 _defaultCenter;
    float _defaultHeight;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _rb = GetComponent<Rigidbody>();
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

        var cameraLook = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up);
        var moveDir = cameraLook * new Vector3(_input.x, 0, _input.y);

        if (_input != Vector2.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, _rotationSpeed);
        }

        _controller.Move(_speed * _animationSpeed * Time.deltaTime * moveDir);
        _controller.Move(_upwardVelocity * Time.deltaTime * Vector3.up);

        _upwardVelocity -= _gravity * Time.deltaTime;

        _legBounce = Mathf.Clamp01(_legBounce + Time.deltaTime * (_controller.isGrounded ? _bounceIncreaseRate : -_bounceDecreaseRate));
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label($"_controller.velocity: {_controller.velocity}");
        GUILayout.Label($"_upperVelocity: {_upwardVelocity}");
        GUILayout.Label($"isGrounded: {_controller.isGrounded}");
        GUILayout.EndVertical();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        var left = _anim.GetIKPosition(AvatarIKGoal.LeftFoot);
        var right = _anim.GetIKPosition(AvatarIKGoal.RightFoot);

        var leftRay = new Ray(left + _hipHeight * Vector3.up, Vector3.down);
        var rightRay = new Ray(right + _hipHeight * Vector3.up, Vector3.down);

        Physics.Raycast(leftRay, out var leftHit, _hipHeight * 2, _groundLayerMask);
        Physics.Raycast(rightRay, out var rightHit, _hipHeight * 2, _groundLayerMask);

        var yOffset = Mathf.Abs(leftHit.point.y - rightHit.point.y);

        AdjustControllerWithFootIK(yOffset);

        if (_controller.isGrounded)
        {
            _anim.SetBool("IsGrounded", true);
            _upwardVelocity = Mathf.Lerp(_upwardVelocity, 0, _legBounce);
        }
        else
        {
            _anim.SetBool("IsGrounded", false);
        }
    }

    void AdjustControllerWithFootIK(float yOffset)
    {
        _controller.center = Vector3.MoveTowards(_controller.center, _defaultCenter + Vector3.up * yOffset / 2, Time.deltaTime);
        _controller.height = _defaultHeight - yOffset;
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _hipHeight, 0.05f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _hipHeight);
    }
#endif
}
