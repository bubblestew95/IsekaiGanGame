using UnityEngine;
using Unity.Services.Multiplay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Collections;
using System;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using System.Collections;


public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    private string joinCode;
    private Coroutine keepAliveCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");

            // Relay �ʱ�ȭ Ȯ��
            if (RelayService.Instance == null)
            {
                Debug.LogError("RelayManager: Relay Service �ʱ�ȭ ����!");
                return;
            }

            Debug.Log("RelayManager: Relay Service �ʱ�ȭ �Ϸ�");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }
    }

    // ȣ��Ʈ(����)�� Relay ����
    public async Task<string> CreateRelay(int maxPlayers)
    {
        try
        {
            // Relay �Ҵ� ��û
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            if (allocation == null)
            {
                Debug.LogError("Relay Allocation ���� ����: Allocation�� null�Դϴ�.");
                return null;
            }

            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("Relay ���� ����: Join Code�� ��ȯ���� ����.");
                return null;
            }

            Debug.Log($"Relay created with join code: {joinCode}");

            // Netcode�� UnityTransport�� Relay ������ ����
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // ȣ��Ʈ ����
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started successfully.");

            // Lobby�� RelayJoinCode ������Ʈ (���� AddHostToLobby �ڵ� �����ϰ� ���⼭ ���� ����)
            if (!string.IsNullOrEmpty(RoomManager.Instance.currentRoom))
            {
                await LobbyService.Instance.UpdateLobbyAsync(RoomManager.Instance.currentRoom, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                {
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
                });

                Debug.Log("Lobby�� RelayJoinCode ������Ʈ �Ϸ�");
            }
            else
            {
                Debug.LogWarning("Lobby ID�� �����ϴ�. RelayJoinCode�� ������Ʈ�� �� �����ϴ�.");
            }

            //  Keep-Alive ����
            if (keepAliveCoroutine == null)
            {
                keepAliveCoroutine = StartCoroutine(SendKeepAlive());
            }

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay ���� ����: {e.Message}");
            return null;
        }
    }








    // Ŭ���̾�Ʈ�� Relay�� ����
    // Relay ���� �� Keep-Alive ����
    public async Task<bool> JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            Debug.Log("Joined Relay successfully!");

            //  Keep-Alive ����
            if (keepAliveCoroutine == null)
            {
                keepAliveCoroutine = StartCoroutine(SendKeepAlive());
            }

            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay ���� ����: {e.Message}");
            return false;
        }
    }

    // Relay �޽��� ��ε�ĳ��Ʈ
    public void SendMessageToClients(string message)
    {
        using (FastBufferWriter writer = new FastBufferWriter(256, Allocator.Temp))
        {
            writer.WriteValueSafe(message);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("OnRelayMessage", writer);
        }
        Debug.Log($"[RelayManager] ��� Ŭ���̾�Ʈ�� �޽��� ����: {message}");
    }


    public async Task TransferHostAndLeave(string currentRoom)
    {
        try
        {
            if (NetworkManager.Singleton.IsHost)
            {
                Debug.Log("Host�� �����Ƿ� �� Host�� �����մϴ�.");

                // 1���� ���� ������ ������
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentRoom);
                if (lobby.Players.Count > 1)
                {
                    // ���� �ִ� ù ��° �÷��̾ �� Host�� ����
                    string newHostId = lobby.Players[1].Id;

                    await LobbyService.Instance.UpdateLobbyAsync(currentRoom, new UpdateLobbyOptions
                    {
                        HostId = newHostId
                    });

                    Debug.Log($"���ο� Host ����: {newHostId}");
                }
                else
                {
                    // ���� �ִ� �÷��̾ ���ٸ� �� ����
                    await LobbyService.Instance.DeleteLobbyAsync(currentRoom);
                    Debug.Log("���� �ִ� �÷��̾ ���� ���� �����Ǿ����ϴ�.");
                }

                // 2 ��Ʈ��ũ ����
                NetworkManager.Singleton.Shutdown();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Host ��ü ����: {e.Message}");
        }
    }


    private IEnumerator SendKeepAlive()
    {
        while (true)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("Sending Keep-Alive packet to prevent timeout...");
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("KeepAlive", NetworkManager.ServerClientId, new FastBufferWriter(1, Allocator.Temp));
            }
            yield return new WaitForSeconds(10); // 10�ʸ��� Keep-Alive ��Ŷ ����
        }
    }
}
