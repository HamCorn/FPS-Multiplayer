using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using System.IO;
using TMPro;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    PhotonView PV;

    GameObject controller;

    public bool isDie;
    public float resurrectionTime;
    public GameObject resurrectionTime_obj;
    public GameObject killCount_obj;
    public TMP_Text resurrectionTime_Text;
    public TMP_Text kill_Text;

    void Awake()
    {
        Instance = this;
        PV = GetComponent<PhotonView>();
    }
    void Start()
    {
        if (PV.IsMine)
        {
            CreateController();
            resurrectionTime = 5f;
        }
    }
    void Update()
    {
        if (PV.IsMine)         
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                //kill_Text.SetText($" Time 1 Kill : {player.GetBlueKillCount()}  /  Time 2 Kill : {player.GetRedKillCount()} ");
            }

            if (isDie)
            {
                resurrectionTime -= Time.deltaTime;
                resurrectionTime_obj.SetActive(true);
                resurrectionTime_Text.SetText($" Time To Deploy : {Math.Round(resurrectionTime, 1)} s");
                
                if (resurrectionTime <= 0)
                {
                    PhotonNetwork.Destroy(controller);
                    CreateController();
                    resurrectionTime = 5;
                    isDie = false;
                }
            }
            else resurrectionTime_obj.SetActive(false);
        }
        else
        {
            Destroy(killCount_obj);
        }
    }
    void CreateController()
    {
        Transform spawnpoint = SpawnManager.Instance.GetSpawnpoint();
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });    //實例化玩家

        //if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code == 1)
        //{
        //    controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });    //實例化玩家
        //    Debug.Log(PhotonNetwork.LocalPlayer.GetPhotonTeam().Code);
        //}
        //else if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code == 2)
        //{
        //    controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });    //實例化玩家
        //    Debug.Log(PhotonNetwork.LocalPlayer.GetPhotonTeam().Code);
        //}
        //else return;
    }
    public void Die()
    {   
        isDie = true;
        //if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code == 1)
        //{
        //    PhotonNetwork.LocalPlayer.AddBlueKillCount(1);
        //}
        //else if (PhotonNetwork.LocalPlayer.GetPhotonTeam().Code == 2)
        //{
        //    PhotonNetwork.LocalPlayer.AddRedKillCount(1);
        //}
    }
}

