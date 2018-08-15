using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DFTGames.Tools;
using UnityStandardAssets.Vehicles.Car;
using ECM.Examples;

public class ExtraRemoteCharControls : MonoBehaviour
{

    // Interaction variables

    // Driving
    [Header("Driving Properties")]
    public bool canInteractWithDriveable = false;
    public bool isDriving = false;
    public GameObject referenceCar;
    Vector3 drivingPos = new Vector3(-0.5f, -0.35f, 0.1f);

    string driveTag = "Driveable";

    // changing models
    [Header("Models (For shifting back and forth between)")]
    public GameObject[] playerModels;
    public Material[] materials;
    int charSelect = 0;
    bool isShifting = false;
    bool canShift = true;

    bool isBlinking = false;
    bool canBlink = true;

    [Header("GameSession References")]
    public bool isHiding = false;
    public bool isLastManStanding = false;
    public bool isChasing = false;
    bool isChasingStarted = false;

    // audio
    [Header("Audio Properties")]
    public AudioSource audioSource;
    public AudioClip smokePoof;

    // fx
    [Header("FX Properties")]
    public ParticleSystem bigSmoke;
    public ParticleSystem lilSmoke;

    [PunRPC]
    public void SetHiding()
    {
        isHiding = true;
    }

    [PunRPC]
    public void SetLastManStanding()
    {
        isLastManStanding = true;
    }

    [PunRPC]
    public void SetChasing()
    {
        isChasing = true;
    }

    [PunRPC]
    public void PlayLoseSound()
    {
        // do nothing on remote char
    }

    private void Start()
    {

        // deactivate all models except first
        for (int i = 0; i < playerModels.Length; i++)
        {
            if (i == 0)
            {
                playerModels[i].SetActive(true);
            }
            else
            {
                playerModels[i].SetActive(false);
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }
    }

    private void Update()
    {
        if (!isDriving)
        {
            // updates when not driving

        }
        else
        {
            // updates when driving
            transform.rotation = referenceCar.transform.rotation;
            transform.localPosition = drivingPos;
        }
    }

    [PunRPC]
    public void CharBlink()
    {
        StartCoroutine("Blink");
    }

    [PunRPC]
    public void CharShift(int sel)
    {
        playerModels[charSelect].SetActive(false);
        charSelect = sel;
        StartCoroutine("Shift");
    }

    private IEnumerator Blink()
    {
        // tele forward
        isBlinking = true;
        canBlink = false;

        lilSmoke.Play();
        audioSource.PlayOneShot(smokePoof);

        /*
        Vector3 goalPos = transform.position + transform.forward * 20;
        goalPos.y += 1.5f;
        transform.position = goalPos;
        */

        lilSmoke.Play();
        audioSource.PlayOneShot(smokePoof);

        isBlinking = false;

        yield return new WaitForSeconds(5);

        canBlink = true;

        StopCoroutine("Blink");
    }

    private IEnumerator Shift()
    {
        // dissapear
        isShifting = true;
        canShift = false;

        bigSmoke.Play();
        audioSource.PlayOneShot(smokePoof);

        yield return new WaitForSeconds(1.5f);

        // Reappear
        lilSmoke.Play();
        audioSource.PlayOneShot(smokePoof);

        yield return new WaitForSeconds(0.2f);
        playerModels[charSelect].GetComponent<Renderer>().material = materials[Random.Range(0, materials.Length)];
        playerModels[charSelect].SetActive(true);
        isShifting = false;
        StartCoroutine("ShiftCooldown");
        StopCoroutine("Shift");
    }

    private IEnumerator ShiftCooldown()
    {
        yield return new WaitForSeconds(5);
        canShift = true;
        Debug.Log("Shift cooldown ended");
        StopCoroutine("ShiftCooldown");
    }
}