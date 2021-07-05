using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PhotonTeamController : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<PhotonTeam> _roomTeams;   //所有隊伍
    [SerializeField] private int _teamSize;                //各隊人數
    [SerializeField] private PhotonTeam _priorTeam;         //先前的隊伍

    public static Action<List<PhotonTeam>, GameMode> OnCreateTeams = delegate { };  //加入隊伍
    public static Action<Player, PhotonTeam> OnSwitchTeam = delegate { };   //更換隊伍
    public static Action<Player> OnRemovePlayer = delegate { }; //換隊或離開
    public static Action OnClearTeams = delegate { };           //離開 清除全部

    #region Unity Methods
    private void Awake()
    {
        UITeam.OnSwitchToTeam += HandleSwitchTeam;                      //更換隊伍
        Launcher.OnJoinRoom += HandleCreateTeams;                       //加入房間
        Launcher.OnRoomLeft += HandleLeaveRoom;                         //離開房間離開隊伍
        Launcher.OnOtherPlayerLeftRoom += HandleOtherPlayerLeftRoom;    //其他玩家離開

        _roomTeams = new List<PhotonTeam>();
    }
    private void OnDestroy()
    {
        UITeam.OnSwitchToTeam -= HandleSwitchTeam;
        Launcher.OnJoinRoom -= HandleCreateTeams;
        Launcher.OnRoomLeft -= HandleLeaveRoom;
        Launcher.OnOtherPlayerLeftRoom -= HandleOtherPlayerLeftRoom;
    }
    #endregion

    #region Handle Methods
    private void HandleSwitchTeam(PhotonTeam newTeam)
    {
        if (PhotonNetwork.LocalPlayer.GetPhotonTeam() == null)      //如果玩家沒有隊伍
        {
            _priorTeam = PhotonNetwork.LocalPlayer.GetPhotonTeam();
            PhotonNetwork.LocalPlayer.JoinTeam(newTeam);            //給予隊伍
        }
        else if (CanSwitchToTeam(newTeam))                          //如果玩家已經有隊伍執行 CanSwitchToTeam
        {
            _priorTeam = PhotonNetwork.LocalPlayer.GetPhotonTeam(); //先前隊伍
            PhotonNetwork.LocalPlayer.SwitchTeam(newTeam);          //給予新隊伍
        }
    }

    private void HandleCreateTeams(GameMode gameMode)
    {
        CreateTeams(gameMode);                                          //創建隊伍

        OnCreateTeams?.Invoke(_roomTeams, gameMode);                    //給予團隊跟模式

        AutoAssignPlayerToTeam(PhotonNetwork.LocalPlayer, gameMode);    //分配給加入的玩家
    }
    private void HandleLeaveRoom()
    {
        PhotonNetwork.LocalPlayer.LeaveCurrentTeam();   //離開隊伍
        _roomTeams.Clear();                             //清除房間列表
        _teamSize = 0;
        OnClearTeams?.Invoke();
    }

    private void HandleOtherPlayerLeftRoom(Player otherPlayer)
    {
        OnRemovePlayer?.Invoke(otherPlayer);
    }
    #endregion

    #region Private Methods
    private void CreateTeams(GameMode gameMode)
    {
        _teamSize = gameMode.TeamSize;
        int numberOfTeams = gameMode.MaxPlayers;    //如果沒分隊的話，隊伍數量就是玩家數量，一人一隊
        if (gameMode.HasTeams)                      //如果有分隊
        {
            numberOfTeams = gameMode.MaxPlayers / gameMode.TeamSize;
        }

        //for (int i = 1; i <= numberOfTeams; i++)
        //{
        //    _roomTeams.Add(new PhotonTeam   
        //    {
        //        Name = $"Team {i}", //在PhotonTeam創建團隊數量
        //        Code = (byte)i
        //    });
        //}
        _roomTeams.Add(new PhotonTeam
        {
            Name = $"Blue", //在PhotonTeam創建團隊數量
            Code = (byte)1
        });
        _roomTeams.Add(new PhotonTeam
        {
            Name = $"Red", //在PhotonTeam創建團隊數量
            Code = (byte)2
        });
    }

    private bool CanSwitchToTeam(PhotonTeam newTeam)    //更換隊伍
    {
        bool canSwitch = false;
        if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code != newTeam.Code)                 //檢查是否加入同一隊
        {
            Player[] players = null;
            if (PhotonTeamsManager.Instance.TryGetTeamMembers(newTeam.Code, out players))
            {
                if (players.Length < _teamSize)     //檢查隊伍空間
                {
                    canSwitch = true;
                }
                else
                {
                    Debug.Log($"{newTeam.Name} 已滿");    //人數已滿
                }   
            }
        }
        else
        {
            Debug.Log($"已經在團隊中 {newTeam.Name}");    //如果已經在隊伍裡
        }

        return canSwitch;
    }

    private void AutoAssignPlayerToTeam(Player player, GameMode gameMode)   //自動分配隊伍
    {
        foreach (PhotonTeam team in _roomTeams)
        {
            int teamPlayerCount = PhotonTeamsManager.Instance.GetTeamMembersCount(team.Code);
            Debug.Log(teamPlayerCount + "  aaaa ");
            if (teamPlayerCount < gameMode.TeamSize)                 //檢查人數較少的隊伍
            {
                if (player.GetPhotonTeam() == null)                 //如果沒有給一個
                {
                    player.JoinTeam(team.Code);
                }
                else if (player.GetPhotonTeam().Code != team.Code)  //如果有換隊伍
                {
                    player.SwitchTeam(team.Code);
                }
                break;
            }
        }
    }
    #endregion

    #region Photon 回調方法
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object teamCodeObject;
        if (changedProps.TryGetValue(PhotonTeamsManager.TeamPlayerProp, out teamCodeObject))
        {
            if (teamCodeObject == null) return;

            byte teamCode = (byte)teamCodeObject;

            PhotonTeam newTeam;
            if (PhotonTeamsManager.Instance.TryGetTeamByCode(teamCode, out newTeam))        //獲得隊伍Code
            {
                //Debug.Log($"更換 {targetPlayer.NickName} 到新隊伍 {newTeam.Name}");
                OnSwitchTeam?.Invoke(targetPlayer, newTeam);
            }
        }
    }
    #endregion
}