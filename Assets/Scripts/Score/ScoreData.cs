using System;
using UnityEngine;

// Makes is so that elements of this class will be visible in the editor.
[System.Serializable]
public class ScoreData : IComparable<ScoreData> {

    #region Fields
    // Public fields --v

    // The name of the player who earned this score.
    public string name;

    // The score this player achieved.
    public int score;


    // Serialized private fields --v


    // Private fields --v
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

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Compare ScoreData objects.
    public int CompareTo(ScoreData other)
    {
        // If other is a null reference,
        if (other == null)
        {
            // then return 1.
            return 1;
        }

        // If this score is higher than the other score,
        if (this.score > other.score)
        {
            // then return 1.
            return 1;
        }

        // If the other score is higher than this one,
        if (this.score < other.score)
        {
            // then return -1.
            return -1;
        }

        // Otherwise, return 0.
        return 0;
    }
    #endregion Dev-Defined Methods
}
