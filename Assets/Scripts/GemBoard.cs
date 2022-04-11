using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class GemBoard : MonoBehaviour {

    [SerializeField] private GameObject blueGemPrefab;
    [SerializeField] private GameObject diamondPrefab;
    [SerializeField] private GameObject greenGemPrefab;
    [SerializeField] private GameObject orangeHeartPrefab;
    [SerializeField] private GameObject purpleGemPrefab;
    [SerializeField] private GameObject redGemPrefab;
    [SerializeField] private GameObject yellowGemPrefab;
    [SerializeField] private GameObject rainbowStarPrefab;

    [SerializeField] private int boardSize;
    [SerializeField] private int gemSize;
    [SerializeField] private float swapSpeed;

    private List<GameObject> gemOptions;
    private Gem[][] board;

    private Gem selectedGem;

    private void Start() {
        gemOptions = new List<GameObject> {
            blueGemPrefab,
            diamondPrefab,
            greenGemPrefab,
            orangeHeartPrefab,
            purpleGemPrefab,
            redGemPrefab,
            yellowGemPrefab
        };

        board = new Gem[boardSize][];
        for (int i = 0; i < boardSize; i += 1) {
            board[i] = new Gem[boardSize];
        }

        FillBoard();
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

                var newGem = GenerateNewGem(type, x, y);
                newGem.gameObject.transform.localPosition = new Vector3(x * gemSize, -(y * gemSize), -5);
            }
        }

        GetComponent<RectTransform>().sizeDelta = new Vector2(boardSize * gemSize, boardSize * gemSize);
    }
    
    private Gem GenerateNewGem(int type, int x, int y) {
        var prefab = Instantiate(gemOptions[type]);
        var gem = prefab.GetComponent<Gem>();
        
        gem.gameObject.SetActive(true);
        gem.gameObject.transform.SetParent(transform, false);
        gem.GemType = type;
        gem.OnGemClick += OnGemClick;
        
        SetGemBoardPosition(gem, x, y);

        return gem;
    }

    private void SetGemBoardPosition(Gem gem, int x, int y) {
        gem.SetBoardPosition(x, y);
        board[x][y] = gem;
    }

    private void OnGemClick(int x, int y) {
        //todo diagonal not working
        var nextSelectedGem = board[x][y];
        
        if (selectedGem == null) {
            selectedGem = nextSelectedGem;
            selectedGem.SetSpinning(true);
        } else if (selectedGem == nextSelectedGem) {
            nextSelectedGem.SetSpinning(false);
            selectedGem = null;
        } else if (selectedGem.X == x + 1 || selectedGem.X == x - 1 || 
                   selectedGem.Y == y + 1 || selectedGem.Y == y - 1) {
            StartCoroutine(SwapGems(nextSelectedGem));
        } else {
            selectedGem.SetSpinning(false);
            nextSelectedGem.SetSpinning(true);
            selectedGem = nextSelectedGem;
        }
    }

    private IEnumerator SwapGems(Gem swapGem) {
        var tempPos = selectedGem.transform.localPosition;
        var tempX = selectedGem.X;
        var tempY = selectedGem.Y;

        var tween = selectedGem.transform.DOLocalMove(swapGem.transform.localPosition, swapSpeed);
        selectedGem.SetBoardPosition(swapGem.X, swapGem.Y);
        board[swapGem.X][swapGem.Y] = selectedGem;

        swapGem.transform.DOLocalMove(tempPos, swapSpeed);
        swapGem.SetBoardPosition(tempX, tempY);
        board[tempX][tempY] = swapGem;
        
        selectedGem.SetSpinning(false);
        selectedGem = null;

        yield return tween.WaitForCompletion();
        yield return CheckForMatches();
    }

    private IEnumerator CheckForMatches() {
        var matches = FindMatches();
        foreach (var match in matches) {
            board[match.X][match.Y] = null;
            match.gameObject.SetActive(false);
            Destroy(match.gameObject);
        }

        yield return MoveGemsDown(matches.ToList());
        //todo check for more cascading matches
    }

    private IEnumerator MoveGemsDown(List<Gem> missingGems) {
        TweenerCore<Vector3, Vector3, VectorOptions> tween = null;
        missingGems.Sort((gem1, gem2) => gem2.Y - gem1.Y);

        foreach (var gem in missingGems) {
            Gem newGem = null;
            for (int y = gem.Y - 1; y >= 0; y--) {
                newGem = board[gem.X][y];

                if (newGem != null) break;
            }

            if (newGem == null) {
                var type = Random.Range(0, gemOptions.Count);
                newGem = GenerateNewGem(type, gem.X, gem.Y);
                //todo set better position above top of board:
                newGem.gameObject.transform.localPosition = new Vector3(gem.X * gemSize, gemSize, -5);
            } else {
                SetGemBoardPosition(newGem, gem.X, gem.Y);
            }

            tween = newGem.transform.DOLocalMoveY(-(gem.Y * gemSize), swapSpeed);
        }

        yield return tween.WaitForCompletion();
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
