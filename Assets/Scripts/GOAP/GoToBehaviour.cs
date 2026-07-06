using System;
using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

namespace subrunner.goap
{
    public class GoToBehaviour : MonoBehaviour
    {

        // STATIC USEFUL METHODS
        public static int GetNavMeshAgentTypeID(string name)
        {
            for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
            {
                NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index: i);
                if (name == NavMesh.GetSettingsNameFromID(agentTypeID: settings.agentTypeID))
                {
                    return settings.agentTypeID;
                }
            }
            return NavMesh.GetSettingsByIndex(0).agentTypeID; // return default agent type id if not found
        }




        [Header("Components")]
        private ITarget target;
        private AgentBehaviour _agent;
        private AgentBehaviour agent
        {
            get
            {
                if (_agent == null) { _agent = transform.parent.GetComponent<AgentBehaviour>(); }
                return _agent;
            }
        }
        private IA _ia;
        private IA ia
        {
            get
            {
                if (transform.parent == null || transform.parent.parent == null) { return null; }
                if (_ia == null) { _ia = transform.parent.parent.GetComponent<IA>(); }
                return _ia;
            }
        }
        private WalkCapacity _walker;
        private WalkCapacity walker
        {
            get
            {
                if (_walker == null) { _walker = ia.GetCapacity<WalkCapacity>(); }
                return _walker;
            }
        }

        [Header("Pathfinding")]
        private NavMeshQueryFilter? _filter;
        public NavMeshQueryFilter Filter
        {
            get
            {
                if (_filter == null)
                {
                    _filter = new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, agentTypeID = GetNavMeshAgentTypeID("humanoid") };
                }
                return _filter.Value;
            }
        }
        [SerializeField] private List<Vector3> path; // the current path


        [Header("TTCBAS")]
        public AvoidanceData data; // the data for the time to collision based avoidance system
        private Vector2 avoidance_force;
        private List<Movable> nearby_agents = new List<Movable>(); // list of nearby agents
        private Vector2 waypoint_movement = Vector2.zero; // the movement towards the current waypoint


        [Header("Current Path")]
        [SerializeField] private Vector2 current_waypoint_destination;
        private int current_waypoint_index = -1; // the current waypoint on the path
        [SerializeField] private float update_path_interval = 0.5f; // interval to update the pathfinding to the target
        [SerializeField] private float waypoint_threshold_distance = 0.25f; // current distance to the waypoint to consider it reached

        [Header("Logs & gizmos")]
        [SerializeField] private bool log = false;
        [SerializeField] private bool log_path_calculation = false;
        [SerializeField] private bool log_avoidance_force_set = false;
        [SerializeField] private bool gizmo_target = false;
        [SerializeField] private bool gizmo_path = false;
        [SerializeField] private bool gizmo_ttcbas = true; // time to collision based avoidance system
        
        // PROPERTIES
        public bool unloaded { get; private set; } = false;
        public bool HasTarget => target != null; // check if we have a target to go to


        // AWAKE & START
        private void Start()
        {
            // if we are an outsider we manually call Load()
            if (CapableBank.Instance.HasCapable(ia)) { return; }
            Load();
        }
        
        // LOAD / UNLOAD
        public void Load(AvoidanceData avoidance_data = null)
        {
            // subscribing to the events
            agent.Events.OnTargetInRange += on_target_in_range;
            agent.Events.OnTargetChanged += on_target_changed;
            agent.Events.OnTargetLost += on_target_lost;
            agent.Events.OnActionStop += on_action_stop;

            // we load the avoidance data
            if (avoidance_data != null) { data = avoidance_data; }

            unloaded = false;
        }
        public void Unload()
        {
            // unsubscribing to the events
            agent.Events.OnTargetInRange -= on_target_in_range;
            agent.Events.OnTargetChanged -= on_target_changed;
            agent.Events.OnTargetLost -= on_target_lost;
            agent.Events.OnActionStop -= on_action_stop;
            
            // we stop invoking the path calculation
            CancelInvoke(nameof(CalculatePath));

            _ia = null;
            _agent = null;
            _filter = null;
            _walker = null;

            target = null;
            data = null;            
            path = null;
            unloaded = true;
        }


        // TARGET CALLBACKS MANAGEMENT
        private void on_action_stop(IAction action) { on_target_lost(); }
        private void on_target_lost()
        {
            // we stop moving
            target = null;
            stop_following_path();
            CancelInvoke(nameof(CalculatePath));

            // we reset the ttcbas
            reset_ttcbas();

            if (log) { Debug.Log("(GoToBehaviour) " + ia.name + " lost the target and stopped moving."); }
        }
        private void on_target_in_range(ITarget target)
        {
            // we stop moving
            this.target = null;
            stop_following_path();
            CancelInvoke(nameof(CalculatePath));

            // we reset the ttcbas
            reset_ttcbas();

            if (log) { Debug.Log("(GoToBehaviour) " + ia.name + " has done moving to its target"); }
        }
        private void on_target_changed(ITarget target, bool inRange)
        {
            if (log)
            {
                string target_info;
                if (target == null) { target_info = "null"; }
                else if (target is CapableTarget capable_target) { target_info = "capable of " + capable_target.CapableData.id + " (currently positionned at " + capable_target.Position + ")   ---- loaded ? " + (capable_target.Capable != null && capable_target.Capable.Loaded); }
                else if (target is TransformTarget transform_target) { target_info = "transform of " + transform_target.Transform.name + " (currently positionned at " + transform_target.Transform.position + ")"; }
                else { target_info = "position " + target.Position; }
                Debug.Log("(GoToBehaviour) " + ia.name + " target just changed : " + target_info);
            }

            this.target = target;
            CalculatePath();
        }


        // PATH MANAGEMENT
        public void CalculatePath()
        {
            if (target == null)
            {
                if (log) { Debug.LogWarning("(GoToBehaviour) " + ia.name + " has no target to go to."); }
                return;
            }

            // we find a path to follow
            NavMeshPath navmesh_path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, target.Position, Filter, navmesh_path))
            {
                path = new List<Vector3>(navmesh_path.corners);
                start_following_path(path);
            }
            else if (log_path_calculation) { Debug.LogError("(GoToBehaviour) " + ia.name + " failed to find a NavMesh path to " + target.Position); }

            // we properly invoke ourselves repeatedly (for chasing moving target)
            CancelInvoke(nameof(CalculatePath));
            Invoke(nameof(CalculatePath), update_path_interval); // we calculate the path every 0.5 seconds
        }
        private void start_following_path(List<Vector3> path)
        {
            // we initialize the path & waypoints variables
            this.path = path;
            current_waypoint_index = 0;
            current_waypoint_destination = path[0];

            // we start walking
            walker.walk_percentage_target = 1f;
            if (log_path_calculation) { Debug.Log("(GoToBehaviour) " + ia.name + " found a path to target with " + path.Count + " waypoints."); }
        }

        private void stop_following_path()
        {
            path = null;
            current_waypoint_index = -1;
            walker.walk_percentage_target = 0f;
        }
        private void reset_ttcbas()
        {
            avoidance_force = Vector2.zero;
            nearby_agents.Clear();
            waypoint_movement = Vector2.zero;
        }

        // UPDATE
        public void Update()
        {
            if (unloaded) { return; }
            if (agent.IsPaused) { return; }

            // check if we have a path & waypoints
            if (path == null || current_waypoint_index < 0) { return; }

            // on regarde si on est arrivé au prochain point
            if (Vector2.Distance(ia.transform.position, current_waypoint_destination) <= waypoint_threshold_distance)
            {
                // we check if we arrived at the end of the path
                current_waypoint_index++;
                if (current_waypoint_index >= path.Count)
                {
                    // stop moving
                    stop_following_path();
                    if (log) { Debug.Log("(GoToBehaviour) " + ia.name + " has reached the end of the path."); }
                    return;
                }

                // we update the current waypoint destination
                current_waypoint_destination = path[current_waypoint_index];
            }

            // we calculate the direction of the movement towards the waypoint
            waypoint_movement = (current_waypoint_destination - (Vector2)ia.transform.position).normalized;
            Vector2 global_movement = waypoint_movement * walker.speed;

            // we handle the avoidance force if we have one !
            // MovableEngine is responsible for calculating (via Jobs) & setting the avoidance force
            if (avoidance_force.magnitude == 0f)
            {
                ia.Orientation = waypoint_movement;
                walker.walk_percentage_target = 1f; // we walk towards the waypoint
            }
            else
            {
                global_movement += avoidance_force * data.avoidance_predisposition;
                ia.Orientation = global_movement.normalized; // we set the orientation to the direction of the movement

                // and we update the walk_percentage_target
                walker.walk_percentage_target = global_movement.magnitude / walker.max_run_speed; // we set the walk percentage target based on the speed
                walker.walk_percentage_target = Mathf.Clamp(walker.walk_percentage_target, 0f, 1f); // we clamp the walk percentage target between 0 and 1
            }
            // avoidance_force = Vector2.zero; // we reset the avoidance force for the next frame
        }
        public void SetAvoidanceForce(Vector2 force)
        {
            if (log_avoidance_force_set) { Debug.Log($"(GoToBehaviour) {ia.data.id} received an avoidance force of {force}"); }
            avoidance_force = force;
        }

        // GIZMOS
        private void OnDrawGizmos()
        {
            if (ia == null || ia.transform == null || walker == null) { return; }


            // TTCBAS
            if (gizmo_ttcbas)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(ia.transform.position, ia.transform.position + (Vector3)avoidance_force * data.avoidance_predisposition);

                Gizmos.color = Color.white;
                if (waypoint_movement * walker.speed != Vector2.zero)
                {
                    Gizmos.DrawLine(ia.transform.position, ia.transform.position + (Vector3)waypoint_movement * walker.speed);
                }
                else
                {
                    Gizmos.DrawLine(ia.transform.position, ia.transform.position + (Vector3)ia.Orientation);
                }

                Gizmos.color = Color.green;
                Gizmos.DrawLine(ia.transform.position, ia.transform.position + (Vector3)ia.Velocity * Time.deltaTime);
            }

            // PATH
            if (gizmo_path && path != null && path.Count >= 0)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }

            // TARGET
            if (gizmo_target && target != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(ia.transform.position, target.Position);
            }
        }

        // GET STATIC DATA
        public AvoidanceData GetStaticAvoidanceData()
        {
            if (data == null) { return new AvoidanceData(); }
            return new AvoidanceData()
            {
                avoidance_predisposition = data.avoidance_predisposition,
                ttc_treshold = data.ttc_treshold,
                neighbour_radius = data.neighbour_radius
            };
        }
    }

    [Serializable] public class AvoidanceData
    {
        public float avoidance_predisposition = 0.5f; // the predisposition to avoid other agents, between 0 and 1
        public float ttc_treshold = 2f; // the time to collision threshold, used to avoid other agents
        public float neighbour_radius = 2f; // the radius to find nearby agents
    
        public AvoidanceData Duplicate()
        {
            return new AvoidanceData
            {
                avoidance_predisposition = this.avoidance_predisposition,
                ttc_treshold = this.ttc_treshold,
                neighbour_radius = this.neighbour_radius
            };
        }

        public string GetDetails()
        {
            string details = "avoidance data :\n";
            details += $"    - avoidance predisposition : {avoidance_predisposition}\n";
            details += $"    - time to collision threshold : {ttc_treshold}\n";
            details += $"    - neighbour radius : {neighbour_radius}\n";
            return details;
        }
    }
}

