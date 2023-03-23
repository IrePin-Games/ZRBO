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
       
       [field: SerializeField] public Transform MainCamTransform { get; private set; }
        
        private void Start()
        {
            MainCamTransform = Cam.transform;
            SwitchState(new PlayerTestState(this));
        }

        
    }
}
