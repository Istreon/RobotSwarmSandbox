using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterManager : MonoBehaviour
{

    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("This is the size of the radius.")]
    private float fieldOfViewSize = 1.0f;
    [SerializeField]
    [Range(0, 360)]
    [Tooltip("This is the size of blind spot of the agent (in degrees)")]
    protected float blindSpotSize = 30;
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

    /*[Header("Feeler parameters")]
    [SerializeField]
    private bool feelerEnable = true;
    [SerializeField]
    [Range(0.0f, 2.0f)]
    [Tooltip("This is the distance of the feeler from the agent.")]
    private float feelerDistance = 0.5f;
    [SerializeField]
    [Range(0.0f, 0.5f)]
    [Tooltip("This is the size of the feeler radius.")]
    private float feelerSize = 0.1f;*/

    [Header("Intensity parameters")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float cohesionIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float alignmentIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float separationIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float avoidingObstaclesIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float moveForwardIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 5.0f)]
    private float randomMovementIntensity = 20.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float frictionIntensity = 0.1f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float maxSpeed = 1.0f;

    [Header("Preservation of connectivity parameters")]
    [SerializeField]
    [Range(0.01f, 2.0f)]
    private float distanceBetweenAgents = 1.0f;


    public float GetFieldOfViewSize()
    {
        return fieldOfViewSize;
    }

    public float GetBlindSpotSize()
    {
        return blindSpotSize;
    }

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

    /*public bool IsFeelerEnable()
    {
        return feelerEnable;
    }
    public float GetFeelerDistance()
    {
        return feelerDistance;
    }

    public float GetFeelerSize()
    {
        return feelerSize;
    }*/

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

    public float GetAvoidingObstaclesIntensity()
    {
        return avoidingObstaclesIntensity;
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

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public float GetDistanceBetweenAgents()
    {
        return distanceBetweenAgents;
    }
}
