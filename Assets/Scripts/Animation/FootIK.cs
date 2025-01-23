using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class FootIK : MonoBehaviour
{
    [SerializeField] bool _enableHint;
    [SerializeField] float _rayDistance = 1;
    [SerializeField] float _footHeight = 0.2f;
    [Header("LeftFoot")]
    [SerializeField] Transform _left;
    [SerializeField, Range(0, 1)] float _leftPositionWeight = 1;
    [SerializeField, Range(0, 1)] float _leftRotationWeight = 1;
    [Header("RightFoot")]
    [SerializeField] Transform _right;
    [SerializeField, Range(0, 1)] float _rightPositionWeight = 1;
    [SerializeField, Range(0, 1)] float _rightRotationWeight = 1;
    [Header("LeftKnee")]
    [SerializeField] Transform _leftKneeAnchor;
    [SerializeField, Range(0, 1)] float _leftKneeWeight = 1;
    [Header("RightKnee")]
    [SerializeField] Transform _rightKneeAnchor;
    [SerializeField, Range(0, 1)] float _rightKneeWeight = 1;

    Animator _anim;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        print(_anim.GetFloat("LeftIKWeight"));
        _leftPositionWeight = _anim.GetFloat("LeftIKWeight");
        _leftRotationWeight = _anim.GetFloat("LeftIKWeight");
        _rightPositionWeight = _anim.GetFloat("RightIKWeight");
        _rightRotationWeight = _anim.GetFloat("RightIKWeight");

        SetFootIK(AvatarIKGoal.LeftFoot, _leftPositionWeight, _leftRotationWeight);
        SetHintIK(AvatarIKHint.LeftKnee, _leftKneeWeight, _leftKneeAnchor);

        SetFootIK(AvatarIKGoal.RightFoot, _rightPositionWeight, _rightRotationWeight);
        SetHintIK(AvatarIKHint.RightKnee, _rightKneeWeight, _rightKneeAnchor);
    }

    private void SetFootIK(AvatarIKGoal part, float positionWeight, float rotationWeight)
    {
        var toHip = _anim.GetBoneTransform(HumanBodyBones.Hips).position.y - _anim.GetIKPosition(part).y;
        Ray ray = new(_anim.GetIKPosition(part) + Vector3.up * toHip, Vector3.down);

        if (Physics.Raycast(ray, out var hit, _rayDistance))
        {
            var newIKPos = hit.point;
            newIKPos.y += _footHeight;

            _anim.SetIKPositionWeight(part, positionWeight);
            _anim.SetIKRotationWeight(part, rotationWeight);
            _anim.SetIKPosition(part, newIKPos);

            Debug.DrawRay(hit.point, hit.normal);
            Debug.DrawRay(hit.point, Vector3.ProjectOnPlane(transform.forward, hit.normal));

            _anim.SetIKRotation(part, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal));
        }
    }

    private void SetHintIK(AvatarIKHint part, float positionWeight, Transform anchor)
    {
        if (_enableHint && anchor)
        {
            _anim.SetIKHintPositionWeight(part, positionWeight);
            _anim.SetIKHintPosition(part, anchor.position);
        }
    }
}
