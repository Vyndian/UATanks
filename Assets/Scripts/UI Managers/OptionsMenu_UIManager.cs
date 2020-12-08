using UnityEngine;
using UnityEngine.UI;
using System;

public class OptionsMenu_UIManager : MonoBehaviour {

    #region Fields
    // Reference to the GameManager.
    private GameManager gm;


    [Header("Number of Players")]
    // Reference to the dropdown menu for the number of players.
    [SerializeField] private Dropdown numPlayers_Dropdown;


    [Header("Music Volume")]
    // Reference to the music volume slider.
    [SerializeField] private Slider musicSlider;

    // Reference to the Text showing the music volume above the slider.
    [SerializeField] private Text musicVolume_Text;


    [Header("SFX Volume")]
    // Reference to the SFX volume slider.
    [SerializeField] private Slider sfxSlider;

    // Reference to the Text showing the SFX volume above the slider.
    [SerializeField] private Text sfxVolume_Text;


    [Header("Map of the Day")]
    // Reference to the Dropdown for the MotD.
    [SerializeField] private Dropdown dropdown_MotD;

    // Reference to the InputField where the player can manually enter the seed for Random.
    [SerializeField] private InputField manualSeed_InputField;
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
    // Called when the slider value for Music volume changes.
    public void OnSilderChanged_Music()
    {
        // Get the slider's value as an int.
        int val = (int)musicSlider.value;

        // Change the music volume Text to show the new value.
        musicVolume_Text.text = val.ToString();

        // Update the value on the GM.
        gm.volume_Music = val;
    }

    // Called when the slider value for SFX volume changes.
    public void OnSilderChanged_SFX()
    {
        // Get the slider's value as an int.
        int val = (int)sfxSlider.value;

        // Change the SFX volume Text to show the new value.
        sfxVolume_Text.text = val.ToString();

        // Update the value on the GM.
        gm.volume_SFX = val;
    }

    // Called when the value for the dropdown menu for Number of Players is changed.
    public void OnDropdownValueChanged_NumPlayers()
    {
        // Update the value.
        gm.numPlayers_Value = numPlayers_Dropdown.value + 1;
    }

    // Called when the value for the dropdown menu for MotD is changed.
    public void OnDropdownValueChanged_MotD()
    {
        // Act according to the dropdown menu's value.
        switch (dropdown_MotD.value)
        {
            // In the case of the value being 0 (Random, meaning DateTime),
            case 0:
                // Update the value.
                gm.randomSeedMethod_Value = MapGenerator.RandomSeedMethod.DateTime;

                // Set the manual seed entry input field to disabled.
                manualSeed_InputField.gameObject.SetActive(false);
                break;


            // In the case of the value being 1 (Map of the Day),
            case 1:
                // Update the value.
                gm.randomSeedMethod_Value = MapGenerator.RandomSeedMethod.MapOfTheDay;

                // Set the manual seed entry input field to disabled.
                manualSeed_InputField.gameObject.SetActive(false);
                break;


            // In the case of the value being 2 (Manual),
            case 2:
                // Update the value.
                gm.randomSeedMethod_Value = MapGenerator.RandomSeedMethod.Manual;

                // Set the manual seed entry input field to enabled.
                manualSeed_InputField.gameObject.SetActive(true);
                break;
        }   
    }

    // Called when the player finishes editing the input field for manual random seed entry.
    public void OnInputFinished_MotD()
    {
        // If the input field is not empty,
        if (manualSeed_InputField.text.Length > 0)
        {
            // then set the value of the manually entered seed.
            // The input field will only accept integers, so this parse has no chance of failing.
            gm.manualSeed_Value = int.Parse(manualSeed_InputField.text);
        }
        // Else, the input field is empty.
        else
        {
            // Choose the seed randomly.
            gm.manualSeed_Value = UnityEngine.Random.Range(0, 9999999);
        }
    }

    // Called when the player pressed the Accept button.
    public void OnClick_AcceptButton()
    {
        // Call the appropriate function on the GM to close the Options menu and reopen the Start menu.
        gm.ShowStartMenu();
    }
    #endregion Dev-Defined Methods
}
