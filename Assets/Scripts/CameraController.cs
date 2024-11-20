using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Player player; //REFERENCE TO THE PLAYER SCRIPT
    private BoxCollider2D cameraBounds; //BOUNDS OF THE CAMERA MOVEMENT

    private void Awake() 
    {
        cameraBounds = GameObject.Find("Camera Bounds").GetComponent<BoxCollider2D>();    
    }

    //[SerializeField]
    private float halfWidth, halfHeight; //HALF WIDTH AND HALF HEIGHT OF THE CAMERA

    // Start is called before the first frame update
    void Start()
    {
        //SETTING THE STARTING POSITION OF THE CAMERA ON THE PLAYER
        player = Player.instance;

        //GETTING THE OTHER VARIABLES
        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //GETTING THE PLAYERS POSITION, ASIGNING IT TO THE CAMERA AND CLAMPING IT IN BOUNDS
        if(player)
        {
            transform.position = new Vector3(
                Mathf.Clamp(player.transform.position.x, cameraBounds.bounds.min.x + halfWidth, cameraBounds.bounds.max.x - halfWidth),
                Mathf.Clamp(player.transform.position.y, cameraBounds.bounds.min.y + halfHeight, cameraBounds.bounds.max.y - halfHeight),
                -10f //KEEPING THE CAMERA AWAY ON THE Z AXIS SO THAT IT CAN SEE THE SCENE AT ALL TIMES
            );
        }
    }
}
