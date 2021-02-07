using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIController : MonoBehaviour
{
    private CanvasGroup _container;

    [SerializeField]
    private Image _cursor;

    [SerializeField]
    private Text _powerText;

    [SerializeField]
    private GameObject _missionCompleteMessage;

    [SerializeField]
    private Text _seed;

    private void Awake()
    {
        _container = GetComponent<CanvasGroup>();
        Reset();
    }

    public void Tick(int power, int seed, CursorState cursorState, bool visible)
    {
        isVisible = visible;
        _powerText.text = "Power: " + power.ToString();
        _seed.text = "Level: " + seed.ToString();

        DrawCursor(cursorState);
    }

    private void DrawCursor(CursorState cursorState)
    {
        _cursor.enabled = cursorState != CursorState.Idle;

        switch(cursorState)
        {
            case CursorState.Aiming:
                _cursor.color = Color.white;
                break;
            case CursorState.Hitting:
                _cursor.color = Color.green;
                break;
            case CursorState.OutOfRange:
                _cursor.color = Color.red;
                break;
        }
    }

    private bool isVisible
    {
        set
        {
            _container.alpha = value ? 1 : 0;
            _container.blocksRaycasts = !value;
        }
    }

    internal void MissionComplete()
    {
        _missionCompleteMessage.SetActive(true);
        _cursor.gameObject.SetActive(false);
    }

    public void Reset()
    {
        _cursor.gameObject.SetActive(true);
        _missionCompleteMessage.SetActive(false);
        isVisible = false;
    }
}
