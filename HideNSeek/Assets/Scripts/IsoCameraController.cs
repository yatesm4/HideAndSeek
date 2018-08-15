using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoCameraController : MonoBehaviour {

    // INIT vars
    public GameObject player;
    private float smooth_time = 0.3f;
    private Vector3 offset;

    public int distance_from_player = 8;
    public int height_from_player = 3;

    public bool inChat = false;

    public Vector3 goalPos;
    private Vector3 velocity = Vector3.zero;
    private Vector3 up;

    // set rotation for viewing angle
    public int rotation = 0;

	// Use this for initialization
	void Start () {
        // get offset from player pos to camera
        offset = transform.position - player.transform.position;
        // set up var
        up = transform.up;
	}

    private void Update()
    {

    }

    void LateUpdate()
    {
        goalPos = player.transform.position;
        goalPos.y = player.transform.position.y + height_from_player;

        if (Input.GetKeyDown("e") && !inChat)
        {
            rotation += 90;
        }
        else if (Input.GetKeyDown("q") && !inChat)
        {
            if (rotation == 0)
            {
                rotation = 360;
            }
            rotation -= 90;
        }

        if (rotation == 0 || rotation == 360)
        {
            goalPos.z += distance_from_player;
            goalPos.x -= distance_from_player;
            // reset rotation incase of 360
            rotation = 0;
        }
        else if (rotation == 90)
        {
            goalPos.z -= distance_from_player;
            goalPos.x -= distance_from_player;
        }
        else if (rotation == 180)
        {
            goalPos.z -= distance_from_player;
            goalPos.x += distance_from_player;
        }
        else if (rotation == 270)
        {
            goalPos.x += distance_from_player;
            goalPos.z += distance_from_player;
        }

        transform.position = Vector3.SmoothDamp(transform.position, goalPos, ref velocity, smooth_time);
        Vector3 relative_pos = player.transform.position - transform.position;
        relative_pos.y += 3;
        transform.rotation = Quaternion.LookRotation(relative_pos);
        //transform.position = player.transform.position + offset;
    }
}
