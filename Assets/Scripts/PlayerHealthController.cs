using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance; //CREATING AN INSTANCE OF THIS SCRIPT SO THAT IT CAN BE EASILY ACCESSED FROM ANYWHERE

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

    [Header("Health")]
    [SerializeField] private int _currentHealth; 
    [SerializeField] private int _maxHealth; //MAXIMUM HEALTH THAT THE PLAYER CAN HAVE 


    [Header("Invincibility")]
    [SerializeField] private float _invincibilityLength; //DURATION OF I-FRAMES AFTER BEING DAMAGED
    [SerializeField] private float _flashLength; //DURATION OF SINGULAR FLASH OF PLAYER'S SPRITE INDICATING THAT THE PLAYER GOT HIT
    [SerializeField] private SpriteRenderer[] _playerSprites; //ARRAY OF PLAYER SPRITE SO THAT THEY CAN BE TURNED ON AND OFF AFTER GETTING DAMAGED
    private float _invincibilityCounter; //COUNTER THAT MONITORS FOR HOW LONG THE PLAYER WILL HAVE I-FRAMES
    
    private float _flashCounter; //COUNTER THAT MONITORS TIME BETWEN SPRITE'S FLASHING WHILE I-FRAMES


    // Start is called before the first frame update
    void Start()
    {
        _currentHealth = _maxHealth; 

        //update health
    }

    // Update is called once per frame
    void Update()
    {
        if(_invincibilityCounter > 0) //IF THE PLAYER CURRENTLY HAS I-FRAMES
        {
            _invincibilityCounter -= Time.deltaTime; //COUNT DOWN THE TIMER OF BOTH I-FRAMES AND FLASHING

            _flashCounter -= Time.deltaTime;

            if(_flashCounter <= 0) //TURN EACH SPRITE ON AND OFF AFTER THE TIMER HAS REACHED ZERO AND THEN RESET THE TIMER
            {
                foreach(SpriteRenderer sr in _playerSprites)
                {
                    sr.enabled = !sr.enabled;
                }
                _flashCounter = _flashLength;
            }

            if(_invincibilityCounter <= 0)
            {
                foreach(SpriteRenderer sr in _playerSprites) //MAKE SURE THAT AFTER THE I-FRAMES RAN OUT, THE PLAYER SPRITE IS TURNED ON AND RESET THE FLASH TIMER
                {
                    sr.enabled = true;
                }
                _flashCounter = 0;
            }
        }
    }

    public void DamagePlayer(int damageAmount) //SUBSTRACTS DAMAGE AMOUNT FROM THE PLAYER'S CURRENT HEALTH
    {
        if(_invincibilityCounter <= 0) //ONLY IF THE PLAYER ISN'T CURRENTLY INVINCIBLE
        {
            _currentHealth -= damageAmount; //SUBSTRACT THE DAMAGE

            if(_currentHealth <= 0) //IF THE PLAYER DIED MAKE SURE HIS HEALTH IS SET TO ZERO AND CONTACT RESPAWN CONTROLLER IN ORDER TO INITIATE RESPAWNING
            {
                _currentHealth = 0;

                RespawnController.instance.Respawn();
            }
            else //IF THE PLAYER SURVIVED, GIVE HIM I-FRAMES
            {
                _invincibilityCounter = _invincibilityLength;
            }

            //update health
        }
    }

    public void RefillHealth() //REFILLS PLAYER;S HEALTH, CURRENTLY USED AFTER RESPAWNING
    {
        _currentHealth = _maxHealth;

        //update health
    }

    public void HealPlayer(int healAmount) //HEALS THE PLAYER BY ADDING HEAL AMOUNT TO HIS CURRENT HEALTH
    {
        _currentHealth += healAmount;

        if(_currentHealth > _maxHealth) //MAKE SURE THAT THE CURRENT HEALTH DOESN'T EXCEED THE MAX HEALTH
        {
            _currentHealth = _maxHealth;
        }

        //update health
    }
}
