using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECM.Common;
using ECM.Controllers;
using ECM.Examples;
using DFTGames.Tools;

public class GamePlayerNetController : Photon.MonoBehaviour {

    [Header("Reference Vars")]
    CustomCharacterController controllerScript;
    CustomRemoteCharController remoteControllerScript;
    ExtraCharacterControls extraScript;
    Vector3 moveDirection = Vector3.zero;
    bool walk = false;
    bool crouch = false;
    bool firstTake = false;

    private void OnEnable()
    {
        firstTake = true;
    }

    private void Awake()
    {
        controllerScript = GetComponent<CustomCharacterController>();
        extraScript = GetComponent<ExtraCharacterControls>();

        if (photonView.isMine)
        {
            Debug.Log("Local player loaded in.");
            controllerScript.enabled = true;
            extraScript.enabled = true;
            Destroy(GetComponent<ExtraRemoteCharControls>());

            controllerScript.playerCamera = Camera.main.transform;
            GetComponent<AudioListener>().enabled = true;
            Camera.main.GetComponent<IsoCameraController>().player = gameObject;
            Camera.main.GetComponent<FadeObstructors>().playerTransform = transform;
            Camera.main.GetComponent<IsoCameraController>().enabled = true;
        } else
        {
            Debug.Log("Remote player loaded in.");
            Destroy(controllerScript);
            Destroy(extraScript);
            remoteControllerScript = gameObject.AddComponent<CustomRemoteCharController>();
            GetComponent<ExtraRemoteCharControls>().enabled = true;
        }

        gameObject.name = gameObject.name + "-" + GetComponent<PhotonView>().owner.NickName + "-" + photonView.viewID;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(controllerScript.moveDirection);
            stream.SendNext(controllerScript.crouch);
            stream.SendNext(controllerScript.walk);
        }
        else
        {
            //Network player, receive data
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
            moveDirection = (Vector3)stream.ReceiveNext();
            crouch = (bool)stream.ReceiveNext();
            walk = (bool)stream.ReceiveNext();


            // avoids lerping the character from "center" to the "current" position when this client joins
            if (firstTake)
            {
                firstTake = false;
                this.transform.position = correctPlayerPos;
                transform.rotation = correctPlayerRot;
            }

        }
    }

    private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

    void Update()
    {
        if (!photonView.isMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5);
            remoteControllerScript.moveDirection = moveDirection;
            remoteControllerScript.crouch = crouch;
            remoteControllerScript.walk = walk;
        }
    }
}
