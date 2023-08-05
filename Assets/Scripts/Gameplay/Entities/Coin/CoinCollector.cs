using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Entities.Coin
{
    public class CoinCollector : NetworkBehaviour
    {
        private GameplayManager _gameplayManager;
        private void Start()
        {
            _gameplayManager = FindObjectOfType<GameplayManager>();
        }
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player")) return;
            var playerNetworkObject = col.GetComponentInParent<NetworkObject>();
            if(playerNetworkObject.IsLocalPlayer) _gameplayManager.CollectCoinServerRpc(playerNetworkObject.NetworkObjectId);
            _gameplayManager.DeSpawnObjectServerRpc(NetworkObjectId);
        }
    }
}
