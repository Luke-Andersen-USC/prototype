using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class SparePartsController : MonoBehaviour
{
    PlayerManager _playerManager;
    [SerializeField] private float _playerMagnetDistance;
    [SerializeField] private float _playerMagnetSpeed;
    void Start()
    {
        _playerManager = FindFirstObjectByType<PlayerManager>();
    }
    
    void Update()
    {
        float closestPlayerDistance = _playerMagnetDistance;
        GameObject currentPlayer = null;
        foreach (var player in _playerManager.Players) 
        {
            if(Vector3.Distance(transform.position, player.transform.position) < closestPlayerDistance) 
            {
                currentPlayer = player;
                closestPlayerDistance = Vector3.Distance(transform.position, player.transform.position);
            }
        }

        if (currentPlayer != null)
        {
            transform.position = Vector3.Lerp(transform.position, currentPlayer.transform.position, _playerMagnetSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            _playerManager.AddSpareParts(1);
            Destroy(gameObject);
        }
    }
}
