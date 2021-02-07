using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	private static GameController s_Instance = null;

	private bool _gameCompleteLock = false;
	private int _seed = 0;
	private PO_Player _player;
	private PO_Sentinel _sentinel;
	private Transform _landingPad;

	[SerializeField]
	private UIController _uiController;

	private Vector3 _sentinelGridPosition = Vector3.positiveInfinity;

	public bool uiVisible = true;

	public UnityEvent ON_GAME_WIN = new UnityEvent();
	public UnityEvent ON_QUIT = new UnityEvent();

	public static GameController instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = FindObjectOfType(typeof(GameController)) as GameController;
			}

			// If it is still null, create a new instance
			if (s_Instance == null)
			{
				GameObject obj = new GameObject("ObjectManager");
				s_Instance = obj.AddComponent(typeof(GameController)) as GameController;
				Debug.Log("Could not locate an ObjectManager object. ObjectManager was Generated Automaticly.");
			}

			return s_Instance;
		}
	}

    public void Init(int seed)
    {
		ObjectManager.instance.SetSeed(seed);
		SetSeed(seed, ObjectManager.instance.GetPlayer(), ObjectManager.instance.GetSenitel());
	}

    private void SetSeed(int seed, PO_Player player, PO_Sentinel sentinel)
    {
		_sentinelGridPosition = Vector3.positiveInfinity;
		_seed = seed;

		_player = player;
		if(_player != null)
        {
			_player.ON_ABSORBED.AddListener(OnPlayerAbsorbed);
			_player.ON_GRID_POSITION_CHANGE.AddListener(OnPlayerGridPositionChanged);
		}
			
		_sentinel = sentinel;
		if(_sentinel != null)
			_sentinel.ON_ABSORBED.AddListener(OnSentinelAbsorbed);
    }

    private void OnPlayerGridPositionChanged(Vector3 position)
    {
		//has obtained sentinel position AND sentinel is DEAD
        if(position == _sentinelGridPosition && _sentinel == null && !_gameCompleteLock)
        {
			_gameCompleteLock = true;
			StartCoroutine("OnGameWin");
		}
    }

	IEnumerator OnGameWin()
	{
		_uiController.MissionComplete();

		yield return new WaitForSeconds(2.3f);

		Reset();
		ON_GAME_WIN.Invoke();
	}

	void Update()
    {
		CursorState cursorState = _player != null ? _player.aimState : CursorState.Idle;
		if (_uiController != null) _uiController.Tick(currentPower, _seed, cursorState, uiVisible);

		if (Input.GetKeyUp(KeyCode.Q))
			ON_QUIT.Invoke();
    }

	private void OnPlayerAbsorbed(int instanceID)
    {
		Debug.Log("Player Dead!");
    }

    private void OnSentinelAbsorbed(int arg0)
    {
		//highlight the position the player has to occupy in order to win
		_landingPad = Instantiate<Transform>(Resources.Load<Transform>("Prefabs/LandingPad"));
		_landingPad.position = _sentinel.transform.position + Vector3.up * .05f;

		if(_player != null) _player.sentinelAbsorbed = true;
		_sentinelGridPosition = _sentinel.gridPosition;
	}

	int currentPower
    {
        get
        {
			return _player != null ? _player.GetPower() : 0;
        }
    }

	public void Reset()
    {
		_uiController.Reset();

		if (_landingPad) Destroy(_landingPad.gameObject);
		if (_player != null) _player.sentinelAbsorbed = false;
		_gameCompleteLock = false;

		ObjectManager.instance.Unpopulate();
	}
}
