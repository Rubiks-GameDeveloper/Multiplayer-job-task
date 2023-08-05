using Gameplay.ObjectMovement;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.PlayerScripts
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private LookDirectionMovement movement;
        [SerializeField] private float moveSpeed;
    
        [SerializeField] private PlayerCombat combat;
        private CanvasController _canvasController;

        public NetworkVariable<int> coinCount;

        [SerializeField] private Joystick joystick;
        private Vector2 _directionPreviousData = Vector2.right;
        
        private void Start()
        {
            _canvasController = FindObjectOfType<CanvasController>();
            joystick = _canvasController.GetJoystick();
            GetComponent<SpriteRenderer>().color = Random.ColorHSV();
            if(IsLocalPlayer) coinCount.OnValueChanged += OnCoinCountChange;
            if(IsLocalPlayer) _canvasController.GetShootButton().onClick.AddListener(OnShootButtonPressed);
        }
        private void OnCoinCountChange(int previousValue, int newValue)
        {
            _canvasController.GetCoinCountText().text = coinCount.Value.ToString();
        }
        private void FixedUpdate()
        {
            if (!IsOwner) return;

            if (joystick.Direction != Vector2.zero) _directionPreviousData = joystick.Direction;

            movement.MoveAndRotateInDirection(joystick.Direction, moveSpeed);
        }
        private void OnShootButtonPressed()
        {
            combat.SpawnProjectile(_directionPreviousData.normalized);
        }
    }
}
