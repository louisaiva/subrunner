using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using Pathfinding;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

namespace subrunner.goap
{
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        GridGraph[] graphs;
        NavMeshQueryFilter defaultFilter = new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, };
        public override void Created()
        {
            AstarPath astarPath = AstarPath.active;
            if (astarPath == null)
            {
                Debug.LogWarning("(WanderTargetSensor) AstarPath is not active. Make sure the A* Pathfinding Project is set up correctly if you want to use it.");
                return;
            }
            NavGraph[] allGraphs = AstarPath.active.data.graphs;

            // we filter the graphs to only keep the grid graphs
            graphs = AstarPath.active.data.graphs.Where(g => g is GridGraph).Cast<GridGraph>().ToArray();
        }
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            // get the ia & exploration range
            IA ia = references.GetCachedComponentInParent<IA>();

            // find a random position to go
            Vector3 random_position = getRandomPositionInRangeNavMesh(agent.Transform.position, ia.exploration_radius,ia.Mover.filter);
            if (random_position == default)
            {
                if (existingTarget is PositionTarget) { return existingTarget as PositionTarget; }
                return null;
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


        // ASTAR PROJECT
        private Vector3 getRandomPositionInRangeAstar(Vector2 center, float range)
        {
            Vector3 position;
            // gets the agent's current node
            GraphNode agent_node = AstarPath.active.GetNearest(center, NNConstraint.None).node;

            for (int i = 0; i < 5; i++)
            {
                // if the position is not valid, we get a new random position
                GraphNode random_node = getRandomNodeInRange(center, range);

                // we check if the position is reachable
                position = (Vector3)random_node.position;
                if (isReachablePosition(agent_node, random_node)) { return position; }
            }

            return default;
        }
        private GraphNode getRandomNodeInRange(Vector2 center, float range)
        {
            // generates a random position in a circle around the center
            Vector2 randomPosition = Random.insideUnitCircle * range;
            return AstarPath.active.GetNearest(center + randomPosition, NNConstraint.Default).node;
        }

        /// <summary>
        /// To know if a position is on a valid graph & reachable
        /// </summary>
        /// <param name="start_node">starting node (position of the ai you want to test)</param>
        /// <param name="destination">destination node</param>
        /// <returns>
        /// <code>
        /// return true if the position parameter represents a walkable node
        /// and that can be reach from the start_node parameter
        /// return false otherwise
        /// </code></returns>
        private bool isReachablePosition(GraphNode start_node, GraphNode destination)
        {
            // GraphNode destination = AstarPath.active.GetNearest(position, NNConstraint.Default).node;
            if (destination == null) { return false; }
            if (!destination.Walkable) { return false; }
            return PathUtilities.IsPathPossible(start_node, destination);
        }

        /* private Vector3? GetRandomPositionOnGraph()
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
        } */


        // NAV MESH
        private Vector3 getRandomPositionInRangeNavMesh(Vector2 center, float range, NavMeshQueryFilter? filter = null)
        {
            
            int attempts = 10;
            for (int i = 0; i < attempts; i++)
            {
                Vector3 randomPosition = getRandomPositionOnNavMesh(center, range,filter);
                if (randomPosition == default) { continue; }

                // checks if the destination is reachable
                NavMeshPath path = new NavMeshPath();
                if (!NavMesh.CalculatePath(center, randomPosition, filter == null ? defaultFilter : filter.Value, path)) { continue; }

                // on le clamp pour faire en sorte que l'ia ne puisse parcourir que range de distance maximale
                return getMaxDistanceAlongPath(path, range);
            }

            Debug.LogWarning("(IdleTargetSensor) No walkable position found on the nav mesh");
            return default;
        }
        private Vector3 getRandomPositionOnNavMesh(Vector2 center, float range, NavMeshQueryFilter? filter = null)
        {
            // pick a random position on the nav mesh
            Vector3 randomPosition = (Vector3) center + Random.insideUnitSphere * range;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 10f, filter == null ? defaultFilter : filter.Value))
            {
                return hit.position;
            }

            return default;
        }
        private Vector3 getMaxDistanceAlongPath(NavMeshPath path, float max_distance)
        {
            // we go through all corners and calculate the distance
            float distance = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                distance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
                if (distance > max_distance)
                {
                    // we return the position of the corner that is at max_distance
                    return path.corners[i + 1];
                }
            }
            return path.corners.Last();
        }
    }
}