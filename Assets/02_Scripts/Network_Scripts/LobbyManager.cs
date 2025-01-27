using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public TMP_InputField sessionNameInput;
    public TMP_InputField sessionCodeInput;
    public Button createButton;
    public Button joinButton;

    private Lobby currentLobby;

    public async void CreateLobby()
    {
        string lobbyName = sessionNameInput.text;
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                MaxPlayers = 4
            };
            currentLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, 4, options);
            Debug.Log("Lobby Created: " + currentLobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Create lobby failed: " + e.Message);
        }
    }

    public async void JoinLobby()
    {
        string code = sessionCodeInput.text;
        try
        {
            currentLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code);
            Debug.Log("Joined Lobby: " + currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Join lobby failed: " + e.Message);
        }
    }
}
