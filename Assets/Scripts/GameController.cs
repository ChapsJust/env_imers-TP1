using System.Collections.Generic;
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

    // État du jeu
    private int[] board = new int[9]; // 0=vide, 1=X, 2=O
    private bool isXTurn = true;
    private bool gameOver = false;
    private List<GameObject> placedSymbols = new List<GameObject>();

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
        int winner = CheckWinner();
        if (winner != 0)
        {
            gameOver = true;
            string winnerName = winner == 1 ? "X" : "O";
            Debug.Log($"{winnerName} a gagné!");
            // TODO: Afficher UI de victoire
            return;
        }

        // Vérifier match nul
        if (IsBoardFull())
        {
            gameOver = true;
            Debug.Log("Match nul!");
            // TODO: Afficher UI match nul
            return;
        }

        // Changer de tour
        isXTurn = !isXTurn;
        Debug.Log($"Tour de {(isXTurn ? "X" : "O")}");
    }

    private int CheckWinner()
    {
        foreach (int[] combo in winCombinations)
        {
            if (board[combo[0]] != 0 &&
                board[combo[0]] == board[combo[1]] &&
                board[combo[1]] == board[combo[2]])
            {
                // TODO: Mettre en évidence la ligne gagnante
                return board[combo[0]];
            }
        }
        return 0; // Pas de gagnant
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

        isXTurn = true;
        gameOver = false;

        Debug.Log("Nouvelle partie! Tour de X");
        // TODO: Mettre à jour l'UI
    }

    public void ResetPlacement()
    {
        NewGame();
        placementManager.ResetPlacement();
        // TODO: Remettre les instructions "Scannez une surface..."
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