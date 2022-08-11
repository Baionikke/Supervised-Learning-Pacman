using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PacmanAgent : Agent
{
    
    private Pacman pacman { get; set; }

    public override void OnEpisodeBegin() // TODO: to understand how the method works
    {
	    	FindObjectOfType<GameManager>().Start();
            SetReward(0f);
    }
    
    public override void CollectObservations(VectorSensor sensor) // -> 265
    {
        // Position
        sensor.AddObservation(FindObjectOfType<Pacman>().transform.localPosition);
        
        // Pellets
        foreach (Transform pellet in FindObjectOfType<GameManager>().pellets)
        {
            //sensor.AddObservation(pellet.localPosition); // Questa forse non serve
            sensor.AddObservation(pellet.gameObject.activeSelf);
        }

        // Ghost
        for (int i = 0; i < FindObjectOfType<GameManager>().ghosts.Length; i++)
        {
            if (FindObjectOfType<GameManager>().ghosts[i].gameObject.activeSelf)
            {
                sensor.AddObservation(FindObjectOfType<GameManager>().ghosts[i].transform.localPosition);
            }
        }
        
        // Ghosts are Frightened
        sensor.AddObservation(FindObjectOfType<Ghost>().frightened.enabled);
        
        // Lives
        sensor.AddObservation(FindObjectOfType<GameManager>().lives);

        // Score
        sensor.AddObservation(FindObjectOfType<GameManager>().score);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        pacman = GetComponent<Pacman>();
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

    private void OnCollisionEnter2D(Collision2D col)
    {
        // Collision with pellet
        if (col.gameObject.layer == LayerMask.NameToLayer("Pellet"))
        {
            AddReward(0.3f);
            if (!FindObjectOfType<GameManager>().HasRemainingPellets())
            {
                gameObject.SetActive(false);
                AddReward(1f);
                EndEpisode();
            }
        }
        
        // Collision with wall
        /* if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            AddReward(-0.05f);
        }*/
        
        // Collision with ghost
        if (col.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            if (FindObjectOfType<Ghost>().frightened.enabled)
            {
                AddReward(0.25f);
            }
            else
            {
                if (FindObjectOfType<GameManager>().lives > 0)
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
