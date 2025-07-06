using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

namespace subrunner.goap
{
    // [GoapId("IdleTargetSensor-c34e9575-d171-4044-9b83-a91a1c32e214")]
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        // 
        public override void Created() { }
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            Vector3? random_position_check = this.GetRandomPositionOnGraph();

            // checks if the position found is valid
            if (!random_position_check.HasValue)
            {
                // return the current target if we have one
                if (existingTarget is PositionTarget) { return existingTarget as PositionTarget; }

                // if no random position is found, return null
                return null;
            }

            // the position is valid, we set the z as 0 for 2D gameplay
            Vector3 random_position = random_position_check.Value;
            random_position.z = 0;

            // Debug.Log($"(IdleTargetSensor) {agent} senses a new position target at {random_position}");

            // and we return the position as a PositionTarget
            if (existingTarget is PositionTarget existingTargetPosition)
            {
                existingTargetPosition.SetPosition(random_position);
                return existingTarget;
            }
            
            return new PositionTarget(random_position);
        }

        private Vector3? GetRandomPositionOnGraph()
        {
            // pick a random walkable node on the current grid graph and returns its position
            GridGraph gridGraph = AstarPath.active.data.gridGraph;
            int randomIndex;
            GridNode randomNode;

            for (int i = 0; i < 5; i++)
            {
                randomIndex = Random.Range(0, gridGraph.nodes.Length);
                randomNode = gridGraph.nodes[randomIndex];
                if (randomNode.Walkable)
                {
                    return (Vector3)randomNode.position;
                }
            }

            Debug.LogWarning("(IdleTargetSensor) No walkable node found");
            return null;
        }
    }
}