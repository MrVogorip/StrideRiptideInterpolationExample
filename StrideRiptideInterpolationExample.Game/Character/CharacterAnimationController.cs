using System;
using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Stride.Animations;
using Stride.Engine;

namespace StrideRiptideInterpolationExample.Character
{
    public class CharacterAnimationController : SyncScript, IBlendTreeBuilder
    {
        public AnimationComponent AnimationComponent { get; set; }
        public CharacterController Character { get; set; }
        public AnimationClip AnimationIdle { get; set; }
        public AnimationClip AnimationWalk { get; set; }
        public AnimationClip AnimationRun { get; set; }
        public AnimationClip AnimationJumpStart { get; set; }
        public AnimationClip AnimationJumpMid { get; set; }
        public AnimationClip AnimationJumpEnd { get; set; }
        public float WalkThreshold { get; set; }
        public double TimeScale { get; set; }

        private double _currentTime;
        private AnimationClipEvaluator _animEvaluatorIdle;
        private AnimationClipEvaluator _animEvaluatorWalk;
        private AnimationClipEvaluator _animEvaluatorRun;
        private AnimationClipEvaluator _animEvaluatorJumpStart;
        private AnimationClipEvaluator _animEvaluatorJumpMid;
        private AnimationClipEvaluator _animEvaluatorJumpEnd;

        private float _walkLerpFactor;
        private AnimationClipEvaluator _animEvaluatorWalkLerp1;
        private AnimationClipEvaluator _animEvaluatorWalkLerp2;
        private AnimationClip _animationClipWalkLerp1;
        private AnimationClip _animationClipWalkLerp2;
        private AnimationState _state;
        private bool _previousIsGrounded;

        public override void Start()
        {
            AnimationComponent.BlendTreeBuilder = this;

            _currentTime = 0;
            _animEvaluatorIdle = AnimationComponent.Blender.CreateEvaluator(AnimationIdle);
            _animEvaluatorWalk = AnimationComponent.Blender.CreateEvaluator(AnimationWalk);
            _animEvaluatorRun = AnimationComponent.Blender.CreateEvaluator(AnimationRun);
            _animEvaluatorJumpStart = AnimationComponent.Blender.CreateEvaluator(AnimationJumpStart);
            _animEvaluatorJumpMid = AnimationComponent.Blender.CreateEvaluator(AnimationJumpMid);
            _animEvaluatorJumpEnd = AnimationComponent.Blender.CreateEvaluator(AnimationJumpEnd);

            _walkLerpFactor = 0;
            _animEvaluatorWalkLerp1 = _animEvaluatorIdle;
            _animEvaluatorWalkLerp2 = _animEvaluatorWalk;
            _animationClipWalkLerp1 = AnimationIdle;
            _animationClipWalkLerp2 = AnimationWalk;
            _state = AnimationState.Airborne;

            base.Start();
        }

        public override void Cancel()
        {
            AnimationComponent.Blender.ReleaseEvaluator(_animEvaluatorIdle);
            AnimationComponent.Blender.ReleaseEvaluator(_animEvaluatorWalk);
            AnimationComponent.Blender.ReleaseEvaluator(_animEvaluatorRun);
            AnimationComponent.Blender.ReleaseEvaluator(_animEvaluatorJumpStart);
            AnimationComponent.Blender.ReleaseEvaluator(_animEvaluatorJumpMid);
            AnimationComponent.Blender.ReleaseEvaluator(_animEvaluatorJumpEnd);

            base.Cancel();
        }

        public override void Update()
        {
            if (_previousIsGrounded != Character.IsGrounded)
            {
                _currentTime = 0;
                _previousIsGrounded = Character.IsGrounded;
                _state = _previousIsGrounded ? AnimationState.Landing : AnimationState.Jumping;
            }

            switch (_state)
            {
                case AnimationState.Walking: UpdateWalking(); break;
                case AnimationState.Jumping: UpdateJumping(); break;
                case AnimationState.Airborne: UpdateAirborne(); break;
                case AnimationState.Landing: UpdateLanding(); break;
                default: break;
            }
        }

        private void UpdateWalking()
        {
            if (Character.RunSpeed < WalkThreshold)
            {
                _walkLerpFactor = Character.RunSpeed / WalkThreshold;
                _walkLerpFactor = (float)Math.Sqrt(_walkLerpFactor);
                _animEvaluatorWalkLerp1 = _animEvaluatorIdle;
                _animEvaluatorWalkLerp2 = _animEvaluatorWalk;
                _animationClipWalkLerp1 = AnimationIdle;
                _animationClipWalkLerp2 = AnimationWalk;
            }
            else
            {
                _walkLerpFactor = (Character.RunSpeed - WalkThreshold) / (1.0f - WalkThreshold);
                _animEvaluatorWalkLerp1 = _animEvaluatorWalk;
                _animEvaluatorWalkLerp2 = _animEvaluatorRun;
                _animationClipWalkLerp1 = AnimationWalk;
                _animationClipWalkLerp2 = AnimationRun;
            }

            var blendedMaxDuration = (long)MathUtil.Lerp(_animationClipWalkLerp1.Duration.Ticks, _animationClipWalkLerp2.Duration.Ticks, _walkLerpFactor);
            var currentTicks = TimeSpan.FromTicks((long)(_currentTime * blendedMaxDuration));
            currentTicks = blendedMaxDuration == 0
                ? TimeSpan.Zero
                : TimeSpan.FromTicks((currentTicks.Ticks + (long)(Game.DrawTime.Elapsed.Ticks * TimeScale)) % blendedMaxDuration);

            _currentTime = currentTicks.Ticks / (double)blendedMaxDuration;
        }

        private void UpdateJumping()
        {
            var currentTicks = TimeSpan.FromTicks((long)(_currentTime * AnimationJumpStart.Duration.Ticks));
            var updatedTicks = currentTicks.Ticks + (long)(Game.DrawTime.Elapsed.Ticks * TimeScale);
            if (updatedTicks < AnimationJumpStart.Duration.Ticks)
            {
                currentTicks = TimeSpan.FromTicks(updatedTicks);
                _currentTime = currentTicks.Ticks / (double)AnimationJumpStart.Duration.Ticks;
            }
            else
            {
                _state = AnimationState.Airborne;
                _currentTime = 0;
                UpdateAirborne();
            }
        }

        private void UpdateAirborne()
        {
            var currentTicks = TimeSpan.FromTicks((long)(_currentTime * AnimationJumpMid.Duration.Ticks));
            currentTicks = TimeSpan.FromTicks((currentTicks.Ticks + (long)(Game.DrawTime.Elapsed.Ticks * TimeScale)) % AnimationJumpMid.Duration.Ticks);
            _currentTime = currentTicks.Ticks / (double)AnimationJumpMid.Duration.Ticks;
        }

        private void UpdateLanding()
        {
            var currentTicks = TimeSpan.FromTicks((long)(_currentTime * AnimationJumpEnd.Duration.Ticks));
            var updatedTicks = currentTicks.Ticks + (long)(Game.DrawTime.Elapsed.Ticks * TimeScale);
            if (updatedTicks < AnimationJumpEnd.Duration.Ticks)
            {
                currentTicks = TimeSpan.FromTicks(updatedTicks);
                _currentTime = currentTicks.Ticks / (double)AnimationJumpEnd.Duration.Ticks;
            }
            else
            {
                _state = AnimationState.Walking;
                _currentTime = 0;
                UpdateWalking();
            }
        }

        public void BuildBlendTree(FastList<AnimationOperation> blendStack)
        {
            switch (_state)
            {
                case AnimationState.Walking:
                    blendStack.Add(AnimationOperation.NewPush(_animEvaluatorWalkLerp1, TimeSpan.FromTicks((long)(_currentTime * _animationClipWalkLerp1.Duration.Ticks))));
                    blendStack.Add(AnimationOperation.NewPush(_animEvaluatorWalkLerp2, TimeSpan.FromTicks((long)(_currentTime * _animationClipWalkLerp2.Duration.Ticks))));
                    blendStack.Add(AnimationOperation.NewBlend(CoreAnimationOperation.Blend, _walkLerpFactor));
                    break;

                case AnimationState.Jumping:
                    blendStack.Add(AnimationOperation.NewPush(_animEvaluatorJumpStart, TimeSpan.FromTicks((long)(_currentTime * AnimationJumpStart.Duration.Ticks))));
                    break;

                case AnimationState.Airborne:
                    blendStack.Add(AnimationOperation.NewPush(_animEvaluatorJumpMid, TimeSpan.FromTicks((long)(_currentTime * AnimationJumpMid.Duration.Ticks))));
                    break;

                case AnimationState.Landing:
                    blendStack.Add(AnimationOperation.NewPush(_animEvaluatorJumpEnd, TimeSpan.FromTicks((long)(_currentTime * AnimationJumpEnd.Duration.Ticks))));
                    break;
                default:
                    break;
            }
        }

        enum AnimationState
        {
            Walking,
            Jumping,
            Airborne,
            Landing,
        }
    }
}
