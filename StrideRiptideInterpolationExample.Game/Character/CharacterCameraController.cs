using System;
using System.Collections.Generic;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;

namespace StrideRiptideInterpolationExample.Character
{
    public class CharacterCameraController : SyncScript
    {
        public CameraComponent CameraComponent { get; set; }
        public float DefaultDistance { get; set; }
        public float MinimumDistance { get; set; }
        public float ConeRadius { get; set; }
        public float MinVerticalAngle { get; set; }
        public float MaxVerticalAngle { get; set; }
        public float RotationSpeed { get; set; }
        public float VerticalSpeed { get; set; }

        [DataMemberIgnore]
        public Vector2 CameraDirection { get; set; }

        private readonly List<HitResult> _resultsOutput = [];
        private Vector3 _cameraRotationXYZ = new(-20, 45, 0);
        private Vector3 _targetRotationXYZ = new(-20, 45, 0);
        private ConeColliderShape _coneShape;
        private Simulation _simulation;
        private TransformComponent _cameraTarget;

        public override void Start()
        {
            _coneShape = new ConeColliderShape(DefaultDistance, ConeRadius, ShapeOrientation.UpZ);
            _simulation = this.GetSimulation();
            _cameraTarget = Entity.GetParent().Transform;

            base.Start();
        }

        public override void Update()
        {
            UpdateCameraRaycast();
            UpdateCameraOrientation();
        }

        private void UpdateCameraRaycast()
        {
            var maxLength = DefaultDistance;
            var cameraVector = new Vector3(0, 0, DefaultDistance);
            _cameraTarget.Rotation.Rotate(ref cameraVector);
            if (ConeRadius <= 0)
            {
                var raycastStart = _cameraTarget.WorldMatrix.TranslationVector;
                var hitResult = _simulation.Raycast(raycastStart, raycastStart + cameraVector);
                if (hitResult.Succeeded)
                    maxLength = Math.Min(DefaultDistance, (raycastStart - hitResult.Point).Length());
            }
            else
            {
                var fromMatrix = Matrix.Translation(0, 0, -DefaultDistance * 0.5f) * _cameraTarget.WorldMatrix;
                var toMatrix = Matrix.Translation(0, 0, DefaultDistance * 0.5f) * _cameraTarget.WorldMatrix;
                _resultsOutput.Clear();
                _simulation.ShapeSweepPenetrating(_coneShape, fromMatrix, toMatrix, _resultsOutput, CollisionFilterGroups.DefaultFilter, CollisionFilterGroupFlags.DefaultFilter);
                foreach (var result in _resultsOutput)
                {
                    if (result.Succeeded)
                    {
                        var signedVector = result.Point - _cameraTarget.WorldMatrix.TranslationVector;
                        var signedDistance = Vector3.Dot(cameraVector, signedVector);
                        var currentLength = DefaultDistance * result.HitFraction;
                        if (signedDistance > 0 && currentLength < maxLength)
                            maxLength = currentLength;
                    }
                }
            }

            if (maxLength < MinimumDistance)
                maxLength = MinimumDistance;

            Entity.Transform.Position.Z = maxLength;
        }

        private void UpdateCameraOrientation()
        {
            _targetRotationXYZ.X += CameraDirection.Y * VerticalSpeed;
            _targetRotationXYZ.Y -= CameraDirection.X * RotationSpeed;
            _targetRotationXYZ.X = Math.Max(_targetRotationXYZ.X, -MaxVerticalAngle);
            _targetRotationXYZ.X = Math.Min(_targetRotationXYZ.X, -MinVerticalAngle);
            _cameraRotationXYZ = Vector3.Lerp(_cameraRotationXYZ, _targetRotationXYZ, 0.25f);
            _cameraTarget.RotationEulerXYZ = new Vector3(MathUtil.DegreesToRadians(_cameraRotationXYZ.X), MathUtil.DegreesToRadians(_cameraRotationXYZ.Y), 0);
        }
    }
}
