using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraCarControls : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player in interact zone");
            other.gameObject.GetComponent<ExtraCharacterControls>().canInteractWithDriveable = true;
            other.gameObject.GetComponent<ExtraCharacterControls>().referenceCar = gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player left interact zone");
            other.gameObject.GetComponent<ExtraCharacterControls>().canInteractWithDriveable = false;
        }
    }
}
