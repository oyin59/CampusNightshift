using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent

namespace AI
{
    /// <summary>
    /// Controls the AI Guard.
    /// Day 4 Goal: Basic Patrol logic.
    /// Day 5 Goal: Advanced AI (Detection, FOV, Raycasts, Chase State).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentController : MonoBehaviour
    {
        // 1. Defining the States our AI can be in
        public enum AIState { Patrolling, Chasing }
        private AIState currentState = AIState.Patrolling;

        [Header("Patrol Settings (Day 4)")]
        [Tooltip("Drag the 4 corner Empty GameObjects into this array.")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointTolerance = 1.0f;

        [Header("Detection Settings (Day 5)")]
        [Tooltip("Direct reference to the Player object.")]
        [SerializeField] private Transform player;
        [Tooltip("How far the guard can see in meters.")]
        [SerializeField] private float viewDistance = 15f;
        [Tooltip("The cone of vision (e.g., 90 means 45 degrees left and right of center).")]
        [Range(0, 360)]
        [SerializeField] private float viewAngle = 90f;
        [Tooltip("The layer mask representing walls so the guard can't see through them.")]
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Speed Settings")]
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float chaseSpeed = 5f;

        private NavMeshAgent agent;
        private int currentWaypointIndex = 0;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            Debug.Assert(waypoints != null && waypoints.Length > 0, "AgentController: No waypoints assigned!");
            
            // Auto-find player if not assigned manually
            if (player == null)
            {
                player = GameObject.Find("Player")?.transform;
                Debug.Assert(player != null, "AgentController: Cannot find object named 'Player' in the scene.");
            }
        }

        private void Start()
        {
            if (waypoints.Length > 0)
            {
                agent.SetDestination(waypoints[0].position);
            }
        }

        private void Update()
        {
            // The FSM (Finite State Machine) Brain
            switch (currentState)
            {
                case AIState.Patrolling:
                    PatrolBehavior();
                    CheckForPlayer(); // Always be looking while patrolling
                    break;
                case AIState.Chasing:
                    ChaseBehavior();
                    CheckForPlayer(); // Keep checking if we lost them
                    break;
            }
        }

        /// <summary>
        /// Day 4: Standard point-to-point movement.
        /// </summary>
        private void PatrolBehavior()
        {
            agent.speed = patrolSpeed;
            if (waypoints.Length == 0) return;

            if (agent.remainingDistance <= waypointTolerance && !agent.pathPending)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length) currentWaypointIndex = 0;
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }

        /// <summary>
        /// Day 5: Abandon patrol and run straight at the player's current coordinate.
        /// </summary>
        private void ChaseBehavior()
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }

        /// <summary>
        /// Day 5: The mathematically driven detection system (Distance + Angle + Raycast).
        /// </summary>
        private void CheckForPlayer()
        {
            if (player == null) return;

            // Define how far/wide the guard can see based on if he's ALREADY chasing you
            float currentViewDistance = (currentState == AIState.Chasing) ? viewDistance * 1.5f : viewDistance;
            float currentViewAngle = (currentState == AIState.Chasing) ? 360f : viewAngle; // 360 means he can "hear" you running behind him during a chase

            // 1. DISTANCE: Is the player close enough?
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= currentViewDistance)
            {
                // 2. ANGLE (Field of Vision)
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

                if (angleToPlayer < currentViewAngle / 2f)
                {
                    // 3. LINE OF SIGHT: Did it hit a wall?
                    Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
                    Vector3 targetPos = player.position + Vector3.up * 1.0f;
                    
                    if (!Physics.Linecast(eyePosition, targetPos, obstacleLayer))
                    {
                        // Guard sees player! Change states.
                        if (currentState != AIState.Chasing)
                        {
                            Debug.Log("Guard has spotted the player! Initiating Chase.");
                            currentState = AIState.Chasing;
                        }
                        return; // Exit method because we successfully spotted them this frame
                    }
                }
            }

            // If we get down here, we failed one of the 3 checks (too far, outside cone, or behind wall)
            if (currentState == AIState.Chasing)
            {
                // We lost the player! Go back to patrolling.
                Debug.Log("Guard lost track of the player. Resuming patrol.");
                currentState = AIState.Patrolling;
                
                // Immediately pick up the patrol where we left off
                if (waypoints.Length > 0)
                {
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                }
            }
        }

        /// <summary>
        /// Day 7: Game Over Condition
        /// If the guard physically touches the player, the game is over.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Systems.GameManager manager = FindObjectOfType<Systems.GameManager>();
                if (manager != null)
                {
                    manager.PlayerCaught();
                }
            }
        }

        /// <summary>
        /// Helpful visualizer so you can see the View Distance in the Editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewDistance);
        }
    }
}
