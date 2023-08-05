using Unity.Netcode;
using UnityEngine;

namespace Gameplay.PlayerScripts
{
    public class PlayerCombat : NetworkBehaviour
    {
        [SerializeField] private Transform projectileSpawnPos;
        public NetworkVariable<ushort> playerHealth;
        private GameplayManager _gameplayManager;
        private CanvasController _canvasController;

        public void SpawnProjectile(Vector2 dirData)
        {
            _gameplayManager.SpawnBulletServerRpc(dirData, projectileSpawnPos.position, new ServerRpcParams());
        }
        private void Start()
        {
            _gameplayManager = FindObjectOfType<GameplayManager>();
            _canvasController = FindObjectOfType<CanvasController>();
            playerHealth.OnValueChanged += TakeDamage;
        }
        private void TakeDamage(ushort previousValue, ushort newValue)
        {
            if (!IsLocalPlayer) return;
            _canvasController.GetHealthBar().fillAmount = (float)playerHealth.Value / 100;
            if (newValue != 0) return;
            var playerNetworkObjectId = transform.parent.GetComponent<NetworkObject>().NetworkObjectId;
            _gameplayManager.RemoveAlivePlayerServerRpc(playerNetworkObjectId);
            _gameplayManager.DeSpawnObjectServerRpc(playerNetworkObjectId);
        }
    }
}
