using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct CalculateAvoidanceForceJob : IJob
{
    [ReadOnly] public MovableStruct movable;
    [ReadOnly] public NativeList<MovableStruct> neighbours;
    [ReadOnly] public float ttc_treshold;
    public NativeArray<float2> avoidanceForces;
 
    public void Execute()
    {
        // reset values
        avoidanceForces[0] = float2.zero;
        if (neighbours.Length == 0) { return; } // no agents to avoid

        // we loop through all the agents
        for (int i = 0; i < neighbours.Length; i++)
        {
            MovableStruct other_agent = neighbours[i];

            // estimate ttc with the agent
            float ttc = estimate_ttc(other_agent);
            if (ttc > ttc_treshold) { continue; } // collision is not going to happen

            // Debug.Log($"TTC between current movable ({movable.position}) and neighbour n°{i} ({other_agent.position}) is {ttc} !!");

            // get the avoidance direction x[i] + v[i]*t – x[j] - v[j]*t
            float2 avoidance_direction = movable.position
                                        + movable.velocity * ttc
                                        - other_agent.position
                                        - other_agent.velocity * ttc;
            avoidance_direction = math.normalizesafe(avoidance_direction);

            // get the avoidance magnitude
            float avoidance_magnitude = 0;
            if (ttc <= 0.01f) // agents are colliding
            {
                avoidance_magnitude = 2f; // max avoidance
            }
            else if (ttc > 0.01f && ttc <= ttc_treshold)
            {
                avoidance_magnitude = (ttc_treshold - ttc) / (ttc + 0.01f + 0.001f);
            }

            // we clamp the avoidance magnitude for it not to be infinite
            avoidance_magnitude = math.clamp(avoidance_magnitude, 0f, 2f);

            // si c un item we divide par 2
            if (other_agent.is_item) { avoidance_magnitude /= 2f; }

            // we add it to the global avoidance vector
            avoidanceForces[0] += avoidance_direction * avoidance_magnitude;

            // Debug.Log($"avoidance force of {movable.position} is now {avoidanceForces[0]} [ against {other_agent.position} we have a force {avoidance_direction * avoidance_magnitude} (dir: {avoidance_direction}, mag: {avoidance_magnitude}) ]");
        }
    }
    private float estimate_ttc(MovableStruct other_agent)
    {
        float r = movable.feet_radius + other_agent.feet_radius;
        float2 w = other_agent.position - movable.position;
        float c = math.dot(w, w) - r * r; // squared distance between the two agents minus the squared radius
        if (c < 0f) // agents are colliding
        {
            return 0f; // collision is immediate
        }

        // else no immediate collision
        float2 v = other_agent.velocity - movable.velocity; // relative velocity
        float a = math.dot(v, v); // squared speed of the relative velocity
        if (a < 0.0001f) { return float.MaxValue; } // agents are not moving relative to each other

        float b = math.dot(w, v); // dot product of the vector between the
        float discr = b * b - a * c; // discriminant of the quadratic equation
        if (discr <= 0f) // no collision in the future
        {
            return float.MaxValue; // no collision
        }

        float ttc = (b - math.sqrt(discr)) / a; // time to collision
        if (ttc < 0f) // collision in the past
        {
            return float.MaxValue; // no collision
        }

        return ttc; // return the time to collision
    }
}

public struct MovableStruct
{
    public int id;
    public float2 position;
    public float2 velocity;
    public float feet_radius;
    public bool is_item;
}