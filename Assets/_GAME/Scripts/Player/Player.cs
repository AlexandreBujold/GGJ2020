using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class Player : MonoBehaviour
{

    [Header("Components")]
    public PlayerController m_playerController;
    public Health m_Health;
    public HandController m_HandController;
    public CameraController m_cameraController;

    [Header("UI")]
    public TextMeshProUGUI healthTextValue;
    public TextMeshProUGUI ammoTextValue;
    public TextMeshProUGUI waveTextValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
