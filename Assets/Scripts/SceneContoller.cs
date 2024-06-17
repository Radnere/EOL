using System;
using System.Collections;
using System.Collections.Generic;
using DebugPanel;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARPlaneManager))]
public class SceneContoller : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _togglePlanesAction;

   [SerializeField]
    private InputActionReference _toggleDebugAction;

    [SerializeField]
    private InputActionReference _activateAction;

    [SerializeField]
    private GameObject _EchoBook;

  
    private ARPlaneManager _planeManager;
    private bool _planesVisible = true;
    private int _numPlanesAddedOccurred = 0;
    private GameObject _currentEchoBookInstance;


    // Start is called before the first frame update
    
    void Start()
    {
        Debug.Log("-> SceneController::Start()");
        _planeManager = GetComponent<ARPlaneManager>();
        if (_planeManager is null)
        {
            Debug.LogError("Can't find 'ARPlaneManager'");
        }
        
        _togglePlanesAction.action.performed += OnTogglePlanesAction;
        _planeManager.planesChanged += OnPlanesChanged;
        _activateAction.action.performed += OnActivateAction;
        _toggleDebugAction.action.performed += OnToggleDebugAction; // Подписка на действие переключения дебага
    }
    public static SceneContoller Instance;   
    void Awake() 
    {       
        Instance = this;
       
    }

    void OnDestroy()
    {
        Instance = null;
        Debug.Log("-> SceneController::OnDestroy()");
        _togglePlanesAction.action.performed -= OnTogglePlanesAction;
        _planeManager.planesChanged -= OnPlanesChanged;
        _activateAction.action.performed -= OnActivateAction;
        _toggleDebugAction.action.performed -= OnToggleDebugAction; // Отписка от действия переключения дебага
    }
    

    private void OnActivateAction(InputAction.CallbackContext obj)
    {
        SpawnEchoBook();
    }

    private void OnToggleDebugAction(InputAction.CallbackContext context)
    {
        if (DebugPanel.DebugPanel.IsVisible())
            DebugPanel.DebugPanel.Hide();
        else
            DebugPanel.DebugPanel.Show();
    }

    
    public void SpawnEchoBook()
    {
        // Check if an EchoBook has already been spawned
        if (_currentEchoBookInstance != null)
        {
            Debug.Log("EchoBook has already been spawned. Only one instance is allowed.");
            return;
        }

        Debug.Log("-> SceneController::SpawnEchoBook()");

        // Create a list to hold all table planes
        List<ARPlane> tables = new List<ARPlane>();

        // Iterate through each plane found in the scene to find all tables
        foreach (var plane in _planeManager.trackables)
        {
            if (plane.classification == PlaneClassification.Table)
            {
                tables.Add(plane);
            }
        }

        // Check if there are any tables found
        if (tables.Count > 0)
        {
            // Select a random table from the list
            int randomIndex = UnityEngine.Random.Range(0, tables.Count);
            ARPlane selectedTable = tables[randomIndex];

            // Calculate the spawn position
            Vector3 spawnPosition = selectedTable.transform.position;
            spawnPosition.y += 0.3f; // Adjust the height slightly above the table

            // Instantiate the EchoBook at the calculated position
            _currentEchoBookInstance = Instantiate(_EchoBook, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.Log("No tables found to spawn the EchoBook.");
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTogglePlanesAction(InputAction.CallbackContext obj)
    {
        _planesVisible = !_planesVisible;
        float fillAlpha = _planesVisible ? 0.3f : 0f;
        float lineAlpha = _planesVisible ? 1.0f : 0f;

        Debug.Log("-> OnTogglePlanesAction() - trackables.count" +_planeManager.trackables.count);

        foreach (var plane in _planeManager.trackables)
        {
            SetPlaneAlpha(plane, fillAlpha, lineAlpha);
        }
    }

    private void SetPlaneAlpha(ARPlane plane, float fillAlpha, float lineAlpha)
    {
        var meshRenderer = plane.GetComponentInChildren<MeshRenderer>();
        var lineRenderer = plane.GetComponentInChildren<LineRenderer>();

        if (meshRenderer != null)
        {
            Color color = meshRenderer.material.color;
            color.a = fillAlpha;
            meshRenderer.material.color = color;
        }

        if (lineRenderer != null)
        {
            // Get the current start and end colors
            Color startColor = lineRenderer.startColor;
            Color endColor = lineRenderer.endColor;

            // Set the alpha component
            startColor.a = lineAlpha;
            endColor.a = lineAlpha;

            // Apply the new colors with updated alpha
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }
    }


   private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (args.added.Count > 0)
        {
            _numPlanesAddedOccurred++;

            foreach (var plane in _planeManager.trackables)
            {
                PrintPlaneLabel(plane);
            }

            Debug.Log("-> Number of planes: " + _planeManager.trackables.count);
            Debug.Log("-> Num Planes Added Occurred: " + _numPlanesAddedOccurred);
        }
    }

    private void PrintPlaneLabel(ARPlane plane)
    {
        string label = plane.classification.ToString();
        string log = $"Plane ID: {plane.trackableId}, Label: {label}";
        Debug.Log(log);
    }


    
}
