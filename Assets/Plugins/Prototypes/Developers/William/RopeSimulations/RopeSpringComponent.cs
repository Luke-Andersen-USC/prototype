using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

namespace Scarecrow
{
    [RequireComponent(typeof(Rope))]
    public class RopeSpringComponent : MonoBehaviour
    {
        private float _ropeLength;
        private Rigidbody _rb;

        [SerializeField] private Transform _ropeAnchor;
        [SerializeField] private Transform _ropeEnd;
        [SerializeField] private float springConstant = 0.5f;
        
        
        [SerializeField, Range(0f, 1f)] private float springDampening = 0.5f;

        void Start()
        {
            _ropeLength = GetComponent<Rope>().ropeLength;
            if (_ropeEnd == null) Debug.LogError("RopeSpringComponent: No rope end assigned!");
            if (_ropeAnchor == null) Debug.LogError("RopeSpringComponent: No anchor assigned!");
            if (!(_rb = _ropeEnd.GetComponent<Rigidbody>())) Debug.LogError("RopeSpringComponent: RopeEnd needs a rigidbody!");
            _rb.linearDamping = springDampening;
        }
        
        void Update()
        {
            _rb.AddForce(CalculateRopeForce());
        }

        Vector3 CalculateRopeForce()
        {
            Vector3 ropeVector = _ropeEnd.position - _ropeAnchor.position;
            float distance = ropeVector.magnitude;
            if (distance <= _ropeLength) return Vector3.zero;
            float dx = distance - _ropeLength * _ropeAnchor.localScale.x;

            float springForce = dx * springConstant;
            Vector3 springForceVector = -ropeVector.normalized * springForce;
            
            return springForceVector;
        }
    }
}
