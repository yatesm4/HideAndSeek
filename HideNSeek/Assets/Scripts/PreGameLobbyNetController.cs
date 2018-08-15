using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreGameLobbyNetController : Photon.MonoBehaviour
{

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
    GameObject lobbyPlayer;
    public Transform[] spawnPositions = new Transform [10]; // or use player limit var

    [Header("Client Settings")]
    bool isMaster = false;

    InRoomChat chat;

    // extra vars
    public int lobbyTimeOutSeconds = 30;

    public void ReadyUp()
    {
        localPlayer.GetComponent<LobbyPlayerNetController>().isReady = true;
    }

    public void CancelReadyUp()
    {
        localPlayer.GetComponent<LobbyPlayerNetController>().isReady = false;
    }

    public void Awake()
    {

        // in case we started this demo with the wrong scene being active, simply load the menu scene
        if (!PhotonNetwork.connected)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        LoadResources();
        SpawnLobby();

    }

    private void LoadResources()
    {
        lobbyPlayer = Resources.Load("Lobby Player") as GameObject;
        chat = GetComponent<InRoomChat>();
    }

    private void SpawnLobby()
    {
        Debug.Log("Spawning lobby...");

        // get current players index
        int playerCountIndex = (PhotonNetwork.room.PlayerCount - 1);

        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
        localPlayerName = (string)PhotonNetwork.player.NickName;
        localPlayer = PhotonNetwork.Instantiate(lobbyPlayer.name, spawnPositions[playerCountIndex].position, spawnPositions[playerCountIndex].rotation, 0);
        localPlayer.name = localPlayerName;

        Debug.Log("Spawned " + localPlayerName);

        if (localPlayer.GetComponent<PhotonView>().owner.IsMasterClient)
        {
            // if the local player is the host, generate world
            isMaster = true;
            //GenerateWorld(); ??
        } else
        {
            isMaster = false;
        }
    }

    public void Update()
    {
        // if lobby host, calculate time
        if (isMaster)
        {
            CheckForAllPlayersReadyUp();
        }
    }

    void CheckForAllPlayersReadyUp()
    {
        var objects = GameObject.FindGameObjectsWithTag("Player");
        int readyCount = 0;
        foreach(var obj in objects)
        {
            if(obj.GetComponent<LobbyPlayerNetController>().isReady == true)
            {
                readyCount++;
            }
        }

        if (readyCount == PhotonNetwork.room.PlayerCount)
        {
            Debug.Log("All players ready");
            PhotonNetwork.LoadLevel("City");

        } else
        {
            Debug.Log("Not all players ready");
        }
    }


    /*
    // called at beginning of lobby on creation by lobby host
    public void GenerateWorld()
    {
        // Do world generation here...
        // spawn npcs, items, quests, etc

        // Load NPCs
        var prefab = Resources.Load("NPC_Croc_A") as GameObject;
        Vector3 npcSpawnPos = transform.position;
        npcSpawnPos.x += 3;
        npc = PhotonNetwork.Instantiate(prefab.name, npcSpawnPos, Quaternion.identity, 0);
        npc.GetComponent<CustomNpcController>().createdLocally = true;
    }
    */

    public void OnGUI()
    {
        if (GUILayout.Button("Return to Main Menu"))
        {
            PhotonNetwork.LeaveRoom();  // we will load the menu level when we successfully left the room
        }
    }

    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        // notify when master host changes
        if (isMaster)
        {
            chat.AddLine("You are now the lobby host.");
        } else
        {
            chat.AddLine(player.NickName + " is now the lobby host");
        }
    }

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
        chat.AddLine(player.NickName + " has joined the room.");
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPlayerDisconneced: " + player);
        chat.AddLine(player.NickName + " has left the room.");

        UpdatePlayerPositions();
    }

    private void UpdatePlayerPositions()
    {
        Debug.Log("Updating player positions");
        var objs = GameObject.FindGameObjectsWithTag("Player");
        int i = 0;
        foreach (var obj in objs)
        {
            obj.transform.position = spawnPositions[i].position;
            obj.transform.rotation = spawnPositions[i].rotation;
            i++;
        }
    }

    public void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");

        // back to main menu
        SceneManager.LoadScene("MainMenu");
    }
}