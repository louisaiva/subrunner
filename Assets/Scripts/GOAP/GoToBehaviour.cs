using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

namespace subrunner.goap
{
    // todo : suivre un pattern ECS pour eviter GC Alloc :
    // todo     on met un GOTOManager qui stocke les agents
    // todo     et les agents s'inscrivent sur le manager puis le manager gère les updates
    // todo     -> faut stocker les choses de chaque agent dans un GOTOAgentData ?
    public class GoToBehaviour : MonoBehaviour
    {

        [Header("GoTo Behaviour")]
        private ITarget target;
        private AgentBehaviour agent;
        private WalkCapacity walker;
        private Seeker seeker;
        protected IA ia;

        [Header("Pathfinding")]
        [SerializeField] private string agent_type = "humanoid";
        public NavMeshQueryFilter filter;
        [SerializeField] private bool use_navmesh = false; // if true, use NavMesh for pathfinding, otherwise use A* Pathfinding
        [SerializeField] private List<Vector3> path; // the current path

        [Header("TTCBAS")]
        [SerializeField] private float avoidance_predisposition = 0.5f; // the predisposition to avoid other agents, between 0 and 1
        public float ttc_treshold = 3f; // the time to collision threshold, used to avoid other agents
        public float neighbour_radius = 2f; // the radius to find nearby agents
        private Vector2 avoidance_force;
        private List<Movable> nearby_agents = new List<Movable>(); // list of nearby agents
        private Vector2 waypoint_movement = Vector2.zero; // the movement towards the current waypoint

        [Header("Current Path")]
        [SerializeField] private Vector2 current_waypoint_destination;
        [SerializeField] private int current_waypoint = 0; // the current waypoint on the path
        [SerializeField] private float update_path_interval = 0.5f; // interval to update the pathfinding to the target
        [SerializeField] private float waypoint_threshold_distance = 0.25f; // current distance to the waypoint to consider it reached

        [Header("Logs & gizmos")]
        [SerializeField] private bool log = false;
        [SerializeField] private bool log_path_calculation = false;
        [SerializeField] private bool gizmo_target = false;
        [SerializeField] private bool gizmo_path = false;
        // [SerializeField] private bool log_ttcbas = false;
        [SerializeField] private bool gizmo_ttcbas = true; // time to collision based avoidance system
        // PROPERTIES
        public bool HasTarget => target != null; // check if we have a target to go to


        // AWAKE & START
        private void Awake()
        {
            agent = transform.parent.GetComponent<AgentBehaviour>();
            ia = transform.parent.parent.GetComponent<IA>();
            seeker = GetComponent<Seeker>();

            int agent_type_id = get_navmesh_agent_type_id(agent_type);
            filter = new NavMeshQueryFilter
            {
                agentTypeID = agent_type_id != -1 ? agent_type_id : NavMesh.GetSettingsByIndex(0).agentTypeID, // if the agent type is not found, use the default agent type
                areaMask = NavMesh.AllAreas
            };
        }
        private void Start()
        {
            walker = ia.GetCapacity<WalkCapacity>();
        }


        // ENABLING / DISABLING CALLBACKS
        private void OnEnable()
        {
            // subscribing to the events
            agent.Events.OnTargetInRange += OnTargetInRange;
            agent.Events.OnTargetChanged += OnTargetChanged;
            agent.Events.OnTargetLost += TargetLost;
        }
        private void OnDisable()
        {
            // unsubscribing to the events
            agent.Events.OnTargetInRange -= this.OnTargetInRange;
            agent.Events.OnTargetChanged -= this.OnTargetChanged;
            agent.Events.OnTargetLost -= this.TargetLost;
        }


        // TARGET CALLBACKS MANAGEMENT
        private void TargetLost()
        {
            if (log) { Debug.Log("(GoToBehaviour) " + ia.name + " lost the target and stopped moving."); }

            target = null;

            // we remove the path
            path = null;

            // we reset the ttcbas
            avoidance_force = Vector2.zero;
            nearby_agents.Clear();
            waypoint_movement = Vector2.zero;

            // we stop moving
            walker.walk_percentage_target = 0f;
        }
        private void OnTargetInRange(ITarget target)
        {
            walker.walk_percentage_target = 0f; // stop moving

            // we reset the path
            path = null;
            CancelInvoke(nameof(CalculatePath));

            // we reset the ttcbas
            avoidance_force = Vector2.zero;
            nearby_agents.Clear();
            waypoint_movement = Vector2.zero;

            if (log) { Debug.Log("(GoToBehaviour) " + ia.name + " has done moving to its target"); }
        }
        private void OnTargetChanged(ITarget target, bool inRange)
        {
            if (log) { Debug.Log("(GoToBehaviour) " + ia.name + " target just change !"); }

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
            if (use_navmesh)
            {
                NavMeshPath navmesh_path = new NavMeshPath();
                if (NavMesh.CalculatePath(transform.position, target.Position, filter, navmesh_path))
                {
                    path = new List<Vector3>(navmesh_path.corners);
                    start_following_path(path);
                }
                else if (log_path_calculation) { Debug.LogError("(GoToBehaviour) " + ia.name + " failed to find a NavMesh path to " + target.Position); }
            }
            else { seeker.StartPath(transform.position, target.Position, OnPathComplete); }

            // we properly invoke ourselves repeatedly (for chasing moving target)
            CancelInvoke(nameof(CalculatePath));
            InvokeRepeating(nameof(CalculatePath), update_path_interval, update_path_interval); // we calculate the path every 0.5 seconds
        }
        public void OnPathComplete(Path path)
        {
            if (path.error)
            {
                if (log_path_calculation) { Debug.LogError("(GoToBehaviour) " + ia.name + " failed to find a A*project path to " + target + ": " + path.errorLog); }
                return;
            }

            start_following_path(path.vectorPath);
        }
        private void start_following_path(List<Vector3> path)
        {
            // we initialize the path & waypoints variables
            this.path = path;
            current_waypoint = 0;
            current_waypoint_destination = path[0];

            // we start walking
            walker.walk_percentage_target = 1f;
            if (log_path_calculation) { Debug.Log("(GoToBehaviour) " + ia.name + " found a path to target with " + path.Count + " waypoints."); }
        }
        private int get_navmesh_agent_type_id(string name)
        {
            for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
            {
                NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(index: i);
                if (name == NavMesh.GetSettingsNameFromID(agentTypeID: settings.agentTypeID))
                {
                    return settings.agentTypeID;
                }
            }
            return -1;
        }

        // UPDATE
        public void Update()
        {
            if (agent.IsPaused) { return; }

            // check if we have a path & waypoints
            if (path == null) { return; }

            // on regarde si on est arrivé au prochain point
            if (Vector2.Distance(ia.transform.position, current_waypoint_destination) <= waypoint_threshold_distance)
            {
                // we check if we arrived at the end of the path
                current_waypoint++;
                if (current_waypoint >= path.Count)
                {
                    // stop moving
                    walker.walk_percentage_target = 0f;
                    if (log) { Debug.Log("(GoToBehaviour) " + ia.name + " has reached the end of the path."); }
                    return;
                }

                // we update the current waypoint destination
                current_waypoint_destination = path[current_waypoint];
            }

            // we calculate the direction of the movement towards the waypoint
            waypoint_movement = (current_waypoint_destination - (Vector2)ia.transform.position).normalized;
            Vector2 global_movement = waypoint_movement * walker.walk_speed;

            // we handle the avoidance force if we have one !
            // MovableEngine is responsible for calculating (via Jobs) & setting the avoidance force
            if (avoidance_force.magnitude == 0f)
            {
                ia.Orientation = waypoint_movement;
                walker.walk_percentage_target = 1f; // we walk towards the waypoint
            }
            else
            {
                global_movement += avoidance_force * avoidance_predisposition;
                ia.Orientation = global_movement.normalized; // we set the orientation to the direction of the movement

                // and we update the walk_percentage_target
                walker.walk_percentage_target = global_movement.magnitude / walker.max_speed; // we set the walk percentage target based on the speed
                walker.walk_percentage_target = Mathf.Clamp(walker.walk_percentage_target, 0f, 1f); // we clamp the walk percentage target between 0 and 1
            }
            avoidance_force = Vector2.zero; // we reset the avoidance force for the next frame
        }
        public void SetAvoidanceForce(Vector2 force) { avoidance_force = force;}

        // GIZMOS
        private void OnDrawGizmos()
        {
            if (ia == null || ia.transform == null) { return; }


            // TTCBAS
            if (gizmo_ttcbas)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(ia.transform.position, ia.transform.position + (Vector3)avoidance_force * avoidance_predisposition);

                Gizmos.color = Color.white;
                if (waypoint_movement * walker.walk_speed != Vector2.zero)
                {
                    Gizmos.DrawLine(ia.transform.position, ia.transform.position + (Vector3)waypoint_movement * walker.walk_speed);
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
    }
}

