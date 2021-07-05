using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    public RoomInfo info;

    public void SetUp(RoomInfo _info)   //房間名稱訊息至伺服器
    {
        info = _info;
        text.text = _info.Name;
    }
    public void OnClick()   //按加入房間
    {
        Launcher.Instance.JoinRoom(info);   
    }
}
