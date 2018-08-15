using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSessionNetController : Photon.MonoBehaviour
{

    [Header("UI Settings")]
    public Text time;
    public Text header;
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip shortWhistle;
    public AudioClip longWhistle;
    public AudioClip crowdCheer;
    public AudioClip letsGo;

    [Header("Player Settings")]
    //local player
    public GameObject localPlayer;
    Transform localPlayerTransform;
    string localPlayerName;

    //remote player
    GameObject remotePlayer;
    Transform remotePlayerTransform;
    string remotePlayerName;

    //player types
    GameObject shiftPlayer;
    public Transform[] spawnPositions = new Transform[10]; // or use player limit var

    [Header("Connection Settings")]
    bool allPlayersConnected = false;
    int playersConnected = 0;

    [Header("Client Settings")]
    bool isMaster = false;
    bool isLoaded = false;

    [Header("Game Settings")]
    bool roundCountdownStarted = false;
    bool roundStarted = false;
    bool roundTimerStarted = false;
    public bool roundInProgress = false;
    public bool lastManStanding = false;
    public GameObject lastMan;
    public bool roundEnded = false;
    public bool roundEndDisplay = false;
    public bool gameEnded = false;
    int currentRound = 0;
    public int roundLimit = 1;
    public string Winner;

    [Header("Game Time Settings")]
    private float RawTime = 360.0f;
    private float ClockHR = 0.0F;
    private float ClockMN = 0.0F;
    private string ClockAMPM = "AM";
    public int ClockSpeedMultiplier = 3;
    public string ReadTime = "00:00AM";

    public GameObject[] chasers;


    // extra vars
    public int lobbyTimeOutSeconds = 30;

    // methods

    /// <summary>
    /// Awake
    /// </summary>
    public void Awake()
    {

        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player.ID);

        // in case we started this demo with the wrong scene being active, simply load the menu scene
        if (!PhotonNetwork.connected)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

    }

    /// <summary>
    /// OnEnable
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// OnDisable
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// OnSceneLoaded
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetComponent<PhotonView>().RPC("SignalLoad", PhotonTargets.All);
    }

    /// <summary>
    /// SignalLoad
    /// </summary>
    [PunRPC]
    public void SignalLoad()
    {
        playersConnected++;
    }

    /// <summary>
    /// Update
    /// </summary>
    public void Update()
    {
        if (playersConnected == PhotonNetwork.room.PlayerCount)
        {
            allPlayersConnected = true;
        }

        if (allPlayersConnected == true && isLoaded != true)
        {
            isLoaded = true;
            LoadResources();
            SpawnLobby();
        }

        if (isMaster)
        {
            if(allPlayersConnected == true && roundCountdownStarted == false && roundStarted == false && roundTimerStarted == false)
            {
                roundCountdownStarted = true;
                StartCoroutine("RoundCountdown");
            }

            // START THE ROUND
            if(roundStarted == true && roundTimerStarted == false)
            {
                GetComponent<PhotonView>().RPC("SetHeader", PhotonTargets.All, "MATCH STARTED");
                audioSource.PlayOneShot(shortWhistle);
                StartCoroutine("LetsGoAudio");
                roundTimerStarted = true;
                StartRound();
            }

            // CALCULATE ROUND
            if (roundStarted == true && roundTimerStarted == true)
            {
                roundInProgress = true;
                CalculateTime();
            }

            if(roundInProgress == true)
            {

                if (lastManStanding != true)
                {
                    Debug.Log("LMS not true");
                    // Check for last man standing
                    var players = GameObject.FindGameObjectsWithTag("Player");
                    List<GameObject> hidingPlayers = new List<GameObject>();

                    foreach (var player in players)
                    {
                        if (player.GetComponent<ExtraRemoteCharControls>() != null)
                        {
                            if (player.GetComponent<ExtraRemoteCharControls>().isHiding == true && player.GetComponent<ExtraRemoteCharControls>().isChasing != true)
                            {
                                hidingPlayers.Add(player);
                            }
                        }
                        else if (player.GetComponent<ExtraCharacterControls>() != null)
                        {
                            if (player.GetComponent<ExtraCharacterControls>().isHiding == true && player.GetComponent<ExtraCharacterControls>().isChasing != true)
                            {
                                hidingPlayers.Add(player);
                            }
                        }
                    }

                    if (hidingPlayers.Count == 1)
                    {
                        Debug.Log("Last man standing: " + hidingPlayers[0].GetComponent<PhotonView>().owner.NickName);
                        lastManStanding = true;
                        lastMan = hidingPlayers[0];
                        lastMan.GetComponent<PhotonView>().RPC("SetLastManStanding", PhotonTargets.All);
                    }
                    Debug.Log("Players hiding: " + hidingPlayers.Count);
                }

                if (lastManStanding == true)
                {
                    Debug.Log("LMS true");
                    if (lastMan.GetComponent<ExtraRemoteCharControls>() != null)
                    {
                        if (lastMan.GetComponent<ExtraRemoteCharControls>().isChasing == true)
                        {
                            roundEnded = true;
                        }
                    }
                    else if (lastMan.GetComponent<ExtraCharacterControls>() != null)
                    {
                        if (lastMan.GetComponent<ExtraCharacterControls>().isChasing == true)
                        {
                            roundEnded = true;
                        }
                    }
                }

                if (roundEnded == true && roundEndDisplay == false)
                {
                    GetComponent<PhotonView>().RPC("SetRoundEnded", PhotonTargets.All);
                }
            }
        }
    }

    [PunRPC]
    public void SetRoundEnded()
    {
        roundEndDisplay = true;
        roundStarted = false;
        audioSource.PlayOneShot(longWhistle, 2);

        GameObject[] all_players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in all_players)
        {
            player.SetActive(false);
        }

        GetComponent<PhotonView>().RPC("SetHeader", PhotonTargets.All, "ROUND END");
        GetComponent<PhotonView>().RPC("SetTime", PhotonTargets.All, "0:00");
        StartCoroutine("EndGame");
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(10);
        PhotonNetwork.DestroyAll();
        PhotonNetwork.LeaveRoom();
    }

    private IEnumerator RoundCountdown()
    {
        GetComponent<PhotonView>().RPC("SetHeader", PhotonTargets.All, "MATCH STARTING IN:");

        for (int i = 30; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            string niceTime = i < 10 ? "00:0" + i: "00:" + i;
            GetComponent<PhotonView>().RPC("SetTime", PhotonTargets.All, niceTime);
        }

        roundCountdownStarted = false;
        roundStarted = true;
        StopCoroutine("RoundCountdown");
    }

    private IEnumerator LetsGoAudio()
    {
        yield return new WaitForSeconds(1.5f);
        audioSource.PlayOneShot(letsGo);
    }

    private void StartRound()
    {
        // pick a random player to be the 'chaser'
        var players = GameObject.FindGameObjectsWithTag("Player");
        int rand_player = Random.Range(1, players.Length);
        int countUp = 0;
        foreach(var player in players)
        {
            countUp++;
            if(rand_player == countUp)
            {
                player.GetComponent<PhotonView>().RPC("SetChasing", PhotonTargets.All);
                if(player.GetComponent<ExtraRemoteCharControls>() != null)
                {
                    player.GetComponent<ExtraRemoteCharControls>().isChasing = true;
                }
            } else
            {
                player.GetComponent<PhotonView>().RPC("SetHiding", PhotonTargets.All);
                if (player.GetComponent<ExtraRemoteCharControls>() != null)
                {
                    player.GetComponent<ExtraRemoteCharControls>().isHiding = true;
                }
            }
        }
    }

    /// <summary>
    /// CalculateTime
    /// </summary>
    public void CalculateTime()
    {
        RawTime -= Time.deltaTime;

        if (RawTime < 0)
        {
            roundEnded = true;
        } else
        {
            int minutes = Mathf.FloorToInt(RawTime / 60F);
            int seconds = Mathf.FloorToInt(RawTime - minutes * 60);
            string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
            GetComponent<PhotonView>().RPC("SetTime", PhotonTargets.All, niceTime);
        }
    }

    [PunRPC]
    public void SetHeader(string _text)
    {
        header.text = _text;
    }

    [PunRPC]
    public void SetTime(string _time)
    {
        time.text = _time;
    }

    /// <summary>
    /// LoadResources
    /// </summary>
    private void LoadResources()
    {
        shiftPlayer = Resources.Load("Shifting Player") as GameObject;
    }

    /// <summary>
    /// SpawnLobby
    /// </summary>
    private void SpawnLobby()
    {
        Debug.Log("Spawning lobby...");

        // get current players index
        var objects = GameObject.FindGameObjectsWithTag("Player");
        int loadedInCount = 0;
        foreach (var obj in objects)
        {
            loadedInCount++;
        }

        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
        localPlayerName = (string)PhotonNetwork.player.NickName;
        localPlayer = PhotonNetwork.Instantiate(shiftPlayer.name, spawnPositions[loadedInCount].position, spawnPositions[loadedInCount].rotation, 0);
        localPlayer.name = localPlayerName;

        Debug.Log("Spawned " + localPlayerName);

        if (localPlayer.GetComponent<PhotonView>().owner.IsMasterClient)
        {
            // if the local player is the host, generate world
            isMaster = true;
            //GenerateWorld(); ??
        }
        else
        {
            isMaster = false;
        }
    }

    /// <summary>
    /// OnGui
    /// </summary>
    public void OnGUI()
    {
        if (GUILayout.Button("Leave Game"))
        {

            PhotonNetwork.LeaveRoom();  // we will load the menu level when we successfully left the room
        }
    }

    /// <summary>
    /// OnMasterClientSwitched
    /// </summary>
    /// <param name="player"></param>
    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        // notify when master host changes?
    }

    /// <summary>
    /// The rest of these functions are standard
    /// </summary>

    public void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom (local)");
        // back to main menu
        SceneManager.LoadScene("MainMenu");
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("OnDisconnectedFromPhoton");

        // back to main menu
        SceneManager.LoadScene("MainMenu");
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPlayerDisconneced: " + player);
    }

    public void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");

        // back to main menu
        SceneManager.LoadScene("MainMenu");
    }
}