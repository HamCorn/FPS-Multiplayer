using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;
    public Transform camera_aim;
    public PlayerController playerController;


    PhotonView PV;
    Animator animator;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        bulletsInMagazine = magazineCapacity; //���j�W�l�u
        originalPosition = transform.localPosition;
    }
    public override void Use()
    {
        if (PlayerManager.Instance.isDie)
            return;

        UpdateAmmoUI(); //UI����
        AimDownSight();
        if (isReloading)
            return;

        if (Input.GetMouseButton(0) && canShoot)
        {
            if (hasAmmo)
            {
                Shoot();
            }
            else dryFireSound.Play();
        }

        if (Input.GetKeyDown(KeyCode.R)) //���UR���u
        {
            if (totalAmmo > 0 && bulletsInMagazine < magazineCapacity && !Input.GetMouseButton(1))   //�`�u�ĭn�j��0
            {
                StartCoroutine(Reload()); //���u
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            if (!isReloading)
            {
                animator.SetBool("Run", true);
            }
        }
        else animator.SetBool("Run", false);

        if (Input.GetKey(KeyCode.Q))
        {
            if (!isReloading && meleeDelay && canShoot)
            {
                MeleeAttack();   
            }
        }
    }

    void Shoot()
    {
        animator.SetTrigger("Shoot");
        PV.RPC("Effect", RpcTarget.All);
        StartCoroutine(ShootRateDlay()); //����g��(�g�t)
        float sideRecoil = Random.Range(sideRecoilMix, sideRecoilMax);
        PlayerController.Instance.WeaponRecoil(upRecoil, sideRecoil);

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));  //�ù�����
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject != this.playerController.gameObject)
            {
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage, PV.ViewID); //�p�G�g��a��IDamageable ����
                if (hit.collider.tag == "Player")
                {
                    StartCoroutine(hitDealy());
                }
            }
        }
        
        PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);

        bulletsInMagazine--;
        if (bulletsInMagazine <= 0) hasAmmo = false;
    }
    IEnumerator hitDealy()
    {
        hitPlayer.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        hitPlayer.SetActive(false);
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        foreach (Collider hit in colliders)
        {
            if (hit.transform.gameObject.tag != this.playerController.gameObject.tag)
            {
                if (hit.transform.tag == "Player")
                {
                    GameObject bloodEffect = Instantiate(bloodEffectPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bloodEffectPrefab.transform.rotation);
                    bloodEffect.transform.SetParent(colliders[0].transform);
                }
                else
                {
                    //�u��
                    GameObject bulletHole = Instantiate(bulletHolePrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletHolePrefab.transform.rotation);
                    bulletHole.transform.SetParent(colliders[0].transform);
                    bulletHole.GetComponent<SpriteRenderer>().sprite = concreteDecals[Random.Range(0, concreteDecals.Length)]; //�H����ܼu�պ���
                    Destroy(bulletHole, 8f);

                    //�����X�Ӫ��H��
                    GameObject impactPrefab = Instantiate(impactParticlePrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * impactParticlePrefab.transform.rotation);
                    impactPrefab.transform.SetParent(colliders[0].transform);
                    Destroy(impactPrefab, 8f);

                    audioSource.PlayOneShot(shootWallAC[Random.Range(0, shootWallAC.Length)]); //���񥴤��𭵮�
                }
            }
            
        }
    }
    [PunRPC]
    void Effect()
    {
        audioSource.PlayOneShot(shootSound);
        muzzleFlash.Play(); //�j�f����
        cartridgeEject.Play(); //�ߴ�   
    }
    public void UpdateAmmoUI() //UI����
    {
        bulletsInMagazineText.text = bulletsInMagazine.ToString();
        totalAmmoText.text = totalAmmo.ToString();

    }
    IEnumerator ShootRateDlay() //�g������
    {
        canShoot = false;
        yield return new WaitForSeconds(shootRateDelay);
        canShoot = true;
    }
    IEnumerator Reload()
    {
        hasAmmo = false;
        canShoot = false;   //���u����g��
        isReloading = true; //���u�P�_
        Debug.Log("���u��...");
        yield return new WaitForSeconds(0.1f);
        animator.SetTrigger("Reload"); //���u�ʧ@
        audioSource.PlayOneShot(reloading);
        audioSource.PlayOneShot(reloadingHuman);
        yield return new WaitForSeconds(2f);

        if (bulletsInMagazine + totalAmmo <= magazineCapacity)
        {
            bulletsInMagazine += totalAmmo;
            totalAmmo = 0;
        }
        else
        {
            totalAmmo -= magazineCapacity - bulletsInMagazine;
            bulletsInMagazine = magazineCapacity;
        }
        hasAmmo = true;
        canShoot = true;
        isReloading = false;
    }
    void AimDownSight()
    {
        if (Input.GetMouseButton(1))
        {
            if (!isReloading)
            {
                ironSightsOn = true;
                transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * aodSpeed);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovIronSights, Time.deltaTime * (aodSpeed - 4f));
                camera_aim.localPosition = Vector3.Lerp(camera_aim.localPosition, camPosition, Time.deltaTime * (aodSpeed - 4f));
                animator.SetBool("AimDownSight", true);
                notAimImage.color = notAimColor; //�Ǥ߳z��
                aimImage.color = aimColor;
            }
        }
        else
        {
            ironSightsOn = false;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * aodSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovNormal, Time.deltaTime * aodSpeed);
            camera_aim.localPosition = Vector3.Lerp(camera_aim.localPosition, originalPosition, Time.deltaTime * aodSpeed);
            animator.SetBool("AimDownSight", false);
            notAimImage.color = Color.Lerp(notAimImage.color, Color.gray, Time.deltaTime * aodSpeed); //�Ǥ��ܦ^����
            aimImage.color = Color.Lerp(aimImage.color, Color.clear, Time.deltaTime * aodSpeed); //�Ǥ��ܦ^����
        }
    }
    void MeleeAttack()
    {
        StartCoroutine(MeleeDelayTime());

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));  //�ù�����
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f))
        {
            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage, PV.ViewID);
            PV.RPC("RPC_Melee", RpcTarget.All, hit.point, hit.normal);
        }
    }
    [PunRPC]
    void RPC_Melee(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
 
        foreach (Collider hit in colliders)
        {
            if (hit.transform.gameObject.tag != this.playerController.gameObject.tag)
            {
                if (hit.transform.tag == "Player")
                {
                    GameObject bloodEffect = Instantiate(bloodEffectPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bloodEffectPrefab.transform.rotation);
                    bloodEffect.transform.SetParent(colliders[0].transform);
                    StartCoroutine(hitDealy());
                }
                else
                {
                    //�u��
                    GameObject bulletHole = Instantiate(bulletHolePrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletHolePrefab.transform.rotation);
                    bulletHole.transform.SetParent(colliders[0].transform);
                    bulletHole.GetComponent<SpriteRenderer>().sprite = concreteDecals[Random.Range(0, concreteDecals.Length)]; //�H����ܼu�պ���
                    Destroy(bulletHole, 8f);

                    //�����X�Ӫ��H��
                    GameObject impactPrefab = Instantiate(impactParticlePrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * impactParticlePrefab.transform.rotation);
                    impactPrefab.transform.SetParent(colliders[0].transform);
                    Destroy(impactPrefab, 8f);

                    audioSource.PlayOneShot(shootWallAC[Random.Range(0, shootWallAC.Length)]); //���񥴤��𭵮�
                }
            }
            
        }
    }
    IEnumerator MeleeDelayTime()
    {
        meleeDelay = false;
        audioSource.PlayOneShot(meleeSound);
        animator.SetBool("Melee", true);
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("Melee", false);
        yield return new WaitForSeconds(1f);
        meleeDelay = true;
    }
}
