using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSystem : ISystemInterface
{
    public void Start(World world)
    {
        // not adding any flags, so im just leaving this empty
    }

    public void Update(World world, float time = 0, float deltaTime = 0)
    {
        var entities = world.entities;
        Vector2 wind = world.wind;
        const float K = 1f;
        float C;
        
        if (wind != new Vector2(0.0f, 0.0f))
        {
            for (var i = 0; i < entities.flags.Count; i++)
            {
                if (entities.flags[i].HasFlag(EntityFlags.kFlagForce))
                {
                    var forceComponent = entities.forceComponents[i];

                    C = K * entities.collisionComponents[i].radius;

                    Vector2 WindForce = K * C * (world.wind - entities.moveComponents[i].velocity);

                    // F = m * a
                    if (forceComponent.massInverse > 1e-6f)
                        forceComponent.force += WindForce / forceComponent.massInverse;
                    entities.forceComponents[i] = forceComponent;
                }
            }
        }
    }
}