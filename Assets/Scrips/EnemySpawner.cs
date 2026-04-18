using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerSpawn = 1;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Si no se asignaron spawn points, buscar todos los GameObjects con tag "EnemySpawnPoint"
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
            spawnPoints = new Transform[spawnPointObjects.Length];
            for (int i = 0; i < spawnPointObjects.Length; i++)
            {
                spawnPoints[i] = spawnPointObjects[i].transform;
            }
        }
    }

    void Start()
    {
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        // NO destruir enemigos - dejar que hagan respawn por su cuenta
        // Solo limpiar la lista
        spawnedEnemies.Clear();

        // Spawn de nuevos enemigos
        if (enemyPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: Falta enemyPrefab o spawnPoints");
            return;
        }

        foreach (Transform spawnPoint in spawnPoints)
        {
            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                Vector3 spawnPos = spawnPoint.position + Random.insideUnitSphere * 0.5f;
                spawnPos.y = spawnPoint.position.y;

                GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                spawnedEnemies.Add(newEnemy);
            }
        }

        Debug.Log($"EnemySpawner: {spawnedEnemies.Count} enemigos spawneados");
    }
}

