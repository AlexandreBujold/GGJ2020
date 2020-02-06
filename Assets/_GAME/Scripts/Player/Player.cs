using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using GamepadInput;

public class Player : MonoBehaviour
{
    public GamePad.Index controllerIndex;
    public bool useKeyboard = false;
    [Space]
    [Header("Components")]
    public PlayerController m_playerController;
    public Health m_Health;
    public HandController m_HandController;
    public CameraController m_cameraController;
    public PlayerAnimation m_playerAnimation;
    public ShockShot m_gun;
    public SceneManager m_sceneManager;
    public WaveManager m_waveManager;
    public RepairObjective m_objective;

    [Header("UI")]
    public TextMeshProUGUI healthTextValue;
    public TextMeshProUGUI ammoTextValue;
    public TextMeshProUGUI waveTextValue;
    public TextMeshProUGUI teslaTextValue;

    // Start is called before the first frame update
    void Start()
    {
        m_sceneManager = GameObject.Find("SceneManager").GetComponent<SceneManager>();
        m_waveManager = GameObject.Find("Singletons").GetComponent<WaveManager>();
        m_objective = GameObject.Find("Objective").GetComponent<RepairObjective>();
        if(m_objective == null)
        {
            Debug.LogError("No objective in scene. Place objective along with objective locations");
        }
        //Death and Hurt animation event hookup
        if (m_Health != null && m_playerAnimation != null)
        {
            m_Health.onDamaged.AddListener(m_playerAnimation.SetHurtTrue);
            m_Health.onDeath.AddListener(m_playerAnimation.SetAliveFalse);
            m_Health.onDeath.AddListener(m_sceneManager.IncrementPlayerDeath);
        }
        
        if (m_cameraController != null)
        {
            m_cameraController.controllerIndex = this.controllerIndex;
            m_cameraController.useKeyboard = useKeyboard;
        }

        if (m_playerController != null)
        {
            m_playerController.controllerIndex = this.controllerIndex;
            m_playerController.useKeyboard = useKeyboard;
        }

        if (m_HandController != null)
        {
            m_HandController.controllerIndex = this.controllerIndex;
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

        healthTextValue.text = m_Health.health.ToString();

        waveTextValue.text = m_waveManager.WaveNumber.ToString();

        teslaTextValue.text = m_objective.activationCost.ToString();

        if(m_gun != null)
        {
            ammoTextValue.text = m_gun.ammo.ToString();
        }
        else
        {
            ammoTextValue.text = "0";
        }
    }
}
