using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : MonoBehaviour {

    Rigidbody rbBolt;
    public float speed;

    private void Start() {

        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            int bulletSpeedLvl = gameControllerObject.GetComponent<GameController>().getSkillLevel("Bullet Speed");
            speed += speed * 0.05f * bulletSpeedLvl;
            print("Bullet speed level " + bulletSpeedLvl);
        }
        else
        {
            Debug.Log("GameController object not found");
        }



        rbBolt = GetComponent<Rigidbody>();
        rbBolt.velocity = transform.forward * speed;
    }
}
