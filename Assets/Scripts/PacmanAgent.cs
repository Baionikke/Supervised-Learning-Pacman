using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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
    private Vector2 suggestedDirection;
    public float distanceBlinky;
    public float distanceInky;
    public float distancePinky;
    public float distanceClyde;
    public LayerMask ghostAndWallLayer;
    public LayerMask pelletAndWallLayer;
    public LayerMask obstacleLayer;

    private float PelletDistance(Transform pellet)
    {
        Vector2 pacmanPos = new Vector2(GameManager.instance.pacman.transform.localPosition.x,
            GameManager.instance.pacman.transform.localPosition.y);
        Vector2 pelletPos = new Vector2(pellet.localPosition.x, pellet.localPosition.y);
        float distance = Vector2.Distance(pacmanPos / 13.5f, pelletPos / 13.5f);
        return distance;
    }

    private bool PelletInSight(Vector2 direction)
    {
        bool pelletInSight = false;
        RaycastHit2D hit = Physics2D.BoxCast(GameManager.instance.pacman.movement.transform.position,
            Vector2.one * 0.1f, 0f, direction, 5f, pelletAndWallLayer);
        if (hit.collider)
        {
            if (hit.collider.name == "Pellet(Clone)")
            {
                pelletInSight = true; 
            }
        }
        return pelletInSight;
    }

    private Vector2 PercDistancesFromPellet(SortedDictionary<float, Vector2> distancesFromPellet,
        Vector2 pacmanPosition, int cont)
    {
        int PERCENTAGE = 10;
        float mean20PelletX = 0;
        float mean20PelletY = 0;
        int numPellet = (int)Math.Ceiling((decimal)cont / 100 * PERCENTAGE); 
        for (int j = 0; j < numPellet; j++)
        {
            mean20PelletX += distancesFromPellet.ElementAt(j).Value.x - pacmanPosition.x;
            mean20PelletY += distancesFromPellet.ElementAt(j).Value.y - pacmanPosition.y;
        }
        Vector2 mean20Pellet = new Vector2(mean20PelletX/numPellet, mean20PelletY/numPellet);
        // Debug.Log(mean20Pellet);
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

    private float GhostDistance(Vector3 ghostPosition)
    {
        return Vector2.Distance(ghostPosition, GameManager.instance.pacman.transform.localPosition);
    }

    public bool GhostInSight(Vector2 direction) // Controlla che non ci siano fantasmi davanti a sé
    {
        bool ghostInSight = false;
        RaycastHit2D hit = Physics2D.BoxCast(GameManager.instance.pacman.movement.transform.position,
            Vector2.one * 0.3f, 0f, direction, 5f, ghostAndWallLayer);
        if (hit.collider)
        {
            if (hit.collider.CompareTag("Ghosts"))
            {
                ghostInSight = true;
                // Debug.Log("ghost in sight");
            }
        }
        return ghostInSight;
    }

    private int SuperGhostInSight(Vector2 direction) // Controlla che non ci siano fantasmi davanti a sé e individua quello presente
    {
        RaycastHit2D hit = Physics2D.BoxCast(GameManager.instance.pacman.movement.transform.position,
            Vector2.one * 0.5f, 0f, direction, 5f, ghostAndWallLayer);
        if (hit.collider)
        {
            if (hit.collider.name == "Ghost_Blinky")
            {
                return 0;
            }
            if (hit.collider.name == "Ghost_Inky")
            {
                return 1;
            }
            if (hit.collider.name == "Ghost_Pinky")
            {
                return 2;
            }
            if (hit.collider.name == "Ghost_Clyde")
            {
                return 3;
            }
        }
        return 4;
    }

    private bool HittingWall(Vector2 direction)
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
    }

    public override void OnEpisodeBegin() // TODO: to understand how the method works
    {
        GameManager.instance.Start();
    }

    public void ChangeDirection(Vector2 newDirection)
    {
        bool occ = pacman.movement.Occupied(newDirection);
        Vector2 oppositeDirection = -newDirection;
        
        // Se gira in una direzione in cui può andare
        if (!occ && movementMemory != newDirection && movementMemory != oppositeDirection)
        {
            //Debug.Log("Buona svolta");
            AddReward(0.012f); // Se gira correttamente ad un incrocio
        }
        
        // Se svolta e ci sono pellet
        if (movementMemory != newDirection && PelletInSight(newDirection))
        {
            AddReward(0.5f);
        }
        
        // Se fa inversione su se stesso
        if (!occ && movementMemory == oppositeDirection)
        {
            //Debug.Log("Inversione!");
            AddReward(-0.016f);
        }

        // Se va nella dritto verso un fantasma
        if (!GameManager.instance.ghosts[0].frightened && GhostInSight(newDirection))
        {
            AddReward(-0.1f); // Stai andando contro un fantasma
            //Debug.Log("Ghost in sight"); 
        }
        
        // Se si allontana da un fantasma con il fantasma alle sue spalle
        if (!GameManager.instance.ghosts[0].frightened && GhostInSight(-newDirection))
        {
            AddReward(0.2f); // Stai andando via da un fantasma
            //Debug.Log("Ghost in sight-retro"); 
        }
        
        // Se va nella direzione in cui ci sono più pellet
        if (Math.Abs(suggestedDirection.x) > Math.Abs(suggestedDirection.y))
        {
            if (newDirection.Equals(new Vector2(Math.Sign(suggestedDirection.x), 0f)))
            {
                //Debug.Log("Direzione giusta");
                AddReward(0.012f);
            }
            else AddReward(-0.004f);
        }
        else
        {
            if (newDirection.Equals(new Vector2(0f, Math.Sign(suggestedDirection.y))))
            {
                //Debug.Log("Direzione giusta");
                AddReward(0.012f);
            }
            else AddReward(-0.004f);
        }
        
        FleeFromGhosts(newDirection); // FUNZIONE CHE METTE LE ROTELLE ALLA BICICLETTA
        //pacman.movement.SetDirection(newDirection);
    }

    private (int,float) NearestGhost()
    {
        int nearestGhostA;
        int nearestGhostB;
        int nearestGhost;
        float minDistanceA;
        float minDistanceB;
        float minDistance;

        if (distanceBlinky <= distanceInky)
        {
            nearestGhostA = 0;
            minDistanceA = distanceBlinky;
        }
        else
        {
            nearestGhostA = 1;
            minDistanceA = distanceInky;
        }

        if (distancePinky <= distanceClyde)
        {
            nearestGhostB = 2;
            minDistanceB = distancePinky;
        }
        else
        {
            nearestGhostB = 3;
            minDistanceB = distanceClyde;
        }

        if (minDistanceA < minDistanceB)
        {
            nearestGhost = nearestGhostA;
            minDistance = minDistanceA;
        }
        else
        {
            nearestGhost = nearestGhostB;
            minDistance = minDistanceB;
        }

        return (nearestGhost, minDistance);
    }

    private void FleeFromGhosts(Vector2 newDirection)
    {
        (int, float) nearestGhost = NearestGhost();
        int ghost = nearestGhost.Item1;
        float distance = nearestGhost.Item2;
        
        if (!GameManager.instance.ghosts[ghost].frightened.enabled && distance < 3f && SuperGhostInSight(-GameManager.instance.ghosts[ghost].movement.direction)==ghost) // Dovrebbe coprire il caso in cui il fantasmino gli va addosso
        {
            Vector2 direction = GameManager.instance.ghosts[ghost].movement.direction;
            if (!pacman.movement.Occupied(direction))
            {
                pacman.movement.SetDirection(direction);
            } else if (!pacman.movement.Occupied(new Vector2(-direction.y, direction.x)))
            {
                pacman.movement.SetDirection(new Vector2(-direction.y, direction.x));

            } else if (!pacman.movement.Occupied(new Vector2(direction.y, -direction.x)))
            {
                pacman.movement.SetDirection(new Vector2(direction.y, -direction.x));

            }
        } else if (!GameManager.instance.ghosts[ghost].frightened.enabled && distance < 3f &&
                   SuperGhostInSight(newDirection) == ghost)
        {
            Vector2 direction = newDirection; 
            if (!pacman.movement.Occupied(new Vector2(-direction.y, direction.x)))
            {
                pacman.movement.SetDirection(new Vector2(-direction.y, direction.x));

            } else if (!pacman.movement.Occupied(new Vector2(direction.y, -direction.x)))
            {
                pacman.movement.SetDirection(new Vector2(direction.y, -direction.x));

            }
            else
            {
                pacman.movement.SetDirection(-direction);
            }
        }
        else
        {
            pacman.movement.SetDirection(newDirection);
        }
    } 
    
    public override void CollectObservations(VectorSensor sensor) // -> 
    {
        // Position
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
            suggestedDirection = PercDistancesFromPellet(distancesFromPellet, pacmanPos, distancesFromPellet.Count);
            
        }
        else
        {
            suggestedDirection = Vector2.zero;
        }
        sensor.AddObservation(suggestedDirection);

        // Ghost Blinky
        sensor.AddObservation(GhostDistanceXY((GameManager.instance.ghosts[0].transform.localPosition) / 13.5f));
        distanceBlinky = GhostDistance(GameManager.instance.ghosts[0].transform.localPosition);
        sensor.AddObservation(distanceBlinky/27f);
        sensor.AddObservation(GameManager.instance.ghosts[0].movement.direction);
        
        // Ghost Inky
        sensor.AddObservation(GhostDistanceXY((GameManager.instance.ghosts[1].transform.localPosition) / 13.5f));
        distanceInky = GhostDistance(GameManager.instance.ghosts[1].transform.localPosition);
        sensor.AddObservation(distanceInky/27f);
        sensor.AddObservation(GameManager.instance.ghosts[1].movement.direction);
        
        // Ghost Pinky
        sensor.AddObservation(GhostDistanceXY((GameManager.instance.ghosts[2].transform.localPosition) / 13.5f));
        distancePinky = GhostDistance(GameManager.instance.ghosts[2].transform.localPosition);
        sensor.AddObservation(distancePinky/27f);
        sensor.AddObservation(GameManager.instance.ghosts[2].movement.direction);
        
        // Ghost Clyde
        sensor.AddObservation(GhostDistanceXY((GameManager.instance.ghosts[3].transform.localPosition) / 13.5f));
        distanceClyde = GhostDistance(GameManager.instance.ghosts[3].transform.localPosition);
        sensor.AddObservation(distanceClyde/27f);
        sensor.AddObservation(GameManager.instance.ghosts[3].movement.direction);


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
                if (!occ) AddReward(0.008f); // Se va dritto e non è occupato
                if (GhostInSight(pacman.movement.direction) && !GameManager.instance.ghosts[0].frightened)
                {
                    AddReward(-0.1f); // Stai andando contro un fantasma
                    //Debug.Log("Ghost in sight");
                }

                break;
            case 1:
                ChangeDirection(Vector2.up);
                break;
            case 2:
                ChangeDirection(Vector2.down);
                break;
            case 3:
                ChangeDirection(Vector2.left);
                break;
            case 4:
                ChangeDirection(Vector2.right);
                break;
        }

        movementMemory = GameManager.instance.pacman.movement.direction;
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
    