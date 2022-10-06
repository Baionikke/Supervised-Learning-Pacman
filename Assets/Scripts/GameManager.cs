using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pellets;

    public Text gameOverText;
    public Text scoreText;
    public Text livesText;
    public bool oneLife;
    
    public static GameManager instance { get; private set; }
    private int pelletCount;
    private int tot_games = 0;
    private int win_games = 0;

    private void Awake()
    {
        instance = this;
    }

    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }
    public int lives { get; private set; }

    public void Start()
    {
        NewGame();
    }

    private void Update()
    {
        /*if (lives <= 0 && Input.anyKeyDown) {
            NewGame();
        }*/
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(3);
        pelletCount = 0;
        NewRound();
    }

    private void NewRound()
    {
        gameOverText.enabled = false;

        foreach (Transform pellet in pellets) {
            pellet.gameObject.SetActive(true);
        }
        
        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].ResetState();
        }

        pacman.ResetState();
    }

    public void GameOver()
    {
        gameOverText.enabled = true;
        
        for (int i = 0; i < ghosts.Length; i++) { ghosts[i].gameObject.SetActive(false); }

        pacman.gameObject.SetActive(false);

    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(2, '0');
    }

    public void PacmanEaten()
    {
        FindObjectOfType<PacmanAgent>().AddReward(-50f);
        
        // Training type based on life: change next line
        // --->
        bool oneLifeOnly = oneLife; // True for one life, false for 3 (standard game)
        // <---
        
        if (oneLifeOnly)
        {
            tot_games += 1;
            FindObjectOfType<PacmanAgent>().EndEpisode();
        }

        pacman.DeathSequence();
        if (!oneLifeOnly) SetLives(lives - 1);

        if (lives > 0) {
            Invoke(nameof(ResetState), 0f);
        } else {
            FindObjectOfType<PacmanAgent>().AddReward(-5f);
            if (!oneLifeOnly)
            {
                tot_games += 1;
                FindObjectOfType<PacmanAgent>().EndEpisode();
            }
            //GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);
        
        FindObjectOfType<PacmanAgent>().AddReward(6f);

        ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        pelletCount++;
        double increment = 0.006 * pelletCount;
        FindObjectOfType<PacmanAgent>().AddReward((float)increment);
        SetScore(score + pellet.points);

        if (!HasRemainingPellets())
        {
            win_games += 1;
            tot_games += 1;
            FindObjectOfType<PacmanAgent>().AddReward(300f);
            //Debug.Log("HA VINTO!");
            Debug.Log("Win/Tot Games: " + win_games + " / " + tot_games);
            FindObjectOfType<PacmanAgent>().EndEpisode();
            //pacman.gameObject.SetActive(false);
            //Invoke(nameof(NewRound), 3f);
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++) {
            ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        
        FindObjectOfType<PacmanAgent>().PowerPelletEaten();
        
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    public bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf) {
                return true;
            }
        }

        return false;
    }

    private void ResetGhostMultiplier()
    {
        ghostMultiplier = 1;
    }

}
