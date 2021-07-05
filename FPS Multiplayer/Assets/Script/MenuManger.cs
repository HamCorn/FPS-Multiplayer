using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManger : MonoBehaviour
{
    public static MenuManger Instance;

    [SerializeField] Menu[] menus;

    void Awake()
    {
        Instance = this;   
    }

    public void OpenMenu(string menuName)   //訪問
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (menus[i].menuName == menuName)  //名稱正確
            {
                menus[i].Open();
            }
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }
    public void OpenMenu(Menu menu) //打開
    {
        for (int i = 0; i < menus.Length; i++)  //只有一個正在打開中
        {
            if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
        menu.Open();
    }
    public void CloseMenu(Menu menu) //關閉
    {
        menu.Close();
    }
}
