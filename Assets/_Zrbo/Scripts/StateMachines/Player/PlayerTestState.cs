using UnityEngine;

namespace _Zrbo.Scripts.StateMachines.Player
{
    public class PlayerTestState : PlayerBaseState
    {
        public PlayerTestState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            stateMachine.InputReader.JumpEvent += OnJump;
            
        }

        public override void Tick(float deltaTime)
        {
            Vector3 movementVector = new Vector3(stateMachine.InputReader.MovementValue.x, 0,
                stateMachine.InputReader.MovementValue.y);
            Vector3 movement = stateMachine.transform.TransformDirection(movementVector).normalized;
            stateMachine.Controller.Move(movement * (stateMachine.WalkMovementSpeed * deltaTime));
            RotateCharacter(); 
        }

        public override void Exit()
        {
           
        }
        
        private void RotateCharacter()
        {
            float yawCamera = stateMachine.Cam.transform.rotation.eulerAngles.y;
            stateMachine.transform.rotation = Quaternion.Euler(0, yawCamera, 0);
            
        }
        private void OnJump()
        {
            stateMachine.SwitchState(new PlayerTestState(stateMachine));
        }
    }
}
