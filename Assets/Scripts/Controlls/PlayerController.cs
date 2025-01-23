using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _animationSpeed = 1f;
    [SerializeField] float _bounce = 0.5f;
    [SerializeField] float _hipHeight = 1f;
    Vector2 _input;
    Animator _anim;
    CharacterController _controller;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        _anim.speed = _animationSpeed;

        transform.Rotate(Vector3.up, _input.x * 3);
    }

    void Bounce()
    {
        var left = _anim.GetIKPosition(AvatarIKGoal.LeftFoot);
        var right = _anim.GetIKPosition(AvatarIKGoal.RightFoot);
        var between = (left + right) / 2;
        var ray = new Ray(between + Vector3.up * (_hipHeight / 2), Vector3.down);

        if (Physics.Raycast(ray, out var hit, _hipHeight))
        {
            var diff = hit.point.y - transform.position.y;
            print(diff);
            _controller.transform.position += (diff * _bounce * Time.deltaTime * Vector3.up);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Bounce();
    }

    void OnMove(InputValue value)
    {
        _input = value.Get<Vector2>();
        print(_input);

        _anim.SetFloat("h", _input.x);
        _anim.SetFloat("v", _input.y);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _hipHeight);
    }
}
