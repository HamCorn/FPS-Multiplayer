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

    public TMP_InputField userNameInputField;   //��J�ж��W��
    public TMP_InputField roomNameInputField;   //��J�ж��W��
    public TMP_Text errorText;                  //�Ыةж�����Error
    public TMP_Text roomNameText;               //��ܩж��W��
    public Transform roomListContent;           //�ж��C��
    public GameObject roomListItemPrefab;       //�Ыةж��W�٦C��
    public Transform playerListContent;         //���a�C��
    public GameObject playerListItemPrefab;     //�Ыت��a�W�٦C��
    public GameObject startGameButton;          //StartGame(����ХD���)
    public Slider maxPlayersSlider;             //���a�̤j�ƶq
    public TMP_Text maxPlayersValue;            //��ܪ��a�̤j�ƶq

    //Team
    public static Action OnLobbyJoined = delegate { };
    public GameMode _selectedGameMode;
    public GameMode[] _availableGameModes;//�i�ιC���Ҧ�
    private const string GAME_MODE = "GAMEMODE";

    public static Action<GameMode> OnJoinRoom = delegate { }; //GameMode�������
    public static Action OnRoomLeft = delegate { };
    public static Action<Player> OnOtherPlayerLeftRoom = delegate { };

    public void Awake()
    {
        Instance = this;
        //UIGameMode.OnGameModeSelected += HandleGameModeSelected;    //���� HandleGameModeSelected
    }
    public void OnDestroy()
    {
        //UIGameMode.OnGameModeSelected -= HandleGameModeSelected;
    }
    public void Update()
    {
        maxPlayersValue.text = maxPlayersSlider.value.ToString();   //�P�BCreateRoomMenu �վ�H�ƷƱ���䪺�Ʀr
    }
    public void PlayGame()  //�}�l�C�� ��J�W��
    {
        if (string.IsNullOrEmpty(userNameInputField.text))  //�p�G���a�W�٬O�ŭȤ����L
        {
            PhotonNetwork.NickName = "Player : " + UnityEngine.Random.Range(100, 1000);
        }
        else
        {
            PhotonNetwork.NickName = userNameInputField.text; //��J���a�W��
        }
        Debug.Log("���b�s���ܦ��A��");
        PhotonNetwork.ConnectUsingSettings();   //�s���ܦ��A��
        MenuManger.Instance.OpenMenu("loading");
    }
    public override void OnConnectedToMaster()  //���\�s���ܦ��A������
    {
        Debug.Log("�s���ܦ��A�����\");
        PhotonNetwork.JoinLobby();  //�[�J�j�U
        PhotonNetwork.AutomaticallySyncScene = true;    //�۰ʦP�B����    
    }
    public override void OnJoinedLobby()    //���\�[�J�j�U����
    {
        MenuManger.Instance.OpenMenu("title");  //���\�[�J�j�U��}��Title
        Debug.Log("�[�J�j�U");
        Debug.Log(PhotonNetwork.NickName + "���Τ�W��");

        //OnLobbyJoined?.Invoke();
    }
    public void CreateRoom()    //�Ыةж�
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))  //�p�G�ЫةЦW�O�ŭȤ����L
        {
            return;
        }

        RoomOptions options = new RoomOptions();    //�ж��ﶵ
        //options.MaxPlayers = _selectedGameMode.MaxPlayers;  //�̤j�H��       
        options.MaxPlayers = (byte)maxPlayersSlider.value;  //�אּ��ʽվ�̤j�H��(�Ʊ�)

        //string[] roomProperties = { GAME_MODE };
        //Hashtable customRoomProperties = new Hashtable()
        //    { {GAME_MODE, _selectedGameMode.Name} };
        //options.CustomRoomPropertiesForLobby = roomProperties;
        //options.CustomRoomProperties = customRoomProperties;

        int duration = 180;
        Hashtable Time = new Hashtable()
            { {"duration", duration} };
        options.CustomRoomProperties = Time;


        PhotonNetwork.CreateRoom(roomNameInputField.text, options);  //�ж��W�� & �ж��ݩ�
        MenuManger.Instance.OpenMenu("loading");
        Debug.Log($"�[�J {roomNameInputField.text} �ж�");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)   //�Ыةж�����
    {
        errorText.text = "�ж��Ыإ���: " + message;
        MenuManger.Instance.OpenMenu("error");
    }
    public void JoinRoom(RoomInfo info) //�[�J�w�Ыةж�
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManger.Instance.OpenMenu("loading");
    }
    public override void OnJoinedRoom() //���\�[�J�ж�
    {
        MenuManger.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name; //��ܩж��W��

        //Team  �ᤩ�C���Ҧ�(�}�Ъ̳]�w���Ҧ�)
        //_selectedGameMode = GetRoomGameMode();  
        //OnJoinRoom?.Invoke(_selectedGameMode);

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)  //���R�� playerListContent �����a
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);  //�b�Ы�(�Ĥ@��}�ЩХD)
        }

        //foreach (Player player in players)
        //{
        //    UITeam.Instance.AddPlayerToTeam(player);
        //}

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);    //�}�l�C�� �ХD�~���
    }
    public override void OnJoinRoomFailed(short returnCode, string message) //�[�J�ж�����
    {
        Debug.Log($"�[�J�ж����� {message}");
        MenuManger.Instance.OpenMenu("error");
    }
    public override void OnMasterClientSwitched(Player newMasterClient) //�ХD�Y���}�A�󴫩ХD
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);    //�󴫶}�l�C�����s�ХD
    }
    public void LeaveRoom() //���}�ж�
    {
        //_selectedGameMode = null;   //���}�ж��Ҧ��M�� 
        //OnRoomLeft?.Invoke();
        PhotonNetwork.LeaveRoom();  //���}�ж�
        MenuManger.Instance.OpenMenu("loading");
    }
    public void StartGame() //�}�l�C��
    {
        PhotonNetwork.LoadLevel(1); //�}�lScens�Ĥ@��
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)  //�ж��C�� ��ܥثe�}�Ъ��W��
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i <= roomList.Count - 1; i++)
        {
            if (roomList[i].RemovedFromList)    //�p�G�ж��Q�аO��"�w����" �|�N��q fullRoomList ������
            {
                for (int a = 0; a < fullRoomList.Count; a++)
                {
                    if (fullRoomList[a].Name.Equals(roomList[i].Name)) fullRoomList.RemoveAt(a);
                }
            }
            if (!fullRoomList.Contains(roomList[i])) fullRoomList.Add(roomList[i]); //�p�G�ж��S���Q�аO��"�w����" �K�[�� fullRoomList

            for (int b = 0; b < fullRoomList.Count; b++)    //�h�ӹ�Ҥ��|��J fullRoomList
            {
                if (fullRoomList[b].Name.Equals(roomList[i].Name)) fullRoomList[b] = roomList[i];
            }
        }
        if (!(fullRoomList.Count == 0)) //�ˬd�O�_�ݭn�b��Ҥ�
        {
            for (int i = 0; i < fullRoomList.Count; i++)
            {
                if (fullRoomList[i].RemovedFromList == false) //�N���������ж���Ҥ�
                {
                    roomListItemPrefab.transform.Find("Player").GetComponent<Text>().text = fullRoomList[i].PlayerCount + " / " + fullRoomList[i].MaxPlayers;   //�ж��ثe�H�� / �̤j�H��
                    Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(fullRoomList[i]);   //�Ыةж��C��      
                }
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)  //���a�[�J�ж�
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);   //�Ыت��a
    }

    public void ExitGame()  //���}�C��
    {
        Application.Quit(); //�����C��
    }

    //#region �B�z
    //private void HandleGameModeSelected(GameMode gameMode)  //�B�z��ܪ��C���Ҧ�
    //{
    //    if (!PhotonNetwork.IsConnectedAndReady) return; //�p�G�٨S�s������A��
    //    if (PhotonNetwork.InRoom) return;   //�p�G�w�g�b��L�H�ж���
    //    _selectedGameMode = gameMode;   //�]�w�C���Ҧ� 
    //    CreateRoom();   //����CreateRoom    
    //}
    //private GameMode GetRoomGameMode()  //��o��L�H�Ъ��ж��Ҧ�
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
