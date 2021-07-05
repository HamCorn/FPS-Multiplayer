using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIDisplayTeam : MonoBehaviour
{
    [SerializeField] private UITeam _uiTeamPrefab;          //UI Team介面
    [SerializeField] private List<UITeam> _uiTeams;         //UI隊伍
    [SerializeField] private Transform _teamContainer;      //生成區域

    public static Action<Player, PhotonTeam> OnAddPlayerToTeam = delegate { };  //有人加入
    public static Action<Player> OnRemovePlayerFromTeam = delegate { };         //有人離開

    private void Awake()
    {
        PhotonTeamController.OnCreateTeams += HandleCreateTeams;        //創建隊伍
        PhotonTeamController.OnSwitchTeam += HandleSwitchTeam;          //更換隊伍
        PhotonTeamController.OnRemovePlayer += HandleRemovePlayer;      //離開隊伍
        PhotonTeamController.OnClearTeams += HandleClearTeams;          //離開房間
        _uiTeams = new List<UITeam>();
    }

    private void OnDestroy()
    {
        PhotonTeamController.OnCreateTeams -= HandleCreateTeams;
        PhotonTeamController.OnSwitchTeam += HandleSwitchTeam;
        PhotonTeamController.OnRemovePlayer += HandleRemovePlayer;
        PhotonTeamController.OnClearTeams -= HandleClearTeams;
    }

    private void HandleCreateTeams(List<PhotonTeam> teams, GameMode gameMode)   //接收 PhotonTeam 裡隊伍數量
    {
        foreach (PhotonTeam team in teams)  //隊伍實例化
        {
            UITeam uiTeam = Instantiate(_uiTeamPrefab, _teamContainer); //隊伍
            uiTeam.Initialize(team, gameMode.TeamSize);                //隊伍跟隊伍人數
            _uiTeams.Add(uiTeam);
        }
    }

    private void HandleSwitchTeam(Player player, PhotonTeam newTeam)        //更新UI移動
    {
        //Debug.Log($" {player.NickName} 移動到 {newTeam.Name}");

        OnRemovePlayerFromTeam?.Invoke(player);                             //玩家離開

        OnAddPlayerToTeam?.Invoke(player, newTeam);                         //加入
    }

    private void HandleRemovePlayer(Player otherPlayer)
    {
        OnRemovePlayerFromTeam?.Invoke(otherPlayer);
    }

    private void HandleClearTeams()
    {
        foreach (UITeam uiTeam in _uiTeams)
        {
            Destroy(uiTeam.gameObject);
        }
        _uiTeams.Clear();
    }
}