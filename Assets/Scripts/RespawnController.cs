using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnController : MonoBehaviour
{
    public static RespawnController instance; //CREATING AN INSTANCE OF THIS SCRIPT SO THAT IT CAN BE EASILY ACCESSED FROM ANYWHERE
    private void Awake() 
    {
        if(!instance) //IF THERE IS NO INSTANCE YET
        {
            instance = this; //SET THIS AS AN INSTANCE
            DontDestroyOnLoad(gameObject); //AND DON'T DESTROY THIS OBJECT ON LOADING TO ANOTHER SCENE
        }
        else
        {
            Destroy(this.gameObject); //DESTROY THIS OBJECT OTHERWISE TO NOT HAVE UNNECESSARY COPIES
        }
    }

    private Vector3 _respawnPoint; //POINT AT WHICH THE PLAYER WILL RESPAWN AFTER DYING OR LOADING INTO ANOTHER SCENE
    public float waitToRespawn; //TIME THE PLAYER MUST WAIT FOR IN ORDER TO RESPAWN
    private GameObject _player; //REFERENCE TO THE PLAYER
    public GameObject deathEffect; //EFFECT THAT IS BEING CREATED UPON THE PLAYER DYING

    // Start is called before the first frame update
    void Start()
    {
        _player = PlayerHealthController.instance.gameObject; //GETTING THE PLAYER GAME OBJECT THROUGH AN INSTANCE OF THE PLAYER HEALTH CONTROLLER

        _respawnPoint = _player.transform.position; //SETTING THE RESPAWN POINT TO THE CURRENT POSITION OF THE PLAYER ON START
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Respawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine() //ROUTINE RESPONSIBLE FOR RESPAWNING THE PLAYER IN THE CORRECT POSITION
    {
        _player.SetActive(false); //DISABLE THE PLAYER GAME OBJECT UPON DYING
        if(deathEffect) //CREATE A DEATH EFFECT AT THE PLAYER'S POSITION IF IT EXISTS
        {
            Instantiate(deathEffect, PlayerHealthController.instance.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(waitToRespawn); //WAIT FOR TIME TO RESPAWN

        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //RELOAD CURRENT SCENE THROUGH SCENE MANAGER (UNITY BUILT IN FEATURE)

        _player.transform.position = _respawnPoint; //SET PLAYER'S POSITION TO THE LAST RESPAWN POINT
        _player.SetActive(true); //ENABLE THE PLAYER GAME OBJECT AFTER RESPAWNING

        PlayerHealthController.instance.RefillHealth(); //MAKE HIS HEALTH FULL BACK AGAIN
    }

    public void SetSpawn(Vector3 newPosition) //UPDATES THE RESPAWN POINT WITH NEW POSITION
    {
        _respawnPoint = newPosition;
    }
}
