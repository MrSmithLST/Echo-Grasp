using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Awake() //GET ALL NECESSARY COMPONENTS IN THE AWAKE
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    [SerializeField] private string playerLayerName = "Player"; //LAYER NAMES
    [SerializeField] private string groundLayerName = "Ground";
    private Vector2 velocity; //VELOCITY OF THE BULLET

    private void Update() 
    {
        rb.velocity = velocity; //CONSTANTLY MAKE SURE THAT THE VELOCITY IS WHAT IT'S MENT TO BE
    }

    public void FlipSprite() => sr.flipX = sr.flipX; //FLIP THE SPRITE ACORDING TO THE DIRECTION OF MOVEMENT

    public void SetVelocity(Vector2 velocity) 
    {
        rb.velocity = velocity; //SET THE VELOCITY OF THE BULLET
        this.velocity = velocity; //AND KEEP IT STORED TO USE IN UPDATE
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //CHECK FOR WHAT CAME IN CONTACT WITH THE BULLET
        if (other.gameObject.layer == LayerMask.NameToLayer(playerLayerName)) //IF IT WAS THE PLAYER
        {
            other.GetComponent<Player>().Knockback(); //KNOCKBACK THE PLAYER
            Destroy(gameObject); //DESTROY IN EACH CASE
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer(groundLayerName)) //CHECK FOR COLLISION WITH THE WALL TO MAKE SURE BULLET DOESN'T COLLIDE WITH PICKUPS AND OTHER ENEMIES
        {
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible() //DESTROY THE BULLET IF IT GOES OUT OF THE PLAYER VIEW
    {
        Destroy(gameObject);
    }
}
