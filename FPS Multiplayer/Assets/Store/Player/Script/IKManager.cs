using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class IKManager : MonoBehaviour
{
    Animator anim;
    public GameObject gun;

    public Transform lookAt;

    public Transform leftHandTarget;//left ik target
    public Transform rightHandTarget;//right ik target

    public Transform leftHandTarget_Pistol;//left ik target
    public Transform rightHandTarget_Pistol;//right ik target

    public Transform leftHandIK;//left hand hint
    public Transform rightHandIK;//right hand hint

    public Transform leftLegHint;
    public Transform rightLegHint;

    public Transform AimPose;
    public Transform IdlePose;
    public Transform RunPose;

    Transform rightShoulder;

    //layer mask to hit everything except the actor's collider (圖層蒙版可以擊中除演員對撞機之外的所有內容)
    public LayerMask layerMask;
    public bool ikActive=true;
    public bool Aiming=true;

    float legRayDist = 1.31f;
    float footOffset = 0.12f;

    public Vector3 idleOffsetPose;
    public Vector3 aimOffsetPose;
    public Vector3 runOffsetPose;

    PhotonView PV;

    private Collider[] ragdollColliders;
    private Rigidbody[] ragdollRigidbodies;
    public CapsuleCollider playerCol;
    PlayerManager playerManager;
    PlayerController playerController;
    public Rigidbody playerRig;

    AudioSource audioSource;
    public AudioClip[] footstepSound;

    void Start()
    {
        PV = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
        playerController = GetComponentInParent<PlayerController>();
        audioSource = GetComponent<AudioSource>();

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = false;
        }
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = true;
        }
    }
    void Update()
    {
        if (!PV.IsMine)
            return;

        if (playerManager.isDie)
        {
            PV.RPC("RPC_IsDie", RpcTarget.All);
            return;
        }

        if (rightShoulder == null)
        {
            rightShoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder);
        }
        else
        {
            AimPose.position = rightShoulder.position + AimPose.forward * aimOffsetPose.z + AimPose.right * aimOffsetPose.x + AimPose.up * aimOffsetPose.y;
            IdlePose.position = rightShoulder.position + IdlePose.forward * idleOffsetPose.z + IdlePose.right * idleOffsetPose.x + IdlePose.up * idleOffsetPose.y;
            RunPose.position = rightShoulder.position + RunPose.forward * runOffsetPose.z + RunPose.right * runOffsetPose.x + RunPose.up * runOffsetPose.y;
            AimPose.LookAt(lookAt.position);
        }

        //if aiming or fire is true then the gun should point at the target (如果瞄准或開火是真的，那麼槍應該指向目標)
        if (Input.GetMouseButton(1))
        {
            gun.transform.position = Vector3.Lerp(gun.transform.position, AimPose.position, Time.deltaTime * 10);
            gun.transform.rotation = Quaternion.Lerp(gun.transform.rotation, AimPose.rotation, Time.deltaTime * 10);
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            gun.transform.position = Vector3.Lerp(gun.transform.position, RunPose.position, Time.deltaTime * 10);
            //gun.transform.rotation = Quaternion.Lerp(gun.transform.rotation, RunPose.rotation, Time.deltaTime * 10);
        }
        else
        {
            gun.transform.position = Vector3.Lerp(gun.transform.position, IdlePose.position, Time.deltaTime * 10);
            gun.transform.rotation = Quaternion.Lerp(gun.transform.rotation, IdlePose.rotation, Time.deltaTime * 10);
            IdlePose.LookAt(lookAt.position);
        }
    }

    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;
        
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (playerManager.isDie)
            return;

        //check if animator and ikActive is set, if it is set then ik targets are handled   (檢查是否設置了動畫師和 ikActive，如果設置了則處理 ik 目標)
        if (anim)
        {
            if (ikActive)
            {
                anim.SetLookAtWeight(0.5f, 1);
                anim.SetLookAtPosition(lookAt.position);

                //set weight of each ik target and hint (設置每個 ik 目標和提示的權重)
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);

                //set ik target position (設置ik目標位置)
                if (GetComponentInParent<PlayerController>().itemIndex == 0)
                {
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                    anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                    anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
                }
                else if (GetComponentInParent<PlayerController>().itemIndex == 1)
                {
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget_Pistol.position);
                    anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget_Pistol.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget_Pistol.rotation);
                    anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget_Pistol.rotation);
                }
                

                //set ik hint position  (set ik hint position)
                anim.SetIKHintPosition(AvatarIKHint.LeftElbow, leftHandIK.position);
                anim.SetIKHintPosition(AvatarIKHint.RightElbow, rightHandIK.position);
                FootIk();
            }
        }
    }
    [PunRPC]
    void FootIk()
    {
        float rweight = anim.GetFloat("rightFootWeight");
        float lweight = anim.GetFloat("leftFootWeight");

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, lweight);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, lweight);

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rweight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rweight);
        RaycastHit hit;
        Ray Lray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
        Debug.DrawRay(Lray.origin, Lray.direction * legRayDist);
        if (Physics.Raycast(Lray, out hit, legRayDist, layerMask))
        {
            Vector3 footPose = hit.point;
            footPose.y += footOffset;
            //print(hit.distance);
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPose);
            Vector3 forward = Vector3.Cross(anim.GetBoneTransform(HumanBodyBones.LeftFoot).right, hit.normal);
            anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(forward, hit.normal));
        }

        Ray Rray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
        Debug.DrawRay(Rray.origin, Rray.direction * legRayDist);
        if (Physics.Raycast(Rray, out hit, legRayDist, layerMask))
        {
            Vector3 footPose = hit.point;
            footPose.y +=footOffset;
            //print(hit.distance);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, footPose);
            Vector3 forward = Vector3.Cross(anim.GetBoneTransform(HumanBodyBones.RightFoot).right, hit.normal);
            anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(forward, hit.normal));

        }
        anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, rweight);
        anim.SetIKHintPosition(AvatarIKHint.RightKnee, rightLegHint.position);
        anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, lweight);
        anim.SetIKHintPosition(AvatarIKHint.LeftKnee, leftLegHint.position);
    }

    [PunRPC]
    public void RPC_IsDie()
    {
        if (playerManager.isDie)
            return;

        anim.enabled = false;

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = true;
        }
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
        }
        gun.SetActive(false);
        playerCol.enabled = false;
    }
    public void Step()
    {
        if (!PV.IsMine)
            return;

        if (playerController.grounded)
        {
            audioSource.PlayOneShot(footstepSound[Random.Range(0, footstepSound.Length)]);
        }
    }
}
