using UnityEngine;

public class SwarmParameters
{
    //--Swarm behaviour--//
    private BehaviourManager.AgentBehaviour agentBehaviour; //Define the current behaviour of the agents

    //--Swarm movement--//
    private MovementManager.AgentMovement agentMovement; //Defines how the agent moves

    //--Map parameters--//
    private float mapSizeX = 7.0f;
    private float mapSizeZ = 7.0f;

    //--Field of view size--//
    private float fieldOfViewSize = 1.0f; //This is the size of the radius
    protected float blindSpotSize = 30; //This is the size of blind spot of the agent (in degrees)

    //--Intensity parameters--//
    private float maxSpeed = 1.0f;
    private float moveForwardIntensity = 1.0f;
    private float randomMovementIntensity = 20.0f;
    private float frictionIntensity = 0.1f;
    private float avoidCollisionWithNeighboursIntensity = 20.0f;


    //--Reynolds model parameters--//
    private float cohesionIntensity = 1.0f;
    private float alignmentIntensity = 1.0f;
    private float separationIntensity = 1.0f;


    //--Couzin model parameters--//
    private float attractionZoneSize = 0.3f; //This is the size of the radius
    private float alignmentZoneSize = 0.3f; //This is the size of the radius
    private float repulsionZoneSize = 0.3f; //This is the size of the radius

    //--Preservation of connectivity parameters--//
    private float distanceBetweenAgents = 1.0f;


    #region Methods - Getter
    //--Agent behaviour--//
    public BehaviourManager.AgentBehaviour GetAgentBehaviour()
    {
        return agentBehaviour;
    }

    //--Agent movement--//
    public MovementManager.AgentMovement GetAgentMovement()
    {
        return agentMovement;
    }

    //--Map parameters--//
    public float GetMapSizeX()
    {
        return mapSizeX;
    }

    public float GetMapSizeZ()
    {
        return mapSizeZ;
    }

    //--Field of view size--//
    public float GetFieldOfViewSize()
    {
        return fieldOfViewSize;
    }

    public float GetBlindSpotSize()
    {
        return blindSpotSize;
    }

    //--Intensity parameters--//
    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public float GetMoveForwardIntensity()
    {
        return moveForwardIntensity;
    }

    public float GetRandomMovementIntensity()
    {
        return randomMovementIntensity;
    }

    public float GetFrictionIntensity()
    {
        return frictionIntensity;
    }

    public float GetAvoidCollisionWithNeighboursIntensity()
    {
        return avoidCollisionWithNeighboursIntensity;
    }

    //--Reynolds model parameters--//
    public float GetSeparationIntensity()
    {
        return separationIntensity;
    }

    public float GetAlignmentIntensity()
    {
        return alignmentIntensity;
    }

    public float GetCohesionIntensity()
    {
        return cohesionIntensity;
    }

    //--Couzin model parameters--//
    public float GetAttractionZoneSize()
    {
        return attractionZoneSize;
    }
    public float GetAlignmentZoneSize()
    {
        return alignmentZoneSize;
    }
    public float GetRepulsionZoneSize()
    {
        return repulsionZoneSize;
    }

    //--Preservation of connectivity parameters--//
    public float GetDistanceBetweenAgents()
    {
        return distanceBetweenAgents;
    }

    #endregion
}
