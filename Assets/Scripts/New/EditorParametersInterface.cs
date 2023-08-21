using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorParametersInterface : MonoBehaviour
{
    [Header("Swarm behaviour")]
    [SerializeField]
    private BehaviourManager.AgentBehaviour agentBehaviour; //Define the current behaviour of the agents

    [Header("Swarm movement")]
    [SerializeField]
    private MovementManager.AgentMovement agentMovement; //Defines how the agent moves

    [Header("Map parameters")]
    [SerializeField]
    [Range(5, 20)]
    private float mapSizeX = 5.0f;

    [SerializeField]
    [Range(5, 20)]
    private float mapSizeZ = 5.0f;

    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("This is the size of the radius.")]
    private float fieldOfViewSize = 1.0f;
    [SerializeField]
    [Range(0, 360)]
    [Tooltip("This is the size of blind spot of the agent (in degrees)")]
    private float blindSpotSize = 30;

    [Header("Intensity parameters")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float maxSpeed = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float moveForwardIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float randomMovementIntensity = 0.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float frictionIntensity = 0.1f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float avoidCollisionWithNeighboursIntensity = 20.0f;


    [Header("Reynolds model parameters")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float cohesionIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float alignmentIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float separationIntensity = 1.0f;


    [Header("Couzin model parameters")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float attractionZoneSize = 0.3f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float alignmentZoneSize = 0.3f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float repulsionZoneSize = 0.3f;

    [Header("Preservation of connectivity parameters")]
    [SerializeField]
    [Range(0.01f, 2.0f)]
    private float distanceBetweenAgents = 1.0f;

    public SwarmParameters GetParameters()
    {
        SwarmParameters parameters = new SwarmParameters(agentBehaviour,
                                                        agentMovement,
                                                        mapSizeX,
                                                        mapSizeZ,
                                                        fieldOfViewSize,
                                                        blindSpotSize,
                                                        maxSpeed,
                                                        moveForwardIntensity,
                                                        randomMovementIntensity,
                                                        frictionIntensity,
                                                        avoidCollisionWithNeighboursIntensity,
                                                        cohesionIntensity,
                                                        alignmentIntensity,
                                                        separationIntensity,
                                                        attractionZoneSize,
                                                        alignmentZoneSize,
                                                        repulsionZoneSize,
                                                        distanceBetweenAgents);
        return parameters;
    }
}
