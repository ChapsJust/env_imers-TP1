using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class GameController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject xPrefab;
    [SerializeField] private GameObject oPrefab;

    [Header("AR")]
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARPlacementManager placementManager;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Material highlightMaterial;

    [Header("Highlight")]
    [SerializeField] private Color winHighlightColor = Color.yellow;
    [SerializeField] private float highlightScaleMultiplier = 1.3f;

    // État du jeu
    private int[] board = new int[9]; // 0=vide, 1=X, 2=O
    private bool isXTurn = true;
    private bool gameOver = false;
    private List<GameObject> placedSymbols = new List<GameObject>();
    private List<GameObject> highlightEffects = new List<GameObject>();

    // Input
    private InputSystem_Actions inputActions;
    private List<ARRaycastHit> arHits = new List<ARRaycastHit>();

    // Combinaisons gagnantes
    private int[][] winCombinations = new int[][]
    {
        new int[] {0, 1, 2}, // Lignes
        new int[] {3, 4, 5},
        new int[] {6, 7, 8},
        new int[] {0, 3, 6}, // Colonnes
        new int[] {1, 4, 7},
        new int[] {2, 5, 8},
        new int[] {0, 4, 8}, // Diagonales
        new int[] {2, 4, 6}
    };

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.AR.Tap.performed += OnTap;
    }

    void OnDisable()
    {
        inputActions.AR.Tap.performed -= OnTap;
        inputActions.Disable();
    }

    void OnTap(InputAction.CallbackContext context)
    {
        // Ne rien faire si la grille n'est pas placée ou le jeu est fini
        if (!placementManager.IsBoardPlaced() || gameOver) return;

        Vector2 screenPos = inputActions.AR.Point.ReadValue<Vector2>();

        // Raycast CLASSIQUE (Physics) vers les cellules de la grille
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Chercher CellID sur l'objet touché OU son parent
            CellID cell = hit.collider.GetComponent<CellID>();
            if (cell == null)
            {
                cell = hit.collider.GetComponentInParent<CellID>();
            }

            if (cell != null)
            {
                TryPlaceSymbol(cell.cellID, cell.transform);
            }
        }
    }

    private void TryPlaceSymbol(int index, Transform cellTransform)
    {
        if (index < 0 || index >= 9)
        {
            Debug.LogError($"Index invalide: {index}. Doit être entre 0 et 8.");
            return;
        }

        if (board[index] != 0)
        {
            Debug.Log("Case déjà occupée!");
            return;
        }

        GameObject prefab = isXTurn ? xPrefab : oPrefab;

        Transform boardTransform = placementManager.GetBoard().transform;

        // Décaler dans la direction "up" du board (fonctionne mur, sol, plafond)
        Vector3 spawnPos = cellTransform.position + boardTransform.up * 0.02f;

        // Utiliser directement la rotation du board
        // Les symboles seront alignés à plat sur le plateau peu importe son orientation
        GameObject symbol = Instantiate(prefab, spawnPos, boardTransform.rotation);
        symbol.transform.SetParent(boardTransform);

        SetLayerRecursive(symbol, LayerMask.NameToLayer("Ignore Raycast"));

        placedSymbols.Add(symbol);

        board[index] = isXTurn ? 1 : 2;

        Debug.Log($"Index: {index}, Position: {cellTransform.position}, Joueur: {(isXTurn ? "X" : "O")}");

        // Vérifier victoire
        int winner = CheckWinner(out int[] winningCombo);
        if (winner != 0)
        {
            gameOver = true;
            string winnerName = winner == 1 ? "X" : "O";
            Debug.Log($"{winnerName} a gagné!");
            if (winningCombo != null)
            {
                HighlightWinningLine(winningCombo);
            }

            uiManager.ShowGameOver($"{winnerName} a gagné!");
            return;
        }

        // Vérifier match nul
        if (IsBoardFull())
        {
            gameOver = true;
            Debug.Log("Match nul!");
            uiManager.ShowGameOver("Match nul!");
            return;
        }

        // Changer de tour
        isXTurn = !isXTurn;
        uiManager.UpdateCurrentPlayer(isXTurn);
    }

    private int CheckWinner(out int[] winningCombo)
    {
        foreach (int[] combo in winCombinations)
        {
            if (board[combo[0]] != 0 &&
                board[combo[0]] == board[combo[1]] &&
                board[combo[1]] == board[combo[2]])
            {
                winningCombo = combo;
                return board[combo[0]];
            }
        }
        winningCombo = null;
        return 0; // Pas de gagnant
    }

    private void HighlightWinningLine(int[] combo)
    {
        GameObject boardObj = placementManager.GetBoard();
        Transform boardTransform = boardObj.transform;

        if (highlightMaterial == null)
        {
            Debug.LogWarning("HighlightMaterial non assigné dans l'Inspector!");
            return;
        }

        Transform plateau = boardTransform.Find("Plateau");
        if (plateau == null)
        {
            plateau = boardTransform;
        }

        foreach (int index in combo)
        {
            string cellName = $"Cell {index + 1}";
            Transform cell = plateau.Find(cellName);
            if (cell == null) continue;

            // Trouver le symbole le plus proche de cette cellule
            GameObject closestSymbol = FindSymbolAtCell(cell);
            if (closestSymbol == null) continue;

            // Dupliquer le symbole pour créer l'aura
            GameObject aura = Instantiate(closestSymbol, closestSymbol.transform.position, closestSymbol.transform.rotation);
            aura.name = $"Aura_{index}";
            aura.transform.SetParent(closestSymbol.transform.parent, true);

            // Agrandir légèrement la copie pour qu'elle dépasse le symbole original
            aura.transform.localScale = closestSymbol.transform.localScale * 1.4f;

            // Appliquer le material jaune sur tous les renderers de l'aura
            Renderer[] renderers = aura.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.material = highlightMaterial;
            }

            // Désactiver tous les colliders de l'aura
            Collider[] colliders = aura.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                Destroy(col);
            }

            highlightEffects.Add(aura);

            Debug.Log($"Aura créée sur {cellName}");
        }
    }

    /// <summary>
    /// Trouve le symbole placé le plus proche d'une cellule.
    /// </summary>
    private GameObject FindSymbolAtCell(Transform cell)
    {
        float closestDist = float.MaxValue;
        GameObject closestSymbol = null;

        foreach (GameObject symbol in placedSymbols)
        {
            if (symbol == null) continue;
            float dist = Vector3.Distance(symbol.transform.position, cell.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestSymbol = symbol;
            }
        }

        // Seulement si assez proche
        if (closestSymbol != null && closestDist < 0.05f)
            return closestSymbol;

        return null;
    }

    private bool IsBoardFull()
    {
        foreach (int cell in board)
        {
            if (cell == 0) return false;
        }
        return true;
    }

    public void NewGame()
    {
        // Réinitialiser le tableau
        for (int i = 0; i < 9; i++)
            board[i] = 0;

        // Détruire les symboles placés
        foreach (GameObject symbol in placedSymbols)
        {
            if (symbol != null) Destroy(symbol);
        }
        placedSymbols.Clear();
        foreach (GameObject highlight in highlightEffects)
        {
            if (highlight != null) Destroy(highlight);
        }
        highlightEffects.Clear();

        isXTurn = true;
        gameOver = false;

        Debug.Log("Nouvelle partie! Tour de X");
        uiManager.HideGameOver();
        uiManager.UpdateCurrentPlayer(true);
    }

    public void ResetPlacement()
    {
        NewGame();
        placementManager.ResetPlacement();
        uiManager.ShowInstructions("Scannez une surface pour placer le plateau.");
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    // Getters pour l'UI
    public bool IsXTurn() => isXTurn;
    public bool IsGameOver() => gameOver;
}