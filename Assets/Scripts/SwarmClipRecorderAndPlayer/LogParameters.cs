
[System.Serializable]
public class LogParameters
{
    #region Private field
    //Agent parameters
    private float fieldOfViewSize;
    private float blindSpotSize;
    float moveForwardIntensity;
    float randomMovementIntensity;
    float frictionIntensity;
    float maxSpeed;
    //Other parameters
    private float cohesionIntensity;
    private float alignmentIntensity;
    private float separationIntensity;
    private float distanceBetweenAgents;
    #endregion

    public LogParameters(float fieldOfViewSize, float blindSpotSize, float moveForwardIntensity, float randomMovementIntensity, float frictionIntensity, float maxSpeed, float cohesionIntensity, float alignmentIntensity, float separationIntensity, float distanceBetweenAgents)
    {
        this.fieldOfViewSize = fieldOfViewSize;
        this.blindSpotSize = blindSpotSize;
        this.moveForwardIntensity = moveForwardIntensity;
        this.randomMovementIntensity = randomMovementIntensity;
        this.frictionIntensity = frictionIntensity;
        this.maxSpeed = maxSpeed;
        this.cohesionIntensity = cohesionIntensity;
        this.alignmentIntensity = alignmentIntensity;
        this.separationIntensity = separationIntensity;
        this.distanceBetweenAgents = distanceBetweenAgents;
    }

    #region Getter

    public float GetFieldOfViewSize()
    {
        return fieldOfViewSize;
    }

    public float GetBlindSpotSize()
    {
        return blindSpotSize;
    }

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

    #endregion

}
