using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private WaveManager waveManager;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        waveManager = GameObject.Find("Singletons").GetComponent<WaveManager>();
    }

    private void Update()
    {
        text.text = waveManager.WaveNumber.ToString();
    }
}
