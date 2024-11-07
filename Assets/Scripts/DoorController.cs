using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    private PlayerController _player;
    private bool _playerExiting;
    public Transform exitPoint;
    public float movePlayerSpeed;
    public string levelToLoad;

    // Start is called before the first frame update
    void Start()
    {
        _player = PlayerController.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(_playerExiting)
        {
            _player.transform.position = Vector3.MoveTowards(_player.transform.position, exitPoint.transform.position, movePlayerSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag == "Player")
        {
            if(!_playerExiting)
            {
                _player.canMove = false;

                StartCoroutine(UseDoorRoutine());
            }
        }    
    }

    IEnumerator UseDoorRoutine()
    {
        _playerExiting = true;

        _player.anim.enabled = false;

        //fade to black

        yield return new WaitForSeconds(1.5f);

        RespawnController.instance.SetSpawn(exitPoint.position);
        _player.canMove = true;
        _player.anim.enabled = true;

        //fade from black

        SceneManager.LoadScene(levelToLoad);
    }
}
