using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Boundary{
    public float xMin, xMax, zMin, zMax;
}

public class PlayerControl : MonoBehaviour {

    private Rigidbody playerRb;
    private AudioSource playerWeapon;
    public float speed;
    public float tiltMultiplier;
    public Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public Transform shotSpawn2;
    public float fireRate;

    GameController gameController;

    private float nextFire;
    private CharacterSelection characterSelection;

    bool playerSkillsLoaded = false;

    private void Start() {

        GameObject gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }
        else
        {
            Debug.Log("GameController object not found");
        }

        GameObject cSelectionObject = GameObject.FindWithTag("CharacterSelection");
        if (cSelectionObject != null) {
            characterSelection = cSelectionObject.GetComponent<CharacterSelection>();
        }
        
        playerRb = GetComponent<Rigidbody>();
        playerWeapon = GetComponent<AudioSource>();
    }

    private void Update() {

        
        int rapidBulletsLvl = gameController.getSkillLevel("Rapid Bullets");
        if(rapidBulletsLvl >= 0 && !playerSkillsLoaded)
        {
            playerSkillsLoaded = true;
            if (rapidBulletsLvl > 0)
            {
                fireRate -= fireRate * 0.007f * rapidBulletsLvl;
                if (fireRate < 0.05) fireRate = 0.05f;
            }

            int playerSpeed = gameController.getSkillLevel("Faster Movement");
            if (playerSpeed > 0)
            {
                speed += speed * 0.03f * playerSpeed;
            }
        }

        if (Input.GetButton("Jump") && Time.time > nextFire){
            nextFire = Time.time + fireRate;
            print(gameObject.name + "trying to shoot");
            if(characterSelection.getIndex() == 1){
                Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
                Instantiate(shot, shotSpawn2.position, shotSpawn2.rotation);
            }
            else{
                Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            }
            playerWeapon.Play();
        }

    }

    private void FixedUpdate() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        playerRb.velocity = new Vector3(moveHorizontal * speed, 0.0f, moveVertical * speed);

        playerRb.position = new Vector3(
            Mathf.Clamp(playerRb.position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(playerRb.position.z, boundary.zMin, boundary.zMax)
        );

        playerRb.rotation = Quaternion.Euler(0.0f, 0.0f, -playerRb.velocity.x * tiltMultiplier);
    }
}
