using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum PlayerStatus { Host, Ready, NotReady }

public class PlayerItem : MonoBehaviour
{
    public TMP_Text usernameText;
    public Image statusIcon;
    public Sprite hostIcon;
    public Sprite readyIcon;
    public Sprite notReadyIcon;


    public void SetPlayerInfo(string username, PlayerStatus status)
    {
        usernameText.text = username;
        SetStatus(status);
    }

    public void SetStatus(PlayerStatus status)
    {
        switch (status)
        {
            case PlayerStatus.Host:
                statusIcon.sprite = hostIcon;
                break;
            case PlayerStatus.Ready:
                statusIcon.sprite = readyIcon;
                break;
            case PlayerStatus.NotReady:
                statusIcon.sprite = notReadyIcon;
                break;
        }
    }

    public void SetReadyState(bool isReady)
    {
        //readyIndicator.SetActive(isReady);
    }

}
