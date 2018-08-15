using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DFTGames.Tools;
using UnityStandardAssets.Vehicles.Car;
using ECM.Examples;

public class PlayerRenderInfo
{
    GameObject player;
    GameObject model;
    Material player_material;

    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }

    public GameObject GetPlayer()
    {
        return this.player;
    }

    public void SetModel(GameObject model)
    {
        this.model = model;
    }
    public GameObject GetModel()
    {
        return this.model;
    }

    public void SetMaterial(Material mat)
    {
        this.player_material = mat;
    }

    public Material GetMaterial()
    {
        return this.player_material;
    }
}

public class ExtraCharacterControls : MonoBehaviour {

    // Interaction variables
    [Header("UI Properties")]
    Text UI_BlinkStack;
    Text UI_BlinkCooldown;
    Text UI_RoundInfo;

    // Driving
    [Header("Driving Properties")]
    public bool canInteractWithDriveable = false;
    public bool isDriving = false;
    public GameObject referenceCar;
    Vector3 drivingPos = new Vector3(-0.5f, -0.35f, 0.1f);

    string driveTag = "Driveable";

    // changing models
    [Header("Abilities Properties")]
    // shifting models
    public GameObject[] playerModels;
    public Material[] materials;
    int charSelect = 0;
    bool isShifting = false;
    bool canShift = true;

    //blinking
    bool isBlinking = false;
    bool canBlink = true;

    public int blinkStackMax = 2;
    public int blinkStack = 2;
    public int blinkStackRegainCooldown = 8;

    // chasing
    public Material seeThruMat;
    bool hasVision = false;
    bool canHaveVision = false;
    public int visionCooldown = 25;
    PlayerRenderInfo[] arr;

    [Header("GameSession References")]
    public bool isHiding = true;
    public bool isChasing = false;
    bool isChasingStarted = false;
    public bool isLastManStanding = false;

    // audio
    [Header("Audio Properties")]
    public AudioSource audioSource;
    public AudioClip smokePoof;

    // fx
    [Header("FX Properties")]
    public ParticleSystem bigSmoke;
    public ParticleSystem lilSmoke;
    public ParticleSystem flare;

    [PunRPC]
    public void SetHiding()
    {
        isHiding = true;
        UI_RoundInfo.text = "START HIDING";
        StartCoroutine("ClearHeaderText");
    }

    [PunRPC]
    public void SetLastManStanding()
    {
        isLastManStanding = true;
        UI_RoundInfo.text = "LAST MAN STANDING";
        StartCoroutine("ClearHeaderText");
    }

    [PunRPC]
    public void SetChasing()
    {
        isHiding = false;
        isChasing = true;
        UI_RoundInfo.text = "HUNT PLAYERS DOWN";
        StartCoroutine("ClearHeaderText");
        blinkStackMax = 3;
        blinkStack = 3;
        UI_BlinkStack.text = "3";
        blinkStackRegainCooldown = 5;
        canHaveVision = true;

    }

    [PunRPC]
    public void PlayLoseSound()
    {
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameSessionNetController>().audioSource.PlayOneShot(GameObject.FindGameObjectWithTag("GameController").GetComponent<GameSessionNetController>().loseSound, 2);
    }

	private void Start () {

        UI_BlinkStack = GameObject.FindGameObjectWithTag("BlinkStackCount").GetComponent<Text>();
        UI_BlinkCooldown = GameObject.FindGameObjectWithTag("BlinkCooldownTimer").GetComponent<Text>();
        UI_RoundInfo = GameObject.FindGameObjectWithTag("RoundInfoHeader").GetComponent<Text>();

        // deactivate all models except first
        for (int i = 0; i < playerModels.Length; i++)
        {
            if (i == 0)
            {
                playerModels[i].SetActive(true);
            } else
            {
                playerModels[i].SetActive(false);
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }
    }
	
	private void Update () {

        // if player falls out of maps
        if(transform.position.y < -2.0f)
        {
            Vector3 goalPos = transform.position;
            goalPos.y = 2.0f;
            transform.position = goalPos;
        }

		if (!isDriving)
        {
            // updates when not driving

            if (!isShifting && !isBlinking)
            {
                // handle inputs
                if (canShift)
                {
                    CharShiftInput();
                }

                if (canBlink && blinkStack > 0)
                {
                    CharBlinkInput();
                }

                if(canHaveVision && !hasVision)
                {
                    VisionInput();
                }

                if (canInteractWithDriveable)
                {
                    DriveableInteractionInput();
                }
            }
        } else
        {
            // updates when driving
            transform.rotation = referenceCar.transform.rotation;
            transform.localPosition = drivingPos;

            IsDrivingInput();
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (isChasing)
        {
            if (other.gameObject.CompareTag("Player") && other.gameObject.GetComponent<ExtraRemoteCharControls>().isChasing == false)
            {
                Debug.Log("Player in interact zone");
                other.gameObject.GetComponent<PhotonView>().RPC("SetChasing", PhotonTargets.All);
                other.gameObject.GetComponent<PhotonView>().RPC("PlayLoseSound", PhotonTargets.All);
                UI_RoundInfo.text = "YOU CAUGHT " + other.gameObject.GetComponent<PhotonView>().owner.NickName;
                StartCoroutine("ClearHeaderText");
            }
        }

    }

    private void IsDrivingInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isDriving = false;

            transform.SetParent(null);
            // transform.localPosition = Vector3.zero;

            GetComponent<CustomCharacterController>().enableMove = true;

            referenceCar.tag = "Driveable";
            referenceCar.GetComponent<CarUserControl>().enabled = false;
            referenceCar = null;

            Camera.main.GetComponent<IsoCameraController>().player = gameObject;
            Camera.main.GetComponent<FadeObstructors>().playerTransform = gameObject.transform;

        }
    }

    private void DriveableInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // set this' vars
            isDriving = true;
            canInteractWithDriveable = false;

            // set player model
            //playerModels[charSelect].SetActive(false);
            transform.SetParent(referenceCar.transform);
            transform.localPosition = drivingPos;
            GetComponent<CustomCharacterController>().enableMove = false;

            // set car
            referenceCar.tag = "Player";
            referenceCar.GetComponent<CarUserControl>().enabled = true;

            // set main camera
            Camera.main.GetComponent<IsoCameraController>().player = referenceCar;
            Camera.main.GetComponent<FadeObstructors>().playerTransform = referenceCar.transform;
        }
    }

    private void CharBlinkInput()
    {
        if (Input.GetMouseButtonDown(1)){
            GetComponent<PhotonView>().RPC("CharBlink", PhotonTargets.All);
        }
    }

    private void CharShiftInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int sel = Random.Range(0, (playerModels.Length - 1));
            GetComponent<PhotonView>().RPC("CharShift", PhotonTargets.All, sel);
        }
    }

    private void VisionInput()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Vision();
        }
    }

    private void Vision()
    {
        hasVision = true;
        Debug.Log("You have vision!");
        var players = GameObject.FindGameObjectsWithTag("Player");
        arr = new PlayerRenderInfo[players.Length];
        int i = 0;
        foreach(var player in players)
        {
            if (player.GetComponent<PhotonView>().isMine)
            {
                continue;
            }
            arr[i] = new PlayerRenderInfo();
            arr[i].SetPlayer(player);
            GameObject[] models = player.GetComponent<ExtraRemoteCharControls>().playerModels;
            foreach(var model in models)
            {
                if(model.activeSelf == true)
                {
                    arr[i].SetModel(model);
                    arr[i].SetMaterial(model.GetComponent<Renderer>().material);
                    model.GetComponent<Renderer>().material = seeThruMat;
                }
            }
            i++;
        }
        StartCoroutine("VisionCoolDown");

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

    private IEnumerator VisionCoolDown()
    {
        yield return new WaitForSeconds(10);
        GameObject activeSkin = new GameObject();
        int i = 0;
        foreach(var player in arr)
        {
            arr[0].GetModel().GetComponent<Renderer>().material = arr[0].GetMaterial();
            i++;
        }
        hasVision = false;
        canHaveVision = false;
        yield return new WaitForSeconds(visionCooldown);
        canHaveVision = true;
    }

    private IEnumerator Blink()
    {
        StopCoroutine("BlinkStackRegain");
        // tele forward
        blinkStack--;

        UI_BlinkStack.text = "" + blinkStack + "";
        Debug.Log("Blinks Available: " + blinkStack);

        StartCoroutine("BlinkStackRegain");

        isBlinking = true;
        canBlink = false;

        Color base_color = UI_BlinkStack.color;
        UI_BlinkStack.color = Color.gray;

        lilSmoke.Play();
        audioSource.PlayOneShot(smokePoof);

        Vector3 goalPos = transform.position + transform.forward * 20;
        goalPos.y += 1.5f;
        transform.position = goalPos;

        isBlinking = false;

        yield return new WaitForSeconds(5);

        canBlink = true;
        UI_BlinkStack.color = base_color;
        yield break;
    }

    private IEnumerator BlinkStackRegain()
    {
        UI_BlinkCooldown.text = "-";
        for (int i = blinkStackRegainCooldown; i > -1; i--)
        {
            yield return new WaitForSeconds(1);
            UI_BlinkCooldown.text = "" + i + "";
            // display cooldown/regain text on gui ?
        }
        UI_BlinkCooldown.text = "";
        if (blinkStack < blinkStackMax)
        {
            blinkStack++;
            UI_BlinkStack.text = "" + blinkStack + "";
        }
        Debug.Log("Blinks Available: " + blinkStack);
        if (blinkStack < blinkStackMax)
        {
            StartCoroutine("BlinkStackRegain");
        }
        yield break;
    }

    private IEnumerator Shift()
    {
        // dissapear
        isShifting = true;
        canShift = false;

        bigSmoke.Play();
        audioSource.PlayOneShot(smokePoof);

        flare.Play();

        yield return new WaitForSeconds(1.5f);

        flare.Stop();

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
        yield return new WaitForSeconds(5f);
        canShift = true;
        Debug.Log("Shift cooldown ended");
        StopCoroutine("ShiftCooldown");
    }

    private IEnumerator ClearHeaderText()
    {
        string text = UI_RoundInfo.text;
        Debug.Log("Clearing text: " + text);
        yield return new WaitForSeconds(2f);
        foreach(char letter in text)
        {
            yield return new WaitForSeconds(Random.Range(0.0f, 0.05f));
            Debug.Log(letter);
            text = text.Remove(text.ToString().LastIndexOf(letter), 1);
            Debug.Log(text);
            UI_RoundInfo.text = text;
        }
        UI_RoundInfo.text = "";
        yield break;
    }
}
