using UnityEngine;

namespace _Zrbo.Scripts.StateMachines.Player
{
    public class PlayerCrouchState : PlayerBaseState
    {
        private Vector2 _inputVector;

        public PlayerCrouchState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
            
        }
        
        public override void Enter()
        {
            stateMachine.Animator.SetBool(stateMachine.CrouchID, true);
            stateMachine.Controller.height = stateMachine.crouchColliderHeight;
            stateMachine.Controller.center = new Vector3(0, stateMachine.crouchColliderHeight / 2f, 0);
        }

        public override void Tick(float fixedDeltaTime)
        {
            Movement();
            HandleAnimator();
        }
        
        private void Movement()
        {
            _inputVector = new Vector2(stateMachine.InputReader.MovementValue.x, stateMachine.InputReader.MovementValue.y);
            float clampedX = Mathf.Clamp(_inputVector.x, -1f, 1f);
            float clampedY = Mathf.Clamp(_inputVector.y, -1f, 1f);
            float forwardMoveSpeed = clampedY * stateMachine.CrouchMovementSpeed;
            float rightMoveSpeed = clampedX * stateMachine.CrouchMovementSpeed;
            Vector3 movementVector = new Vector3(rightMoveSpeed, 0, forwardMoveSpeed);
            Vector3 normalizedMovementVector = movementVector.normalized;
            Vector3 movement = stateMachine.transform.TransformDirection(normalizedMovementVector);
            stateMachine.Controller.Move(movement * (stateMachine.CrouchMovementSpeed * Time.deltaTime));
        }

        private void HandleAnimator()
        { 
            bool isInputReceived = _inputVector.x != 0f || _inputVector.y != 0f;
            stateMachine.Animator.SetBool(stateMachine.WalkID, false);
            stateMachine.Animator.SetBool(stateMachine.CrouchID, isInputReceived);
        }

        public override void Exit()
        {
            stateMachine.Animator.SetBool(stateMachine.CrouchID, false);
            stateMachine.Controller.height = stateMachine.normalColliderHeight;
            stateMachine.Controller.center = new Vector3(0, stateMachine.normalColliderHeight / 2f, 0);
        }
    }
}
