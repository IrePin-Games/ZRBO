using UnityEngine;

namespace _Zrbo.Scripts.StateMachines.Player
{
    public class PlayerTestState : PlayerBaseState
    {
        private float timer;
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
            Vector3 currentVector = stateMachine.transform.TransformDirection(moveVec).normalized;
            moveVec.x = stateMachine.InputReader.MovementValue.x;
            moveVec.y = 0f;
            moveVec.z = stateMachine.InputReader.MovementValue.y;
            
            
            stateMachine.Controller.Move(currentVector * stateMachine.WalkMovementSpeed * deltaTime);

            // if (stateMachine.InputReader.MovementValue == Vector2.zero)
            // {
            //     return;
            // }

            HandleCharacterRotation();

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
