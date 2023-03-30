using System;
using UnityEngine;

namespace _Zrbo.Scripts.StateMachines.Player
{
    public class PlayerStateMachine : StateMachine
    {
       [field: SerializeField] public InputReader InputReader { get; private set; }
       [field: SerializeField] public Camera Cam { get; private set; }
       [field: SerializeField] public CharacterController Controller { get; private set; }
       [field: SerializeField] public Animator Animator { get; private set; }
       [field: SerializeField] public float WalkMovementSpeed { get; private set; }
       
       [field: SerializeField] public float CrouchMovementSpeed { get; private set; }
       
       //states ID
       public PlayerCrouchState CrouchState { get; private set; }
       
       [Header("Move values")]
       public float gravity = -9.81f;
       public float runSpeed = 3;
       public float sprintSpeed = 5;
       public float crouchSpeed = 1;
       
       [Header("Colider values")]
       public float crouchColliderHeight = 1f;
       public float normalColliderHeight { get; private set; }
       public float grounCheckDistance;
       
       [Header("Velocity values")]
       public Vector3 moveVelocity;
       public Vector3 velocity;
      
       // animator ids
       public int HorizontalInputID { get; private set; }
       public int VerticalInputID { get; private set; }
       public int WalkID { get; private set; }
       public int CrouchID { get; private set; }
       public int IsGroundID { get; private set; }
       public int SprintID { get; private set; }
       public int RollID { get; private set; }

       private void Awake()
       {
           Cursor.lockState = CursorLockMode.Locked;
           Cursor.visible = false;
           SwitchState(new PlayerMoveState(this));
           AnimatorIDs();
           CrouchState = new PlayerCrouchState(this);
       }

       private void FixedUpdate()
       {
           HandleGravity();
           Debug.Log(IsGrounded(Controller));
        }

       public bool IsGrounded(CharacterController Controller)
       {
           return Physics.Raycast(Controller.transform.position + Vector3.up * 0.1f, 
               -Vector3.up, 
               Controller.height / 1.8f + 0.1f);
       }

       private void HandleGravity()
       {
           if (IsGrounded(Controller))
           {
               if (!Animator.GetBool(IsGroundID))
                   Animator.SetBool(IsGroundID, true);

               velocity.y = -2f;
           }
           else
           {
               if (Animator.GetBool(IsGroundID))
                   Animator.SetBool(IsGroundID, false);

               velocity.y += gravity * Time.deltaTime;
           }

           Controller.Move(velocity * Time.deltaTime);
       }
       
       private void AnimatorIDs()
        {
            HorizontalInputID = Animator.StringToHash("Velocity X");
            VerticalInputID = Animator.StringToHash("Velocity Z");
            IsGroundID = Animator.StringToHash("isGrounded");
            SprintID = Animator.StringToHash("sprint");
            RollID = Animator.StringToHash("roll");
            WalkID = Animator.StringToHash("walk");
            CrouchID = Animator.StringToHash("crouch");
        }
       
    }
}
