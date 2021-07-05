using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] PhotonView playerPV;
    [SerializeField] TMP_Text text;
    PlayerManager playerManager;

    void Start()
    {
        playerManager = PhotonView.Find((int)playerPV.InstantiationData[0]).GetComponent<PlayerManager>();
        if (playerPV.IsMine)
        {
            gameObject.SetActive(false);
            if (playerManager.isDie)
            {
                GetComponent<TMP_Text>().SetText($"");
            }
        }
        text.text = playerPV.Owner.NickName;
    }
}
