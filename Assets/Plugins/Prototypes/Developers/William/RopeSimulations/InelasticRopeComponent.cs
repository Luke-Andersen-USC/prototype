using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

namespace Scarecrow
{
    [RequireComponent(typeof(Rope))]
    public class InelasticRopeComponent : MonoBehaviour
    {
        private float _ropeLength;
        private Rigidbody _rb;
        private Vector3 _accelerationVector;

        [SerializeField] private bool _useGravity = true;
        [SerializeField] private Transform _ropeAnchor;
        [SerializeField] private Transform _ropeEnd;
        [SerializeField] private float _acceleration = 0.0f;

        void Start()
        {
            _ropeLength = GetComponent<Rope>().ropeLength;
            if (_ropeEnd == null) Debug.LogError("InelasticRopeComponent: No rope end assigned!");
            if (_ropeAnchor == null) Debug.LogError("InelasticRopeComponent: No anchor assigned!");
            if (!(_rb = _ropeEnd.GetComponent<Rigidbody>())) Debug.LogError("InelasticRopeComponent: RopeEnd needs a rigidbody!");
            if (_useGravity)
            {
                _accelerationVector = Vector3.zero;
            }
            else
            {
                _rb.useGravity = false; 
                _accelerationVector = Vector3.up * _acceleration;
            }
            
        }

        void FixedUpdate()
        {
            EnforceRopeLength();
            _rb.AddForce(_accelerationVector * _rb.mass);
            _rb.linearDamping = 0.2f;
        }

        void EnforceRopeLength()
        {
            Vector3 ropeVector = _ropeEnd.position - _ropeAnchor.position;
            float distance = ropeVector.magnitude;

            // If the rope exceeds the allowed length, correct it
            if (distance > _ropeLength)
            {
                Vector3 correctionDirection = ropeVector.normalized;
                Vector3 correctedPosition = _ropeAnchor.position + correctionDirection * _ropeLength;
                _rb.MovePosition(correctedPosition);

                // Zero out velocity along the rope's direction to prevent snapping motion
                Vector3 velocityAlongRope = Vector3.Project(_rb.linearVelocity, correctionDirection);
                _rb.linearVelocity -= velocityAlongRope;
            }
        }
    }
}