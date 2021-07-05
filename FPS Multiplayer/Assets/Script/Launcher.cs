using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using TMPro;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    List<RoomInfo> fullRoomList = new List<RoomInfo>();

    public TMP_InputField userNameInputField;   //輸入房間名稱
    public TMP_InputField roomNameInputField;   //輸入房間名稱
    public TMP_Text errorText;                  //創建房間失敗Error
    public TMP_Text roomNameText;               //顯示房間名稱
    public Transform roomListContent;           //房間列表
    public GameObject roomListItemPrefab;       //創建房間名稱列表
    public Transform playerListContent;         //玩家列表
    public GameObject playerListItemPrefab;     //創建玩家名稱列表
    public GameObject startGameButton;          //StartGame(限制房主能按)
    public Slider maxPlayersSlider;             //玩家最大數量
    public TMP_Text maxPlayersValue;            //顯示玩家最大數量

    //Team
    public static Action OnLobbyJoined = delegate { };
    public GameMode _selectedGameMode;
    public GameMode[] _availableGameModes;//可用遊戲模式
    private const string GAME_MODE = "GAMEMODE";

    public static Action<GameMode> OnJoinRoom = delegate { }; //GameMode內部資料
    public static Action OnRoomLeft = delegate { };
    public static Action<Player> OnOtherPlayerLeftRoom = delegate { };

    public void Awake()
    {
        Instance = this;
        //UIGameMode.OnGameModeSelected += HandleGameModeSelected;    //執行 HandleGameModeSelected
    }
    public void OnDestroy()
    {
        //UIGameMode.OnGameModeSelected -= HandleGameModeSelected;
    }
    public void Update()
    {
        maxPlayersValue.text = maxPlayersSlider.value.ToString();   //同步CreateRoomMenu 調整人數滑桿旁邊的數字
    }
    public void PlayGame()  //開始遊戲 輸入名稱
    {
        if (string.IsNullOrEmpty(userNameInputField.text))  //如果玩家名稱是空值不給過
        {
            PhotonNetwork.NickName = "Player : " + UnityEngine.Random.Range(100, 1000);
        }
        else
        {
            PhotonNetwork.NickName = userNameInputField.text; //輸入玩家名稱
        }
        Debug.Log("正在連接至伺服器");
        PhotonNetwork.ConnectUsingSettings();   //連接至伺服器
        MenuManger.Instance.OpenMenu("loading");
    }
    public override void OnConnectedToMaster()  //成功連接至伺服器執行
    {
        Debug.Log("連接至伺服器成功");
        PhotonNetwork.JoinLobby();  //加入大廳
        PhotonNetwork.AutomaticallySyncScene = true;    //自動同步場景    
    }
    public override void OnJoinedLobby()    //成功加入大廳執行
    {
        MenuManger.Instance.OpenMenu("title");  //成功加入大廳後開啟Title
        Debug.Log("加入大廳");
        Debug.Log(PhotonNetwork.NickName + "為用戶名稱");

        //OnLobbyJoined?.Invoke();
    }
    public void CreateRoom()    //創建房間
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))  //如果創建房名是空值不給過
        {
            return;
        }

        RoomOptions options = new RoomOptions();    //房間選項
        //options.MaxPlayers = _selectedGameMode.MaxPlayers;  //最大人數       
        options.MaxPlayers = (byte)maxPlayersSlider.value;  //改為手動調整最大人數(滑桿)

        //string[] roomProperties = { GAME_MODE };
        //Hashtable customRoomProperties = new Hashtable()
        //    { {GAME_MODE, _selectedGameMode.Name} };
        //options.CustomRoomPropertiesForLobby = roomProperties;
        //options.CustomRoomProperties = customRoomProperties;

        int duration = 180;
        Hashtable Time = new Hashtable()
            { {"duration", duration} };
        options.CustomRoomProperties = Time;


        PhotonNetwork.CreateRoom(roomNameInputField.text, options);  //房間名稱 & 房間屬性
        MenuManger.Instance.OpenMenu("loading");
        Debug.Log($"加入 {roomNameInputField.text} 房間");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)   //創建房間失敗
    {
        errorText.text = "房間創建失敗: " + message;
        MenuManger.Instance.OpenMenu("error");
    }
    public void JoinRoom(RoomInfo info) //加入已創建房間
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManger.Instance.OpenMenu("loading");
    }
    public override void OnJoinedRoom() //成功加入房間
    {
        MenuManger.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name; //顯示房間名稱

        //Team  賦予遊戲模式(開房者設定的模式)
        //_selectedGameMode = GetRoomGameMode();  
        //OnJoinRoom?.Invoke(_selectedGameMode);

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)  //先刪除 playerListContent 的玩家
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);  //在創建(第一位開房房主)
        }

        //foreach (Player player in players)
        //{
        //    UITeam.Instance.AddPlayerToTeam(player);
        //}

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);    //開始遊戲 房主才顯示
    }
    public override void OnJoinRoomFailed(short returnCode, string message) //加入房間失敗
    {
        Debug.Log($"加入房間失敗 {message}");
        MenuManger.Instance.OpenMenu("error");
    }
    public override void OnMasterClientSwitched(Player newMasterClient) //房主若離開，更換房主
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);    //更換開始遊戲給新房主
    }
    public void LeaveRoom() //離開房間
    {
        //_selectedGameMode = null;   //離開房間模式清除 
        //OnRoomLeft?.Invoke();
        PhotonNetwork.LeaveRoom();  //離開房間
        MenuManger.Instance.OpenMenu("loading");
    }
    public void StartGame() //開始遊戲
    {
        PhotonNetwork.LoadLevel(1); //開始Scens第一個
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)  //房間列表 顯示目前開房的名稱
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i <= roomList.Count - 1; i++)
        {
            if (roomList[i].RemovedFromList)    //如果房間被標記為"已移除" 會將其從 fullRoomList 中移除
            {
                for (int a = 0; a < fullRoomList.Count; a++)
                {
                    if (fullRoomList[a].Name.Equals(roomList[i].Name)) fullRoomList.RemoveAt(a);
                }
            }
            if (!fullRoomList.Contains(roomList[i])) fullRoomList.Add(roomList[i]); //如果房間沒有被標記為"已移除" 添加到 fullRoomList

            for (int b = 0; b < fullRoomList.Count; b++)    //多個實例不會放入 fullRoomList
            {
                if (fullRoomList[b].Name.Equals(roomList[i].Name)) fullRoomList[b] = roomList[i];
            }
        }
        if (!(fullRoomList.Count == 0)) //檢查是否需要在實例化
        {
            for (int i = 0; i < fullRoomList.Count; i++)
            {
                if (fullRoomList[i].RemovedFromList == false) //將未移除的房間實例化
                {
                    roomListItemPrefab.transform.Find("Player").GetComponent<Text>().text = fullRoomList[i].PlayerCount + " / " + fullRoomList[i].MaxPlayers;   //房間目前人數 / 最大人數
                    Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(fullRoomList[i]);   //創建房間列表      
                }
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)  //玩家加入房間
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);   //創建玩家
    }

    public void ExitGame()  //離開遊戲
    {
        Application.Quit(); //關閉遊戲
    }

    //#region 處理
    //private void HandleGameModeSelected(GameMode gameMode)  //處理選擇的遊戲模式
    //{
    //    if (!PhotonNetwork.IsConnectedAndReady) return; //如果還沒連接到伺服器
    //    if (PhotonNetwork.InRoom) return;   //如果已經在其他人房間裡
    //    _selectedGameMode = gameMode;   //設定遊戲模式 
    //    CreateRoom();   //執行CreateRoom    
    //}
    //private GameMode GetRoomGameMode()  //獲得其他人創的房間模式
    //{
    //    string gameModeName = (string)PhotonNetwork.CurrentRoom.CustomProperties[GAME_MODE];
    //    GameMode gameMode = null;
    //    for (int i = 0; i < _availableGameModes.Length; i++)
    //    {
    //        if (string.Compare(_availableGameModes[i].Name, gameModeName) == 0)
    //        {
    //            gameMode = _availableGameModes[i];
    //            break;
    //        }
    //    }
    //    return gameMode;
    //}
    //#endregion
}
