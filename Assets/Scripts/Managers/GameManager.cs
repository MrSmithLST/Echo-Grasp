using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance; //CREATING AN INSTANCE SO THAT EVERY SCRIPT CAN EASILY COMMUNICATE WITH THE GAME MANAGER

    private void Awake()
    {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Variables
    public Player player;

    [Header("Health")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth; //MAXIMUM HEALTH THAT THE PLAYER CAN HAVE 

    [Header("Invincibility")]
    [SerializeField] private float invincibilityLength; //DURATION OF I-FRAMES AFTER BEING DAMAGED
    [SerializeField] private float flashLength; //DURATION OF SINGULAR FLASH OF PLAYER'S SPRITE INDICATING THAT THE PLAYER GOT HIT
    [SerializeField] private SpriteRenderer[] playerSprites; //ARRAY OF PLAYER SPRITE SO THAT THEY CAN BE TURNED ON AND OFF AFTER GETTING DAMAGED
    private float invincibilityCounter; //COUNTER THAT MONITORS FOR HOW LONG THE PLAYER WILL HAVE I-FRAMES

    private float flashCounter; //COUNTER THAT MONITORS TIME BETWEN SPRITE'S FLASHING WHILE I-FRAMES

    [Header("Abilities")] //VARIABLES RESPONSIBLE FOR KEEPING TRACK OF PLAYER ABILITIES
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private bool canDash;
    [SerializeField] private bool canWallJump;

    //GETTERS AND SETTERS
    public bool CanDoubleJump {  get { return canDoubleJump; } set { canDoubleJump = value; } }
    public bool CanDash { get { return canDash; } set { canDash = value; } }
    public bool CanWallJump { get {return canWallJump; } set { canWallJump = value; } }

    [Header("Respawning")]
    [SerializeField] private float waitToRespawn; //TIME THE PLAYER MUST WAIT FOR IN ORDER TO RESPAWN
    [SerializeField] private GameObject deathEffect; //EFFECT THAT IS BEING CREATED UPON THE PLAYER DYING
    private Vector3 respawnPoint; //POINT AT WHICH THE PLAYER WILL RESPAWN AFTER DYING OR LOADING INTO ANOTHER SCENE
    #endregion

    private void Start()
    {
        currentHealth = maxHealth; //ALWAYS START THE GAME WITH PLAYER'S HEALTH AT MAX

        player = Player.instance; //GETTING THE PLAYER REFERENCE
        //player.SetActive(false);
        respawnPoint = player.gameObject.transform.position; //SETTING THE RESPAWN POINT TO THE CURRENT POSITION OF THE PLAYER ON START
        //ASSIGNING THOSE VARIABLES IN THE AWAKE IS IMPOSSIBLE, DUE TO THE FACT THAT AWAKE FUNCTIONS OF TWO SCRIPTS CAN BE EXECUTED IN AN UNEXPECTED ORDER
        playerSprites = player.gameObject.GetComponentsInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        HandleIFrames(); //MAKE SURE TO TRACK IFRAMES STATUS EVERY FRAME
    }

    #region Health
    private void HandleIFrames()
    {
        if (invincibilityCounter > 0) //IF THE PLAYER CURRENTLY HAS I-FRAMES
        {
            invincibilityCounter -= Time.deltaTime; //COUNT DOWN THE TIMER OF BOTH I-FRAMES AND FLASHING

            flashCounter -= Time.deltaTime;

            if (flashCounter <= 0) //TURN EACH SPRITE ON AND OFF AFTER THE TIMER HAS REACHED ZERO AND THEN RESET THE TIMER
            {
                foreach (SpriteRenderer sr in playerSprites)
                {
                    sr.enabled = !sr.enabled;
                }
                flashCounter = flashLength;
            }

            if (invincibilityCounter <= 0)
            {
                foreach (SpriteRenderer sr in playerSprites) //MAKE SURE THAT AFTER THE I-FRAMES RAN OUT, THE PLAYER SPRITE IS TURNED ON AND RESET THE FLASH TIMER
                {
                    sr.enabled = true;
                }
                flashCounter = 0;
            }
        }
    }

    public void DamagePlayer(int damageAmount) //SUBSTRACTS DAMAGE AMOUNT FROM THE PLAYER'S CURRENT HEALTH
    {
        if (invincibilityCounter <= 0) //ONLY IF THE PLAYER ISN'T CURRENTLY INVINCIBLE
        {
            currentHealth -= damageAmount; //SUBSTRACT THE DAMAGE

            if (currentHealth <= 0) //IF THE PLAYER DIED MAKE SURE HIS HEALTH IS SET TO ZERO AND CONTACT RESPAWN CONTROLLER IN ORDER TO INITIATE RESPAWNING
            {
                currentHealth = 0;

                Respawn();
            }
            else //IF THE PLAYER SURVIVED, GIVE HIM I-FRAMES
            {
                invincibilityCounter = invincibilityLength;
            }

            //update health
        }
    }

    public void RefillHealth() //REFILLS PLAYER;S HEALTH, CURRENTLY USED AFTER RESPAWNING
    {
        currentHealth = maxHealth;

        //update health
    }

    public void HealPlayer(int healAmount) //HEALS THE PLAYER BY ADDING HEAL AMOUNT TO HIS CURRENT HEALTH
    {
        currentHealth += healAmount;

        if (currentHealth > maxHealth) //MAKE SURE THAT THE CURRENT HEALTH DOESN'T EXCEED THE MAX HEALTH
        {
            currentHealth = maxHealth;
        }

        //update health
    }
    #endregion

    #region Respawning
    public void Respawn() => StartCoroutine(RespawnRoutine());

    IEnumerator RespawnRoutine() //ROUTINE RESPONSIBLE FOR RESPAWNING THE PLAYER IN THE CORRECT POSITION
    {
        player.gameObject.SetActive(false); //DISABLE THE PLAYER GAME OBJECT UPON DYING
        if (deathEffect) //CREATE A DEATH EFFECT AT THE PLAYER'S POSITION IF IT EXISTS
            Instantiate(deathEffect, player.gameObject.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(waitToRespawn); //WAIT FOR TIME TO RESPAWN

        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //RELOAD CURRENT SCENE THROUGH SCENE MANAGER (UNITY BUILT IN FEATURE)

        player.transform.position = respawnPoint; //SET PLAYER'S POSITION TO THE LAST RESPAWN POINT
        player.gameObject.SetActive(true); //ENABLE THE PLAYER GAME OBJECT AFTER RESPAWNING

        RefillHealth(); //MAKE HIS HEALTH FULL BACK AGAIN
    }

    public void SetSpawn(Vector3 newPosition) //UPDATES THE RESPAWN POINT WITH NEW POSITION
    {
        respawnPoint = newPosition;
    }
    #endregion
}
