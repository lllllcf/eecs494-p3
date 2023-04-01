using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HitHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] public int health;
    // [SerializeField] private string enemyTag;
    [SerializeField] private SpriteRenderer healthBar;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    // [SerializeField] private float time_eat_hyphae = 1f;
    [SerializeField] private float hit_cd_time = 0.5f;
    [SerializeField] private float health_recover_rate = 0.1f; //10 s one health
    [SerializeField] private List<String> enemyTagList;


    private bool canGetHit;
    private float deltaHP = 0;
    private float original_bar_length;

    bool hitlock;

    private GameObject currentOpponent;
    

    private void Start()
    {
        hitlock = false;
        original_bar_length = healthBar.size.x;
        canGetHit = true;
        healthBar.size =
            new Vector2((float)health / (float)maxHealth * original_bar_length, healthBar.size.y);
    }

    private void OnTriggerStay(Collider other)
    {
        // bool ishit = false;
        // foreach (var _enemyTag in enemyTagList)
        // {
        //     if (other.gameObject.CompareTag(_enemyTag))
        //     {
        //         ishit = true;
        //         break;
        //     }
        // }
        //
        // if (!ishit)
        // {
        //     return;;
        // }

        if (other.gameObject.GetComponent<HitHealth>() == null || other.gameObject.GetComponent<HitHealth>().currentOpponent != gameObject)
        {
            return;
        }
        if (health > 0)
        {
            canGetHit = false;
            StartCoroutine(HitEffect());
        }
        if (health == 0)
        {
            EventBus.Publish(new BuilderTutorialSnailDeadEvent());
            if (gameObject.tag == "Building")
            {
                Destroy(gameObject);
            }  
            else
            {
                Transform parent = transform.parent;
                if (parent.GetComponent<SpriteRenderer>() != null)
                {
                    parent.GetComponent<SpriteRenderer>().color = Color.red;
                }
                if (parent.GetComponent<GameEndTrigger>() != null)
                {
                    parent.GetComponent<GameEndTrigger>().TriggerDeath();
                }
                else
                {
                    Destroy(parent.gameObject);
                }
            }
        }
    }

    private void Update()
    {
        RecoverHealth();
    }

    void RecoverHealth() {
        if (deltaHP > 1) {
            if (health + 1 <= maxHealth) {
                health += 1;
            }
            deltaHP = 0;
            healthBar.size =
                    new Vector2((float)health / (float)maxHealth * original_bar_length, healthBar.size.y);
        }
        else {
            deltaHP += health_recover_rate * Time.deltaTime;
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{

    //    if (collision.gameObject.CompareTag(enemyTag))
    //    {
    //        Debug.Log("hit" + enemyTag);
    //    }
    //}

    // private void OnTriggerStay(Collider other)
    // {
    //     if (other.gameObject.CompareTag("Hyphae"))
    //     {
    //         if (Time.time - collisionTime > time_eat_hyphae)
    //         {
    //             other.gameObject.SetActive(false);
    //         }
    //     }
    // }
    // private void OnCollisionStay(Collision collision)
    // {
    //     if(collision.gameObject.CompareTag("Hyphae")) {
    //         if (Time.time - collisionTime > time_eat_hyphae) {
    //             collision.gameObject.SetActive(false);
    //         }
    //     }
    // }

    public void GetDamage() {
        // if (health > 0)
        // {
        //     canGetHit = false;
        //     StartCoroutine(HitEffect());
        //     health -= 1;
        //     healthBar.size =
        //         new Vector2((float)health / (float)maxHealth * original_bar_length, healthBar.size.y);
        // }
        // if (health == 0)
        // {
        //     Transform parent = transform.parent;
        //     parent.GetComponent<SpriteRenderer>().color = Color.red;
        //     if (parent.GetComponent<GameEndTrigger>() != null)
        //     {
        //         parent.GetComponent<GameEndTrigger>().TriggerDeath();
        //     }
        //     
        // }
        
        if (health > 0)
        {
            canGetHit = false;
            StartCoroutine(HitEffect());
        }
        if (health == 0)
        {
            EventBus.Publish(new BuilderTutorialSnailDeadEvent());
            if (gameObject.tag == "Building")
            {
                Destroy(gameObject);
            }  
            else
            {
                Transform parent = transform.parent;
                if (parent.GetComponent<SpriteRenderer>() != null)
                {
                    parent.GetComponent<SpriteRenderer>().color = Color.red;
                }
                if (parent.GetComponent<GameEndTrigger>() != null)
                {
                    parent.GetComponent<GameEndTrigger>().TriggerDeath();
                }
                else
                {
                    Destroy(parent.gameObject);
                }
            }
        }

    }
    private IEnumerator HitEffect()
    {
        if (!hitlock) {
            hitlock = true;
            _spriteRenderer.color = new Color32(0xFF, 0x00, 0x00, 0xFF);
            health -= 1;
            healthBar.size =
                new Vector2((float)health / (float)maxHealth * original_bar_length, healthBar.size.y);
            // if (gameObject.CompareTag("BaseCar"))
            // {
            //     GetComponent<BoxCollider>().enabled = false;
            // }
            yield return new WaitForSeconds(1f);
            // if (gameObject.CompareTag("BaseCar"))
            // {
            //     GetComponent<BoxCollider>().enabled = true;
            // }
            canGetHit = true;
            _spriteRenderer.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
            hitlock = false;
        }
        else {
            yield return null;
        }
    }

    public void SetHealthRestoreRate(float rate)
    {
        health_recover_rate = rate;
    }

    public void SetCurrentOpponent(GameObject _opponent)
    {
        currentOpponent = _opponent;
    }
}