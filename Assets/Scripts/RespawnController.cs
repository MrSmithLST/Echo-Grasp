using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnController : MonoBehaviour
{
    public static RespawnController instance;
    private void Awake() 
    {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private Vector3 _respawnPoint;
    public float waitToRespawn;
    private GameObject _player;
    public GameObject deathEffect;

    // Start is called before the first frame update
    void Start()
    {
        _player = PlayerHealthController.instance.gameObject;

        _respawnPoint = _player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Respawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        _player.SetActive(false);
        if(deathEffect)
        {
            Instantiate(deathEffect, PlayerHealthController.instance.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(waitToRespawn);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        _player.transform.position = _respawnPoint;
        _player.SetActive(true);

        PlayerHealthController.instance.RefillHealth();
    }

    public void SetSpawn(Vector3 newPosition)
    {
        _respawnPoint = newPosition;
    }
}
