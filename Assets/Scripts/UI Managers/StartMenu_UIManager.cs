using UnityEngine;

public class StartMenu_UIManager : MonoBehaviour {

    #region Fields
    // Public fields --v


    // Serialized private fields --v


    // Private fields --v

    // Reference to the GameManager.
    private GameManager gm;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        
    }

    // Called before the first frame.
    public void Start()
    {
        // Set variables --v

        // Get a reference to the GM.
        gm = GameManager.instance;
    }

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Called when player clicks on the Play button on the start menu.
    public void OnClick_PlayButton()
    {
        // Tells the GM to start the game.
        gm.ShowGame();
    }

    // Called when player clicks on the Options button on the start menu.
    public void OnClick_OptionsButton()
    {
        // Tells the GM to open the options menu.
        gm.ShowOptionsMenu();
    }

    // Called when the player finished editing the player name input for either player.
    public void OnPlayerNameInputEdited(int player)
    {
        // If this was for Player1,
        if (player == 1)
        {

        }
        // Else, this was for Player2.
        else
        {

        }
    }
    #endregion Dev-Defined Methods
}
