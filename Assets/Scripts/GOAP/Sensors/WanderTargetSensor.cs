using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using Pathfinding;
using System.Collections.Generic;
using System.Linq;

namespace subrunner.goap
{
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        GridGraph[] graphs;
        public override void Created()
        {
            NavGraph[] allGraphs = AstarPath.active.data.graphs;

            // we filter the graphs to only keep the grid graphs
            graphs = AstarPath.active.data.graphs.Where(g => g is GridGraph).Cast<GridGraph>().ToArray();
        }
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            Vector3 random_position = existingTarget is PositionTarget positionTarget ? positionTarget.Position : Vector3.zero;
            for (int i = 0; i < 5; i++)
            {
                // if the position is not valid, we get a new random position
                random_position = getRandomPositionInRange(agent.Transform.position, 3f);

                // we check if the position is valid
                if (isValidPosition(random_position)) { i = 1000; } // we break the loop if the position is on a graph

                if (i == 4)
                {
                    if (existingTarget is PositionTarget) { return existingTarget as PositionTarget; }
                    return null;
                }
            }

            // the position is valid, we set the z as 0 for 2D gameplay
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

        private Vector2 getRandomPositionInRange(Vector2 center, float range)
        {
            // generates a random position in a circle around the center
            Vector2 randomPosition = Random.insideUnitCircle * range;
            return center + randomPosition;
        }
        private bool isValidPosition(Vector2 position)
        {
            // checks if the position is valid by checking if it's on the grid graph
            foreach (GridGraph graph in graphs)
            {
                if (graph.GetNearest(position, NNConstraint.Default).node != null)
                {
                    return true;
                }
            }

            return false;
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