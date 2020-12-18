using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameOverMenu_UIManager : MonoBehaviour {

    #region Fields
    [Header("Levers")]
    // Accessing High Scores through PlayerPrefs will require dynamc keys.
    // This string is the base for the name of the keys used to gather those scores.
    [SerializeField] private string keyBase_HighScores = "HIGH_SCORES_";

    // How many of the highScores should be shown on the GameOver screen.
    [SerializeField] private int numScoresToShow = 6;


    [Header("Gears")]
    // List of high scores achieved on this program.
    [SerializeField] private List<int> highScores;

    // This int is used when constructing the dynamic keys for accessing the High Scores.
    // It is increased every time the next score needs to be accessed (or set, if setting).
    // It should be reset to 0 once completed with either getting or setting.
    private int keyIndex_HighScores = 0;


    [Header("Object & Component References")]
    // References the parent gameObject for the Player2 "this Game" score and label.
    [SerializeField] private GameObject section_ThisGame_Player2;

    // References the Player1 score Text.
    [SerializeField] private Text player1Score_Text;

    // References the Player2 score Text.
    [SerializeField] private Text player2Score_Text;

    // References the Text showing the players' scores in the high score list.
    [SerializeField] private Text highScores_Text;

    // References the GM.
    private GameManager gm;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v


    }

    // Called before the first frame.
    public void Start()
    {
        
    }

    // Called every time this gameObject is activated.
    public void OnEnable()
    {
        // If the gm is null,
        if (gm == null)
        {
            // then get reference to the GM.
            gm = GameManager.instance;
        }

        // Set up the "This Game" section of the GameOver screen.
        ThisGame_Setup();

        // Set up the "High Scores" section of the GameOver screen.
        HighScores_Setup();
    }

    // Called every frame.
    public void Update()
    {

    }

    // Called when this MonoBehavior is destroyed.
    public void OnDestroy()
    {
        // Save the highScores to PlayerPrefs.
        SaveHighScores();
    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Sets up the "This Game" section of the GameOver screen.
    private void ThisGame_Setup()
    {
        // Set the Text for the Player1 score for this game.
        player1Score_Text.text = gm.score_Player1.ToString();

        // If there are two players,
        if (gm.numPlayers == 2)
        {
            // then activate the Player2 section of the "This Game" area.
            section_ThisGame_Player2.SetActive(true);

            // Set the Text for the Player2 score for this game.
            player2Score_Text.text = gm.score_Player2.ToString();
        }
        // Else, only 1 player.
        else
        {
            // Deactivate the Player2 section of the "This Game" area.
            section_ThisGame_Player2.SetActive(false);
        }
    }

    // Sets up the "High Scores" section of the GameOver screen.
    private void HighScores_Setup()
    {
        // Get the highScores (all time + this game, regardless of score this game).
        GetHighScores();

        // Sort those highScores highest to lowest.
        SortHighScores();

        // Trim the highScores list to the appropriate number.
        TrimHighScores();

        // Show the highScores on the Text list visible to the player.
        ShowHighScores();
    }

    // Clears, then fills out the highScores list completely.
    // First, gets all of the high scores saved previously to PlayerPrefs.
    // Then, adds in the score(s) earned this game by the player(s).
    private void GetHighScores()
    {
        // First, ensure that the keyIndex is at 0.
        keyIndex_HighScores = 0;
        
        // Clear the list.
        highScores.Clear();

        // As long as the dynamically created key exists,
        while (PlayerPrefs.HasKey(keyBase_HighScores + keyIndex_HighScores))
        {
            // then get that score and add it to the high scores list.
            highScores.Add(PlayerPrefs.GetInt(keyBase_HighScores + keyIndex_HighScores));

            // Increment the keyIndex.
            keyIndex_HighScores++;
        }

        // Get and add the score that the GM has for Player1.
        highScores.Add(gm.score_Player1);

        // If this was a multiplayer game,
        if (gm.numPlayers == 2)
        {
            // then add the GM's score for Player2 to the list as well.
            highScores.Add(gm.score_Player2);
        }
    }

    // Sorts the highScores list into highest-to-lowest.
    private void SortHighScores()
    {
        // Sort the list (lowest to highest).
        highScores.Sort();

        // Reverse the list so that it is highest to lowest.
        highScores.Reverse();
    }

    // Puts the high scores onto the Text visible to the player.
    private void ShowHighScores()
    {
        // Clear out the highScores visible to the player (just in case).
        highScores_Text.text = "";

        // The highScores list should already have been trimmed down to the correct number of scores.
        // Iterate through the highScores list.
        foreach (int score in highScores)
        {
            // Add the score to the Text list of high scores that the player can see.
            highScores_Text.text = highScores_Text.text + score + "\n";
        }
    }

    // Trims the highScores down to the appropriate number of scores.
    private void TrimHighScores()
    {
        // If the highScores list count is greater than numScoresToShow,
        if (highScores.Count > numScoresToShow)
        {
            // then trim down the highScores list to only numScoresToShow elements.
            highScores = highScores.GetRange(0, numScoresToShow);
        }
    }

    // Called when the player clicks on the RestartGame button. Reloads the Main scene completely.
    public void OnClick_RestartGameButton()
    {
        // Tell the gm to restart the game.
        gm.RestartGame();        
    }

    // Saves the new high scores in PlayerPrefs, and deletes unnecessary keys.
    private void SaveHighScores()
    {
        // First, ensure that the keyIndex is at 0.
        keyIndex_HighScores = 0;

        // The highScores list will already have been filled out, sorted, and trimmed.
        // Iterate through the highScores list.
        foreach (int score in highScores)
        {
            // Save that score to PlayerPrefs using the dynamic key.
            PlayerPrefs.SetInt((keyBase_HighScores + keyIndex_HighScores), score);

            // Increment the keyIndex.
            keyIndex_HighScores++;
        }

        // Now to delete any keys still being used past what was just saved.
        // If a key exists using the next dynamic key,
        while (PlayerPrefs.HasKey(keyBase_HighScores + keyIndex_HighScores))
        {
            // then delete that key.
            PlayerPrefs.DeleteKey(keyBase_HighScores + keyIndex_HighScores);

            // Increment the keyIndex.
            keyIndex_HighScores++;
        }
    }
    #endregion Dev-Defined Methods
}
