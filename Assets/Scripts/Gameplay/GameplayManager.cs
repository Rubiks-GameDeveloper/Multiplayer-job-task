using Gameplay.Entities.Bullet;
using Unity.Netcode;
using Gameplay.PlayerScripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class GameplayManager : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private float verticalMaxPosition;
        [SerializeField] private float verticalMinPosition;
        [SerializeField] private float horizontalMaxPosition;
        [SerializeField] private float horizontalMinPosition;

        private CanvasController _canvasController;
        private NetworkObject _playerInstance;
        public NetworkVariable<int> loadedPlayersCount;
        private NetworkList<ulong> _alivePlayers;

        public void StartGame()
        {
            SceneManager.LoadScene("Game");
            
            SceneManager.sceneLoaded += (_, _) =>
            {
                UpdateLoadedPlayersCountServerRpc();
                _canvasController = FindObjectOfType<CanvasController>();
            };
        }
        [ServerRpc(RequireOwnership = false)]
        public void SpawnBulletServerRpc(Vector2 dir, Vector3 spawnPosition, ServerRpcParams rpcParams)
        {
            var instance = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            instance.GetComponent<ProjectileController>().projectileDirection = dir;
            instance.GetComponent<NetworkObject>().Spawn();
        }
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageFromPlayerServerRpc(ulong playerObjectId, uint damage)
        {
            var playerInstance = NetworkManager.SpawnManager.SpawnedObjects[playerObjectId].gameObject;
            playerInstance.GetComponentInChildren<PlayerCombat>().playerHealth.Value -= (ushort)damage;
        }
        [ServerRpc(RequireOwnership = false)]
        public void DeSpawnObjectServerRpc(ulong objectId)
        {
            var objectInstance = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject;
            objectInstance.GetComponent<NetworkObject>().Despawn();
        }
        [ServerRpc(RequireOwnership = false)]
        public void CollectCoinServerRpc(ulong objectId)
        {
            NetworkManager.SpawnManager.SpawnedObjects[objectId].GetComponentInChildren<PlayerController>().coinCount.Value += 1;
        }
        [ServerRpc(RequireOwnership = false)]
        public void RemoveAlivePlayerServerRpc(ulong playerNetworkObjectId)
        {
            _alivePlayers.Remove(playerNetworkObjectId);
            if (_alivePlayers.Count != 1) return;
            var playerNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[_alivePlayers[0]];
            var coinCount = _canvasController.GetCoinCountText();
            EndGameClientRpc(playerNetworkObject.OwnerClientId, coinCount.text);
        }
        
        private void Awake()
        {
            DontDestroyOnLoad(this.GameObject());
            Application.targetFrameRate = 60;
            _alivePlayers = new NetworkList<ulong>();
        }
        [ServerRpc(RequireOwnership = false)]
        private void UpdateLoadedPlayersCountServerRpc()
        {
            loadedPlayersCount.Value += 1;
            if (loadedPlayersCount.Value != NetworkManager.ConnectedClients.Count) return;
            OnStartServer();
            SpawnCoins(Random.Range(5, 15));
        }
        [ClientRpc]
        private void EndGameClientRpc(ulong playerId, string coinCount)
        {
            _canvasController.GetWinDisplay().SetActive(true);
            _canvasController.GetWinScreenData().GetWinPlayerName().text = "Player " + playerId;
            _canvasController.GetWinScreenData().GetWinPlayerCoinCount().text = coinCount;
        }
        private void OnStartServer()
        {
            if (!IsHost) return;
            
            _alivePlayers.Initialize(GetNetworkBehaviour(NetworkBehaviourId));
            
            foreach (var player in NetworkManager.ConnectedClients)
            {
                _playerInstance = Instantiate(playerPrefab, GetRandomSpawnPosition(), Quaternion.identity).GetComponent<NetworkObject>();
                _playerInstance.SpawnAsPlayerObject(player.Key, true);
                _alivePlayers.Add(_playerInstance.NetworkObjectId);
            }
        }
        private Vector3 GetRandomSpawnPosition()
        {
            return new Vector3(Random.Range(horizontalMinPosition, horizontalMaxPosition),
                Random.Range(verticalMinPosition, verticalMaxPosition), 0);
        }
        private void SpawnCoins(int coinsCount)
        {
            if (!IsHost) return;
            for (var i = 0; i <= coinsCount; i++)
            {
                var coinInstance = Instantiate(coinPrefab, GetRandomSpawnPosition(), Quaternion.identity)
                    .GetComponent<NetworkObject>();
                coinInstance.Spawn();
            }
        }
    }
}
