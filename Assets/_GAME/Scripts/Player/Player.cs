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
    public PlayerAnimation m_playerAnimation;

    [Header("UI")]
    public TextMeshProUGUI healthTextValue;
    public TextMeshProUGUI ammoTextValue;
    public TextMeshProUGUI waveTextValue;

    // Start is called before the first frame update
    void Start()
    {
        //Death and Hurt animation event hookup
        if (m_Health != null && m_playerAnimation != null)
        {
            m_Health.onDamaged.AddListener(m_playerAnimation.SetHurtTrue);
            m_Health.onDeath.AddListener(m_playerAnimation.SetAliveFalse);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (m_cameraController != null && m_playerAnimation != null)
        {
            if (m_playerAnimation.modelGameObject != null)
            {
                m_playerAnimation.modelGameObject.transform.rotation = Quaternion.Euler(0, m_cameraController.transform.eulerAngles.y, 0);
            }
        }
    }
}
