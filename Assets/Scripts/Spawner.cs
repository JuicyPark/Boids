using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    GameObject _fishPrefab;

    [Range(0, 300), SerializeField]
    int _count;

    [SerializeField]
    float _spawnRadius = 10f;

    void Start()
    {
        for (int i = 0; i < _count; i++)
        {
            Vector2 spawnPosition = (Vector2)transform.position + Random.insideUnitCircle * _spawnRadius;
            var boid = Instantiate(_fishPrefab);
            boid.transform.position = spawnPosition;
        }
    }
}