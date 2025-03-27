using UnityEngine;

namespace Scarecrow
{
    public class SquirrelCanon : MonoBehaviour
    {
        public Transform player;
        public LayerMask whatIsPlayer;
        public float sightRange;
        public float timeBetweenAttacks;
        public bool playerInAttackRange;
        public bool alreadyAttacked;
        public GameObject projectile;
        public Transform spawnPoint;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            player = GameObject.Find("Rig").transform;
        }

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            playerInAttackRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

            if(playerInAttackRange)
            {
                AttackPlayer();
            }
            else
            {

            }
        }

        private void AttackPlayer()
        {
            transform.LookAt(player);

            if(!alreadyAttacked)
            {
                Rigidbody rb = Instantiate(projectile, spawnPoint.transform.position, Quaternion.identity).GetComponent<Rigidbody>();
                rb.AddForce(transform.forward * 64f, ForceMode.Impulse);

                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }

        private void ResetAttack()
        {
            alreadyAttacked = false;
        }
    }
}
