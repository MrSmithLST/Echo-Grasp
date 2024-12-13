using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    [SerializeField] private string playerLayerName = "Player";
    [SerializeField] private string groundLayerName = "Ground";
    private Vector2 velocity;

    private void Update() 
    {
        rb.velocity = velocity;
    }

    public void FlipSprite() => sr.flipX = sr.flipX;

    public void SetVelocity(Vector2 velocity) 
    {
        rb.velocity = velocity;
        this.velocity = velocity;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(playerLayerName))
        {
            other.GetComponent<Player>().Knockback();
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer(groundLayerName))
        {
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
