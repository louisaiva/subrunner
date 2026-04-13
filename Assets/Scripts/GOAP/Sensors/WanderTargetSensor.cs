using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

namespace subrunner.goap
{
    public class WanderTargetSensor : LocalTargetSensorBase, LoadedSensor
    {
        public override void Created() {}
        public override void Update() { }

        // SENSE
        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            // get the ia & exploration range
            IA ia = references.GetCachedComponentInParent<IA>();
            if (ia.JustLoaded)
            {
                // we check that we have a valid target, if yes we return it (it was loaded when the MotorCapacity loaded the MotorData' local world data)
                if (existingTarget != null && existingTarget is PositionTarget)
                {
                    if (Logger.Instance.LOG_WANDER_TARGET_SENSOR) { Debug.Log($"(WanderLoadedSensor - Sense) {ia.data.id} just loaded and has an existing target : {existingTarget}. We keep it."); }
                    return existingTarget;
                }
            }

            /* if (Logger.Instance.LOG_WANDER_TARGET_SENSOR)
            {
                Debug.Log($"(WanderLoadedSensor - Sense) {agent} is sensing a new wander target for IA {ia.name}");
            } */

            // find a random position to go
            Vector3 random_position = getRandomPositionInRangeNavMesh(agent.Transform.position, ia.SocialData.exploration_radius, ia.Mover.Filter);
            if (random_position == default)
            {
                if (Logger.Instance.LOG_WANDER_TARGET_SENSOR) { Debug.LogWarning("(WanderLoadedSensor - Sense) No walkable position found on the nav mesh for : " + ia.name); }
                if (existingTarget is PositionTarget) { return existingTarget; }
                return null;
            }

            // the position is valid, we set the z as 0 for 2D gameplay
            random_position.z = 0;

            if (Logger.Instance.LOG_WANDER_TARGET_SENSOR) { Debug.Log($"(WanderLoadedSensor - Sense) {agent} senses a new position target at {random_position}"); }

            // and we return the position as a PositionTarget
            if (existingTarget is PositionTarget existingTargetPosition)
            {
                existingTargetPosition.SetPosition(random_position);
                return existingTargetPosition;
            }

            return new PositionTarget(random_position);
        }

        // NAV MESH
        NavMeshQueryFilter defaultFilter = new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, };
        private Vector3 getRandomPositionInRangeNavMesh(Vector2 center, float range, NavMeshQueryFilter? filter = null)
        {
            int attempts = 10;
            for (int i = 0; i < attempts; i++)
            {
                Vector3 randomPosition = getRandomPositionOnNavMesh(center, range, filter);
                if (randomPosition == default) { continue; }

                // checks if the destination is reachable
                NavMeshPath path = new NavMeshPath();
                if (!NavMesh.CalculatePath(center, randomPosition, filter == null ? defaultFilter : filter.Value, path)) { continue; }

                // on le clamp pour faire en sorte que l'ia ne puisse parcourir que range de distance maximale
                return getMaxDistanceAlongPath(path, range);
            }

            return default;
        }
        private Vector3 getRandomPositionOnNavMesh(Vector2 center, float range, NavMeshQueryFilter? filter = null)
        {
            // pick a random position on the nav mesh
            Vector3 randomPosition = (Vector3)center + Random.insideUnitSphere * range;

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