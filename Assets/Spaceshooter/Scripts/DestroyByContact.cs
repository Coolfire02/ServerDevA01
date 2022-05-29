using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByContact : MonoBehaviour {

    public GameObject explosion;
    public GameObject playerExplosion;
    public GameObject coin;
    public int scoreValue;
    GameController gameController;

    private void Start() {
        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if(gameControllerObject != null){
            gameController = gameControllerObject.GetComponent<GameController>();
        } 
        else{
            Debug.Log("GameController object not found");
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(gameObject.tag != "Collectable" && other.gameObject.tag != "Collectable")
        {
            if (other.tag != "Boundary")
            {
                Instantiate(explosion, transform.position, transform.rotation);
                if (other.tag == "Player")
                {
                    Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
                    gameController.gameIsOver();
                }
                Instantiate(coin, transform.position, transform.rotation);
                print("Instantiate coin");
                gameController.addScore(scoreValue);
                Destroy(other.gameObject);
                Destroy(gameObject);
            }
        }else
        {
            print("Collision with coin with GO Tag" + other.gameObject.tag);
            if(other.gameObject.tag == "Player")
            {
                gameObject.GetComponent<AudioSource>().Play();
                Destroy(gameObject);
                if(gameController.getSkillLevel("Double Coins") > 0)
                {
                    System.Random ran = new System.Random();
                    if(ran.Next(0, 10 - gameController.getSkillLevel("Double Coins")) == 0)
                    {
                        gameController.addCoins(1);
                        print("Bonus coin");
                    }
                }
                gameController.addCoins(1);
            }
        }
    }
}
