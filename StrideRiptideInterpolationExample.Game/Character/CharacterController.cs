using System;
using System.Collections.Generic;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using StrideRiptideInterpolationExample.Messages;

namespace StrideRiptideInterpolationExample.Character
{
    public class CharacterController : SyncScript
    {
        public CharacterComponent CharacterComponent { get; set; }
        public RigidbodyComponent RigidbodyComponent { get; set; }
        public CharacterInputController CharacterInput { get; set; }
        public CharacterCameraController CharacterCamera { get; set; }
        public Entity CharacterModel { get; set; }
        public float MaxRunSpeed { get; set; }
        public float JumpReactionThreshold { get; set; }

        [DataMemberIgnore]
        public Vector3 MoveDirection { get; set; }

        [DataMemberIgnore]
        public float RunSpeed { get; set; }

        [DataMemberIgnore]
        public bool IsJumped { get; set; }

        [DataMemberIgnore]
        public bool IsGrounded
        {
            get => IsLocalPlayer ? CharacterComponent.IsGrounded : _isGrounded;
            set => _isGrounded = value;
        }

        [DataMemberIgnore]
        public ushort PlayerId { get; set; }

        [DataMemberIgnore]
        public bool IsLocalPlayer { get; set; }

        [DataMemberIgnore]
        public Vector3 CurrentPosition
        {
            get => Entity.Transform.Position;
            set => Entity.Transform.Position = value;
        }

        [DataMemberIgnore]
        public Quaternion CurrentRotation
        {
            get => CharacterModel.Transform.Rotation;
            set => CharacterModel.Transform.Rotation = value;
        }

        [DataMemberIgnore]
        public SortedList<double, CharacterActionData> FutureActions { get; set; } = [];

        private Simulation _simulation;
        private float _jumpReactionRemaining;
        private Vector3 _moveDirection;
        private bool _isGrounded;

        public override void Start()
        {
            _simulation = this.GetSimulation();
            _jumpReactionRemaining = JumpReactionThreshold;
            _moveDirection = Vector3.Zero;

            base.Start();
        }

        public override void Update()
        {
            if (IsLocalPlayer)
            {
                Move();
                Jump();
            }
            else
            {
                this.Interpolate();
            }
        }

        private void Move()
        {
            _moveDirection = _moveDirection * 0.85f + MoveDirection * 0.15f;
            CharacterComponent.SetVelocity(_moveDirection * MaxRunSpeed);
            RunSpeed = _moveDirection.Length();
            if (RunSpeed > 0.001f)
            {
                var yawOrientation = MathUtil.RadiansToDegrees((float)Math.Atan2(-_moveDirection.Z, _moveDirection.X) + MathUtil.PiOverTwo);
                CurrentRotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(yawOrientation), 0, 0);
            }
        }

        private void Jump()
        {
            if (JumpReactionThreshold <= 0 && !IsGrounded)
                return;

            if (JumpReactionThreshold > 0)
            {
                if (_jumpReactionRemaining > 0)
                    _jumpReactionRemaining -= _simulation.FixedTimeStep;

                if (IsGrounded)
                    _jumpReactionRemaining = JumpReactionThreshold;

                if (_jumpReactionRemaining <= 0)
                    return;
            }

            if (!IsJumped)
                return;

            _jumpReactionRemaining = 0;
            CharacterComponent.Jump();
        }
    }
}
