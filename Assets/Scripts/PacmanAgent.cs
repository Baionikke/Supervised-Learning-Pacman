using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PacmanAgent : Agent
{
    
    public Pacman pacman { get; private set; }
        
    public override void CollectObservations(VectorSensor sensor)
    {
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        pacman = GetComponent<Pacman>();
        Debug.Log(actionBuffers.DiscreteActions[0]);
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
    
}
