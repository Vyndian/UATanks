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

        // Set the music and SFX volumes to the player's preferences.
        ApplyVolumePreferences();
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

    // Applies the player's volume preferences.
    private void ApplyVolumePreferences()
    {
        // If there is a preference saved for music volume,
        if (PlayerPrefs.HasKey(gm.key_MusicVolume))
        {
            // then get that value.
            float musicVolume = PlayerPrefs.GetFloat(gm.key_MusicVolume);

            // Set the music volume to that value.
            gm.main_AudioSource.volume = musicVolume;

            // Set the GM's music volume accordingly.
            gm.volume_Music = musicVolume;
        }

        // If there is a preference saved for SFX volume,
        if (PlayerPrefs.HasKey(gm.key_SFXVolume))
        {
            // then set the GM's SFX volume accordingly.
            gm.volume_SFX = PlayerPrefs.GetFloat(gm.key_SFXVolume);
        }
    }
    #endregion Dev-Defined Methods
}
