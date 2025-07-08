using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using Pathfinding;
using UnityEngine;

namespace subrunner.goap
{
    public class GoToBehaviour : MonoBehaviour
    {

        [Header("GoTo Behaviour")]
        [SerializeField] private ITarget target;
        private AgentBehaviour agent;
        private WalkCapacity walker;
        protected IA ia;


        [Header("Pathfinding")]
        [SerializeField] private Vector2 current_waypoint_destination;
        [SerializeField] private Path path; // the current path
        [SerializeField] private int current_waypoint = 0; // the current waypoint on the path
        [SerializeField] private float update_path_interval = 0.5f; // interval to update the pathfinding to the target
        private float waypoint_threshold_distance = 0.25f; // current distance to the waypoint to consider it reached

        [Header("Logs")]
        [SerializeField] private bool log = false;
        [SerializeField] private bool log_path_calculation = false;

        // PROPERTIES
        public bool HasTarget => target != null; // check if we have a target to go to


        // AWAKE & START
        private void Awake()
        {
            agent = GetComponent<AgentBehaviour>();
            ia = transform.parent.GetComponent<IA>();
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

            // we stop moving
            walker.walk_percentage_target = 0f;
        }
        private void OnTargetInRange(ITarget target)
        {
            walker.walk_percentage_target = 0f; // stop moving

            // we reset the path
            path = null;
            CancelInvoke(nameof(CalculatePath));

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
            ia.seeker.StartPath(ia.transform.position, target.Position, OnPathComplete);

            // we stop invoke ourselves
            CancelInvoke(nameof(CalculatePath));
            InvokeRepeating(nameof(CalculatePath), update_path_interval, update_path_interval); // we calculate the path every 0.5 seconds
        }
        public void OnPathComplete(Path path)
        {
            if (path.error)
            {
                if (log_path_calculation) { Debug.LogError("(GoToBehaviour) " + ia.name + " failed to find a path to " + target + ": " + path.errorLog); }
                return;
            }

            // we initialize the path & waypoints variables
            this.path = path;
            current_waypoint = 0;
            current_waypoint_destination = path.vectorPath[0];

            // we start walking
            walker.walk_percentage_target = 1f;
            if (log_path_calculation) { Debug.Log("(GoToBehaviour) " + ia.name + " found a path to target with " + path.vectorPath.Count + " waypoints."); }
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
                if (current_waypoint >= path.vectorPath.Count)
                {
                    // stop moving
                    walker.walk_percentage_target = 0f;
                    if (log) { Debug.Log("(GoToBehaviour) " + ia.name + " has reached the end of the path."); }
                    return;
                }

                // we update the current waypoint destination
                current_waypoint_destination = path.vectorPath[current_waypoint];
            }

            // we calculate the direction of the movement towards the waypoint
            Vector2 direction_to_waypoint = (current_waypoint_destination - (Vector2)ia.transform.position).normalized;
            ia.Orientation = direction_to_waypoint;
        }

        // GIZMOS
        private void OnDrawGizmos()
        {
            if (target == null) { return; }

            Gizmos.DrawLine(ia.transform.position, this.target.Position);
        }
    }
}
