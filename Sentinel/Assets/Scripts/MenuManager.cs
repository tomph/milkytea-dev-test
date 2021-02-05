using UnityEngine;

public class MenuManager : MonoBehaviour
{

	private delegate void MenuDelegate();

	private MenuDelegate menuFunction;

	private int seed;
	string levelString = "0";
	private static MenuManager s_Instance = null;
	private bool showMenu = true;
    public bool isAR = false;

	public static MenuManager instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = FindObjectOfType(typeof(MenuManager)) as MenuManager;
			}
			
			// If it is still null, create a new instance
			if (s_Instance == null)
			{
				GameObject obj = new GameObject("MenuManager");
				s_Instance = obj.AddComponent(typeof(MenuManager)) as MenuManager;
				Debug.Log("Could not locate an MenuManager object. MenuManager was Generated Automaticly.");
			}
			
			return s_Instance;
		}
	}

	void OnApplicationQuit()
	{
		s_Instance = null;
	}

	// Use this for initialization
	void Start()
	{
        menuFunction = SPLevelSelectMenu;// AnyKeyMenu;
	}
	
	void OnGUI()
	{
		if(!showMenu)
			return;

		if(menuFunction != null)
			menuFunction();
	}

	void AnyKeyMenu()
	{
		int screenWidth = Screen.width;
		int screenHeight = Screen.height;

		if (Input.anyKey)
			menuFunction = GameTypeMenu;

		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.Label(new Rect(screenWidth * 0.45f, screenHeight * 0.45f, screenWidth * 0.1f, screenHeight * 0.1f), "Press Any Key To Continue");
	}

	void GameTypeMenu()
	{
		int screenWidth = Screen.width;
		int screenHeight = Screen.height;
		float buttonWidth = screenWidth * 0.4f;
		float buttonHeight = screenHeight * 0.3f;

		if (GUI.Button(new Rect((screenWidth - buttonWidth) * 0.5f, screenHeight * 0.1f, buttonWidth, buttonHeight), "Single Player"))
			menuFunction = SPLevelSelectMenu;

		if (GUI.Button(new Rect((screenWidth - buttonWidth) * 0.5f, screenHeight * 0.1f + buttonHeight, buttonWidth, buttonHeight), "Multi Player"))
		{
			menuFunction = MPInitLobbyMenu;
		}

		if (GUI.Button(new Rect((screenWidth - buttonWidth) * 0.5f, screenHeight * 0.1f + (buttonHeight * 2.0f), buttonWidth, buttonHeight), "Quit Game"))
			Application.Quit();
	}

	void SPLevelSelectMenu()
	{
		int screenWidth = Screen.width;
		int screenHeight = Screen.height;
		float buttonWidth = screenWidth * 0.4f;
		float buttonHeight = screenHeight * 0.3f;

		levelString = GUI.TextField(new Rect(10, 10, 200, 20), levelString, 25);
		if (GUI.changed)
		{
			int temp;
			if (int.TryParse(levelString, out temp))
			{
				seed = temp;
			} else if (levelString == "")
				seed = 0;
		}

		if (GUI.Button(new Rect((screenWidth - buttonWidth) * 0.5f, screenHeight * 0.1f + (buttonHeight * 2.0f), buttonWidth, buttonHeight), "Load Level"))
		{
			ObjectManager.instance.SetSeed(seed);
			//ObjectManager.instance.PopulateLevel();
			//ObjectManager.instance.PopulatePlayer();
			showMenu = false;
		}
	}

	private void MPInitLobbyMenu()
	{
		GUILayout.Label("Waiting for other players");
	}
}
