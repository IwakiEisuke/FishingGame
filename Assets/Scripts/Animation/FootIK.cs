using UnityEngine;

public class FootIK : MonoBehaviour
{
    [SerializeField] bool _enableHint;
    [SerializeField] LayerMask _layerMask;
    [Header("General")]
    [SerializeField] float _footHeight = 0.2f;
    [SerializeField] float _footRadius = 0.2f;
    //[SerializeField, Tooltip("踵から爪先までの長さ（未使用）")] float _lengthFromHeelToToes = 0.1f;
    [SerializeField, Tooltip("足を上げる最大の高さ")] float _maxIKLength = 0.5f;
    [SerializeField, Tooltip("踵と爪先の最大高低差")] float _maxSlopeHeight = 0.1f;
    [SerializeField, Tooltip("足をIKターゲットに持っていく速さ")] float _footMoveSpeed = 0.2f;
    [Header("LeftFoot")]
    [SerializeField, Range(0, 1)] float _leftPositionWeight = 1;
    [SerializeField, Range(0, 1)] float _leftRotationWeight = 1;
    [Header("RightFoot")]
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

        SetFootIK(FootSide.Left, _leftPositionWeight, _leftRotationWeight);
        SetHintIK(AvatarIKHint.LeftKnee, _leftKneeWeight, _leftKneeAnchor);

        SetFootIK(FootSide.Right, _rightPositionWeight, _rightRotationWeight);
        SetHintIK(AvatarIKHint.RightKnee, _rightKneeWeight, _rightKneeAnchor);
    }

    private void SetFootIK(FootSide side, float positionWeight, float rotationWeight)
    {
        var part = side == FootSide.Left ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
        var bone = side == FootSide.Left ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes;

        var ikLength = Vector3.up * _maxIKLength;

        var heelRay = new Ray(_anim.GetIKPosition(part) + ikLength, Vector3.down);
        var toesRay = new Ray(_anim.GetBoneTransform(bone).transform.position + ikLength, Vector3.down);

        var isHitHeel = Physics.Raycast(heelRay, out var heelHit, _layerMask);
        if (!isHitHeel) heelHit.point = heelRay.origin + heelRay.direction * (_maxIKLength + _maxSlopeHeight);

        var isHitToes = Physics.Raycast(toesRay, out var toesHit, _layerMask);
        if (!isHitToes) toesHit.point = toesRay.origin + toesRay.direction * (_maxIKLength + _maxSlopeHeight);

        var firstHit = heelHit.point.y > toesHit.point.y ? FootParts.Heel : FootParts.Toes;

        var targetPos = Vector3.zero;
        var targetRot = Quaternion.identity;

        if (isHitHeel && isHitToes)
        {
            var slopeHeight = Mathf.Abs(toesHit.point.y - heelHit.point.y);

            if (slopeHeight < _maxSlopeHeight)
            {
                targetRot = Quaternion.LookRotation(toesHit.point - heelHit.point);
            }
            else
            {
                var footLook = toesHit.point - heelHit.point;
                footLook.y = 0;
                targetRot = Quaternion.LookRotation(footLook);
            }

            if (firstHit == FootParts.Heel)
            {
                targetPos = heelHit.point;
            }
            else
            {
                targetPos = new Vector3(heelHit.point.x, toesHit.point.y, heelHit.point.z);
            }
        }
        else if (isHitHeel)
        {
            targetPos = heelHit.point;
            targetRot = _anim.GetIKRotation(part);
        }
        else if (isHitToes)
        {
            targetPos = new Vector3(heelHit.point.x, toesHit.point.y, heelHit.point.z);
            targetRot = _anim.GetIKRotation(part);
        }

        targetPos.y += _footHeight;

        var posWeight = side == FootSide.Left ? _leftPositionWeight : _rightPositionWeight;
        var rotWeight = side == FootSide.Left ? _leftRotationWeight : _rightRotationWeight;

        _anim.SetIKPositionWeight(part, positionWeight);
        _anim.SetIKRotationWeight(part, rotationWeight);

        _anim.SetIKPosition(part, targetPos);
        _anim.SetIKRotation(part, targetRot);

        Debug.DrawRay(heelRay.origin, heelRay.direction * (_maxIKLength + _maxSlopeHeight), Color.magenta);
        Debug.DrawRay(toesRay.origin, toesRay.direction * (_maxIKLength + _maxSlopeHeight), Color.cyan);
        Debug.DrawLine(heelHit.point, toesHit.point, Color.yellow);
        Debug.DrawLine(toesHit.point + Vector3.up * _maxSlopeHeight, toesHit.point - Vector3.up * _maxSlopeHeight, Color.yellow);

        _gismos += () =>
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(heelHit.point, 0.05f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(toesHit.point, 0.05f);
        };
    }

    enum FootParts
    {
        Heel, Toes
    }

    enum FootSide
    {
        Left, Right
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
