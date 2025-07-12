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

        [Header("GoTo Behaviour")]
        private ITarget target;
        private AgentBehaviour agent;
        private WalkCapacity walker;
        private Seeker seeker;
        protected IA ia;

        [Header("Pathfinding")]
        [SerializeField] private bool use_navmesh = false; // if true, use NavMesh for pathfinding, otherwise use A* Pathfinding
        [SerializeField] private List<Vector3> path; // the current path

        [Header("TTCBAS")]
        [SerializeField] private float avoidance_predisposition = 0.5f; // the predisposition to avoid other agents, between 0 and 1
        [SerializeField] private float ttc_treshold = 3f; // the time to collision threshold, used to avoid other agents
        [SerializeField] private float neighbour_radius = 2f; // the radius to find nearby agents
        private Vector2 avoidance_force;
        private List<Movable> nearby_agents = new List<Movable>(); // list of nearby agents
        private Vector2 waypoint_movement;

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
        [SerializeField] private bool log_ttcbas = false;
        [SerializeField] private bool gizmo_ttcbas = true; // time to collision based avoidance system
        // PROPERTIES
        public bool HasTarget => target != null; // check if we have a target to go to


        // AWAKE & START
        private void Awake()
        {
            agent = transform.parent.GetComponent<AgentBehaviour>();
            ia = transform.parent.parent.GetComponent<IA>();
            seeker = GetComponent<Seeker>();
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
                if (NavMesh.CalculatePath(transform.position, target.Position, NavMesh.AllAreas, navmesh_path))
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

            // we calculate the avoidance force
            List<Movable> neighbours = find_neighbours();
            avoidance_force = calculate_avoidance_force(neighbours);
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

            string log_msg = $"(GoToBehaviour) {ia.name} is moving along path at waypoint n°{current_waypoint}"
                + $"\n\nmovement:"
                + $"\n\torientation: {ia.Orientation}"
                + $"\n\twaypoint movement: {waypoint_movement} (magnitude: {waypoint_movement.magnitude})"
                + $"\n\tglobal movement: {global_movement} (magnitude: {global_movement.magnitude})"
                + $"\n\nspeed:"
                + $"\n\twalk_speed: {walker.walk_speed}"
                + $"\n\twalk_speed_target: {walker.walk_percentage_target * walker.max_speed}"
                + $"\n\navoidance:"
                + $"\n\tneighbours: {neighbours.Count}"
                + $"\n\tavoidance magnitude: {avoidance_force.magnitude}"
                + $"\n\tavoidance magnitude with predisposition: {avoidance_force.magnitude * avoidance_predisposition}"
                + $"\n\tangle between orientation and avoidance: {Vector2.Angle(ia.Orientation, avoidance_force)}°"
                ;
            if (log_ttcbas) { Debug.Log(log_msg); }
        }


        // TTCBAS (time to collision based avoidance system)
        private List<Movable> find_neighbours()
        {
            nearby_agents = new List<Movable>();

            // checks if we have a target we want to collide with it so we don't count it as a neighbour
            Movable target_movable = null;
            if (target != null && target is TransformTarget transformTarget)
            {
                // we get the target's movable
                Movable targetTransform = transformTarget.Transform.GetComponent<Movable>();
                if (targetTransform != null) { target_movable = targetTransform; }
            }

            // we find all the nearby agents
            // todo upgrade this bcz for now we do an overlap but it is not efficient
            Collider2D[] results = Physics2D.OverlapCircleAll(ia.transform.position, neighbour_radius, LayerMask.GetMask("Feet"));
            if (results.Length == 0) { return new List<Movable>(); }

            // we try to find their movable
            foreach (Collider2D collider in results)
            {
                Movable movable = collider.transform.parent.GetComponent<Movable>();
                if (movable == null || movable == ia || movable == target_movable) { continue; }

                nearby_agents.Add(movable);
            }

            if (log_ttcbas) { Debug.Log($"(GoToBehaviour) {ia.name} found {nearby_agents.Count} nearby agents for TTCBAS."); }

            return nearby_agents;
        }
        private Vector2 calculate_avoidance_force(List<Movable> agents)
        {
            if (agents.Count == 0) { return Vector2.zero; } // no agents to avoid

            Vector2 frame_avoidance_force = Vector2.zero;

            // we loop through all the agents
            foreach (Movable other_agent in agents)
            {
                // estimate ttc with the agent
                float ttc = estimate_ttc(other_agent);
                if (ttc == float.MaxValue) { continue; } // collision is not going to happen

                // get the avoidance direction x[i] + v[i]*t – x[j] - v[j]*t
                Vector2 avoidance_direction = ((Vector2)ia.transform.position)
                                            + ia.Velocity * ttc
                                            - ((Vector2)other_agent.transform.position)
                                            - other_agent.Velocity * ttc;
                avoidance_direction.Normalize();

                // get the avoidance magnitude
                float avoidance_magnitude = 0;
                if (ttc >= 0f && ttc <= ttc_treshold)
                {
                    avoidance_magnitude = (ttc_treshold - ttc) / ttc + 0.001f;
                }

                // we clamp the avoidance magnitude for it not to be infinite
                avoidance_magnitude = Mathf.Clamp(avoidance_magnitude, 0f, 2f);

                // si c un item we divide per 2
                if (other_agent is Item item) { avoidance_magnitude /= 2f; }

                if (log_ttcbas) { Debug.Log($"(GoToBehaviour) {ia.name} calculated avoidance for agent {other_agent.name} with ttc {ttc}, direction {avoidance_direction}, magnitude {avoidance_magnitude}"); }
                
                // we add it to the global avoidance vector
                frame_avoidance_force += avoidance_direction * avoidance_magnitude;
            }

            if (log_ttcbas) { Debug.Log($"(GoToBehaviour) {ia.name} calculated global avoidance force: {frame_avoidance_force} of magnitude {frame_avoidance_force.magnitude}"); }

            return frame_avoidance_force;
        }
        private float estimate_ttc(Movable other_agent)
        {
            float r = ia.feet_radius + other_agent.feet_radius;
            Vector2 w = other_agent.transform.position - ia.transform.position; // vector between the two agents
            float c = Vector2.Dot(w, w) - r * r; // squared distance between the two agents minus the squared radius
            if (c < 0) // agents are colliding
            {
                return 0f; // collision is immediate
            }

            // else no immediate collision

            Vector2 v = other_agent.Velocity - ia.Velocity; // relative velocity
            float a = Vector2.Dot(v, v); // squared speed of the relative velocity
            float b = Vector2.Dot(w, v); // dot product of the vector between the
            float discr = b * b - a * c; // discriminant of the quadratic equation
            if (discr <= 0) // no collision in the future
            {
                return float.MaxValue; // no collision
            }

            float ttc = (b - Mathf.Sqrt(discr)) / a; // time to collision
            if (ttc < 0) // collision in the past
            {
                return float.MaxValue; // no collision
            }

            return ttc; // return the time to collision


            /*r = r[i] + r[j]
            w = x[j] – x[i]
            c = dot(w, w) – r* r
            if (c < 0): //agents are colliding
                return 0
            v = v[i] – v[j]
            a = dot(v, v)
            b = dot(w, v)
            discr = b * b – a* c
            if (discr <= 0):
                return INFTY
            tau = (b – sqrt(discr)) / a
            if (tau < 0):
                return INFTY
            return tau*/
        }




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
