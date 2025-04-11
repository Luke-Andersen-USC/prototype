using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [Header("Player")] 
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<Transform> _playerSpawns;

    public GameObject ShipDeck;
    public GameObject BalloonsPrefab;
    
    public static PlayerManager Instance;
    [HideInInspector] public List<GameObject> Players;
    public int _sharedSpareParts = 0;
    [SerializeField] private AudioClip collectScrapSound;
    [SerializeField] private float _collectScrapVol = 1f;
    [SerializeField] private float _showSparePartsChangeDuration = 2f;

    private AudioSource _audioSource;
    
    private void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
        
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            GameObject player = Instantiate(_playerPrefab, _playerSpawns[i]);
            player.transform.SetParent(transform);

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetupInput(Gamepad.all[i]);
                pc.SetupUI(i);
            }
            
            Players.Add(player);
            
            Debug.Log("Adding player: " + i);
        }

        if (Gamepad.all.Count == 0)
        {
            GameObject player = Instantiate(_playerPrefab, _playerSpawns[0]);
            player.transform.SetParent(transform);

            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SetupInput(Keyboard.current);
                pc.SetupUI(0);
            }
            
            Players.Add(player);
            
            Debug.LogWarning("No controller input detected- adding keyboard player");
        }
    }

    public void AddSpareParts(int amount)
    {
        if (amount > 0)
        {
            _audioSource.PlayOneShot(collectScrapSound, _collectScrapVol);
        }
        _sharedSpareParts += amount;
        StartCoroutine(ShipController.Instance.ShowSparePartsChange(_showSparePartsChangeDuration, amount));
    }
    
    public bool HasSpareParts()
    {
        return _sharedSpareParts > 0;
    }
}