using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public string menuName; //開啟名稱
    public bool open;

    public void Open()  //開啟
    {
        open = true;
        gameObject.SetActive(true);
    }
    public void Close() //關閉
    {
        open = false;
        gameObject.SetActive(false);
    }
}
