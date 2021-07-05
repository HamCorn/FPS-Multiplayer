using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public abstract class Gun : Item
{
    public abstract override void Use();

    public Sprite[] concreteDecals; //�g���������Ϯ�
    public GameObject bulletHolePrefab;   //�u��
    public GameObject impactParticlePrefab; //���쪫��H��
    public GameObject bloodEffectPrefab; //���쪱�a�y��ĪG
    public ParticleSystem muzzleFlash; //�j�f����
    public ParticleSystem cartridgeEject; //�ߴ�
    public Transform leftHandIkPose;
    public Transform rightHandIkPose;


    public int magazineCapacity; //�u�X�e�q
    public int totalAmmo; //�̤j�`�u�Ķq
    public int bulletsInMagazine; //�ثe�j�W�u�Ķq
    public float shootRateDelay = 0.15f; //�g������ɶ�(�g�t)
    public float upRecoil;
    public float sideRecoilMix;
    public float sideRecoilMax;
    public bool isReloading = false; //�ˬd�O�_���b���u��
    public bool canShoot = true; //�ˬd�O�_��g��
    public bool hasAmmo = true; //�ˬd���L�l�u
    public bool meleeDelay = true; //���CD

    [Header("����")]
    [HideInInspector]
    public AudioSource audioSource;
    public AudioSource dryFireSound;//�S�l�u����
    public AudioClip shootSound;    //�}�j����
    public AudioClip reloading;     //���u����
    public AudioClip reloadingHuman;//���u����(�H�n)
    public AudioClip meleeSound;    //��ԭ���
    public AudioClip[] shootWallAC; //�l�u�����������

    [Header("�˷�")]
    [HideInInspector] public Vector3 originalPosition; //��l��m(���˷�)
    [HideInInspector] public int fovNormal = 60;
    [HideInInspector] public int fovIronSights = 30;
    [HideInInspector] public float smoothZoom = 3f;
    public Vector3 aimPosition; //�ؼЦ�m(�˷�)
    public Vector3 camPosition; //�ؼЦ�m(�˷�)
    public float aodSpeed = 8f; //�}��t��   
    public bool ironSightsOn;


    //UI
    public TMP_Text bulletsInMagazineText; //UI ��e�l�u
    public TMP_Text totalAmmoText; //UI �`�l�u
    public Color notAimColor;//�y�gui
    public Image notAimImage;//�y�gui
    public Color aimColor;  //�˷�ui
    public Image aimImage;  //�˷�ui
    [HideInInspector] public float colorSmoothing = 24f;    //�Ǥߤ����ɶ�
    public GameObject hitPlayer;
}
