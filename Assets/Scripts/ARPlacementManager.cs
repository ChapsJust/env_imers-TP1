using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private ARAnchorManager anchorManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject boardPrefab;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;

    // État
    private GameObject placedBoard;
    private bool boardIsPlaced = false;
    private ARAnchor boardAnchor;

    // Input
    private InputSystem_Actions inputActions;
    private List<ARRaycastHit> arHits = new List<ARRaycastHit>();

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
        if (boardIsPlaced) return; // Grille déjà placée

        Vector2 screenPosition = inputActions.AR.Point.ReadValue<Vector2>();

        // Raycast AR vers les plans détectés
        if (arRaycastManager.Raycast(screenPosition, arHits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = arHits[0].pose;
            PlaceBoard(hitPose);
        }
    }

    private void PlaceBoard(Pose pose)
    {
        placedBoard = Instantiate(boardPrefab, pose.position, pose.rotation);
        boardAnchor = anchorManager.AddComponent<ARAnchor>();
        boardIsPlaced = true;

        // Désactiver la visualisation des plans
        TogglePlaneVisuals(false);
        if (uiManager != null)
            uiManager.UpdateForBoardPlaced();

        Debug.Log("Grille placée!");
    }

    private void TogglePlaneVisuals(bool visible)
    {
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(visible);
        }
        arPlaneManager.planePrefab = visible ? arPlaneManager.planePrefab : null;
    }

    public void ResetPlacement()
    {
        if (placedBoard != null)
        {
            Destroy(placedBoard);
        }
        boardAnchor = null;
        boardIsPlaced = false;
        TogglePlaneVisuals(true);
        Debug.Log("Placement réinitialisé");
    }

    public bool IsBoardPlaced() => boardIsPlaced;
    public GameObject GetBoard() => placedBoard;
}