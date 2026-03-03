using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    Pawn, Knight, Bishop, Rook, Queen, King
}

[Serializable]
public class StockEntry
{
    public PieceType type;
    public GameObject prefab;
    [Min(0)] public int count = 1;
}

[Serializable]
public class PlayerStocks
{
    [Tooltip("Usually 1 = White, 2 = Black.")]
    public int playerId = 1;

    public Transform[] spawnPoints;

    public List<StockEntry> stockList = new();
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Players")]
    [SerializeField] private PlayerStocks _player1 = new PlayerStocks { playerId = 1 };
    [SerializeField] private PlayerStocks _player2 = new PlayerStocks { playerId = 2 };

    [Header("Respawn")]
    [SerializeField] private float _respawnDelay = 0.75f;
    [SerializeField] private bool _spawnNextStockInOrder = true; // if false: pick any remaining stock type

    [Header("Debug")]
    [SerializeField] private bool _autoStartOnPlay = true;

    // Runtime state
    private readonly Dictionary<int, Queue<StockEntry>> _queues = new();
    private readonly Dictionary<int, Dictionary<PieceType, (GameObject prefab, int remaining)>> _remaining = new();
    private readonly Dictionary<int, GameObject> _currentPiece = new();
    private bool _matchEnded;

    public event Action<int> OnPlayerDefeated;
    public event Action<int, PieceType, int> OnStockConsumed;
    public event Action<int, GameObject> OnPieceSpawned;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (_autoStartOnPlay)
        {
            StartMatch();
        }
    }

    public void StartMatch()
    {
        _matchEnded = false;

        BuildPlayerState(_player1);
        BuildPlayerState(_player2);

        SpawnNextForPlayer(_player1.playerId);
        SpawnNextForPlayer(_player2.playerId);
    }

    private void BuildPlayerState(PlayerStocks playerStock)
    {
        Dictionary<PieceType, (GameObject prefab, int remaining)> playerStockDictionary = new Dictionary<PieceType, (GameObject prefab, int remaining)>();
        foreach (StockEntry stock in playerStock.stockList)
        {
            if (stock == null || stock.prefab == null || stock.count <= 0) continue;
            playerStockDictionary[stock.type] = (stock.prefab, stock.count);
        }
        _remaining[playerStock.playerId] = playerStockDictionary;

        Queue<StockEntry> stockQueue = new Queue<StockEntry>();
        if (_spawnNextStockInOrder)
        {
            foreach (StockEntry stock in playerStock.stockList)
            {
                if (stock == null || stock.prefab == null || stock.count <= 0) continue;
                for (int i = 0; i < stock.count; i++)
                {
                    stockQueue.Enqueue(new StockEntry { type = stock.type, prefab = stock.prefab, count = 1 });
                }
            }
        }
        _queues[playerStock.playerId] = stockQueue;
    }

    public void NotifyPieceDied(GameObject pieceGameObject, int ownerId, PieceType pieceType)
    {
        if (_matchEnded) return;

        if (_currentPiece.TryGetValue(ownerId, out var curr) && curr != pieceGameObject) return;

        if (_spawnNextStockInOrder && _queues.TryGetValue(ownerId, out Queue<StockEntry> stockQueue))
        {
            if (stockQueue.Count > 0) stockQueue.Dequeue();

            int totalRemaining = stockQueue.Count;
            OnStockConsumed?.Invoke(ownerId, pieceType, totalRemaining);

            if (totalRemaining <= 0)
            {
                EndMatch(defeatedPlayerId: ownerId);
                return;
            }

            StartCoroutine(RespawnRoutine(ownerId));
            return;
        }

        if (_remaining.TryGetValue(ownerId, out Dictionary<PieceType, (GameObject prefab, int remaining)> remainingPieces) &&
            remainingPieces.TryGetValue(pieceType, out (GameObject prefab, int remaining) data))
        {
            int newRemaining = Mathf.Max(0, data.remaining - 1);
            remainingPieces[pieceType] = (data.prefab, newRemaining);
        }

        int total = GetTotalRemaining(ownerId);
        OnStockConsumed?.Invoke(ownerId, pieceType, total);

        if (total <= 0)
        {
            EndMatch(defeatedPlayerId: ownerId);
            return;
        }

        StartCoroutine(RespawnRoutine(ownerId));
    }

    private IEnumerator RespawnRoutine(int ownerId)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnNextForPlayer(ownerId);
    }

    private void SpawnNextForPlayer(int ownerId)
    {
        if (_matchEnded) return;

        PlayerStocks playerStock = (ownerId == _player1.playerId) ? _player1 : _player2;
        if (playerStock.spawnPoints == null || playerStock.spawnPoints.Length == 0)
        {
            Debug.LogError($"No spawn points set for player {ownerId}.");
            return;
        }

        GameObject prefab = null;
        PieceType chosenType = PieceType.Pawn;

        if (_spawnNextStockInOrder && _queues.TryGetValue(ownerId, out Queue<StockEntry> pieceQueue) && pieceQueue.Count > 0)
        {
            StockEntry next = pieceQueue.Peek();
            prefab = next.prefab;
            chosenType = next.type;
        }
        else
        {
            if (_remaining.TryGetValue(ownerId, out var remaining))
            {
                foreach (var keyValue in remaining)
                {
                    if (keyValue.Value.remaining > 0 && keyValue.Value.prefab != null)
                    {
                        chosenType = keyValue.Key;
                        prefab = keyValue.Value.prefab;
                        break;
                    }
                }
            }
        }

        if (prefab == null)
        {
            EndMatch(defeatedPlayerId: ownerId);
            return;
        }

        Transform spawn = playerStock.spawnPoints[UnityEngine.Random.Range(0, playerStock.spawnPoints.Length)];
        GameObject gameObject = Instantiate(prefab, spawn.position, spawn.rotation);

        OwnerInfo ownerInfo = gameObject.GetComponent<OwnerInfo>();
        if (ownerInfo != null)
        {
            ownerInfo.OwnerID = ownerId;
        }

        _currentPiece[ownerId] = gameObject;
        OnPieceSpawned?.Invoke(ownerId, gameObject);
    }

    private int GetTotalRemaining(int ownerId)
    {
        if (!_remaining.TryGetValue(ownerId, out var remaining)) return 0;

        int sum = 0;
        foreach (KeyValuePair<PieceType, (GameObject prefab, int remaining)> keyValue in remaining)
        {
            sum += keyValue.Value.remaining;
        }

        if (_spawnNextStockInOrder && _queues.TryGetValue(ownerId, out Queue<StockEntry> stockQueue))
        {
            sum = stockQueue.Count;
        }

        return sum;
    }

    private void EndMatch(int defeatedPlayerId)
    {
        if (_matchEnded) return;
        _matchEnded = true;

        OnPlayerDefeated?.Invoke(defeatedPlayerId);

        int winner = (defeatedPlayerId == _player1.playerId) ? _player2.playerId : _player1.playerId;
        Debug.Log($"Match ended. Player {winner} wins (Player {defeatedPlayerId} is out of stocks).");
    }

}
