using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class KillFeedManager : MonoBehaviourPunCallbacks
{
    public static KillFeedManager Instance;

    PhotonView PV;

    public Transform killContent;
    public GameObject killPrefab;

    public GameObject tabScore;
    public Transform tabContent;
    public GameObject tabPrefab;
    public Dictionary<Player, GameObject> _tabSelections;

    float timeValue = 600f;
    float time, minutes, secons;
    public TMP_Text timerText;

    bool perpetual = false;

    Player leftPlayer;

    void Awake()
    {
        if (Instance)   //檢查是否重複Room Manager
        {
            Destroy(gameObject);    //重複刪除
            return;
        }
        _tabSelections = new Dictionary<Player, GameObject>();
        DontDestroyOnLoad(gameObject);
        Instance = this;
        PV = GetComponent<PhotonView>();

    }
    void Update()
    {
        StartCoroutine(EndGame());

        if (Input.GetKey(KeyCode.Tab))
        {
            tabScore.SetActive(true);
        }
        else if (!perpetual)
        {
            tabScore.SetActive(false);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (timeValue > 0)
            {
                timeValue -= Time.deltaTime;
                Hashtable CountdownTimer = new Hashtable();
                CountdownTimer.Add("timeValue", timeValue);
                PhotonNetwork.LocalPlayer.SetCustomProperties(CountdownTimer);
            }
            else timeValue = 0;
        }
        timerText.SetText($"{minutes}:{secons}");
    }
    void FixedUpdate()
    {
        Tab();
    }
    public void Tab()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject TabScorePrefab = Instantiate(tabPrefab, tabContent);
            TabScorePrefab.transform.Find("Name_Text").GetComponent<TMP_Text>().SetText($"{player.NickName}");
            TabScorePrefab.transform.Find("KillScore_Text").GetComponent<TMP_Text>().SetText($"{player.GetKillScore()}");
            TabScorePrefab.transform.Find("DeathScore_Text").GetComponent<TMP_Text>().SetText($"{player.GetDeathScore()}");
            if (_tabSelections.ContainsKey(player))
            {
                Destroy(TabScorePrefab);
            }
            else
            {
                _tabSelections.Add(player, TabScorePrefab);
            }
        }      
    }
    public void OnPlayerDeath(int _Victim, int _Killer)
    {
        PV.RPC("RPC_Kill", RpcTarget.All, _Victim, _Killer);
    }

    [PunRPC]
    public void RPC_Kill(int _Victim, int _Killer)
    {
        PhotonView VictimPV = PhotonView.Find(_Victim);
        PhotonView KillerPV = PhotonView.Find(_Killer);

        GameObject KillObj = Instantiate(killPrefab, killContent);
        KillObj.GetComponentInChildren<TMP_Text>().SetText($" {KillerPV.Owner.NickName} Killed {VictimPV.Owner.NickName}     ");
        Destroy(KillObj, 5f);

        KillerPV.Owner.AddKillScore(1);
        VictimPV.Owner.AddDeathScore(1);

        PV.RPC("RPC_Socre", RpcTarget.All);
    }
    [PunRPC]
    public void RPC_Socre()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (_tabSelections.ContainsKey(player))
            {
                _tabSelections[player].gameObject.transform.Find("KillScore_Text").GetComponent<TMP_Text>().SetText($"{player.GetKillScore()}");
                _tabSelections[player].gameObject.transform.Find("DeathScore_Text").GetComponent<TMP_Text>().SetText($"{player.GetDeathScore()}");
            }
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        object _time;
        object _leftObj;
        if (targetPlayer.CustomProperties.TryGetValue("timeValue", out _time))
        {
            time = (float)_time;
            if (time < 0)
            {
                time = 0;
            }
            minutes = Mathf.FloorToInt(time / 60);
            secons = Mathf.FloorToInt(time % 60);  
        }
        if (targetPlayer.CustomProperties.TryGetValue("leftRoom", out _leftObj))
        {
            leftPlayer = (Player)_leftObj;
            Destroy(_tabSelections[leftPlayer]);
        }
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(50f);
        if (time <= 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.DestroyAll();
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            perpetual = true;
            tabScore.SetActive(true);
            StartCoroutine(End());
        }
    }
    public IEnumerator End()
    {
        yield return new WaitForSeconds(6f);

        Hashtable leftRoom = new Hashtable();
        leftRoom.Add("leftRoom", PhotonNetwork.LocalPlayer);
        leftRoom["_KillScore"] = 0;
        leftRoom["_DeathScore"] = 0;
        PhotonNetwork.LocalPlayer.SetCustomProperties(leftRoom);

        Cursor.lockState = CursorLockMode.None;
        PhotonNetwork.AutomaticallySyncScene = false;
        Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }
}
