﻿using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// Apply eye direction to bone transforms.
    /// </summary>
    internal sealed class LookAtEyeDirectionApplicableToBone : ILookAtEyeDirectionApplicable
    {
        private readonly Transform _leftEye;
        private readonly Quaternion _leftEyePreMultiplyRotation;
        private readonly Quaternion _leftEyePostMultiplyRotation;

        private readonly Transform _rightEye;
        private readonly Quaternion _rightEyePreMultiplyRotation;
        private readonly Quaternion _rightEyePostMultiplyRotation;

        private readonly CurveMapper _horizontalOuter;
        private readonly CurveMapper _horizontalInner;
        private readonly CurveMapper _verticalDown;
        private readonly CurveMapper _verticalUp;
        
        public LookAtEyeDirectionApplicableToBone(Transform leftEye, Transform rightEye,
            CurveMapper horizontalOuter, CurveMapper horizontalInner, CurveMapper verticalDown, CurveMapper verticalUp)
        {
            _leftEye = leftEye;
            var leftEyeRotation = _leftEye.rotation;
            _leftEyePreMultiplyRotation = _leftEye.localRotation * Quaternion.Inverse(leftEyeRotation);
            _leftEyePostMultiplyRotation = leftEyeRotation;

            _rightEye = rightEye;
            var rightEyeRotation = _rightEye.rotation;
            _rightEyePreMultiplyRotation = _rightEye.localRotation * Quaternion.Inverse(rightEyeRotation);
            _rightEyePostMultiplyRotation = rightEyeRotation;

            _horizontalOuter = horizontalOuter;
            _horizontalInner = horizontalInner;
            _verticalDown = verticalDown;
            _verticalUp = verticalUp;
        }
        
        public void Apply(LookAtEyeDirection eyeDirection, Dictionary<ExpressionKey, float> actualWeights)
        {
            // FIXME
            var yaw = eyeDirection.Yaw;
            var pitch = eyeDirection.Pitch;

            // horizontal
            float leftYaw, rightYaw;
            if (yaw < 0)
            {
                leftYaw = -_horizontalOuter.Map(-yaw);
                rightYaw = -_horizontalInner.Map(-yaw);
            }
            else
            {
                rightYaw = _horizontalOuter.Map(yaw);
                leftYaw = _horizontalInner.Map(yaw);
            }

            // vertical
            if (pitch < 0)
            {
                pitch = -_verticalDown.Map(-pitch);
            }
            else
            {
                pitch = _verticalUp.Map(pitch);
            }

            SetYawPitchToBones(new LookAtEyeDirection(leftYaw, pitch));
        }

        public void Restore()
        {
            SetYawPitchToBones(new LookAtEyeDirection(0, 0));
        }

        private void SetYawPitchToBones(LookAtEyeDirection eyeDirection)
        {
            if (_leftEye != null && _rightEye != null)
            {
                _leftEye.localRotation = _leftEyePreMultiplyRotation *
                                         Quaternion.Euler(-eyeDirection.Pitch, eyeDirection.Yaw, 0) *
                                         _leftEyePostMultiplyRotation;
                _rightEye.localRotation = _rightEyePreMultiplyRotation *
                                         Quaternion.Euler(-eyeDirection.Pitch, eyeDirection.Yaw, 0) *
                                         _rightEyePostMultiplyRotation;
            }
        }
    }
}