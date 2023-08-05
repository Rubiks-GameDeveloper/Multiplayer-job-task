using Gameplay.ObjectMovement;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Entities.Bullet
{
    public class ProjectileController : NetworkBehaviour
    {
        [SerializeField] private LookDirectionMovement movement;
        [SerializeField] private float projectileSpeed;

        [SerializeField] private int bulletDamage;

        private GameplayManager _gameplayManager;

        public Vector2 projectileDirection = Vector2.zero;

        private void Start()
        {
            _gameplayManager = FindObjectOfType<GameplayManager>();
        }
        private void FixedUpdate()
        {
            movement.MoveInDirection(projectileDirection, projectileSpeed);
        }
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player") && !col.CompareTag("Wall")) return;
            if (col.CompareTag("Player")) _gameplayManager.TakeDamageFromPlayerServerRpc(col.GetComponentInParent<NetworkObject>().NetworkObjectId, (uint)bulletDamage);
            _gameplayManager.DeSpawnObjectServerRpc(NetworkObjectId);
        }
    }
}
