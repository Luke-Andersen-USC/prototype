using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy")] 
    [SerializeField] private GameObject _enemyPrefab;
    public List<Transform> EnemySpawns;
    public Transform ParachuteSpawn;

    public GameObject ShipDeck;

    [Header("Spawning Vars")] 
    [SerializeField] private bool _shouldSpawnOnTimer;
    [SerializeField, Range(0f, 10f)] private float _minSpawnTime;
    [SerializeField, Range(0f, 15f)] private float _maxSpawnTime;
    [SerializeField] private float _initialSpawnWait = 5f;

    private float _spawnTimer = 0f;
    private float _timeTillSpawn = 0f;

    public static EnemyManager Instance;
    [HideInInspector] public List<GameObject> Enemies;

    private void Awake()
    {
        Instance = this;
        _timeTillSpawn = _initialSpawnWait;
    }

    private void Update()
    {
        if (_shouldSpawnOnTimer)
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer > _timeTillSpawn)
            {
                _timeTillSpawn = Random.Range(_minSpawnTime, _maxSpawnTime);
                _spawnTimer = 0f;
                SpawnEnemy();
            }
        }
        
    }

    public void SpawnEnemy()
    {
        GameObject enemy = Instantiate(_enemyPrefab, ParachuteSpawn);
        Enemies.Add(enemy);
    }

    public void DestroyEnemy(GameObject enemy)
    {
        Enemies.Remove(enemy);
        Destroy(enemy);
    }

    public void DestroyAllEnemies()
    {
        while (Enemies.Count > 0)
        {
            DestroyEnemy(Enemies[0]);
        }
    }
}