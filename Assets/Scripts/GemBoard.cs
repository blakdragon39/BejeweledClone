using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemBoard : MonoBehaviour {

    [SerializeField] private GameObject orangeHeartPrefab;
    [SerializeField] private GameObject greenGemPrefab;
    [SerializeField] private GameObject purpleGemPrefab;
    [SerializeField] private GameObject blueGemPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject yellowGemPrefab;
    [SerializeField] private GameObject rainbowStarPrefab;

    [SerializeField] private int boardSize;

    private List<GameObject> gemOptions;
    private Gem[][] board;

    private Gem selectedGem;

    private void Start() {
        gemOptions = new List<GameObject> {
            orangeHeartPrefab,
            greenGemPrefab,
            purpleGemPrefab,
            blueGemPrefab,
            diamondPrefab,
            yellowGemPrefab
        };

        board = new Gem[boardSize][];
        for (int i = 0; i < boardSize; i += 1) {
            board[i] = new Gem[boardSize];
        }

        GetComponent<GridLayoutGroup>().constraintCount = boardSize;

        FillBoard();
    }

    private bool checkedForMatches = false;
    
    private void Update() {
        if (!checkedForMatches) {
            var matches = CheckForMatches();
            Debug.Log($"Found {matches.Count} matches");
            foreach (var match in matches) {
                match.SetSpinning(true);
            }
        }

        checkedForMatches = true;
    }

    private void FillBoard() {
        for (int x = 0; x < boardSize; x += 1) {
            for (int y = 0; y < boardSize; y += 1) {
                //todo check previous x and y to make sure they aren't the same as this one
                var gemIndex = Random.Range(0, gemOptions.Count);
                var gemPrefab = gemOptions[gemIndex];
                var prefab = Instantiate(gemPrefab);
                var gem = prefab.GetComponent<Gem>();

                gem.GemType = gemIndex;
                gem.SetBoardPosition(x, y);
                gem.OnGemClick += OnGemClick;
                
                board[x][y] = gem;
                prefab.SetActive(true);
                prefab.transform.SetParent(transform, false);
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

        selectedGem.transform.position = swapGem.transform.position;
        selectedGem.SetBoardPosition(swapGem.X, swapGem.Y);
        board[swapGem.X][swapGem.Y] = selectedGem;

        swapGem.transform.position = tempPos;
        swapGem.SetBoardPosition(tempX, tempY);
        board[tempX][tempY] = swapGem;
        
        selectedGem.SetSpinning(false);
        selectedGem = null;
    }

    private HashSet<Gem> CheckForMatches() {
        var matches = new HashSet<Gem>();
        
        for (int x = 0; x < boardSize; x += 1) {
            for (int y = 0; y < boardSize; y += 1) {
                int horizontalMatches = CheckHorizontalMatches(x, y);

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
                int verticalMatches = CheckVerticalMatches(x, y);

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

    private int CheckHorizontalMatches(int x, int y) {
        if (y >= boardSize - 1) return 0;
        if (board[x][y].GemType != board[x][y + 1].GemType) return 0;
        
        return 1 + CheckHorizontalMatches(x, y + 1);
    }

    private int CheckVerticalMatches(int x, int y) {
        if (x >= boardSize - 1) return 0;
        if (board[x][y].GemType != board[x + 1][y].GemType) return 0;

        return 1 + CheckVerticalMatches(x + 1, y);
    }
}
