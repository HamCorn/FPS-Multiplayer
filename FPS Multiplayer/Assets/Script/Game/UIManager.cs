using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public static UIManager singleton;

    public GameObject pauseMenuUI;          //�Ȱ����

    //FPS
    public int fpsTarget;
    public float updateInterval = 0.5f;  //�C�X���@��
    public TMP_Text FPS_text; //��UITEXT��i��
    private float lastInterval;
    private int frames = 0;
    private float fps;

    public static bool GameIsPaused = false;

    private void Start()
    {
        singleton = this;
        Application.targetFrameRate = fpsTarget;  //�T�w�V��
        lastInterval = Time.realtimeSinceStartup;  //�۹C���}�l�ɶ�
        frames = 0;  //��lframes =0
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
        if (timeNow >= lastInterval + updateInterval)  //�C0.5���s�@��
        {
            fps = frames / (timeNow - lastInterval); //�V��= �C�V/�C�V���j�@�� 
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
        //Application.Quit(); //�����C��
        StartCoroutine(KillFeedManager.Instance.End());
    }

    void OnGUI()
    {
        FPS_text.text = "FPS: " + fps.ToString(); //�bUI�W��ܴV��
    }
}
