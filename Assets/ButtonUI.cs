using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ButtonUI : MonoBehaviour
{
    private List<TMP_Text> m_TextComponentList;
    private TMP_Text m_TextComponent;
    private float _playTime;
    [SerializeField] private Pause isPaused;

    private void Awake()
    {
        m_TextComponentList = FindObjectsOfType<TMP_Text>().ToList();
        
        foreach (TMP_Text text in m_TextComponentList)
        {
            if (text.transform.parent.name == "PauseButton")
            {
                m_TextComponent = text;
            }
        }
        _playTime = Time.timeScale;
    }

    public void PlayButton()
    {
        if (!isPaused.pause)
        {
            isPaused.pause = true;
            Time.timeScale = 0;
            m_TextComponent.text = "Play";
        }
        else
        {
            Time.timeScale = _playTime;
            isPaused.pause = false;
            m_TextComponent.text = "Pause";
        }
    }
}
