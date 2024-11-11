using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    private Player _player; //REFERENCE TO THE PLAYER CONTROLLER SO THAT PLAYER'S MOVEMENT CAN BE DISABLED UPOON ENTERING A DOORWAY
    private Transform _exitPoint; //POINT TO WHICH THE PLAYER IS BEING TRANSPORTED UPON GOING THROUGH A DOORWAY

    private void Awake() 
    {
        _exitPoint = GameObject.Find("Exit Point").GetComponent<Transform>();
    }

    private bool _playerExiting; //TRUE IF THE PLAYER IS ALREADY EXITING THROUGH A DOORWAY
    [SerializeField] private float _movePlayerSpeed; //SPEED OF BEING TRANSPORTED THROUGH A DOORWAY
    [SerializeField] public string _levelToLoad; //NAME OF A SCENE THAT IS BEING LOADED UPON EXITING THROUGH THIS DOORWAY

    // Start is called before the first frame update
    void Start()
    {
        _player = Player.instance; //GETTING THE PLAYER SCRIPT REFERENCE
    }

    // Update is called once per frame
    void Update()
    {
        if(_playerExiting) //IF THE PLAYER IS EXITING, MOVE HIM FROM HIS CURRENT POSITION TOWARDS THE EXIT POINT AT SAID SPEED
        {
            _player.transform.position = Vector3.MoveTowards(_player.transform.position, _exitPoint.transform.position, _movePlayerSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag == "Player") 
        {
            if(!_playerExiting) //ACTIVATE ONLY IF THE PLAYER ISN'T ALREADY EXITING
            {
                _player.canMove = false; //BLOCK HIS MOVEMENT SO THAT HE CAN BE TRANSPORTED WITHOUT TURNIGN BACK, JUMPING ETC.

                StartCoroutine(UseDoorRoutine());
            }
        }    
    }

    IEnumerator UseDoorRoutine() //COROUTINE RESPONSIBLE FOR APPROPRIATE FUNCTIONALITY OF THE DOOR
    {
        _playerExiting = true; //SETTING THIS VARIABLE AS TRUE SO THAT THIS COROUTINE ISN'T STARTED EACH FRAME THE PLAYER IS EXITING

        _player.StopResumeAnim(); //FREEZING PLAYER'S CURRENT SPRITE (DESIGN CHOICE, CAN BE CHANGED)

        UIController.instance.StartFadeToBlack(); //CONTACTING THE UI CONTROLLER AND STARTING FADING TO BLACK

        yield return new WaitForSeconds(1.5f); //WAITING FOR SOME TIME SO THAT THE PLAYER CAN BE MOVED TO HIS DESTINATION

        RespawnController.instance.SetSpawn(_exitPoint.position); //CONTACTING RESPAWN MANAGER AND SETTING PLAYER'S SPAWN TO HIS CURRENT POSITION
        _player.canMove = true; //UNFREEZING PLAYER'S MOVEMENT AND SPRITE
        _player.StopResumeAnim();

        UIController.instance.StartFadeFromBlack(); //FADING BACK FROM BLACK

        SceneManager.LoadScene(_levelToLoad); //LAODING SAID SCENE USING SCENE MANAGER (UNITY BUILT IN FEATURE)
    }
}
