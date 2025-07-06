using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

namespace subrunner.goap
{
    // Defining a GoapId is only necessary when using the ScriptableObject configuration method.
    [GoapId("IdleTargetSensor-c34e9575-d171-4044-9b83-a91a1c32e214")]
    public class IdleTargetSensor : LocalTargetSensorBase
    {
        // private static readonly Bounds Bounds = new(Vector3.zero, new Vector3(50, 50, 0));

        // Is called when this script is initialzed
        public override void Created() { }

        // Is called every frame that an agent of an `AgentType` that uses this sensor needs it.
        // This can be used to 'cache' data that is used in the `Sense` method.
        // Eg look up all the trees in the scene, and then find the closest one in the Sense method.
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            Vector3 random_position = this.GetRandomPositionOnGraph();
            random_position.z = 0; // Ensure the z-coordinate is zero for 2D gameplay
            // Debug.Log($"(IdleTargetSensor) {agent} senses a new position target at {random_position}");

            if (existingTarget is PositionTarget existingTargetPosition)
            {
                // If an existing target is provided, update its position
                existingTargetPosition.SetPosition(random_position);
                return existingTarget;
            }
            
            return new PositionTarget(random_position);
        }

        private Vector3 GetRandomPositionOnGraph()
        {
            // pick a random walkable node on the current grid graph and returns its position
            GridGraph gridGraph = AstarPath.active.data.gridGraph;
            int randomIndex;
            GridNode randomNode;

            for (int i = 0; i < gridGraph.nodes.Length; i++)
            {
                randomIndex = Random.Range(0, gridGraph.nodes.Length);
                randomNode = gridGraph.nodes[randomIndex];
                if (randomNode.Walkable)
                {
                    return (Vector3)randomNode.position;
                }
            }

            Debug.LogWarning("(IdleTargetSensor) No walkable node found, returning the first node's position as a fallback.");
            return (Vector3)gridGraph.nodes[0].position;
        }
    }
}