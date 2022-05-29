using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public Vector3 positionAsteroid;
    public GameObject asteroid;
    public GameObject asteroid2;
    public GameObject asteroid3;
    public int hazardCount;
    public float startWait;
    public float spawnWait;
    public float waitForWaves;
    public Text scoreText;
    public Text gameOverText;
    public Text restartText;
    public Text mainMenuText;
    public Text coinsText;

    private int coinsCollected;

    private bool gameJustEnded;
    private bool restart;
    private bool gameOver;
    private int score;
    private List<GameObject> asteroids;

    [SerializeField]
    GameObject playFabUtils;

    [SerializeField]
    GameObject skillBoxManager;

    private void Start() {
        asteroids = new List<GameObject> {
            asteroid,
            asteroid2,
            asteroid3
        };
        gameOverText.text = "";
        restartText.text = "";
        mainMenuText.text = "";
        coinsText.text = "0 Coins";
        scoreText.text = "Score 0";
        restart = false;
        gameOver = false;
        gameJustEnded = true;
        score = 0;
        coinsCollected = 0;
        StartCoroutine(spawnWaves());
        updateScore();
    }

    private void Update() {
        if(restart){
            if(Input.GetKey(KeyCode.R)){
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            } 
            else if(Input.GetKey(KeyCode.Q)){
                SceneManager.LoadScene("Menu");
            }
        }
        if (gameOver) {
            restartText.text = "Press R to restart game";
            mainMenuText.text = "Press Q to go back to main menu";
            if(gameJustEnded)
            {
                playFabUtils.GetComponent<PlayFabUserUtils>().updateHighscore(score);
                playFabUtils.GetComponent<PlayFabUserUtils>().addXP((int)(score * 0.5f));
                playFabUtils.GetComponent<PlayFabUserUtils>().addCoins(coinsCollected);
                gameJustEnded = false;
            }

            restart = true;
        }
    }

    private IEnumerator spawnWaves(){
        yield return new WaitForSeconds(startWait);
        while(true){
            for (int i = 0; i < hazardCount;i++){
                Vector3 position = new Vector3(Random.Range(-positionAsteroid.x, positionAsteroid.x), positionAsteroid.y, positionAsteroid.z);
                Quaternion rotation = Quaternion.identity;
                Instantiate(asteroids[Random.Range(0,3)], position, rotation);
                yield return new WaitForSeconds(spawnWait);
            }
            yield return new WaitForSeconds(waitForWaves);
            if(gameOver){
                break;
            }
        }
    }

    public int getSkillLevel(string skillName)
    {
        return skillBoxManager.GetComponent<SkillBoxManager>().getLatestJSONFetchSkillLevel(skillName);
    }

    public void gameIsOver(){
        gameOverText.text = "Game Over";
        gameOver = true;

    }

    public void addScore(int score){
        this.score += score;
        updateScore();
    }

    public void addCoins(int coins)
    {
        this.coinsCollected += coins;
        coinsText.text = this.coinsCollected + " Coins";
    }

    void updateScore(){
        scoreText.text = "Score:" + score;
    }

}
