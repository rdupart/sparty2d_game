using UnityEngine;
using System.Collections;
using UnityEngine.UI; // include UI namespace since references UI Buttons directly
using UnityEngine.EventSystems; // include EventSystems namespace so can set initial input for controller support
using UnityEngine.SceneManagement; // include so we can load new scenes

public class MainMenuManager : MonoBehaviour {

	public int startLives=3; // how many lives to start the game with on New Game

	// references to Submenus
	public GameObject _MainMenu;
	public GameObject _LevelsMenu;
	public GameObject _AboutMenu;

	// references to Button GameObjects
	public GameObject MenuDefaultButton;
	public GameObject AboutDefaultButton;
	public GameObject LevelSelectDefaultButton;
	public GameObject QuitButton;

	// list the level names
	public string[] LevelNames;

	// reference to the LevelsPanel gameObject where the buttons should be childed
	public GameObject LevelsPanel;

	// reference to the default Level Button template
	public GameObject LevelButtonPrefab;
	
	// reference the titleText so we can change it dynamically
	public Text titleText;

	// store the initial title so we can set it back
	private string _mainTitle;

	// init the menu
	void Awake()
	{
		// store the initial title so we can set it back
		_mainTitle = titleText.text;

		// disable/enable Level buttons based on player progress
		setLevelSelect();

		// determine if Quit button should be shown
		displayQuitWhenAppropriate();

		// Show the proper menu
		ShowMenu("MainMenu");
	}

	// loop through all the LevelButtons and set them to interactable 
	// based on if PlayerPref key is set for the level.
	void setLevelSelect() {
		// turn on levels menu while setting it up so no null refs
		_LevelsMenu.SetActive(true);

		// loop through each levelName defined in the editor
		for (int i = 0; i < LevelNames.Length; i++)
		{
			string levelname = LevelNames[i]; // capture in local variable
			GameObject levelButton = Instantiate(LevelButtonPrefab, Vector3.zero, Quaternion.identity);
			levelButton.name = levelname + " Button";
			levelButton.transform.SetParent(LevelsPanel.transform, false);

			Button levelButtonScript = levelButton.GetComponent<Button>();

			// Capture a reference to the manager (this script)
			MainMenuManager managerInstance = this;

			// Assign listener using local copy of levelname
			levelButtonScript.onClick.RemoveAllListeners();
			levelButtonScript.onClick.AddListener(() => managerInstance.loadLevel(levelname));

			Text levelButtonLabel = levelButton.GetComponentInChildren<Text>();
			levelButtonLabel.text = levelname;

			levelButtonScript.interactable = PlayerPrefManager.LevelIsUnlocked(levelname);
		}

	}

	// determine if the QUIT button should be present based on what platform the game is running on
	void displayQuitWhenAppropriate() 
	{
		switch (Application.platform) {
			// platforms that should have quit button
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.LinuxPlayer:
				QuitButton.SetActive(true);
				break;

			// platforms that should not have quit button
			// note: included just for demonstration purposed since
			// default will cover all of these. 
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.WebGLPlayer: 
				QuitButton.SetActive(false);
				break;

			// all other platforms default to no quit button
			default:
				QuitButton.SetActive(false);
				break;
		}
	}

	// Public functions below that are available via the UI Event Triggers, such as on Buttons.

	// Show the proper menu
	public void ShowMenu(string name)
	{
		// turn all menus off
		_MainMenu.SetActive (false);
		_AboutMenu.SetActive(false);
		_LevelsMenu.SetActive(false);

		// turn on desired menu and set default selected button for controller input
		switch(name) {
		case "MainMenu":
			_MainMenu.SetActive (true);
			EventSystem.current.SetSelectedGameObject (MenuDefaultButton);
			titleText.text = _mainTitle;
			break;
		case "LevelSelect":
			_LevelsMenu.SetActive(true);
			EventSystem.current.SetSelectedGameObject (LevelSelectDefaultButton);
			titleText.text = "Level Select";
			break;
		case "About":
			_AboutMenu.SetActive(true);
			EventSystem.current.SetSelectedGameObject (AboutDefaultButton);
			titleText.text = "About";
			break;
		}
	}

	// load the specified Unity level
	public void loadLevel(string levelToLoad)
	{
		Debug.Log("Loading level: " + levelToLoad);

		// start new game so initialize player state
		PlayerPrefManager.ResetPlayerState(startLives,false);

		// load the specified level
		SceneManager.LoadScene(levelToLoad);
	}

	// quit the game
	public void QuitGame()
	{
		Application.Quit ();
	}
	
}
