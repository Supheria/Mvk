using MvkClient.Actions;
using MvkServer.Util;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект параметров перемещения
    /// </summary>
    public class MovementInputFromOptions : MovementInput
    {
        private readonly KeyBinding keyBind;

        public MovementInputFromOptions(KeyBinding keyBind) => this.keyBind = keyBind;

        public override void UpdatePlayerMoveState()
        {
            Forward = keyBind.forward;
            Back = keyBind.back;
            Left = keyBind.left;
            Right = keyBind.right;
            Jump = keyBind.up;
            Sneak = keyBind.down;
            Sprinting = keyBind.sprinting;
        }
    }
}
