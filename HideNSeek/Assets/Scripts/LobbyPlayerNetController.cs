using UnityEngine;
using System.Collections;
using ECM.Controllers;
using ECM.Common;

public class LobbyPlayerNetController : Photon.MonoBehaviour
{
    #region EDITOR EXPOSED FIELDS

    [Header("Reference Vars")]
    public string playerName;
	bool firstTake = false;
    public TextMesh playerNameText;
    public GameObject statusText;

    public bool isReady = false;

    public Material[] materials;
    public GameObject[] playerModels;
    int charSelect = 0;

    #endregion

    #region METHODS

    #region OnEnable
    void OnEnable()
	{
	 	firstTake = true;	 
	}
    #endregion

    #region Awake
    /// <summary>
    /// On Object awaken, immediately set controller script to get Custom Player Controller
    /// if the owner of this is local player, enable the controller script
    /// else ensure to disable controller script, and add other player controller script (destroy controller script too)
    /// </summary>
    void Awake()
    {

        playerNameText = GetComponentInChildren<TextMesh>();

        // deactivate all models
        for (int i = 0; i < playerModels.Length; i++)
        {
            playerModels[i].SetActive(false);

        }

        charSelect = Random.Range(0, playerModels.Length);

        playerModels[charSelect].GetComponent<Renderer>().material = materials[Random.Range(0, materials.Length)];
        playerModels[charSelect].SetActive(true);

        if (photonView.isMine)
        {
            //MINE: local player, simply enable the local scripts
            Debug.Log("Your player is loaded into the lobby.");
            playerName = PhotonNetwork.player.NickName;
        }
        else
        {
            Debug.Log("Another player has loaded into the lobby.");
            playerName = GetComponent<PhotonView>().owner.NickName;
        }

        playerNameText.text = playerName;
        gameObject.name = gameObject.name + "-" + playerName + "-" + photonView.viewID;
    }
    #endregion

    #region Photon - SerializeView (Stream)
    /// <summary>
    /// Essentially once the object is connect and is connected to the server:
    ///     if the object is writing to the server:
    ///         send position, rotation, move direction, and punch input
    ///     if the object is reading from the server:
    ///         receive position, rotation, move direction, and punch input
    /// </summary>
    /// <param name="stream">the network stream being written to/read from</param>
    /// <param name="info">info about the message/sender</param>
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isReady);
        }
        else
        {
            //Network player, receive data
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
            isReady = (bool)stream.ReceiveNext();


			// avoids lerping the character from "center" to the "current" position when this client joins
			if (firstTake)
			{
				firstTake = false;
				this.transform.position = correctPlayerPos;
				transform.rotation = correctPlayerRot;
			}

        }
    }
    #endregion

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    #region Update
    /// <summary>
    /// On every frame if not owned by local player:
    ///     set position, rotation, move direction, and punch input to valuess read from network stream
    /// </summary>
    void Update()
    {

        if(isReady == true)
        {
            statusText.SetActive(true);
        } else
        {
            statusText.SetActive(false);
        }

        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
        }
    }
    #endregion

    #endregion

}
