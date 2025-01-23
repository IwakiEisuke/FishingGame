using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _animationSpeed = 1f;
    Vector2 _input;
    Animator _anim;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    void Update()
    {
        _anim.speed = _animationSpeed;

        transform.Rotate(Vector3.up, _input.x);
    }

    void OnMove(InputValue value)
    {
        _input = value.Get<Vector2>();
        print(_input);

        _anim.SetFloat("h", _input.y);
    }
}
