using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITeam : MonoBehaviourPunCallbacks
{
    public static UITeam Instance;
    [SerializeField] private int _teamSize;
    [SerializeField] private int _maxTeamSize; //各隊人數
    [SerializeField] private PhotonTeam _team;
    [SerializeField] private TMP_Text _teamNameText;
    [SerializeField] public Transform _playerSelectionContainer;
    [SerializeField] public UIPlayerSelection _playerSelectionPrefab;
    [SerializeField] public Dictionary<Player, UIPlayerSelection> _playerSelections;
    public static Action<PhotonTeam> OnSwitchToTeam = delegate { };
    Player player;

    private void Awake()
    {
        Instance = this;
        UIDisplayTeam.OnAddPlayerToTeam += HandleAddPlayerToTeam;
        UIDisplayTeam.OnRemovePlayerFromTeam += HandleRemovePlayerFromTeam;
        Launcher.OnRoomLeft += HandleLeaveRoom;
    }
    private void OnDestroy()
    {
        UIDisplayTeam.OnAddPlayerToTeam -= HandleAddPlayerToTeam;
        UIDisplayTeam.OnRemovePlayerFromTeam -= HandleRemovePlayerFromTeam;
        Launcher.OnRoomLeft -= HandleLeaveRoom;
    }
    public void Initialize(PhotonTeam team, int teamSize)
    {
        _team = team;
        _maxTeamSize = teamSize;
        Debug.Log($"{_team.Name} 最大人數為 {_maxTeamSize}");
        _playerSelections = new Dictionary<Player, UIPlayerSelection>();
        UpdateTeamUI();

        Player[] teamMembers;
        if (PhotonTeamsManager.Instance.TryGetTeamMembers(_team.Code, out teamMembers))
        {
            foreach (Player player in teamMembers)
            {
                AddPlayerToTeam(player);
            }
        }
    }

    public void HandleAddPlayerToTeam(Player player, PhotonTeam team)
    {
        if (_team.Code == team.Code)
        {
            Debug.Log($" {player.NickName} 加入 {_team.Name}");
            AddPlayerToTeam(player);
        }
    }

    public void HandleRemovePlayerFromTeam(Player player)
    {
        RemovePlayerFromTeam(player);                       //執行 RemovePlayerFromTeam
    }

    private void HandleLeaveRoom()
    {
        Destroy(gameObject);
    }
    public void PlayerList()
    {
        for (int i = 0; i < _playerSelections.Count; i++)
        {
            UIPlayerSelection uiPlayerSelection = Instantiate(_playerSelectionPrefab, _playerSelectionContainer);
            uiPlayerSelection.GetComponent<UIPlayerSelection>().Initialize(player);
        }
    }
    private void UpdateTeamUI()
    {
        _teamNameText.SetText($"{_team.Name} [ {_playerSelections.Count} / {_maxTeamSize} ]");
    }

    public void AddPlayerToTeam(Player player)                 //實例化玩家名稱
    {
        UIPlayerSelection uiPlayerSelection = Instantiate(_playerSelectionPrefab, _playerSelectionContainer);
        uiPlayerSelection.GetComponent<UIPlayerSelection>().Initialize(player);
        _playerSelections.Add(player, uiPlayerSelection);
        UpdateTeamUI(); 
    }

    public void RemovePlayerFromTeam(Player player)
    {
        if (_playerSelections.ContainsKey(player))           //檢查玩家是否存在
        {
            //Debug.Log($"Updating {_team.Name} UI to remove {player.NickName}");
            Destroy(_playerSelections[player].gameObject);  //刪除玩家UI
            _playerSelections.Remove(player);               //玩家移除隊伍
            UpdateTeamUI();
        }
    }

    public void SwitchToTeam()
    {
        //Debug.Log($"嘗試切換到隊伍 {_team.Name}");
        if (_teamSize >= _maxTeamSize) return;  //如果已滿不給換

        //Debug.Log($"成功切換到隊伍 {_team.Name}");
        OnSwitchToTeam?.Invoke(_team);           //換過去
    }
}