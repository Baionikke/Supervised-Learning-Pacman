using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PacmanAgent : Agent
{
    
    private Pacman pacman { get; set; }

    private float PelletDistance(Transform pellet)
    {
        Vector2 pacmanPos = new Vector2(GameManager.instance.pacman.transform.localPosition.x,
            GameManager.instance.pacman.transform.localPosition.y);
        Vector2 pelletPos = new Vector2(pellet.localPosition.x, pellet.localPosition.y);
        float distance = Vector2.Distance(pacmanPos, pelletPos);
        return distance;
    }

    private Vector2 GhostDistance(Vector3 ghostPosition)
    {
        float distanceX = (GameManager.instance.pacman.transform.localPosition.x - ghostPosition.x);
        float distanceY = (GameManager.instance.pacman.transform.localPosition.y - ghostPosition.y); // Volutamente non in val. assoluto
        Vector2 res = new Vector2(distanceX, distanceY);
        return res;
    }

    public override void OnEpisodeBegin() // TODO: to understand how the method works
    {
	    	GameManager.instance.Start();
            SetReward(0f);
    }
    
    public override void CollectObservations(VectorSensor sensor) // -> 501
    {
        // Position
        sensor.AddObservation((Vector2)GameManager.instance.pacman.transform.localPosition);
        
        // Pellets
        foreach (Transform pellet in GameManager.instance.pellets)
        {
            //sensor.AddObservation(pellet.localPosition); // Questa forse non serve
            sensor.AddObservation(pellet.gameObject.activeSelf);
            sensor.AddObservation(PelletDistance(pellet)); // TODO: Pensare al discorso dei 20 più vicini
        }

        // Ghost
        for (int i = 0; i < GameManager.instance.ghosts.Length; i++)
        {
            sensor.AddObservation(GhostDistance(GameManager.instance.ghosts[i].transform.localPosition));
        }
        
        // Ghosts are Frightened
        sensor.AddObservation(GameManager.instance.ghosts[0].frightened.enabled);
        
        // Lives
        sensor.AddObservation(GameManager.instance.lives); // Forse eliminabile

        // Score
        sensor.AddObservation(GameManager.instance.score); // Forse eliminabile
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        pacman = GameManager.instance.pacman;
        //Debug.Log(actionBuffers.DiscreteActions[0]);
        int movementControl = actionBuffers.DiscreteActions[0];
        // Set the new direction based on the current input
        if (movementControl == 0) {
            pacman.movement.SetDirection(Vector2.up);
        }
        else if (movementControl == 1) {
            pacman.movement.SetDirection(Vector2.down);
        }
        else if (movementControl == 2) {
            pacman.movement.SetDirection(Vector2.left);
        }
        else if (movementControl == 3) {
            pacman.movement.SetDirection(Vector2.right);
        }

        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(pacman.movement.direction.y, pacman.movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    private void OnCollisionExit2D(Collision2D other) // OnCollisionStay2D non funziona perché pacman tocca sempre i muri laterali
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            AddReward(0.05f);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        /* TODO:
        1. Penalizzare se rimane troppo nella stessa zona. -> vettore di posizioni trascorse
        2. Vicinanza ai fantasmi attiva la routine "scappa!" -> Va sulle euristiche
        3. Distanza dai fantasmini -> forse sostituire a posizione #FATTO
        4. 
        */
        
        // Collision with pellet // SPOSTATO IN PELLET
        /*if (other.gameObject.layer == LayerMask.NameToLayer("Pellet"))
        {
            AddReward(0.3f);
            Debug.Log("preso!");
            if (!FindObjectOfType<GameManager>().HasRemainingPellets())
            {
                gameObject.SetActive(false);
                AddReward(1f);
                EndEpisode();
            }
        }*/
        
        // Collision with wall
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            AddReward(-0.05f);
        }
        
        // Collision with ghost
        if (other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            if (GameManager.instance.ghosts[0].frightened.enabled)
            {
                AddReward(0.25f);
            }
            else
            {
                if (GameManager.instance.lives > 0)
                {
                    AddReward(-0.25f);
                }
                else
                {
                    AddReward(-1);
                    EndEpisode();
                }
            }
        }
    }
}
