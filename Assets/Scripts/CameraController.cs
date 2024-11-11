using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Player _player; //REFERENCE TO THE PLAYER SCRIPT
    private BoxCollider2D _cameraBounds; //BOUNDS OF THE CAMERA MOVEMENT

    private void Awake() 
    {
        _cameraBounds = GameObject.Find("Camera Bounds").GetComponent<BoxCollider2D>();    
    }

    //[SerializeField]
    private float _halfWidth, _halfHeight; //HALF WIDTH AND HALF HEIGHT OF THE CAMERA

    // Start is called before the first frame update
    void Start()
    {
        //SETTING THE STARTING POSITION OF THE CAMERA ON THE PLAYER
        _player = Player.instance;

        //GETTING THE OTHER VARIABLES
        _halfHeight = Camera.main.orthographicSize;
        _halfWidth = _halfHeight * Camera.main.aspect;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //GETTING THE PLAYERS POSITION, ASIGNING IT TO THE CAMERA AND CLAMPING IT IN BOUNDS
        if(_player)
        {
            transform.position = new Vector3(
                Mathf.Clamp(_player.transform.position.x, _cameraBounds.bounds.min.x + _halfWidth, _cameraBounds.bounds.max.x - _halfWidth),
                Mathf.Clamp(_player.transform.position.y, _cameraBounds.bounds.min.y + _halfHeight, _cameraBounds.bounds.max.y - _halfHeight),
                -10f //KEEPING THE CAMERA AWAY ON THE Z AXIS SO THAT IT CAN SEE THE SCENE AT ALL TIMES
            );
        }
    }
}
