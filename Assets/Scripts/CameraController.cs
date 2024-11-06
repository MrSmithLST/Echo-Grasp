using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public BoxCollider2D cameraBounds; //BOUNDS OF THE CAMERA MOVEMENT
    private float _halfWidth, _halfHeight; //HALF WIDTH AND HALF HEIGHT OF THE CAMERA
    public PlayerController _player; //REFERENCE TO THE PLAYER SCRIPT

    // Start is called before the first frame update
    void Start()
    {
        //SETTING THE STARTING POSITION OF THE CAMERA ON THE PLAYER
        transform.position = _player.transform.position;

        //GETTING THE OTHER VARIABLES
        _halfHeight = Camera.main.orthographicSize;
        _halfWidth = _halfHeight * Camera.main.aspect;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //FOLLOWING THE PLAYER WITH THE CAMERA IN BOUNDS
        if(_player)
        {
            transform.position = new Vector3(
                Mathf.Clamp(_player.transform.position.x, cameraBounds.bounds.min.x + _halfWidth, cameraBounds.bounds.max.x + _halfWidth),
                Mathf.Clamp(_player.transform.position.y, cameraBounds.bounds.min.y + _halfHeight, cameraBounds.bounds.max.y + _halfHeight),
                -10f
            );
        }
    }
}
