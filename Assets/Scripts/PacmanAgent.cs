using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;

public class PacmanAgent : Agent
{
    private Pacman pacman { get; set; }
    public Vector3 positionMemory;
    public LayerMask ghostAndWallLayer;
    public LayerMask pelletAndWallLayer;
    public LayerMask obstacleLayer;
    
    public struct State
    {
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
        int PERCENTAGE = 15;
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
        return 4; // Nessun ghost
    }

    public void PowerPelletEaten()
    {
        if (pastState.distanceBlinky <= 3f
            || pastState.distanceInky <= 3f
            || pastState.distancePinky <= 3f
            || pastState.distanceClyde <= 3f)
        {
            AddReward(5f);
        }
        else if (pastState.distanceBlinky <= 5f
                 || pastState.distanceInky <= 5f
                 || pastState.distancePinky <= 5f
                 || pastState.distanceClyde <= 5f)
        {
            AddReward(3f);
        }
        else 
        {
            AddReward(-0.5f);
        }
    }

    public override void OnEpisodeBegin()
    {
        GameManager.instance.Start();
    }

    public void ChangeDirection(Vector2 newDirection)
    {
        bool occ = pacman.movement.Occupied(newDirection);
        Vector2 oppositeDirection = -newDirection;

        // Se la nuova direzione è occupata
        // if (occ) 
        // {
        //     AddReward(-0.5f);
        //     Debug.Log("Occupied");
        // } else if (pastState.pacmanDirection != newDirection && pastState.pacmanDirection != oppositeDirection) // Se gira in una direzione in cui può andare
        // {
        //     //Debug.Log("Buona svolta");
        //     AddReward(0.012f);

        if (pastState.pacmanDirection == oppositeDirection) // Se fa inversione su se stesso
         {
             AddReward(-0.1f);
             Debug.Log("Inversione");

         }

        if (pastState.pacmanDirection != newDirection && pastState.pacmanDirection != oppositeDirection) // Se non sta facendo iversione e...
        {
            if (PelletInSight(newDirection)) // Gira in direzione di pellet
            {
                AddReward(0.5f);
            }
        //     else if (PelletInSight(-newDirection) && !PelletInSight(newDirection)) // Gira in direzione in cui non ci sono pellet ma dall'altra parte c'erano
        //     {
        //         AddReward(-2f);
        //     }
        }
            
        // Se va contro un fantasma spaventato per mangiarlo
        // if (GameManager.instance.ghosts[0].frightened.enabled && GhostInSight(newDirection))
        // {
        //     AddReward(0.2f);
        // }

        // Se va nella direzione in cui ci sono più pellet
        /*if (Mathf.Abs(pastState.suggestedDirection.x) > Mathf.Abs(pastState.suggestedDirection.y))
        {
            if (newDirection.Equals(new Vector2(Mathf.Sign(pastState.suggestedDirection.x), 0f)))
            {
                //Debug.Log("Direzione giusta");
                AddReward(0.024f);
            }
            else AddReward(-0.008f);
        }
        else
        {
            if (newDirection.Equals(new Vector2(0f, Mathf.Sign(pastState.suggestedDirection.y))))
            {
                //Debug.Log("Direzione giusta");
                AddReward(0.024f);
            }
            else AddReward(-0.008f);
        }*/
        
        // FleeFromGhosts(newDirection); // FUNZIONE CHE METTE LE ROTELLE ALLA BICICLETTA
        
        // if (pastState.pacmanDirection != newDirection) 
        // {
        //     RewardFromGhosts(newDirection); // FUNZIONE CHE VORREBBE INSEGNARE A PEDALARE LONTANO DAI FANTASMI
        // }
        
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
    
    public bool NearActiveGhost(float distance)
    { 
        return !GameManager.instance.ghosts[0].home.enabled && pastState.distanceBlinky < distance || 
               !GameManager.instance.ghosts[1].home.enabled && pastState.distanceInky < distance || 
               !GameManager.instance.ghosts[2].home.enabled && pastState.distancePinky < distance || 
               !GameManager.instance.ghosts[3].home.enabled && pastState.distanceClyde < distance;
    }
    
    private void FleeFromGhosts(Vector2 newDirection)
    {
        (int, float) nearestGhost = NearestGhost();
        int ghost = nearestGhost.Item1;
        float distance = nearestGhost.Item2;
        if (!GameManager.instance.ghosts[ghost].frightened.enabled && distance < 4f)
        {
            if (SuperGhostInSight(-GameManager.instance.ghosts[ghost].movement.direction) == ghost) // Dovrebbe coprire il caso in cui il fantasmino gli va addosso
            {
                Vector2 direction = GameManager.instance.ghosts[ghost].movement.direction;
                if (!pacman.movement.Occupied(direction))
                {
                    pacman.movement.SetDirection(direction);
                }
                else if (!pacman.movement.Occupied(new Vector2(-direction.y, direction.x)))
                {
                    pacman.movement.SetDirection(new Vector2(-direction.y, direction.x));

                }
                else if (!pacman.movement.Occupied(new Vector2(direction.y, -direction.x)))
                {
                    pacman.movement.SetDirection(new Vector2(direction.y, -direction.x));

                }
            }
            else if (newDirection.Equals(-GameManager.instance.ghosts[ghost].movement.direction) && SuperGhostInSight(newDirection) == ghost)
            {
                Vector2 direction = newDirection;
                if (!pacman.movement.Occupied(new Vector2(-direction.y, direction.x)))
                {
                    pacman.movement.SetDirection(new Vector2(-direction.y, direction.x));

                }
                else if (!pacman.movement.Occupied(new Vector2(direction.y, -direction.x)))
                {
                    pacman.movement.SetDirection(new Vector2(direction.y, -direction.x));

                }
                else
                {
                    pacman.movement.SetDirection(-direction);
                }
            }
            else if ((SuperGhostInSight(Vector2.down) == 4 && SuperGhostInSight(Vector2.left) == 4 &&
                      SuperGhostInSight(Vector2.right) == 4 && SuperGhostInSight(Vector2.up) == 4))
                // se il fantasmino più vicino non si trova nei 4 assi di pacman ma comunque a distanza < 4f
            {
                Vector2 distanceNearestGhostInSpace = GhostDistanceXY(GameManager.instance.ghosts[ghost].transform.localPosition);
                Vector2 new1 = new Vector2(-Mathf.Sign(distanceNearestGhostInSpace.x), 0);
                if (!pacman.movement.Occupied(new1))
                {
                    pacman.movement.SetDirection(new1);
                }
                else
                {
                    Vector2 new2 = new Vector2(0, -Mathf.Sign(distanceNearestGhostInSpace.y));
                    pacman.movement.SetDirection(new2);
                }
            }
        }
        else
        {
            pacman.movement.SetDirection(newDirection);
        }
    } 
    
    private void RewardFromGhosts(Vector2 newDirection)
    {
        (int, float) nearestGhost = NearestGhost();
        int ghost = nearestGhost.Item1;
        float distance = nearestGhost.Item2;
        if (!GameManager.instance.ghosts[ghost].frightened.enabled && distance < 5f)
        {
            if (SuperGhostInSight(-GameManager.instance.ghosts[ghost].movement.direction) == ghost) // Dovrebbe coprire il caso in cui il fantasmino gli va addosso
            {
                Vector2 ghostDirection = GameManager.instance.ghosts[ghost].movement.direction;
                if (!pacman.movement.Occupied(ghostDirection)) // Se la direzione di fuga diretta dal fantasma non è occupata...
                {
                    // pacman.movement.SetDirection(direction);
                    if (newDirection.Equals(ghostDirection)) {AddReward(1.5f);} else {AddReward(-0.5f);} // e coincide con quella scelta
                    
                }
                else if (!pacman.movement.Occupied(new Vector2(-ghostDirection.y, ghostDirection.x))) // Se è disponibile una via di fuga perpendicolare
                {
                    // pacman.movement.SetDirection(new Vector2(-direction.y, direction.x));
                    if (newDirection.Equals(new Vector2(-ghostDirection.y, ghostDirection.x))) {AddReward(1.5f);}
                
                }
                else if (!pacman.movement.Occupied(new Vector2(ghostDirection.y, -ghostDirection.x)))
                {
                    // pacman.movement.SetDirection(new Vector2(direction.y, -direction.x));
                    if (newDirection.Equals(new Vector2(ghostDirection.y, -ghostDirection.x))) {AddReward(1.5f);}
                }
            }
            else if (newDirection.Equals(-GameManager.instance.ghosts[ghost].movement.direction) && SuperGhostInSight(newDirection) == ghost) // Se va verso un fantasma
            {
                Vector2 direction = newDirection;
                if (!pacman.movement.Occupied(new Vector2(-direction.y, direction.x)))
                {
                    // pacman.movement.SetDirection(new Vector2(-direction.y, direction.x));
                    AddReward(-0.8f);
                }
                else if (!pacman.movement.Occupied(new Vector2(direction.y, -direction.x)))
                {
                    // pacman.movement.SetDirection(new Vector2(direction.y, -direction.x));
                    AddReward(-0.8f);
                }
                else if (!pacman.movement.Occupied(-direction))
                {
                    // pacman.movement.SetDirection(-direction);
                    AddReward(-0.8f);
                }
            }
            else if (!GameManager.instance.ghosts[ghost].home.enabled && (SuperGhostInSight(Vector2.down) == 4 && SuperGhostInSight(Vector2.left) == 4 &&
                      SuperGhostInSight(Vector2.right) == 4 && SuperGhostInSight(Vector2.up) == 4))
                // se il fantasmino più vicino non si trova nei 4 assi di pacman ma comunque a distanza < 5f
            {
                Vector2 distanceNearestGhostInSpace = GhostDistanceXY(GameManager.instance.ghosts[ghost].transform.localPosition);
                Vector2 new1 = new Vector2(-Mathf.Sign(distanceNearestGhostInSpace.x), 0);
                if (!pacman.movement.Occupied(new1))
                {
                    // pacman.movement.SetDirection(new1);
                    if (newDirection.Equals(new1)) AddReward(1.5f);
                }
                else
                {
                    Vector2 new2 = new Vector2(0, -Mathf.Sign(distanceNearestGhostInSpace.y));
                    // pacman.movement.SetDirection(new2);
                    if (newDirection.Equals(new2)) AddReward(1.5f);
                }
            }
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
        // Debug.Log(pastState.suggestedDirection);
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
        // bool occ; // Occupied per verificare che la direzione corrente sia occupata dal muro o meno

        if (GameManager.instance.pacman.transform.localPosition.Equals(positionMemory))
        {
            // Debug.Log("Stuck!");
            AddReward(-1f); // Se fermo nello stesso punto
        }

        // if (NearActiveGhost(3f))
        // {
        //     AddReward(-0.2f);
        // }
        //
        // if (NearActiveGhost(8f))
        // {
        //     AddReward(-0.02f);
        // }

        // Set the new direction based on the current input
        switch (movementControl)
        {
            case 0:
                // occ = pacman.movement.Occupied(pacman.movement.direction);
                // if (!occ) AddReward(0.008f); // Se va dritto e non è occupato
                // RewardFromGhosts(pacman.movement.direction);
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

        positionMemory = GameManager.instance.pacman.transform.localPosition;
        
        // AddReward(-0.01f); //No win
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
    