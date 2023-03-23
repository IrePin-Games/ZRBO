using UnityEngine;

namespace _Zrbo.Scripts.StateMachines.Player
{
    public class PlayerStateMachine : StateMachine
    {
       [field: SerializeField] public InputReader InputReader { get; private set; }
       
       [field: SerializeField] public Camera Cam { get; private set; }
       [field: SerializeField] public CharacterController Controller { get; private set; }
       [field: SerializeField] public float WalkMovementSpeed { get; private set; }
        
        private void Start()
        {
            SwitchState(new PlayerTestState(this));
        }

        
    }
}
