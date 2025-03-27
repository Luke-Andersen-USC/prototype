using System;
using UnityEngine;

namespace Scarecrow
{
    public class BalloonPhysics : MonoBehaviour
    {
        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            
            // Get the other object involved in the collision
            Rigidbody otherRb = collision.rigidbody;
            if (otherRb == null) return; // Ensure the other object has a Rigidbody

            // Get masses
            float m1 = _rb.mass;
            float m2 = otherRb.mass;

            // Get velocities
            Vector3 v1 = _rb.linearVelocity;
            Vector3 v2 = otherRb.linearVelocity;

            // Calculate the normal of the collision
            Vector3 collisionNormal = collision.contacts[0].normal;

            // Calculate velocities along the normal
            float v1AlongNormal = Vector3.Dot(v1, collisionNormal);
            float v2AlongNormal = Vector3.Dot(v2, collisionNormal);

            // Only proceed if the objects are moving towards each other
            if (v1AlongNormal - v2AlongNormal <= 0) return;

            // Calculate the new velocities along the normal using the formula for 1D elastic collisions
            float v1Final = ((m1 - m2) / (m1 + m2)) * v1AlongNormal + ((2 * m2) / (m1 + m2)) * v2AlongNormal;
            float v2Final = ((2 * m1) / (m1 + m2)) * v1AlongNormal + ((m2 - m1) / (m1 + m2)) * v2AlongNormal;

            // Convert the scalar normal velocities back to vectors
            Vector3 v1FinalVec = v1 + (v1Final - v1AlongNormal) * collisionNormal;
            Vector3 v2FinalVec = v2 + (v2Final - v2AlongNormal) * collisionNormal;

            // Assign the new velocities
            _rb.linearVelocity = v1FinalVec;
            otherRb.linearVelocity = v2FinalVec;
        }
    }
}
