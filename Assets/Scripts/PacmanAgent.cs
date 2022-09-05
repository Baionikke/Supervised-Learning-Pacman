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
    public LayerMask ghostAndWallLayer;
    public LayerMask pelletAndWallLayer;
    public LayerMask obstacleLayer;
    
    public struct State
    {
        // Put **normalized** positions here
        public Vector2 pacmanDirection { get; set; }
        public Vector2 pacmanPosition { get; set; }
        public Vector2 suggestedDirection { get; set; }
        public float distanceBlinky { get; set; }
        public Vector2 directionBlinky { get; set; }
        public Vector2 distanceVectorBlinky { get; set; }
        public float distanceInky { get; set; }
        public Vector2 directionInky { get; set; }
        public Vector2 distanceVectorInky { get; set; }
        public float distancePinky { get; set; }
        public Vector2 directionPinky { get; set; }
        public Vector2 distanceVectorPinky { get; set; }
        public float distanceClyde { get; set; }
        public Vector2 directionClyde { get; set; }
        public Vector2 distanceVectorClyde { get; set; }
        public bool ghostFrightened { get; set; }

        public override string ToString() => $"Pacman Direction: \t\t{pacmanDirection}\n" +
                                             $"Pacman Position: \t\t{pacmanPosition}\n" +
                                             $"Suggested Direction: \t\t{suggestedDirection}\n" +
                                             $"Blinky (red)\n" +
                                             $"\tDistance: \t\t{distanceBlinky}\n" +
                                             $"\tDistance (Vectorial): \t{distanceVectorBlinky}\n" +
                                             $"\tDirection: \t\t{directionBlinky}\n" +
                                             $"Inky (turquoise)\n" +
                                             $"\tDistance: \t\t{distanceInky}\n" +
                                             $"\tDistance (Vectorial): \t{distanceVectorInky}\n" +
                                             $"\tDirection: \t\t{directionInky}\n" +
                                             $"Pinky (pink)\n" +
                                             $"\tDistance: \t\t{distancePinky}\n" +
                                             $"\tDistance (Vectorial): \t{distanceVectorPinky}\n" +
                                             $"\tDirection: \t\t{directionPinky}\n" +
                                             $"Clyde (yellow)\n" +
                                             $"\tDistance: \t\t{distanceClyde}\n" +
                                             $"\tDistance (Vectorial): \t{distanceVectorClyde}\n" +
                                             $"\tDirection: \t\t{directionClyde}\n" +
                                             $"Ghosts are frightened: \t\t{ghostFrightened}\n";
    }

    public State pastState;

    private float PelletDistance(Transform pellet)
    {
        Vector2 pacmanPos = new Vector2(GameManager.instance.pacman.transform.localPosition.x,
            GameManager.instance.pacman.transform.localPosition.y);
        Vector2 pelletPos = new Vector2(pellet.localPosition.x, pellet.localPosition.y);
        float distance = Vector2.Distance(pacmanPos, pelletPos);
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
        // Debug.Log(mean20Pellet/2f);
        return mean20Pellet/2f;
    }

    private Vector2 GhostDistanceXY(Vector3 ghostPosition)
    {
        float distanceX = (ghostPosition.x)-(GameManager.instance.pacman.transform.localPosition.x);
        float distanceY = (ghostPosition.y)-(GameManager.instance.pacman.transform.localPosition.y); // Volutamente non in val. assoluto
        return new Vector2(distanceX, distanceY);
    }

    private float GhostDistance(Vector2 ghostPosition)
    {
        return Vector2.Distance((ghostPosition), (GameManager.instance.pacman.transform.localPosition));
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

    public void PowerPelletEaten()
    {
        if (pastState.distanceBlinky <= 0.22f
            || pastState.distanceInky <= 0.22
            || pastState.distancePinky <= 0.22
            || pastState.distanceClyde <= 0.22)
        {
            AddReward(5f);
        }
        else if ((pastState.distanceBlinky > 0.22 && pastState.distanceBlinky <= 0.37f)
                 || (pastState.distanceInky > 0.22 && pastState.distanceInky <= 0.37f)
                 || (pastState.distancePinky > 0.22 && pastState.distancePinky <= 0.37f)
                 || (pastState.distanceClyde > 0.22 && pastState.distanceClyde <= 0.37f))
        {
            AddReward(3f);
        }
        else 
        {
            AddReward(-0.1f);
        }
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
        if (!occ && pastState.pacmanDirection != newDirection && pastState.pacmanDirection != oppositeDirection)
        {
            //Debug.Log("Buona svolta");
            AddReward(0.012f); // Se gira correttamente ad un incrocio
        }
        
        // Se svolta e ci sono pellet
        if (pastState.pacmanDirection != newDirection && pastState.pacmanDirection != oppositeDirection && PelletInSight(newDirection))
        {
            AddReward(0.5f);
        }
        
        // Se fa inversione su se stesso
        if (!occ && pastState.pacmanDirection == oppositeDirection)
        {
            //Debug.Log("Inversione!");
            AddReward(-0.05f);
        }

        // Se va dritto verso un fantasma
        if (!GameManager.instance.ghosts[0].frightened && GhostInSight(newDirection))
        {
            AddReward(-0.2f); // Stai andando contro un fantasma
            //Debug.Log("Ghost in sight"); 
        }
        
        // Se si allontana da un fantasma con il fantasma alle sue spalle
        // oppure
        // Se il fantasma è spaventato e gli va contro per mangiarlo
        if (!GameManager.instance.ghosts[0].frightened.enabled && GhostInSight(oppositeDirection))
        {
            AddReward(0.2f); // Stai andando via da un fantasma
            //Debug.Log("Ghost in sight-retro"); 
        } else if (GameManager.instance.ghosts[0].frightened.enabled && GhostInSight(newDirection))
        {
            AddReward(0.2f);
        }

        // Se va nella direzione in cui ci sono più pellet
        if (Math.Abs(pastState.suggestedDirection.x) > Math.Abs(pastState.suggestedDirection.y))
        {
            if (newDirection.Equals(new Vector2(Math.Sign(pastState.suggestedDirection.x), 0f)))
            {
                //Debug.Log("Direzione giusta");
                AddReward(0.024f);
            }
            else AddReward(-0.008f);
        }
        else
        {
            if (newDirection.Equals(new Vector2(0f, Math.Sign(pastState.suggestedDirection.y))))
            {
                //Debug.Log("Direzione giusta");
                AddReward(0.024f);
            }
            else AddReward(-0.008f);
        }
        
        // FleeFromGhosts(newDirection); // FUNZIONE CHE METTE LE ROTELLE ALLA BICICLETTA
        pacman.movement.SetDirection(newDirection);
    }

    private (int,float) NearestGhost()
    {
        int nearestGhostA;
        int nearestGhostB;
        int nearestGhost;
        float minDistanceA;
        float minDistanceB;
        float minDistance;

        if (pastState.distanceBlinky <= pastState.distanceInky)
        {
            nearestGhostA = 0;
            minDistanceA = pastState.distanceBlinky;
        }
        else
        {
            nearestGhostA = 1;
            minDistanceA = pastState.distanceInky;
        }

        if (pastState.distancePinky <= pastState.distanceClyde)
        {
            nearestGhostB = 2;
            minDistanceB = pastState.distancePinky;
        }
        else
        {
            nearestGhostB = 3;
            minDistanceB = pastState.distanceClyde;
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
        Vector2 pacmanPos = GameManager.instance.pacman.transform.localPosition;
        Vector2 pacmanDirection = GameManager.instance.pacman.movement.direction;
        sensor.AddObservation(pacmanPos);
        sensor.AddObservation(pacmanDirection);
        pastState.pacmanPosition = pacmanPos;
        pastState.pacmanDirection = pacmanDirection;
        

        // Pellets
        SortedDictionary<float, Vector2> distancesFromPellet = new SortedDictionary<float, Vector2>();

        foreach (Transform pellet in GameManager.instance.pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                Vector2 pelletsPosition = new Vector2(pellet.localPosition.x, pellet.localPosition.y);
                if (!distancesFromPellet.ContainsKey(PelletDistance(pellet)))
                {
                    distancesFromPellet.Add(PelletDistance(pellet), pelletsPosition);
                }
            }
        }

        if (distancesFromPellet.Count > 0)
        {
            pastState.suggestedDirection = PercDistancesFromPellet(distancesFromPellet, pacmanPos, distancesFromPellet.Count);
            
        }
        else
        {
            pastState.suggestedDirection = Vector2.zero;
        }
        sensor.AddObservation(pastState.suggestedDirection);
        
        // Ghost Blinky
        pastState.distanceBlinky = GhostDistance(GameManager.instance.ghosts[0].transform.localPosition);
        pastState.distanceVectorBlinky =
            GhostDistanceXY(GameManager.instance.ghosts[0].transform.localPosition);
        pastState.directionBlinky = GameManager.instance.ghosts[0].movement.direction;
        sensor.AddObservation(pastState.distanceVectorBlinky);
        sensor.AddObservation(pastState.distanceBlinky);
        sensor.AddObservation(pastState.directionBlinky);
        
        // Ghost Inky
        pastState.distanceInky = GhostDistance(GameManager.instance.ghosts[1].transform.localPosition);
        pastState.distanceVectorInky =
            GhostDistanceXY(GameManager.instance.ghosts[1].transform.localPosition);
        pastState.directionInky = GameManager.instance.ghosts[1].movement.direction;
        sensor.AddObservation(pastState.distanceVectorInky);
        sensor.AddObservation(pastState.distanceInky);
        sensor.AddObservation(pastState.directionInky);

        // Ghost Pinky
        pastState.distancePinky = GhostDistance(GameManager.instance.ghosts[2].transform.localPosition);
        pastState.distanceVectorPinky =
            GhostDistanceXY(GameManager.instance.ghosts[2].transform.localPosition);
        pastState.directionPinky = GameManager.instance.ghosts[2].movement.direction;
        sensor.AddObservation(pastState.distanceVectorPinky);
        sensor.AddObservation(pastState.distancePinky);
        sensor.AddObservation(pastState.directionPinky);

        // Ghost Clyde
        pastState.distanceClyde = GhostDistance(GameManager.instance.ghosts[3].transform.localPosition);
        pastState.distanceVectorClyde =
            GhostDistanceXY(GameManager.instance.ghosts[3].transform.localPosition);
        pastState.directionClyde = GameManager.instance.ghosts[3].movement.direction;
        sensor.AddObservation(pastState.distanceVectorClyde);
        sensor.AddObservation(pastState.distanceClyde);
        sensor.AddObservation(pastState.directionClyde);

        // Ghosts are Frightened
        pastState.ghostFrightened = GameManager.instance.ghosts[0].frightened.enabled;
        sensor.AddObservation(pastState.ghostFrightened);

        // Lives
        sensor.AddObservation(GameManager.instance.lives); // Forse eliminabile

        // Score
        // sensor.AddObservation(GameManager.instance.score); // Forse eliminabile
        
        //Debug.Log(pastState);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        pacman = GameManager.instance.pacman;
        //Debug.Log(actionBuffers.DiscreteActions[0]);
        int movementControl = actionBuffers.DiscreteActions[0];
        bool occ; // Occupied per verificare che la direzione corrente sia occupata dal muro o meno

        if ((Vector2)GameManager.instance.pacman.transform.localPosition == pastState.pacmanPosition)
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

        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(pacman.movement.direction.y, pacman.movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        
        AddReward(-0.005f); //No win
    }

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
    