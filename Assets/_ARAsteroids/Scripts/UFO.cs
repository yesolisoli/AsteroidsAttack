using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class UFO : MonoBehaviour
{
    public enum UFOStates
    {
        Idle,
        Attacking
    }

    [SerializeField] private UFOStates _currentState;
    [SerializeField] private List<Vector3> _trajectoryVectors = new List<Vector3>();
    [SerializeField] private int _trajectoriesPerSpawn = 2;
    [SerializeField] private float _spawnDistanceFromPlayer = 20;
    [SerializeField] private float _xyOffset = 10;
    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private int _cooldownMinTime = 5;
    [SerializeField] private int _cooldownMaxTime = 15;

    [SerializeField] private GameState _gameState;

    [SerializeField] private UnityEvent OnStartAttacking;
    [SerializeField] private UnityEvent OnStopAttacking;

    private Transform _player;

    [SerializeField] private UnityEvent OnDie;

    public void Die()
    {
        OnDie?.Invoke();
        OnStopAttacking?.Invoke();

        StopAllCoroutines();
        StartCooldown();
    }

    private Vector3 GetNewPositionVector()
    {
        // Generate a random X coordinate within the specified range
        float randomX = Random.Range(-_xyOffset, _xyOffset);

        // Generate a random Y coordinate within the specified range
        float randomY = Random.Range(-_xyOffset, _xyOffset);

        // Get the player's current position and add the specified distance in the Z direction
        Vector3 playerPosition = transform.position;
        float newZ = playerPosition.z + _spawnDistanceFromPlayer;

        // Return a new Vector3 with the random X and Y coordinates, and the calculated Z coordinate
        return new Vector3(randomX, randomY, newZ);
    }

    public void StartAttacking()
    {
        // Check if the player is available
        if (_player == null) return;

        // Define the new spawn position
        Vector3 spawnPosition = GetNewPositionVector();
        transform.position = spawnPosition;

        // Define new random trajectory vectors
        for (int i = 0; i < _trajectoriesPerSpawn; i++)
        {
            _trajectoryVectors.Add(GetNewPositionVector());
        }

        StartCoroutine(AttackMovement());
    }

    IEnumerator AttackMovement()
    {
        for (int i = 0; i < _trajectoryVectors.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, _trajectoryVectors[i]);

            while (distance > 0.5f && !_gameState.GameOver)
            {
                // wait a frame
                yield return null;

                transform.position = Vector3.MoveTowards(transform.position, _trajectoryVectors[i], Time.deltaTime * _movementSpeed);

                distance = Vector3.Distance(transform.position, _trajectoryVectors[i]);
            }
        }

        CurrentState = UFOStates.Idle;
    }

    private IEnumerator IdleRoutine()
    {
        transform.position = new Vector3(1000, 1000, 1000);

        _trajectoryVectors.Clear();

        yield return new WaitForSeconds(Random.Range(_cooldownMinTime, _cooldownMaxTime));

        CurrentState = UFOStates.Attacking;
    }

    public void StartCooldown()
    {
        StartCoroutine(IdleRoutine());
    }

    public UFOStates CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            if (_currentState == UFOStates.Attacking)
            {
                OnStartAttacking?.Invoke();
            }
            else
            {
                OnStopAttacking?.Invoke();
            }
        }
    }

    private void Start()
    {

        // find the player in the scene.
        GameObject playerObject = GameObject.Find("Player");

        // If the Player is found,then grab its transform object.
        if (playerObject)
        {
            _player = playerObject.transform;
        }

        // Players should start in this state.
        CurrentState = UFOStates.Idle;
    }

}