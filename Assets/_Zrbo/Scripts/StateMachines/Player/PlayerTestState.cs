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
            Vector3 moveVec = new Vector3();
            moveVec.x = stateMachine.InputReader.MovementValue.x;
            moveVec.z = stateMachine.InputReader.MovementValue.y;
            Vector3 movement = stateMachine.transform.TransformDirection(moveVec).normalized;
            
            
            stateMachine.Controller.Move(movement * (stateMachine.WalkMovementSpeed * deltaTime));
            HandleCharacterRotation();
            // if (stateMachine.InputReader.MovementValue == Vector2.zero)
            // {
            //     return;
            // }



        }

        public override void Exit()
        {
           
        }
        
        private void HandleCharacterRotation()
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
