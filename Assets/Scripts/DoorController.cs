using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    private Player player; //REFERENCE TO THE PLAYER CONTROLLER SO THAT PLAYER'S MOVEMENT CAN BE DISABLED UPOON ENTERING A DOORWAY
    private Transform exitPoint; //POINT TO WHICH THE PLAYER IS BEING TRANSPORTED UPON GOING THROUGH A DOORWAY

    private void Awake() 
    {
        Transform[] getExitPoints = GetComponentsInChildren<Transform>();

        exitPoint = getExitPoints[1];
    }

    private bool playerExiting; //TRUE IF THE PLAYER IS ALREADY EXITING THROUGH A DOORWAY
    [SerializeField] private float movePlayerSpeed; //SPEED OF BEING TRANSPORTED THROUGH A DOORWAY
    [SerializeField] public string levelToLoad; //NAME OF A SCENE THAT IS BEING LOADED UPON EXITING THROUGH THIS DOORWAY

    // Start is called before the first frame update
    void Start()
    {
        player = Player.instance; //GETTING THE PLAYER SCRIPT REFERENCE
    }

    // Update is called once per frame
    void Update()
    {
        if(playerExiting) //IF THE PLAYER IS EXITING, MOVE HIM FROM HIS CURRENT POSITION TOWARDS THE EXIT POINT AT SAID SPEED
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, exitPoint.transform.position, movePlayerSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag == "Player") 
        {
            if(!playerExiting) //ACTIVATE ONLY IF THE PLAYER ISN'T ALREADY EXITING
            {
                player.canMove = false; //BLOCK HIS MOVEMENT SO THAT HE CAN BE TRANSPORTED WITHOUT TURNIGN BACK, JUMPING ETC.

                StartCoroutine(UseDoorRoutine());
            }
        }    
    }

    IEnumerator UseDoorRoutine() //COROUTINE RESPONSIBLE FOR APPROPRIATE FUNCTIONALITY OF THE DOOR
    {
        playerExiting = true; //SETTING THIS VARIABLE AS TRUE SO THAT THIS COROUTINE ISN'T STARTED EACH FRAME THE PLAYER IS EXITING

        //player.StopResumeAnim(); //FREEZING PLAYER'S CURRENT SPRITE (DESIGN CHOICE, CAN BE CHANGED)

        UIController.instance.StartFadeToBlack(); //CONTACTING THE UI CONTROLLER AND STARTING FADING TO BLACK

        yield return new WaitForSeconds(1.5f); //WAITING FOR SOME TIME SO THAT THE PLAYER CAN BE MOVED TO HIS DESTINATION

        GameManager.instance.SetSpawn(exitPoint.position); //CONTACTING RESPAWN MANAGER AND SETTING PLAYER'S SPAWN TO HIS CURRENT POSITION
        player.canMove = true; //UNFREEZING PLAYER'S MOVEMENT AND SPRITE
        player.StopResumeAnim();

        UIController.instance.StartFadeFromBlack(); //FADING BACK FROM BLACK

        SceneManager.LoadScene(levelToLoad); //LAODING SAID SCENE USING SCENE MANAGER (UNITY BUILT IN FEATURE)
    }
}
