using System;
using UnityEngine;

public class MovementManager
{
    public enum AgentMovement
    {
        Particle,   //Use the basic particle system, without physical constraints
        MonaRobot   //Use the mona robot movement system, using 2 wheels and physical characteristics
    }

    public static Tuple<Vector3,Vector3> ApplyAgentMovement(AgentMovement agentMovement, AgentData agent, float elapsedTime)
    {
        Tuple<Vector3,Vector3> newPositionAndDirection;

        switch(agentMovement)
        {
            case AgentMovement.Particle:
                newPositionAndDirection = ParticuleMovement(agent, elapsedTime);
                break;
            default:
                Debug.LogError("Unimplemented movement.");
                newPositionAndDirection = new Tuple<Vector3, Vector3>(agent.GetPosition(), agent.GetSpeed().normalized);
                break;
        }
        return newPositionAndDirection;
    }

    private static Tuple<Vector3, Vector3> ParticuleMovement(AgentData agent, float elapsedTime)
    {
        Vector3 newPosition = agent.GetPosition();
        newPosition += agent.GetSpeed() * elapsedTime;

        Tuple<Vector3, Vector3> newPositionAndDirection = new Tuple<Vector3, Vector3>(newPosition, agent.GetSpeed().normalized);
        return newPositionAndDirection;
    }
}
