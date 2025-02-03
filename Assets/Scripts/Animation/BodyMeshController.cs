using UnityEngine;

public class BodyMeshController : MonoBehaviour
{
    Animator _anim;
    CharacterController _controller;
    PlayerController _player;

    [Header("IK")]
    [SerializeField] float _hipHeight = 1f;
    [SerializeField] float _maxIKLength = 0.5f;
    [SerializeField] Transform _renderer;


    Vector3 _defaultCenter;
    float _defaultHeight;

    private void Start()
    {
        _anim = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _player = GetComponent<PlayerController>();

        _defaultCenter = _controller.center;
        _defaultHeight = _controller.height;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        var left = _anim.GetIKPosition(AvatarIKGoal.LeftFoot);
        var right = _anim.GetIKPosition(AvatarIKGoal.RightFoot);

        var leftFloating = _anim.GetBoneTransform(HumanBodyBones.LeftFoot).position.y - left.y;
        var rightFloating = _anim.GetBoneTransform(HumanBodyBones.RightFoot).position.y - right.y;

        var floatingHeight = Mathf.Max(leftFloating, rightFloating);

        var yOffset = Mathf.Abs(left.y - right.y);

        var mag = _player.HorizontalVelocityMagnitude;
        var scale = mag > 1 ? 1 / mag : 1;

        if (yOffset < _maxIKLength)
            AdjustControllerWithFootIK(yOffset * scale);
        else
            AdjustControllerWithFootIK(0);
    }

    void AdjustControllerWithFootIK(float yOffset)
    {
        _controller.center = _defaultCenter + Vector3.up * yOffset / 2;
        _controller.height = _defaultHeight - yOffset;

        //_anim.bodyPosition = Vector3.Lerp(_anim.bodyPosition, _anim.bodyPosition + Vector3.down * yOffset, 0.1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _hipHeight, 0.05f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _hipHeight);
    }
}
