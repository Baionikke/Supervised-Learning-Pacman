using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class PacmanAgent : Agent
{
    
    private Pacman pacman { get; set; }
    private Vector2 movementMemory;
    private Vector2 positionMemory;

    private float PelletDistance(Transform pellet)
    {
        Vector2 pacmanPos = new Vector2(GameManager.instance.pacman.transform.localPosition.x,
            GameManager.instance.pacman.transform.localPosition.y);
        Vector2 pelletPos = new Vector2(pellet.localPosition.x, pellet.localPosition.y);
        float distance = Vector2.Distance(pacmanPos, pelletPos);
        return distance;
    }

    private float GhostDistance(Vector3 ghostPosition)
    {
        //float distanceX = (GameManager.instance.pacman.transform.localPosition.x - ghostPosition.x);
        //float distanceY = (GameManager.instance.pacman.transform.localPosition.y - ghostPosition.y); // Volutamente non in val. assoluto
        //Vector2 res = new Vector2(distanceX, distanceY);
        float res = Vector2.Distance(ghostPosition, GameManager.instance.pacman.transform.localPosition);
        return res;
    }

    public override void OnEpisodeBegin() // TODO: to understand how the method works
    {
	    	GameManager.instance.Start();
    }
    
    public override void CollectObservations(VectorSensor sensor) // -> 501
    {
        // Position
        sensor.AddObservation((Vector2)GameManager.instance.pacman.transform.localPosition);
        sensor.AddObservation(GameManager.instance.pacman.movement.direction);
        
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
            sensor.AddObservation(GameManager.instance.ghosts[i].movement.direction);
        }
        
        // Ghosts are Frightened
        sensor.AddObservation(GameManager.instance.ghosts[0].frightened.enabled);
        
        // Lives
        sensor.AddObservation(GameManager.instance.lives); // Forse eliminabile

        // Score
        // sensor.AddObservation(GameManager.instance.score); // Forse eliminabile
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        pacman = GameManager.instance.pacman;
        //Debug.Log(actionBuffers.DiscreteActions[0]);
        int movementControl = actionBuffers.DiscreteActions[0];
        movementMemory = GameManager.instance.pacman.movement.direction;
        if ((Vector2)GameManager.instance.pacman.transform.localPosition == positionMemory)
        {
            // Debug.Log("Stuck!");
            AddReward(-0.1f); // Se fermo nello stesso punto
        }
        bool occ;
        // Set the new direction based on the current input
        switch (movementControl)
        {
            case 0:
                occ = pacman.movement.Occupied(pacman.movement.direction);
                if (!occ) AddReward(0.008f); // Se va dritto e non è occupato
                break;
            case 1:
                occ = pacman.movement.Occupied(Vector2.up);
                if (!occ && movementMemory!=Vector2.up && movementMemory!=Vector2.down) {
                    //Debug.Log("Buona svolta");
                    AddReward(0.012f); // Se gira correttamente ad un incrocio
                }
                if (!occ && movementMemory == Vector2.down)
                {
                    //Debug.Log("Inversione!");
                    AddReward(-0.016f);
                }
                //if (occ) AddReward(-0.01f); // Se gira su un muro
                pacman.movement.SetDirection(Vector2.up);
                break;
            case 2:
                occ = pacman.movement.Occupied(Vector2.down);
                if (!occ && movementMemory!=Vector2.down && movementMemory!=Vector2.up) {
                    //Debug.Log("Buona svolta");
                    AddReward(0.012f); // Se gira correttamente ad un incrocio
                }
                if (!occ && movementMemory == Vector2.up)
                {
                    //Debug.Log("Inversione!");
                    AddReward(-0.016f);
                }
                //if (occ) AddReward(-0.01f); // Se gira su un muro
                pacman.movement.SetDirection(Vector2.down);
                break;
            case 3:
                occ = pacman.movement.Occupied(Vector2.left);
                if (!occ && movementMemory!=Vector2.left && movementMemory!=Vector2.right) {
                    //Debug.Log("Buona svolta");
                    AddReward(0.012f); // Se gira correttamente ad un incrocio
                }
                if (!occ && movementMemory == Vector2.right)
                {
                    //Debug.Log("Inversione!");
                    AddReward(-0.016f);
                }
                //if (occ) AddReward(-0.01f); // Se gira su un muro
                pacman.movement.SetDirection(Vector2.left);
                break;
            case 4:
                occ = pacman.movement.Occupied(Vector2.right);
                if (!occ && movementMemory!=Vector2.right && movementMemory!=Vector2.left)
                {
                    //Debug.Log("Buona svolta");
                    AddReward(0.012f); // Se gira correttamente ad un incrocio
                }
                if (!occ && movementMemory == Vector2.left)
                {
                    //Debug.Log("Inversione!");
                    AddReward(-0.016f);
                }
                //if (occ) AddReward(-0.01f); // Se gira su un muro
                pacman.movement.SetDirection(Vector2.right);
                break;
        }

        positionMemory = (Vector2)GameManager.instance.pacman.transform.localPosition;
        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(pacman.movement.direction.y, pacman.movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    /*private void OnCollisionExit2D(Collision2D other) // OnCollisionStay2D non funziona perché pacman tocca sempre i muri laterali
    {  // Anche questo non funziona sempre perché non becca bene l'uscita dai bivi, ma ne trova anche quando non ha più muri ai lati
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            AddReward(0.1f);
            Debug.Log("Exited Collision!");
        }
    }*/

    private void OnCollisionEnter2D(Collision2D other)
    {
        /* TODO:
        1. Penalizzare se rimane troppo nella stessa zona. -> vettore di posizioni trascorse #FATTO
        2. Vicinanza ai fantasmi attiva la routine "scappa!" -> Va sulle euristiche
        3. Distanza dai fantasmini -> forse sostituire a posizione #FATTO
        4. Percezione di densità
        5. Provare con un raycast a capire se dietro "l'angolo" c'è un pellet
        6. Incrementare rendita dei pellet #FATTO
        */
        
        // Collision with pellet // --> SPOSTATO IN PELLET.CS <--
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
        /*if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle")) // CORREGGERE non so come, non prende le collisioni giuste
        {
            AddReward(-0.08f);
            Debug.Log("Wall hit!");
            RequestDecision();
        }*/
        
        // Collision with ghost
        /*if (other.gameObject.layer == LayerMask.NameToLayer("Ghost")) // SPOSTATO IN GAME MANAGER
        {
            if (!GameManager.instance.ghosts[0].frightened.enabled)
            {
                if (GameManager.instance.lives > 0)
                {
                    AddReward(-0.25f); // Se mangiato dal fantasma
                    EndEpisode();
                }
                else
                {
                    AddReward(-0.5f); // Se mangiato dal fantasma e game over (ha senso solo se non va in EndEpisode quando è soltanto mangiato dal fantasma
                    EndEpisode();
                }
            }
            else
            {
                AddReward(0.1f); // Se mangia un fantasmino
            }*/
        }
    }

