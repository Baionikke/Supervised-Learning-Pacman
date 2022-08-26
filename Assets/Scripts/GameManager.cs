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

    public static GameManager instance { get; private set; }
    private int pelletCount;

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
        FindObjectOfType<PacmanAgent>().AddReward(-5f);
        FindObjectOfType<PacmanAgent>().EndEpisode(); // PER RUN SU SINGOLA VITA

        pacman.DeathSequence();
        //SetLives(lives - 1); // PER RUN SU 3 VITE

        if (lives > 0) {
            Invoke(nameof(ResetState), 0f);
        } else {
            FindObjectOfType<PacmanAgent>().AddReward(-5f);
            //FindObjectOfType<PacmanAgent>().EndEpisode(); // PER RUN SU 3 VITE
            //GameOver();
        }
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * ghostMultiplier;
        SetScore(score + points);
        
        FindObjectOfType<PacmanAgent>().AddReward(0.5f);

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
            FindObjectOfType<PacmanAgent>().AddReward(300f);
            Debug.Log("HA VINTO!");
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
        FindObjectOfType<PacmanAgent>().AddReward(1f);
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
