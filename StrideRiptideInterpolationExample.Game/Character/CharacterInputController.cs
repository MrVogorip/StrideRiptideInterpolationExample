using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;

namespace StrideRiptideInterpolationExample.Character
{
    public class CharacterInputController : SyncScript
    {
        public CharacterController Character { get; set; }
        public float DeadZone { get; set; }
        public float MouseSensitivity { get; set; }

        public override void Update()
        {
            if (Input.IsMouseButtonDown(MouseButton.Left))
            {
                Input.LockMousePosition(true);
                Game.IsMouseVisible = false;
            }
            if (Input.IsKeyPressed(Keys.Escape))
            {
                Input.UnlockMousePosition();
                Game.IsMouseVisible = true;
            }

            Character.MoveDirection = GetMoveDirection();
            Character.CharacterCamera.CameraDirection = GetCameraDirection();
            Character.IsJumped = Input.IsKeyPressed(Keys.Space);
        }

        private Vector3 GetMoveDirection()
        {
            var moveDirection = Vector2.Zero;

            if (Input.IsKeyDown(Keys.A)) moveDirection += -Vector2.UnitX;
            if (Input.IsKeyDown(Keys.D)) moveDirection += +Vector2.UnitX;
            if (Input.IsKeyDown(Keys.W)) moveDirection += +Vector2.UnitY;
            if (Input.IsKeyDown(Keys.S)) moveDirection += -Vector2.UnitY;

            var worldDirection = Character?.CharacterCamera?.CameraComponent != null
                ? LogicDirectionToWorldDirection(moveDirection)
                : new Vector3(moveDirection.X, 0, moveDirection.Y);

            var moveLength = moveDirection.Length();
            if (moveLength < DeadZone)
                return Vector3.Zero;

            worldDirection *= moveLength > 1 ? 1 : (moveLength - DeadZone) / (1f - DeadZone);

            return worldDirection;
        }

        private Vector2 GetCameraDirection() =>
            Input.IsMousePositionLocked ? new Vector2(Input.MouseDelta.X, -Input.MouseDelta.Y) * MouseSensitivity : Vector2.Zero;

        private Vector3 LogicDirectionToWorldDirection(Vector2 logicDirection)
        {
            Character.CharacterCamera.CameraComponent.Update();
            var inverseView = Matrix.Invert(Character.CharacterCamera.CameraComponent.ViewMatrix);
            var forward = Vector3.Cross(Vector3.UnitY, inverseView.Right);
            forward.Normalize();

            var right = Vector3.Cross(forward, Vector3.UnitY);
            var worldDirection = forward * logicDirection.Y + right * logicDirection.X;
            worldDirection.Normalize();

            return worldDirection;
        }
    }
}
