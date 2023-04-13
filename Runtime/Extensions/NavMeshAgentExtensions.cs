// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using UnityEngine.AI;

namespace DoubleHelix.Crystal
{
    public static class NavMeshAgentExtensions
    {
        public static bool AtDestination(this NavMeshAgent agent)
        {
            // All of these conditions must be true.
            return agent.enabled &&
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance;
        }

        /// <summary>
        /// The agent is stuck if its current speed is less than its maximum speed
        /// multiplied by the threshold.
        /// </summary>
        public static bool IsStuck(this NavMeshAgent agent, float threshold = 0.01f)
        {
            return agent.velocity.magnitude < agent.speed * threshold;
        }
    }
}
