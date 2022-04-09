using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemBoard : MonoBehaviour {

    // [SerializeField] private GameObject orangeHeartPrefab;
    // [SerializeField] private GameObject greenGemPrefab;
    // [SerializeField] private GameObject purpleGemPrefab;
    // [SerializeField] private GameObject blueGemPrefab;
    // [SerializeField] private GameObject diamondPrefab;
    // [SerializeField] private GameObject yellowGemPrefab;
    // [SerializeField] private GameObject rainbowStarPrefab;

    [SerializeField] private int boardSize;
    [SerializeField] private int gemSize;

    private List<GameObject> gemOptions;
    private Gem[][] board;

    private Gem selectedGem;

    private void Start() {
        // gemOptions = new List<GameObject> {
        //     orangeHeartPrefab,
        //     greenGemPrefab,
        //     purpleGemPrefab,
        //     blueGemPrefab,
        //     diamondPrefab,
        //     yellowGemPrefab
        // };

        // board = new Gem[boardSize][];
        // for (int i = 0; i < boardSize; i += 1) {
        //     board[i] = new Gem[boardSize];
        // }

        // FillBoard();
    }
    
    private void FillBoard() {
        for (int x = 0; x < boardSize; x += 1) {
            for (int y = 0; y < boardSize; y += 1) {
                var type = Random.Range(0, gemOptions.Count);
                
                //checking previous gems to make sure we're not generating matches on the initial board
                while ((x >= 2 && board[x - 1][y].GemType == type && board[x - 2][y].GemType == type) ||
                       (y >= 2 && board[x][y - 1].GemType == type && board[x][y-2].GemType == type)) {
                    type = Random.Range(0, gemOptions.Count);
                }
                
                var gemPrefab = gemOptions[type];
                var prefab = Instantiate(gemPrefab);
                prefab.SetActive(true);
                prefab.transform.SetParent(transform, false);
                prefab.transform.position = new Vector3(x * gemSize, -(y * gemSize), -5);
                
                var gem = prefab.GetComponent<Gem>();
                gem.GemType = type;
                gem.SetBoardPosition(x, y);
                gem.OnGemClick += OnGemClick;
                
                board[x][y] = gem;
            }
        }
    }

    private void OnGemClick(int x, int y) {
        var nextSelectedGem = board[x][y];
        
        if (selectedGem == null) {
            selectedGem = nextSelectedGem;
            selectedGem.SetSpinning(true);
        } else if (selectedGem == nextSelectedGem) {
            nextSelectedGem.SetSpinning(false);
            selectedGem = null;
        } else if (selectedGem.X == x + 1 || selectedGem.X == x - 1 || 
                   selectedGem.Y == y + 1 || selectedGem.Y == y - 1) {
            SwapGems(nextSelectedGem);
        } else {
            selectedGem.SetSpinning(false);
            nextSelectedGem.SetSpinning(true);
            selectedGem = nextSelectedGem;
        }
    }

    private void SwapGems(Gem swapGem) {
        var tempPos = selectedGem.transform.position;
        var tempX = selectedGem.X;
        var tempY = selectedGem.Y;

        //todo animate swapping
        selectedGem.transform.position = swapGem.transform.position;
        selectedGem.SetBoardPosition(swapGem.X, swapGem.Y);
        board[swapGem.X][swapGem.Y] = selectedGem;

        swapGem.transform.position = tempPos;
        swapGem.SetBoardPosition(tempX, tempY);
        board[tempX][tempY] = swapGem;
        
        selectedGem.SetSpinning(false);
        selectedGem = null;

        CheckForMatches();
    }

    private void CheckForMatches() {
        var matches = FindMatches();

        foreach (var match in matches) {
            board[match.X][match.Y] = null;
            match.gameObject.SetActive(false);
            Destroy(match.gameObject);
        }
        //destroy each game object
        //move all higher gems downwards
        //create new gems in empty spots
        //check for more cascading matches
    }

    private HashSet<Gem> FindMatches() {
        var matches = new HashSet<Gem>();
        
        for (int x = 0; x < boardSize; x += 1) {
            for (int y = 0; y < boardSize; y += 1) {
                int horizontalMatches = FindHorizontalMatches(x, y);

                if (horizontalMatches > 1) {
                    for (int i = 0; i <= horizontalMatches; i++) {
                        matches.Add(board[x][y + i]);
                    }

                    y += horizontalMatches - 1;
                }
            }
        }
        
        for (int y = 0; y < boardSize; y += 1) {
            for (int x = 0; x < boardSize; x += 1) {
                int verticalMatches = FindVerticalMatches(x, y);

                if (verticalMatches > 1) {
                    for (int i = 0; i <= verticalMatches; i++) {
                        matches.Add(board[x + i][y]);
                    }

                    x += verticalMatches - 1;
                }
            }
        }

        return matches;
    }

    private int FindHorizontalMatches(int x, int y) {
        if (y >= boardSize - 1) return 0;
        if (board[x][y].GemType != board[x][y + 1].GemType) return 0;
        
        return 1 + FindHorizontalMatches(x, y + 1);
    }

    private int FindVerticalMatches(int x, int y) {
        if (x >= boardSize - 1) return 0;
        if (board[x][y].GemType != board[x + 1][y].GemType) return 0;

        return 1 + FindVerticalMatches(x + 1, y);
    }
}
