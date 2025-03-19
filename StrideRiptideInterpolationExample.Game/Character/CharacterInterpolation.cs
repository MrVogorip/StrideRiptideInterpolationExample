using Stride.Core.Mathematics;
using StrideRiptideInterpolationExample.Messages;
using System;

namespace StrideRiptideInterpolationExample.Character
{
    public static class CharacterInterpolation
    {
        public static void Interpolate(this CharacterController character)
        {
            var futureActions = character.FutureActions;
            if (futureActions.Count == 0)
                return;

            var localTime = DateTime.UtcNow.ToOADate();
            var firstIndex = futureActions.Count - 1;
            var secondIndex = firstIndex;
            var dt = 0f;

            for (var i = 0; i < futureActions.Count - 1; i++)
            {
                var currentAction = futureActions.Values[i];
                var nextAction = futureActions.Values[i + 1];
                if (localTime >= currentAction.RemoteTime && localTime <= nextAction.RemoteTime)
                {
                    firstIndex = i;
                    secondIndex = i + 1;
                    dt = (float)MathUtil.InverseLerp(currentAction.RemoteTime, nextAction.RemoteTime, localTime);
                    break;
                }
            }

            Apply(character, futureActions.Values[firstIndex], futureActions.Values[secondIndex], dt);
            for (var i = 0; i < firstIndex && i < futureActions.Count; ++i)
                futureActions.RemoveAt(0);
        }

        private static void Apply(CharacterController character, CharacterActionData first, CharacterActionData second, float dt)
        {
            character.RunSpeed = (first.RunSpeed + second.RunSpeed) / 2f;
            character.IsJumped = first.IsJumped || second.IsJumped;
            character.IsGrounded = first.IsGrounded || second.IsGrounded;
            character.CurrentPosition = Vector3.Lerp(first.Position, second.Position, dt);
            character.CurrentRotation = Quaternion.Lerp(first.Rotation, second.Rotation, dt);
        }
    }
}