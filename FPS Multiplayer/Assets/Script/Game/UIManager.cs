using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public static UIManager singleton;

    public GameObject pauseMenuUI;          //暫停菜單

    //FPS
    public int fpsTarget;
    public float updateInterval = 0.5f;  //每幾秒算一次
    public TMP_Text FPS_text; //讓UITEXT放進來
    private float lastInterval;
    private int frames = 0;
    private float fps;

    public static bool GameIsPaused = false;

    private void Start()
    {
        singleton = this;
        Application.targetFrameRate = fpsTarget;  //固定幀數
        lastInterval = Time.realtimeSinceStartup;  //自遊戲開始時間
        frames = 0;  //初始frames =0
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        frames++;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow >= lastInterval + updateInterval)  //每0.5秒更新一次
        {
            fps = frames / (timeNow - lastInterval); //幀數= 每幀/每幀間隔毫秒 
            frames = 0;
            lastInterval = timeNow;
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
        Cursor.lockState = (GameIsPaused) ? CursorLockMode.None : CursorLockMode.Confined;
        Cursor.visible = GameIsPaused;
    }
    public void ExitGame()
    {
        //PhotonNetwork.LeaveRoom();
        //SceneManager.LoadScene(0);
        //Application.Quit(); //關閉遊戲
        StartCoroutine(KillFeedManager.Instance.End());
    }

    void OnGUI()
    {
        FPS_text.text = "FPS: " + fps.ToString(); //在UI上顯示幀數
    }
}
