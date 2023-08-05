using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gameplay;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace LobbyScripts
{
    public class LobbyManager : NetworkBehaviour
    {
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TMP_InputField connectLobbyNameInputField;
        [SerializeField] private TextMeshProUGUI lobbyName;

        [SerializeField] private GameplayManager manager;

        private Allocation _currentAlloc;
        
        private Lobby _hostLobby;
        private Lobby _joinedLobby;
        private float _heartBeatTimer;

        public UnityEvent onPlayerJoin;

        public async void CreateLobby()
        {
            try
            {
                if (IsInputFieldsEmpty())
                {
                    print("Input fields empty!");
                    return;
                }
                
                _currentAlloc = await RelayService.Instance.CreateAllocationAsync(10);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(_currentAlloc.AllocationId);

                var relayServerData = new RelayServerData(_currentAlloc, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();
                
                var playerInfo = AuthenticationService.Instance.PlayerInfo;
                
                var options = new CreateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        [playerInfo.Id] = new (DataObject.VisibilityOptions.Member),
                        ["RelayJoinCode"] = new (DataObject.VisibilityOptions.Member, joinCode) 
                    }
                };

                var lobby = await Lobbies.Instance.CreateLobbyAsync(nameInputField.text, 20, options);

                _hostLobby = lobby;
            }
            catch(LobbyServiceException e)
            {
                print("Exception" + e.Message);
            }
        }
        public async void JoinLobby()
        {
            if (_hostLobby != null)
            {
                var isHost = _hostLobby.Players.Any(player => player.Id == AuthenticationService.Instance.PlayerId);
                if (isHost)
                {
                    lobbyName.text = _hostLobby.Name;
                    onPlayerJoin.Invoke();
                    StartCoroutine(CheckForAcceptablePlayersCount());
                    return;
                }
            }
            try
            {
                _joinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync((await GetLobbyByName(connectLobbyNameInputField.text)).Id);
                JoinRelay(_joinedLobby.Data["RelayJoinCode"].Value);
                lobbyName.text = _joinedLobby.Name;
                onPlayerJoin.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(this.GameObject());
        }
        [ClientRpc]
        private void StartGameClientRpc()
        {
            manager.StartGame();
        }
        private async void Start()
        {
            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        private void Update()
        {
            HandleLobbyHeartBeat();
        }
        private async void HandleLobbyHeartBeat()
        {
            if (_hostLobby == null) return;
            _heartBeatTimer -= Time.deltaTime;
            if (!(_heartBeatTimer < 0)) return;
            var heartBeatTimerMax = 10;
            _heartBeatTimer = heartBeatTimerMax;
            
            await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
        }
        private static async Task<QueryResponse> GetLobbyList()
        {
            try
            {
                return await Lobbies.Instance.QueryLobbiesAsync();
            }
            catch (LobbyServiceException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private IEnumerator CheckForAcceptablePlayersCount()
        {
            yield return new WaitForSeconds(4);
            if (IsHost && NetworkManager.ConnectedClients.Count > 1)
            {
                StartGameClientRpc();
            }
            else
            {
                StartCoroutine(CheckForAcceptablePlayersCount());
            }
        }
        private static async Task<Lobby> GetLobbyByName(string lobbyName)
        {
            var list = await GetLobbyList();
            return list.Results.FirstOrDefault(data => data.Name == lobbyName);
        }
        private bool IsInputFieldsEmpty()
        {
            return nameInputField.text == "";
        }
        private async void JoinRelay(string joinCode)
        {
            try
            {
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                var relayServerData = new RelayServerData(joinAllocation, "dtls");
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartClient();

                StartCoroutine(CheckForAcceptablePlayersCount());
            }
            catch (RelayServiceException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}

