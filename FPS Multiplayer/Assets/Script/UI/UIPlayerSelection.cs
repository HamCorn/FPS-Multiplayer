using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class UIPlayerSelection : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text _usernameText;
    Player player;
    public void Initialize(Player _player)
    {
        player = _player;
        SetupPlayerSelection();
    }
    private void SetupPlayerSelection()
    {
        _usernameText.SetText(player.NickName);
    }
}