using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance;

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

    public int currentHealth;
    public int maxHealth;
    public float invincibilityLength;
    private float _invincibilityCounter;
    
    public float flashLength;
    private float _flashCounter;

    public SpriteRenderer[] playerSprites;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;

        //update health
    }

    // Update is called once per frame
    void Update()
    {
        if(_invincibilityCounter > 0)
        {
            _invincibilityCounter -= Time.deltaTime;

            _flashCounter -= Time.deltaTime;
            if(_flashCounter <=0)
            {
                foreach(SpriteRenderer sr in playerSprites)
                {
                    sr.enabled = !sr.enabled;
                }
                _flashCounter = flashLength;
            }

            if(_invincibilityCounter <= 0)
            {
                foreach(SpriteRenderer sr in playerSprites)
                {
                    sr.enabled = true;
                }
                _flashCounter = 0;
            }
        }
    }

    public void DamagePlayer(int damageAmount)
    {
        if(_invincibilityCounter <= 0)
        {
            currentHealth -= damageAmount;

            if(currentHealth <= 0)
            {
                currentHealth = 0;

                RespawnController.instance.Respawn();
            }
            else
            {
                _invincibilityCounter = invincibilityLength;
            }

            //update health
        }
    }

    public void RefillHealth()
    {
        currentHealth = maxHealth;

        //update health
    }

    public void HealPlayer(int healAmount)
    {
        currentHealth += healAmount;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        //update health
    }
}
