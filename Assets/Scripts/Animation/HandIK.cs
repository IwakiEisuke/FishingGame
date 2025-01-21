using UnityEngine;

public class HandIK : MonoBehaviour
{
    [SerializeField] bool _enableHint;
    [Header("LeftHand")]
    [SerializeField] Transform _leftHandAnchor;
    [SerializeField, Range(0, 1)] float _leftPositionWeight = 1;
    [SerializeField, Range(0, 1)] float _leftRotationWeight = 1;
    [Header("RightHand")]
    [SerializeField] Transform _rightHandAnchor;
    [SerializeField, Range(0, 1)] float _rightPositionWeight = 1;
    [SerializeField, Range(0, 1)] float _rightRotationWeight = 1;
    [Header("LightElbow")]
    [SerializeField] Transform _leftElbowAnchor;
    [SerializeField, Range(0, 1)] float _leftElbowWeight = 1;
    [Header("RightElbow")]
    [SerializeField] Transform _rightElbowAnchor;
    [SerializeField, Range(0, 1)] float _rightElbowWeight = 1;

    Animator _anim;
    
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_leftHandAnchor)
        {
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, _leftPositionWeight);
            _anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, _leftRotationWeight);
            _anim.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandAnchor.position);
            _anim.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandAnchor.rotation);

            if (_enableHint && _leftElbowAnchor)
            {
                _anim.SetIKHintPosition(AvatarIKHint.LeftElbow, _leftElbowAnchor.position);
                _anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, _leftElbowWeight);
            }
        }

        if (_rightHandAnchor)
        {
            _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, _rightPositionWeight);
            _anim.SetIKRotationWeight(AvatarIKGoal.RightHand, _rightRotationWeight);
            _anim.SetIKPosition(AvatarIKGoal.RightHand, _rightHandAnchor.position);
            _anim.SetIKRotation(AvatarIKGoal.RightHand, _rightHandAnchor.rotation);

            if (_enableHint && _rightElbowAnchor)
            {
                _anim.SetIKHintPosition(AvatarIKHint.RightElbow, _rightElbowAnchor.position);
                _anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, _rightElbowWeight);
            }
        }
    }
}
