using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    public static PlayerController Instance;

    [SerializeField] GameObject cameraHolder;

    public Camera playerCamera;

    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    bool isRunning, isWalking;
    bool switchWeaponDelay = true;
    bool killScoreDelay = true;

    [SerializeField] Item[] items; //Weapon
    public int itemIndex;
    int previousItemIndex = -1;

    public Animator[] animator;
    AudioSource audioSource;
    public AudioClip[] weaponReadySound;    //換槍音效
    public AudioClip jumpSound;
    public AudioClip heavyBreathingSound;   //當runningTime超過秒數 跑步呼吸聲
    float runningTime;

    float horizontalLookRotation;   //水平視角旋轉
    float verticalLookRotation;     //垂直視角旋轉
    public bool grounded;
    Vector3 smoothMoveVelocity; //平滑移動速度
    Vector3 moveAmount; //移動量

    

    Rigidbody rb;

    PhotonView PV;

    public GameObject[] hand;
    public GameObject[] body;

    const float maxHealth = 100f;   //生命值
    [HideInInspector]public float currentHealth = maxHealth; //給扣的生命值

    PlayerManager playerManager;

    [Header("UI")]
    [SerializeField] GameObject canvas; //UI
    [SerializeField] Image healthbarImage;  //血量圖表(左下)
    [SerializeField] TMP_Text healthCountText;  //血量數字(左下)
    public GameObject worldNmae;

    float sideRecoil;
    float upRecoil;
    float recoilSpeed = 20f;

    Sensitive sensitive;


    [System.Obsolete]
    void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
        sensitive = GetComponentInChildren<Sensitive>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }
    void Start()
    {
        if (PV.IsMine)
        {
            EquipItem(0);
            for (int i = 0; i < body.Length; i++)
            {
                body[i].layer = 12;
            }
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);   //刪除非自身的Camera
            Destroy(rb);    //刪除非自身的Rigidbody
            Destroy(canvas);    //刪除非自身的UI

            for (int i = 0; i < hand.Length; i++)
            {
                hand[i].layer = 12;
            }  
        }
    }
    void Update()
    {
        if (!PV.IsMine)
            return;
        if (playerManager.isDie)
            return;

        if (UIManager.GameIsPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            moveAmount = new Vector3(0,0,0);

            isRunning = false;
            isWalking = false;
            return;
        }
        else
        {
            Look();
            Move();
            JumpAndCrouch();
            WeaponSwitch();

            items[itemIndex].Use();

            if (transform.position.y < -10f)    //掉地圖外死
            {
                Die();
            }
        }
    }
    void Look()
    {
        horizontalLookRotation += sideRecoil + mouseSensitivity * Input.GetAxis("Mouse X") * sensitive.sensitiveSlider.value; //取得X軸
        verticalLookRotation -= upRecoil + mouseSensitivity * Input.GetAxis("Mouse Y") * sensitive.sensitiveSlider.value; //取得Y軸

        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60f, 60f);

        cameraHolder.transform.eulerAngles = new Vector3(verticalLookRotation, cameraHolder.transform.eulerAngles.y, 0.0f); //垂直
        transform.eulerAngles = new Vector3(0.0f, horizontalLookRotation, 0.0f); //水平

        Cursor.lockState = CursorLockMode.Locked;   //隱藏滑鼠

        sideRecoil -= recoilSpeed * Time.deltaTime;
        upRecoil -= recoilSpeed * Time.deltaTime;

        if (sideRecoil < 0)
        {
            sideRecoil = 0;
        }

        if (upRecoil < 0)
        {
            upRecoil = 0;
        }
    }
    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) && !(Input.GetMouseButton(1)) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
        
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            isWalking = false;
            isRunning = true;
            runningTime += Time.deltaTime;
        }
        else if (moveDir.magnitude > 0)
        {
            isWalking = true;
            isRunning = false;
        }
        else if (runningTime >= 5f)
        {
            runningTime = 0f;
            audioSource.PlayOneShot(heavyBreathingSound);
        }
        else
        {
            isRunning = false;
            isWalking = false;
        }
    }
    void JumpAndCrouch()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
            audioSource.PlayOneShot(jumpSound);
        }
    }
    void WeaponSwitch()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f && switchWeaponDelay)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            else
            {
                EquipItem(itemIndex + 1);
            }
            StartCoroutine(SwitchWeaponDelay());
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f && switchWeaponDelay)
        {
            if (itemIndex <= 0)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex - 1);
            }
            StartCoroutine(SwitchWeaponDelay());
        }
    }
    IEnumerator SwitchWeaponDelay() //切換武器延遲 避免無限切換
    {
        switchWeaponDelay = false;
        yield return new WaitForSeconds(0.1f);
        switchWeaponDelay = true;
    }
    public void EquipItem(int _index)  //Weapon Switch
    {
        if (_index == previousItemIndex)    //禁止重複按同一個
            return;

        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);
        animator[itemIndex].SetTrigger("TakeGun");
        audioSource.PlayOneShot(weaponReadySound[itemIndex]);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            //hash.Add("itemIndex", itemIndex);
            hash["itemIndex"] = itemIndex;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            //EquipItem((int)changedProps["itemIndex"]);
            object Score;
            if (targetPlayer.CustomProperties.TryGetValue("itemIndex", out Score))
            {
                EquipItem((int)Score);
            }   
        }
    }
    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }
    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        if (playerManager.isDie)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);

        setParam();
        Ground();
    }
    public void TakeDamage(float damage, int damagerViewID)    //在本機上發送傷害
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, damagerViewID);
    }
    [PunRPC]
    void RPC_TakeDamage(float damage, int damagerViewID)   //在每個玩家發送，但!PV.IsMine檢查被射中的才執行
    {
        if (!PV.IsMine)
            return;

        currentHealth -= damage;

        healthbarImage.fillAmount = currentHealth / maxHealth;  //當前生命值 / 最大生命值 算出百分比
        healthCountText.text = currentHealth.ToString(); //更新血量數字

        if (currentHealth <= 0)
        {
            if (killScoreDelay && !playerManager.isDie) //避免重複加分
            {
                KillFeedManager.Instance.OnPlayerDeath(PV.ViewID, damagerViewID);
                StartCoroutine(KillScoreDelay());
            }
            playerCamera.enabled = false;
            Die();
        }
    }
    IEnumerator KillScoreDelay()
    {
        killScoreDelay = false;
        yield return new WaitForSeconds(0.5f);
        killScoreDelay = true;
    }
    void Die()
    {
        playerManager.Die();
    }
    public void WeaponRecoil(float _upRecoil, float _sideRecoil)  //射擊後座力
    {
        upRecoil += _upRecoil;
        sideRecoil += _sideRecoil;
    }

    public Animator anim;
    public bool onGround;
    public bool gravityGround = true;
    bool locomotion;
    public int numOfRays = 4;
    public float disFromGround;
    public float radiusGroundRays;
    public float crouchdisFromGround = 0.5f;
    float currDist = 0;
    public LayerMask layerMask;
    void setParam()
    {
        locomotion = (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 || Mathf.Abs(Input.GetAxis("Vertical")) > 0);

        AimPitch();
        anim.SetFloat(AnimStatics.horizontal, Input.GetAxis("Horizontal"));
        anim.SetFloat(AnimStatics.vertical, Input.GetAxis("Vertical"));
        anim.SetBool(AnimStatics.walking, isWalking);
        anim.SetBool(AnimStatics.running, isRunning);
        anim.SetBool(AnimStatics.aim, Input.GetMouseButton(1));
        anim.SetBool(AnimStatics.crouch, Input.GetKey(KeyCode.LeftControl));
        anim.SetBool(AnimStatics.onGround, onGround);

        setLayerWeight(AnimStatics.animLayers.crouchLayer, (Input.GetKey(KeyCode.LeftControl)) ? 1 : 0);
        setLayerWeight(AnimStatics.animLayers.aimLayer, (Input.GetMouseButton(1)) ? 0 : 0);
    }
    void AimPitch()
    {
        //calculate the look angle with the pitch and set the angle in animator for blending between look-up and look-down animation
        anim.SetFloat(AnimStatics.angle, -Vector3.SignedAngle(cameraHolder.transform.forward, Vector3.up, cameraHolder.transform.right));
    }
    void setLayerWeight(AnimStatics.animLayers layerID, float weight)
    {
        anim.SetLayerWeight((int)layerID, Mathf.LerpUnclamped(anim.GetLayerWeight((int)layerID), weight, Time.deltaTime * 7));
    }
    void Ground()
    {
        Ray r = new Ray(transform.position + new Vector3(0, 1, 0), Vector3.down);
        Debug.DrawRay(r.origin, r.direction * disFromGround);

        RaycastHit hit;
        bool gHit = Physics.Raycast(r, out hit, disFromGround, layerMask);

        for (int i = 0; i < numOfRays; i++)
        {
            if (gHit)
                break;
            Vector3 newOrigin = r.origin + Quaternion.Euler(0, 360 / numOfRays * i, 0) * transform.forward * radiusGroundRays;
            Debug.DrawRay(newOrigin, r.direction * disFromGround, Color.cyan);
            Ray newR = new Ray(newOrigin, Vector3.down);
            gHit = Physics.Raycast(newR, out hit, disFromGround, layerMask);
        }

        if (gHit)
        {
            currDist = Mathf.Lerp(currDist, (Input.GetKey(ControllerStatics.crouch) ? crouchdisFromGround : disFromGround), Time.deltaTime * 10);
            //transform.position = Vector3.Lerp(transform.position, transform.position + new Vector3(0, -(hit.distance - currDist + 0.2f), 0), Time.deltaTime * 100);
        }
        else
        {
            if (gravityGround)
                transform.position = new Vector3(transform.position.x, transform.position.y - Time.deltaTime * 5f, transform.position.z);
        }
        onGround = gHit;
    }
}
