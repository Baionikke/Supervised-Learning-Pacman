using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public LayerMask GhostLayer;
    //public LayerMask obstacleLayer;
    public LayerMask pelletLayer;

    private float PelletDistance(Transform pellet)
    {
        Vector2 pacmanPos = new Vector2(GameManager.instance.pacman.transform.localPosition.x / 13.5f,
            GameManager.instance.pacman.transform.localPosition.y / 13.5f);
        Vector2 pelletPos = new Vector2(pellet.localPosition.x / 13.5f, pellet.localPosition.y / 13.5f);
        float distance = Vector2.Distance(pacmanPos / 13.5f, pelletPos / 13.5f);
        return distance;
    }
    
    private void PelletInSight(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(GameManager.instance.pacman.movement.transform.position,
            Vector2.one * 0.3f, 0f, direction, 2f, pelletLayer);

        if (hit.collider) AddReward(0.5f);
    }

    private Vector2 TwentyDistancesFromPellet(SortedDictionary<float, Vector2> distancesFromPellet,
        Vector2 pacmanPosition, int cont)
    {
        float mean20PelletX = 0;
        float mean20PelletY = 0;
        if (cont > 20)
        {
            for (int j = 0; j < 20; j++)
            {
                //mean20Pellet += Vector2.Distance(pacmanPosition, distancesFromPellet.ElementAt(j).Value);
                mean20PelletX += distancesFromPellet.ElementAt(j).Value.x - pacmanPosition.x;
                mean20PelletY += distancesFromPellet.ElementAt(j).Value.y - pacmanPosition.y;
            }
        }
        else
        {
            for (int j = 0; j < cont; j++)
            {
                mean20PelletX += distancesFromPellet.ElementAt(j).Value.x - pacmanPosition.x;
                mean20PelletY += distancesFromPellet.ElementAt(j).Value.y - pacmanPosition.y;
            }
        }
        Vector2 mean20Pellet = new Vector2(mean20PelletX/cont, mean20PelletY/cont);
        return mean20Pellet/13.5f;
    }

    private Vector2 GhostDistanceXY(Vector3 ghostPosition)
    {
        float distanceX = (GameManager.instance.pacman.transform.localPosition.x / 13.5f - ghostPosition.x);
        float distanceY =
            (GameManager.instance.pacman.transform.localPosition.y / 13.5f -
             ghostPosition.y); // Volutamente non in val. assoluto
        Vector2 res = new Vector2(distanceX, distanceY);
        //float res = Vector2.Distance(ghostPosition, GameManager.instance.pacman.transform.localPosition);
        
        return res;
    }

    public float GhostDistance(Vector3 ghostPosition)
    {
        return Vector2.Distance(ghostPosition, GameManager.instance.pacman.transform.localPosition / 13.5f);
    }
    
    private bool GhostInSight(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(GameManager.instance.pacman.movement.transform.position,
            Vector2.one * 0.3f, 0f, direction, 3f, GhostLayer);

        if (hit.collider)
        {
            return true;
        }
        
        return false;
    }

    /*private bool HittingWall(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(GameManager.instance.pacman.movement.transform.position,
            Vector2.one * 0.3f, 0f, direction, 0.6f, obstacleLayer);
        return hit.collider;
    }

    private bool HittingLateralWall(Vector2 directionOfMovement)
    {
        Vector2 perpendicularClockwise = new Vector2(-directionOfMovement.y, directionOfMovement.x);
        Vector2 perpendicularCounterClockwise = new Vector2(directionOfMovement.y, -directionOfMovement.x);
        RaycastHit2D hit1 = Physics2D.BoxCast(GameManager.instance.pacman.movement.transform.position,
            Vector2.one * 0.2f, 0f, perpendicularClockwise, 0.6f, obstacleLayer);
        RaycastHit2D hit2 = Physics2D.BoxCast(GameManager.instance.pacman.movement.transform.position,
            Vector2.one * 0.2f, 0f, perpendicularCounterClockwise, 0.6f, obstacleLayer);
        return hit1.collider && hit2.collider;
    }*/

    public override void OnEpisodeBegin() // TODO: to understand how the method works
    {
        GameManager.instance.Start();
    }

    /*public void FixedUpdate() // Non funziona il controllo manuale poi
    {
        Vector2 direction = GameManager.instance.pacman.movement.direction;
        if (HittingWall(direction)) RequestDecision();
        if (!HittingLateralWall(direction)) RequestDecision();
        if (GhostInSight(direction)) RequestDecision();
    }*/

    public override void CollectObservations(VectorSensor sensor) // -> 
    {
        // Pacman - Position
        Vector2 pacmanPos = GameManager.instance.pacman.transform.localPosition / 13.5f;
        sensor.AddObservation(pacmanPos);
        sensor.AddObservation(GameManager.instance.pacman.movement.direction);

        // Pellets
        SortedDictionary<float, Vector2> distancesFromPellet = new SortedDictionary<float, Vector2>();

        foreach (Transform pellet in GameManager.instance.pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                Vector2 pelletsPosition = new Vector2(pellet.localPosition.x / 13.5f, pellet.localPosition.y / 13.5f);
                if (!distancesFromPellet.ContainsKey(PelletDistance(pellet)))
                {
                    distancesFromPellet.Add(PelletDistance(pellet), pelletsPosition);
                }
            }
        }

        if (distancesFromPellet.Count > 0)
        {
            sensor.AddObservation(TwentyDistancesFromPellet(distancesFromPellet, pacmanPos, distancesFromPellet.Count));
        }
        else
        {
            sensor.AddObservation(Vector2.zero);
        }

        // Ghost
        for (int i = 0; i < GameManager.instance.ghosts.Length; i++)
        {
            sensor.AddObservation(GhostDistanceXY((GameManager.instance.ghosts[i].transform.localPosition) / 13.5f));
            sensor.AddObservation(GhostDistance((GameManager.instance.ghosts[i].transform.localPosition) / 13.5f));
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
        bool occ; // Occupied per verificare che la direzione corrente sia occupata dal muro o meno

        if ((Vector2)GameManager.instance.pacman.transform.localPosition == positionMemory)
        {
            // Debug.Log("Stuck!");
            AddReward(-0.1f); // Se fermo nello stesso punto
        }

        // Set the new direction based on the current input
        switch (movementControl)
        {
            case 0:
                occ = pacman.movement.Occupied(pacman.movement.direction);
                if (!occ) AddReward(0.010f); // Se va dritto e non è occupato

                break;
            case 1:
                occ = pacman.movement.Occupied(Vector2.up);
                
                if (!occ && movementMemory != Vector2.up && movementMemory != Vector2.down)
                {
                    //Debug.Log("Buona svolta");
                    AddReward(0.012f); // Se gira correttamente ad un incrocio
                    PelletInSight(Vector2.up);
                }

                if (!occ && movementMemory == Vector2.down)
                {
                    //Debug.Log("Inversione!");
                    AddReward(-0.020f);
                }
                
                float ghostDistanceAccU = float.MaxValue;
                int accU = 0;
                
                for (int i = 0; i < GameManager.instance.ghosts.Length; i++)
                {
                    if (ghostDistanceAccU < GhostDistance(GameManager.instance.ghosts[i].transform.localPosition / 13.5f))
                    {
                        ghostDistanceAccU = GhostDistance(GameManager.instance.ghosts[i].transform.localPosition / 13.5f);
                        accU = i;
                    }
                }

                if (ghostDistanceAccU < 3f && !GameManager.instance.ghosts[accU].frightened)
                {
                    Debug.Log("sono dentro!");
                    if (!GhostInSight(Vector2.down))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.down);
                    }
                    else if (!GhostInSight(Vector2.right))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.right);
                    }
                    else if (!GhostInSight(Vector2.left))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.left);
                    }
                    else
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(GameManager.instance.ghosts[accU].movement.direction);
                    }
                }
                else
                {Debug.Log("direzione normale");
                    pacman.movement.SetDirection(Vector2.up);
                }
                
                break;
            case 2:
                occ = pacman.movement.Occupied(Vector2.down);
                
                if (!occ && movementMemory != Vector2.down && movementMemory != Vector2.up)
                {
                    //Debug.Log("Buona svolta");
                    AddReward(0.012f); // Se gira correttamente ad un incrocio
                    PelletInSight(Vector2.down);
                }

                if (!occ && movementMemory == Vector2.up)
                {
                    //Debug.Log("Inversione!");
                    AddReward(-0.020f);
                }

                float ghostDistanceAccD = float.MaxValue;
                int accD = 0;
                
                for (int i = 0; i < GameManager.instance.ghosts.Length; i++)
                {
                    if (ghostDistanceAccD < GhostDistance(GameManager.instance.ghosts[i].transform.localPosition / 13.5f))
                    {
                        ghostDistanceAccD = GhostDistance(GameManager.instance.ghosts[i].transform.localPosition / 13.5f);
                        accD = i;
                    }
                }

                if (ghostDistanceAccD < 3f && !GameManager.instance.ghosts[accD].frightened)
                {Debug.Log("sono dentro!");
                    if (!GhostInSight(Vector2.up))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.up);
                    }
                    else if (!GhostInSight(Vector2.right))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.right);
                    }
                    else if (!GhostInSight(Vector2.left))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.left);
                    }
                    else
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(GameManager.instance.ghosts[accD].movement.direction);
                    }
                }
                else
                {Debug.Log("direzione normale");
                    pacman.movement.SetDirection(Vector2.down);
                }
                
                break;
            case 3:
                occ = pacman.movement.Occupied(Vector2.left);
                
                if (!occ && movementMemory != Vector2.left && movementMemory != Vector2.right)
                {
                    //Debug.Log("Buona svolta");
                    AddReward(0.012f); // Se gira correttamente ad un incrocio
                    PelletInSight(Vector2.left);
                }

                if (!occ && movementMemory == Vector2.right)
                {
                    //Debug.Log("Inversione!");
                    AddReward(-0.020f);
                }

                float ghostDistanceAccL = float.MaxValue;
                int accL = 0;
                
                for (int i = 0; i < GameManager.instance.ghosts.Length; i++)
                {
                    if (ghostDistanceAccL < GhostDistance(GameManager.instance.ghosts[i].transform.localPosition / 13.5f))
                    {
                        ghostDistanceAccL = GhostDistance(GameManager.instance.ghosts[i].transform.localPosition / 13.5f);
                        accL = i;
                    }
                }

                if (ghostDistanceAccL < 3f && !GameManager.instance.ghosts[accL].frightened)
                {Debug.Log("sono dentro");
                    if (!GhostInSight(Vector2.up))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.up);
                    }
                    else if (!GhostInSight(Vector2.right))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.right);
                    }
                    else if (!GhostInSight(Vector2.down))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.down);
                    }
                    else
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(GameManager.instance.ghosts[accL].movement.direction);
                    }
                }
                else
                {Debug.Log("direzione normale");
                    pacman.movement.SetDirection(Vector2.left);
                }
                
                break;
            case 4:
                occ = pacman.movement.Occupied(Vector2.right);
                
                if (!occ && movementMemory != Vector2.right && movementMemory != Vector2.left)
                {
                    //Debug.Log("Buona svolta");
                    AddReward(0.012f); // Se gira correttamente ad un incrocio
                    PelletInSight(Vector2.right);
                }

                if (!occ && movementMemory == Vector2.left)
                {
                    //Debug.Log("Inversione!");
                    AddReward(-0.020f);
                }

                float ghostDistanceAccR = float.MaxValue;
                int accR = 0;
                
                for (int i = 0; i < GameManager.instance.ghosts.Length; i++)
                {
                    if (ghostDistanceAccR < GhostDistance(GameManager.instance.ghosts[i].transform.localPosition / 13.5f))
                    {
                        ghostDistanceAccR = GhostDistance(GameManager.instance.ghosts[i].transform.localPosition / 13.5f);
                        accR = i;
                    }
                }

                if (ghostDistanceAccR < 3f && !GameManager.instance.ghosts[accR].frightened)
                {Debug.Log("sono dentro");
                    if (!GhostInSight(Vector2.up))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.up);
                    }
                    else if (!GhostInSight(Vector2.left))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.left);
                    }
                    else if (!GhostInSight(Vector2.down))
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(Vector2.down);
                    }
                    else
                    {Debug.Log("cambio");
                        pacman.movement.SetDirection(GameManager.instance.ghosts[accR].movement.direction);
                    }
                }
                else
                {Debug.Log("direzione normale");
                    pacman.movement.SetDirection(Vector2.right);
                }
                
                break;
        }

        positionMemory = (Vector2)GameManager.instance.pacman.transform.localPosition;
        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(pacman.movement.direction.y, pacman.movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        
        AddReward(-0.005f); //No win
    }

    /*private void OnCollisionExit2D(Collision2D other) // OnCollisionStay2D non funziona perché pacman tocca sempre i muri laterali
    {  // Anche questo non funziona sempre perché non becca bene l'uscita dai bivi, ma ne trova anche quando non ha più muri ai lati
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            AddReward(0.1f);
            Debug.Log("Exited Collision!");
        }
    }*/

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Set the new direction based on the current input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 2;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 4;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }
    }
}
    