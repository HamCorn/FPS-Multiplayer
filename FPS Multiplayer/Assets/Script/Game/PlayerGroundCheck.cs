using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    PlayerController playerController;
    AudioSource audioSource;
    public AudioClip[] onEnterSound;

    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        audioSource.PlayOneShot(onEnterSound[Random.Range(0, onEnterSound.Length)]);
        if (other.gameObject == playerController.gameObject)
            return;

        playerController.SetGroundedState(true);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerController.gameObject)
            return;

        playerController.SetGroundedState(false);
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == playerController.gameObject)
            return;

        playerController.SetGroundedState(true);
    }
    void OnCollisionEnter(Collision collision)
    {
        audioSource.PlayOneShot(onEnterSound[Random.Range(0, onEnterSound.Length)]);
        if (collision.gameObject == playerController.gameObject)
            return;

        playerController.SetGroundedState(true);
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject == playerController.gameObject)
            return;

        playerController.SetGroundedState(false);
    }
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == playerController.gameObject)
            return;

        playerController.SetGroundedState(true);
    }
}
