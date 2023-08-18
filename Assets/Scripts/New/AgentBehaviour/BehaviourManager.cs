using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourManager
{
    public enum AgentBehaviour
    {
        None,
        Reynolds,
        Couzin,
        PreservationConnectivity
    }

    public static List<Vector3> ApplySocialBehaviour(AgentBehaviour agentBehaviour, AgentData agent, SwarmData swarm)
    {
        List<Vector3> forces;

        switch(agentBehaviour)
        {
            case AgentBehaviour.None:
                forces = new List<Vector3>();
                break;
            case AgentBehaviour.Reynolds:
                forces = ReynoldsBehaviour(agent, swarm);
                break;
            default:
                forces = null;
                Debug.LogError("Confronted with unimplemented behaviour.");
                break;
        }

        return forces;
    }

    private static List<Vector3> ReynoldsBehaviour(AgentData agent, SwarmData swarm)
    {
        List<Vector3> forces = new List<Vector3>();

        SwarmParameters parameters = swarm.GetParameters();

        List<AgentData> neighbours = SwarmTools.GetNeighbours(agent, swarm.GetAgentsData(), parameters.GetFieldOfViewSize(), parameters.GetBlindSpotSize());

        List<Vector3> neighboursPositions = new List<Vector3>();
        List<Vector3> neighboursSpeeds = new List<Vector3>();
        foreach(AgentData a in neighbours)
        {
            neighboursPositions.Add(a.GetPosition());
            neighboursSpeeds.Add(a.GetSpeed());
        }

        forces.Add(BehaviourRules.RandomMovement(parameters.GetRandomMovementIntensity()));
        forces.Add(BehaviourRules.MoveForward(parameters.GetMoveForwardIntensity(), agent.GetSpeed()));
        forces.Add(BehaviourRules.Friction(parameters.GetFrictionIntensity(), agent.GetSpeed()));
        forces.Add(BehaviourRules.AvoidCollisionWithNeighbours(parameters.GetAvoidCollisionWithNeighboursIntensity(), agent.GetPosition(),neighboursPositions,parameters.GetMaxSpeed(), 0.09f));
        forces.Add(BehaviourRules.Cohesion(parameters.GetCohesionIntensity(),agent.GetPosition(), neighboursPositions));
        forces.Add(BehaviourRules.Separation(parameters.GetSeparationIntensity(), agent.GetPosition(), neighboursPositions));
        forces.Add(BehaviourRules.Alignment(parameters.GetAlignmentIntensity(), neighboursSpeeds));

        return forces;
    }
}
