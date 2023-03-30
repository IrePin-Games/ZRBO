using UnityEngine;

namespace _Zrbo.Scripts.StateMachines.Player
{
    public class PlayerMoveState : PlayerBaseState
    {
        private Vector2 _inputVector;
       
        //anim velocity blending
        private Vector2 _currentAnimationBlendVector;
        private Vector2 _animationVelocity;
        
        private float _forwardMoveSpeed;
        private float _rightMoveSpeed;
        public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
            
        }
        public override void Enter()
        {
            
        }

        public override void Tick(float fixedDeltaTime)
        {
            Movement();
            RotateCharacter();
            HandleAnimator();
        }
        
        private void Movement()
        {
            _inputVector = new Vector2(stateMachine.InputReader.MovementValue.x, stateMachine.InputReader.MovementValue.y);
            float clampedX = Mathf.Clamp(_inputVector.x, -1f, 1f);
            float clampedY = Mathf.Clamp(_inputVector.y, -1f, 1f);
            _forwardMoveSpeed = clampedY * stateMachine.WalkMovementSpeed;
            _rightMoveSpeed = clampedX * stateMachine.WalkMovementSpeed;
            Vector3 movementVector = new Vector3(_rightMoveSpeed, 0, _forwardMoveSpeed);
            Vector3 normalizedMovementVector = movementVector.normalized;
            Vector3 movement = stateMachine.transform.TransformDirection(normalizedMovementVector);
            stateMachine.Controller.Move(movement * (stateMachine.WalkMovementSpeed * Time.deltaTime));
        }

        private void HandleAnimator()
        { 
            // check if there is any input from the user
            bool isInputReceived = _inputVector.x != 0f || _inputVector.y != 0f;

            stateMachine.Animator.SetBool(stateMachine.WalkID, isInputReceived);

            if (isInputReceived)
            {
                _currentAnimationBlendVector = Vector2.SmoothDamp(_currentAnimationBlendVector, _inputVector,
                    ref _animationVelocity, 0.2f);
                stateMachine.Animator.SetFloat(stateMachine.HorizontalInputID, _currentAnimationBlendVector.x);
                stateMachine.Animator.SetFloat(stateMachine.VerticalInputID, _currentAnimationBlendVector.y);
            }
        }

        private void RotateCharacter()
        {
            float yawCamera = stateMachine.Cam.transform.rotation.eulerAngles.y;
            stateMachine.transform.rotation = Quaternion.Euler(0, yawCamera, 0);
        }
        public override void Exit()
        {
           
        }
    }
}
