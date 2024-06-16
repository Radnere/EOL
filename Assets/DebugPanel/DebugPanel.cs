using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DebugPanel
{
    public class DebugPanel : MonoBehaviour
    {
        private static Canvas _canvas;
        private static Text _debugText;
        private static Text _fpsText;
        private static Text _statusText; 
        
        private float _elapsedTime;
        private uint _fpsSamples;
        private float _sumFps;

        private Queue<string> _queuedMessages;

        private const int MAX_LINES = 23;

        private Transform _cameraTransform;
        private Vector3 _dirToPlayer = Vector3.zero;

        [SerializeField]
        private float distanceFromCamera = 1.0f; // Настраиваемое расстояние

        void Awake()
        {
            AcquireObjects();
            _elapsedTime = 0;
            _fpsSamples = 0;
            _fpsText.text = "0";
            _queuedMessages = new Queue<string>();

            Application.logMessageReceived += OnMessageReceived;
            SetVisibility(false);
        }

        void Start()
        {
            _cameraTransform = Camera.main.transform;
        }
        
        void OnDestroy()
        {
            Application.logMessageReceived -= OnMessageReceived;
        }

        private void AcquireObjects()
        {
            _canvas = this.gameObject.GetComponent<Canvas>();
            Transform ui = this.transform.Find("UI");
            
            _debugText = ui.Find("DebugText").GetComponent<Text>();
            _fpsText = ui.Find("FpsText").GetComponent<Text>();
            _statusText = ui.Find("StatusText").GetComponent<Text>();
        }
        
        void OnMessageReceived(string message, string stackTrace, LogType type)
        {
            _queuedMessages.Enqueue(message);
        }
        
        void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (_elapsedTime > 0.5f)
            {
                //Update FPS every half second 
                _fpsText.text = (Mathf.Round((_sumFps / _fpsSamples))).ToString();

                _elapsedTime = 0f;
                _sumFps = 0f;
                _fpsSamples = 0;
            }

            _sumFps += (1.0f / Time.smoothDeltaTime);
            _fpsSamples++;
            
            //Face the Camera (Billboard)
            _dirToPlayer = (this.transform.position - _cameraTransform.position).normalized;
            _dirToPlayer.y = 0; // This ensures rotation only around the Y-axis
            this.transform.rotation = Quaternion.LookRotation(_dirToPlayer);

            //Display any queued Debug Log messages...
            if (_queuedMessages.Count > 0)
            {
                while (_queuedMessages.Count > 0)
                {
                    _debugText.text += (_queuedMessages.Dequeue() + "\n");
                }

                TrimText();
            }  
        }

        public static void Clear()
        {
            if (_debugText is null) return;
            _debugText.text = "";
        }
        
        public static void Show()
        {
            if (_canvas is null) return;
            SetVisibility(true);
            PositionPanelInFrontOfPlayer();
        }

        public static void Hide()
        {
            SetVisibility(false);
        }
        
        public static void SetVisibility(bool visible)
        {
            if (_canvas is null) return;
            _canvas.enabled = visible;
        }
        
        public static void ToggleVisibility()
        {
            if (_canvas is null) return;
            _canvas.enabled = !_canvas.enabled;
        }
        
        public static void SetStatus(string message)
        {
            if (_statusText is null) return;
            _statusText.text = message;
        }
        
        private static void TrimText()
        {
            string[] lines = _debugText.text.Split('\n');
            
            if (lines.Length > MAX_LINES)
            {
                _debugText.text = string.Join("\n", lines, lines.Length - MAX_LINES, MAX_LINES);
            }
        }

        public static bool IsVisible()
        {
            return _canvas != null && _canvas.enabled;
        }

        private static void PositionPanelInFrontOfPlayer()
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            // Установка панели перед камерой на заданном расстоянии
            float distance = 1.0f; // Значение по умолчанию, если доступ к переменной из экземпляра невозможен
            DebugPanel instance = FindObjectOfType<DebugPanel>();
            if (instance != null)
            {
                distance = instance.distanceFromCamera;
            }

            Vector3 positionInFront = camera.transform.position + camera.transform.forward * distance;
            _canvas.transform.position = positionInFront;
            
            // Ориентация панели к камере
            Quaternion rotationToFaceCamera = Quaternion.LookRotation(positionInFront - camera.transform.position);
            _canvas.transform.rotation = Quaternion.Euler(0, rotationToFaceCamera.eulerAngles.y, 0);
        }
    }
}
