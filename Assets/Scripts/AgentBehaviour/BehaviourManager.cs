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

        switch (agentBehaviour)
        {
            case AgentBehaviour.None:
                forces = new List<Vector3>();
                break;
            case AgentBehaviour.Reynolds:
                forces = ReynoldsBehaviour(agent, swarm);
                break;
            case AgentBehaviour.Couzin:
                forces = CouzinBehaviour(agent, swarm);
                break;
            case AgentBehaviour.PreservationConnectivity:
                forces = PreservationConnectivityBehaviour(agent, swarm);
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
        foreach (AgentData a in neighbours)
        {
            neighboursPositions.Add(a.GetPosition());
            neighboursSpeeds.Add(a.GetSpeed());
        }

        forces.Add(BehaviourRules.RandomMovement(parameters.GetRandomMovementIntensity(), swarm.GetRandomGenerator()));
        forces.Add(BehaviourRules.MoveForward(parameters.GetMoveForwardIntensity(), agent.GetSpeed()));
        forces.Add(BehaviourRules.Friction(parameters.GetFrictionIntensity(), agent.GetSpeed()));
        forces.Add(BehaviourRules.AvoidCollisionWithNeighbours(parameters.GetAvoidCollisionWithNeighboursIntensity(), agent.GetPosition(), neighboursPositions, parameters.GetMaxSpeed(), 0.09f));
        forces.Add(BehaviourRules.BouncesOffWall(agent.GetPosition(), parameters.GetMaxSpeed(), parameters.GetMapSizeX(), parameters.GetMapSizeZ()));

        forces.Add(BehaviourRules.Cohesion(parameters.GetCohesionIntensity(), agent.GetPosition(), neighboursPositions));
        forces.Add(BehaviourRules.Separation(parameters.GetSeparationIntensity(), agent.GetPosition(), neighboursPositions));
        forces.Add(BehaviourRules.Alignment(parameters.GetAlignmentIntensity(), neighboursSpeeds));


        return forces;
    }

    private static List<Vector3> CouzinBehaviour(AgentData agent, SwarmData swarm)
    {
        List<Vector3> forces = new List<Vector3>();

        SwarmParameters parameters = swarm.GetParameters();

        float zone1 = parameters.GetRepulsionZoneSize();
        float zone2 = parameters.GetRepulsionZoneSize() + parameters.GetAlignmentZoneSize();
        float zone3 = parameters.GetRepulsionZoneSize() + parameters.GetAlignmentZoneSize() + parameters.GetAttractionZoneSize();

        //--Get neighbours in the different detection zones--//


        List<AgentData> neighboursSeparation = SwarmTools.GetNeighbours(agent, swarm.GetAgentsData(), zone1, parameters.GetBlindSpotSize());
        List<AgentData> neighboursAlignment = SwarmTools.GetNeighbours(agent, swarm.GetAgentsData(), zone2, parameters.GetBlindSpotSize());
        List<AgentData> neighboursAttraction = SwarmTools.GetNeighbours(agent, swarm.GetAgentsData(), zone3, parameters.GetBlindSpotSize());

        List<AgentData> neighbours = new List<AgentData>(neighboursAttraction);

        foreach (AgentData a in neighboursSeparation)
        {
            neighboursAlignment.Remove(a);
            neighboursAttraction.Remove(a);
        }

        foreach (AgentData a in neighboursAlignment)
        {
            neighboursAttraction.Remove(a);
        }

        List<Vector3> neighboursPositions = new List<Vector3>();
        foreach (AgentData a in neighbours)
        {
            neighboursPositions.Add(a.GetPosition());
        }

        //Attraction positions
        List<Vector3> attractionPositions = new List<Vector3>();
        foreach (AgentData a in neighboursAttraction)
        {
            attractionPositions.Add(a.GetPosition());
        }

        //Alignement speeds
        List<Vector3> alignmentSpeed = new List<Vector3>();
        foreach (AgentData a in neighboursAlignment)
        {
            alignmentSpeed.Add(a.GetSpeed());
        }

        //Separation positions
        List<Vector3> separationPositions = new List<Vector3>();
        foreach (AgentData a in neighboursSeparation)
        {
            separationPositions.Add(a.GetPosition());
        }

        forces.Add(BehaviourRules.RandomMovement(parameters.GetRandomMovementIntensity(), swarm.GetRandomGenerator()));
        forces.Add(BehaviourRules.MoveForward(parameters.GetMoveForwardIntensity(), agent.GetSpeed()));
        forces.Add(BehaviourRules.Friction(parameters.GetFrictionIntensity(), agent.GetSpeed()));
        forces.Add(BehaviourRules.AvoidCollisionWithNeighbours(parameters.GetAvoidCollisionWithNeighboursIntensity(), agent.GetPosition(), neighboursPositions, parameters.GetMaxSpeed(), 0.09f));
        forces.Add(BehaviourRules.BouncesOffWall(agent.GetPosition(), parameters.GetMaxSpeed(), parameters.GetMapSizeX(), parameters.GetMapSizeZ()));

        forces.Add(BehaviourRules.Cohesion(parameters.GetCohesionIntensity(), agent.GetPosition(), attractionPositions));
        forces.Add(BehaviourRules.Separation(parameters.GetSeparationIntensity(), agent.GetPosition(), separationPositions));
        forces.Add(BehaviourRules.Alignment(parameters.GetAlignmentIntensity(), alignmentSpeed));


        return forces;
    }

    private static List<Vector3> PreservationConnectivityBehaviour(AgentData agent, SwarmData swarm)
    {
        List<Vector3> forces = new List<Vector3>();

        SwarmParameters parameters = swarm.GetParameters();

        List<AgentData> neighbours = SwarmTools.GetNeighbours(agent, swarm.GetAgentsData(), parameters.GetFieldOfViewSize(), parameters.GetBlindSpotSize());

        List<Vector3> neighboursPositions = new List<Vector3>();
        List<Vector3> neighboursSpeeds = new List<Vector3>();
        foreach (AgentData a in neighbours)
        {
            neighboursPositions.Add(a.GetPosition());
            neighboursSpeeds.Add(a.GetSpeed());
        }

        forces.Add(BehaviourRules.RandomMovement(parameters.GetRandomMovementIntensity(), swarm.GetRandomGenerator()));
        forces.Add(BehaviourRules.MoveForward(parameters.GetMoveForwardIntensity(), agent.GetSpeed()));
        forces.Add(BehaviourRules.Friction(parameters.GetFrictionIntensity(), agent.GetSpeed()));
        forces.Add(BehaviourRules.AvoidCollisionWithNeighbours(parameters.GetAvoidCollisionWithNeighboursIntensity(), agent.GetPosition(), neighboursPositions, parameters.GetMaxSpeed(), 0.09f));
        forces.Add(BehaviourRules.BouncesOffWall(agent.GetPosition(), parameters.GetMaxSpeed(), parameters.GetMapSizeX(), parameters.GetMapSizeZ()));

        forces.Add(BehaviourRules.PotentialFunction(parameters.GetDistanceBetweenAgents(), agent.GetPosition(), neighboursPositions));
        forces.Add(BehaviourRules.AlignmentUsingDifference(parameters.GetAlignmentIntensity(), agent.GetSpeed(), neighboursSpeeds));

        return forces;
    }
}
