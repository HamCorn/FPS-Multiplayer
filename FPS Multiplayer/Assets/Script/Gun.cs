using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public abstract class Gun : Item
{
    public abstract override void Use();

    public Sprite[] concreteDecals; //射擊物體火花圖案
    public GameObject bulletHolePrefab;   //彈孔
    public GameObject impactParticlePrefab; //打到物件碎片
    public GameObject bloodEffectPrefab; //打到玩家流血效果
    public ParticleSystem muzzleFlash; //槍口火花
    public ParticleSystem cartridgeEject; //拋殼
    public Transform leftHandIkPose;
    public Transform rightHandIkPose;


    public int magazineCapacity; //彈匣容量
    public int totalAmmo; //最大總彈藥量
    public int bulletsInMagazine; //目前槍上彈藥量
    public float shootRateDelay = 0.15f; //射擊延遲時間(射速)
    public float upRecoil;
    public float sideRecoilMix;
    public float sideRecoilMax;
    public bool isReloading = false; //檢查是否正在換彈中
    public bool canShoot = true; //檢查是否能射擊
    public bool hasAmmo = true; //檢查有無子彈
    public bool meleeDelay = true; //近戰CD

    [Header("音效")]
    [HideInInspector]
    public AudioSource audioSource;
    public AudioSource dryFireSound;//沒子彈音效
    public AudioClip shootSound;    //開槍音效
    public AudioClip reloading;     //換彈音效
    public AudioClip reloadingHuman;//換彈音效(人聲)
    public AudioClip meleeSound;    //近戰音效
    public AudioClip[] shootWallAC; //子彈撞擊牆壁音效

    [Header("瞄準")]
    [HideInInspector] public Vector3 originalPosition; //原始位置(未瞄準)
    [HideInInspector] public int fovNormal = 60;
    [HideInInspector] public int fovIronSights = 30;
    [HideInInspector] public float smoothZoom = 3f;
    public Vector3 aimPosition; //目標位置(瞄準)
    public Vector3 camPosition; //目標位置(瞄準)
    public float aodSpeed = 8f; //開鏡速度   
    public bool ironSightsOn;


    //UI
    public TMP_Text bulletsInMagazineText; //UI 當前子彈
    public TMP_Text totalAmmoText; //UI 總子彈
    public Color notAimColor;//腰射ui
    public Image notAimImage;//腰射ui
    public Color aimColor;  //瞄準ui
    public Image aimImage;  //瞄準ui
    [HideInInspector] public float colorSmoothing = 24f;    //準心切換時間
    public GameObject hitPlayer;
}
