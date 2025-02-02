using UnityEngine;

public class FootIK : MonoBehaviour
{
    [SerializeField] bool _enableHint;
    [SerializeField] LayerMask _layerMask;
    [Header("General")]
    [SerializeField] float _footHeight = 0.2f;
    [SerializeField] float _footRadius = 0.2f;
    [SerializeField, Tooltip("踵から爪先までの長さ（未使用）")] float _lengthFromHeelToToes = 0.1f;
    [SerializeField, Tooltip("足を上げる最大の高さ")] float _maxIKLength = 0.5f;
    [SerializeField, Tooltip("踵と爪先の最大高低差")] float _maxToesHeight = 0.1f;
    [SerializeField, Tooltip("踵・爪先の位置から探索する地面までの距離")] float _groundCheckRayDistance = 1;
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
    System.Action _gismos;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _leftPositionWeight = _anim.GetFloat("LeftIKWeight");
        _leftRotationWeight = _anim.GetFloat("LeftIKWeight");
        _rightPositionWeight = _anim.GetFloat("RightIKWeight");
        _rightRotationWeight = _anim.GetFloat("RightIKWeight");

        SetFootIK(AvatarIKGoal.LeftFoot, HumanBodyBones.LeftToes, _leftPositionWeight, _leftRotationWeight);
        SetHintIK(AvatarIKHint.LeftKnee, _leftKneeWeight, _leftKneeAnchor);

        SetFootIK(AvatarIKGoal.RightFoot, HumanBodyBones.RightToes, _rightPositionWeight, _rightRotationWeight);
        SetHintIK(AvatarIKHint.RightKnee, _rightKneeWeight, _rightKneeAnchor);
    }

    private void SetFootIK(AvatarIKGoal part, HumanBodyBones t, float positionWeight, float rotationWeight)
    {
        var heelOrigin = _anim.GetIKPosition(part);
        Ray heelRay = new(heelOrigin + Vector3.up * _maxIKLength, Vector3.down);

        var toesOrigin = _anim.GetBoneTransform(t).transform.position;
        Ray toesRay = new(toesOrigin + Vector3.up * _maxIKLength, Vector3.down);

        var isHitHeel = Physics.Raycast(heelRay, out var heelHit, _maxIKLength + _groundCheckRayDistance, _layerMask);
        var isHitToes = Physics.Raycast(toesRay, out var toesHit, _maxIKLength + _groundCheckRayDistance, _layerMask);

        var footAngleHeight = Mathf.Abs(heelHit.point.y - toesHit.point.y);

        var newFootForward = toesHit.point - heelHit.point;
        var newFootRotation = Quaternion.LookRotation(newFootForward);

        var newIKPos = heelHit.collider ? heelHit.point : toesHit.collider ? toesHit.point : _anim.GetIKPosition(part);
        newIKPos.y += _footHeight;

        _anim.SetIKPositionWeight(part, positionWeight);
        _anim.SetIKRotationWeight(part, rotationWeight); 

        _anim.SetIKPosition(part, newIKPos);
        _anim.SetIKRotation(part, newFootRotation);


        Debug.DrawRay(heelRay.origin, heelRay.direction * (_maxIKLength + _groundCheckRayDistance), Color.magenta);
        Debug.DrawRay(toesRay.origin, toesRay.direction * (_maxIKLength + _groundCheckRayDistance), Color.cyan);
        Debug.DrawLine(heelHit.point, toesHit.point, Color.yellow);

        _gismos += () =>
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(heelHit.point, 0.05f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(toesHit.point, 0.05f);
        };
    }

    private void SetHintIK(AvatarIKHint part, float positionWeight, Transform anchor)
    {
        if (_enableHint && anchor)
        {
            _anim.SetIKHintPositionWeight(part, positionWeight);
            _anim.SetIKHintPosition(part, anchor.position);
        }
    }

    private void OnDrawGizmos()
    {
        _gismos?.Invoke();
        _gismos = null;
    }
}
