using UnityEngine;
using System.Collections.Generic;

public class AI_Controller : MonoBehaviour {

    #region Fields
    // Enum definitions.

    // The definition for the enum for the personality type of this tank.
    public enum PersonalityType { Hunter, Guard, Assassin, Caravan };

    // The definition for the enum for the state of the tank AI (if in Hunter personality).
    public enum Hunter_AIState { Chase, ChaseAndFire, CheckForFlee, Flee, Rest };

    // The definition for the enum for the state of the tank AI (if in Guard personality).
    public enum Guard_AIState { Patrol, RotateTowardSound, FireAtIntruder };

    // The definition for the enum for the state of the tank AI (if in Assassin personality).
    public enum Assassin_AIState { LayInWait, TakeAim, Assassinate, VerifyKill };

    // The definition for the enum for the state of the tank AI (if in Caravan personality).
    public enum Caravan_AIState { Transport, CheckForFlee, Flee, Hide };

    // The definition for the enum for the three methods a Guard or Caravan might traverse their waypoints.
    public enum LoopType { Stop, Loop, PingPong };


    [Header("AI variables")]
    // The current PersonalityType of this tank.
    public PersonalityType personality = PersonalityType.Hunter;

    // The state of this tank AI regarding the Hunter personality.
    public Hunter_AIState hunter_AIState = Hunter_AIState.Chase;

    // The state of this tank AI regarding the Guard personality.
    public Guard_AIState guard_AIState = Guard_AIState.Patrol;

    // The state of this tank AI regarding the Assassin personality.
    public Assassin_AIState assassin_AIState = Assassin_AIState.LayInWait;

    // The state of this tank AI regarding the Caravan personality.
    public Caravan_AIState caravan_AIState = Caravan_AIState.Transport;

    // The radius at which the tank can sense other tanks.
    [SerializeField] private readonly float aiSenseRadius = 30.0f;

    // The square of aiSenseRadius, mathed out in Awake.
    private float aiSenseRadius_Squared;

    // The time that the tank entered this state.
    private float timeStateEntered;

    // A STATIC int representing the index of the next player to be targeted.
    // The index is for the GM's list of alive players.
    static private int playerToTarget = 0;


    [Header("Material References")]
    // A reference to the material used for the tank's body when it's personality is set to Hunter,
    [SerializeField] private Material mat_HunterBody;

    // A reference to the material used for the tank's cannon when it's personality is set to Hunter,
    [SerializeField] private Material mat_HunterCannon;

    // A reference to the material used for the tank's body when it's personality is set to Guard,
    [SerializeField] private Material mat_GuardBody;

    // A reference to the material used for the tank's cannon when it's personality is set to Guard,
    [SerializeField] private Material mat_GuardCannon;

    // A reference to the material used for the tank's body when it's personality is set to Assassin,
    [SerializeField] private Material mat_AssassinBody;

    // A reference to the material used for the tank's cannon when it's personality is set to Assassin,
    [SerializeField] private Material mat_AssassinCannon;

    // A reference to the material used for the tank's body when it's personality is set to Caravan,
    [SerializeField] private Material mat_CaravanBody;

    // A reference to the material used for the tank's cannon when it's personality is set to Caravan,
    [SerializeField] private Material mat_CaravanCannon;


    [Header("Hearing & FOV variables")]
    // The maximum hearing distance of this tank.
    public float hearingDistance = 25.0f;

    // The most recent player that this tank can hear.
    public Transform heardPlayer;

    // The square of hearingDistance, calculated at Awake.
    [HideInInspector] public float hearingDistance_Squared;

    // The max angle for the tank's line of sight.
    [SerializeField] private float fov_sightAngle = 45.0f;

    // The max range that the tank's line of sight extends.
    [SerializeField] private float fov_sightRange = 15.0f;

    // The max angle between this tank and a player tank where this tank can fire at the player.
    [SerializeField] private float lockedOnAngle = 30.0f;


    [Header("Repair/Healing variables")]
    // The method of which this tank heals, either per second or per tick.
    public RepairType repairType = RepairType.PerSecond;

    // The definition for the enum for whether this tank heals/repairs per second or per tick.
    public enum RepairType { PerSecond, PerTick };

    // The amount of time between each tick, is repair/healing is done that way.
    [SerializeField, Tooltip("Time between each tick (in seconds) for healing purposes, if repairType == PerTick")]
    private float tickLength = 1.0f;

    // The amount of HP healed per second when in rest state for this tank.
    [SerializeField, Tooltip("Either the amount healed per second or per tick, depending on the Repair Type.")]
    private float restingHealRate = 0.8f;

    // The time that the last tick occurred. Initialized at 0 so it won't be null for the first check.
    private float timeLastTick = 0.0f;


    [Header("Obstacle Avoidance variables")]
    // The multiplier for the original raycast's length when
    // firing raycasts at 45 deg angles to determine which direction to turn. 1 for the same, 2 for double, etc.
    [SerializeField] private float raycastLength_Modifier = 1.8f;

    // The amounce of time this tank will stay in avoidance stage 2.
    [SerializeField] private float avoidanceTime = 2.0f;

    // The current stage of avoidance the tank is in.
    // Value of 0 indicates that tank is in its normal move state (not avoiding obstacles).
    private int avoidanceStage = 0;

    // The direction the tank should turn when avoiding obstacles. -1 for left, 1 for right.
    private int avoidance_TurnDirection = -1;

    // The time that the tank will exit avoidance stage 2.
    private float avoidanceStage2_ExitTime;


    [Header("Flee variables, used for both Hunter & Caravan")]
    // The amount of time the tank should flee before it starts attempting obstacle avoidance (still while fleeing).
    // This allows the tank to get partially turned around before trying to avoid obstacles it doesn't need to.
    [SerializeField] private float duration_FleeBeforeAvoiding = 3.0f;

    // The duration the tank will flee before checking again if it is safe to rest. In seconds.
    [SerializeField] private float duration_FleeBeforeChecking = 30.0f;

    // The distance ahead of the tank that the tank chooses as a spot to flee towards.
    [SerializeField] private float fleeDistance = 1.0f;

    // A decimal representing the threshold of percentage of health that the tank will flee.
    // Determined at Start based on value held in data.
    [SerializeField] private float fleeThreshold_Decimal;


    [Header("Hunter variables")]
    // The Transform of the target this tank will Chase.
    [SerializeField] private Transform target;

    // The distance at which the tank is close enough to the player (when chasing) to stop moving closer.
    [SerializeField] private readonly float chasing_CloseEnough = 7.0f;

    // The square of chasing_CloseEnough. Determined by maths at Awake.
    private float chasing_CloseEnough_Squared;


    [Header("Guard & Caravan Info")]
    // The actual enum variable set by designers for how this tank should traverse the waypoints.
    public LoopType loopType = LoopType.Loop;

    // The Guard that is protecting this Caravan, if this is a Caravan.
    public GameObject escortingGuard;

    // The Caravan gameObject that the Guard is protecting, if it is doing so.
    [SerializeField] private GameObject protectedCaravan;

    // Array of Transforms of the GameObjects being used as waypoints for this AI.
    [SerializeField] private Transform[] waypoints;

    // The amount of "wiggle room" the tank has for being close enough to the waypoint (allowed variance).
    [SerializeField] private float waypoints_CloseEnough = 1.0f;

    // The amount of points that a Caravan's pointValue decreases by whenever it finishes a circuit.
    [SerializeField] private int pointDropPerCircuit = 20;

    // The square of waypoints_CloseEnough, calculated in Awake.
    private float waypoints_CloseEnough_Squared;

    // The index of the current waypoint the AI is moving towards.
    private int currentWaypoint = 0;

    // Whether or not the tank is heading forward through the waypoint array or not (backwards).
    // Used only for the PingPong behavior.
    private bool waypoints_IsGoingForward = true;

    // Whether or not the tank has reached the end of the waypoint system.
    // Used only for the Stop behavior.
    private bool waypoints_IsStopped = false;

    // The current target of the Guard that is intruder in their area.
    private Transform intruder;

    // Whether or not this is a Guard that is also protecting a Caravan.
    private bool isGuardProtectingCaravan = false;


    [Header("Assassin variables")]
    // The damage multiplier for the one, strong attack the Assassin makes.
    [SerializeField, Tooltip("Assassin's one powerful shot's damage will be multiplied by this number.")]
    private float sneakAttack_DamageMultiplier = 2.5f;

    // The speed multiplier for the one, strong attacj the Assassin makes.
    [SerializeField, Tooltip("Assassin's one powerful shot's shell speed will be multiplied by this number.")]
    private float sneakAttack_SpeedMultiplier = 2.5f;

    // The amount of time after attemptiong to assassinate the target that the Assassin waits before
    // assuming that the target lived.
    [SerializeField] private float duration_VerifyKill = 1.0f;

    // The victim that the Assassin is trying to kill.
    // Set when a player is heard, nulled out when that player leaves earshot.
    private Transform victim;

    // Whether or not the Assassin has fired its one shot.
    private bool shotFired = false;

    // The health the Assassin had when last transitioning to the LayInWait state.
    public float hiding_UndamagedHealth;


    [Header("Component & Object References")]
    // The transform on this gameObject.
    [SerializeField] private Transform tf;

    // The TankMotor on this gameObject.
    [SerializeField] private TankMotor motor;

    // The TankData on this gameObject.
    [SerializeField] private TankData data;

    // The TankCannon on this gameObject.
    [SerializeField] private TankCannon cannon;

    // Reference to the MeshRenderer on the tank's Shell visual (body).
    [SerializeField] private MeshRenderer shell_Visual;

    // Reference to the MeshRenderer on the tank's cannon's base visual.
    [SerializeField] private MeshRenderer cannonBase_Visual;

    // Reference to the MeshRenderer on the tank's cannon's barrel visual.
    [SerializeField] private MeshRenderer cannonBarrel_Visual;

    // Reference to the GameManager.
    private GameManager gm;

    // Reference to the GameManager's list of player tanks.
    private List<TankData> playerList;

    #endregion Fields

    #region Unity Methods
    // Performed before Start.
    public void Awake()
    {
        // Set variables --v
        
        // If this var is null,
        if (tf == null)
        {
            // Get the transform component from this object.
            tf = transform;
        }

        // If this var is null,
        if (motor == null)
        {
            // Get the TankMotor on this gameObject.
            motor = gameObject.GetComponent<TankMotor>();
        }

        // If this var is null,
        if (data == null)
        {
            // Get the TankData on this gameObject.
            data = gameObject.GetComponent<TankData>();
        }

        // If this var is null,
        if (cannon == null)
        {
            // Get the TankCannon on this gameObject.
            cannon = gameObject.GetComponent<TankCannon>();
        }

        // Get the square of aiSenseRadius and save it.
        aiSenseRadius_Squared = aiSenseRadius * aiSenseRadius;

        // Get the square of chasing_CloseEnough and save it.
        chasing_CloseEnough_Squared = chasing_CloseEnough * chasing_CloseEnough;

        // Get the square of waypoints_CloseEnough and save it.
        waypoints_CloseEnough_Squared = waypoints_CloseEnough * waypoints_CloseEnough;

        // Get the square of hearingDistance and save it.
        hearingDistance_Squared = hearingDistance * hearingDistance;
    }

    // Called before the first frame.
    public void Start()
    {
        // Take the fleethresholdPercentage from data and save it as a float temporarily.
        float temp = data.fleeThreshold_Percentage;

        // Convert that new float to a decimal.
        // Must be in Start so that data has had time to initialize its values.
        fleeThreshold_Decimal = temp / 100;

        // "Change" the personality type to itself. This allows for the proper setup.
        ChangePersonality(personality);
    }

    // Called every frame.
    public void Update()
    {
        // This cannot be done at Start or earlier because the players and GM need time to set up.
        // If gm is not set,
        if (gm == null)
        {
            // then set it up.
            // Get a reference to the GM.
            gm = GameManager.instance;
        }

        // If playerList is not set,
        if (playerList == null)
        {
            // then set it up.
            // Get a reference to the GM's list of players and save it.
            playerList = gm.player_tanks;
        }

        // If target is null,
        if (target == null)
        {
            // then find a new target.
            FindNewTarget();
        }

        // If heardPlayer is non-null,
        if (heardPlayer != null)
        {
            // and if that player is now too far away to hear,
            if (!CanHear(heardPlayer))
            {
                // then set heardPlayer to null. This tank can't hear that player.
                heardPlayer = null;
            }
        }

        // Act depending on the current personalityType.
        switch (personality)
        {
            // In the case that the personality is Hunter,
            case PersonalityType.Hunter:
                // Perform the FSM for Hunter.
                FSM_Hunter();
                break;


            // In the case that the personality is Guard,
            case PersonalityType.Guard:
                // Perform the FSM for Guard.
                FSM_Guard();
                break;


            // In the case that the personality is Assassin,
            case PersonalityType.Assassin:
                // Perform the FSM for Assassin.
                FSM_Assassin();
                break;


            // In the case that the personality is Caravan,
            case PersonalityType.Caravan:
                // Perform the FSM for Caravan.
                FSM_Caravan();
                break;
        }
    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // Changes the Personality of the tank and makes any necessary adjustments.
    private void ChangePersonality(PersonalityType type)
    {
        // Make the change.
        personality = type;

        // Act according to the new type.
        switch (personality)
        {
            // In the case that the new personality is the Hunter,
            case PersonalityType.Hunter:
                // then perform setup for Hunter.
                // Set up the materials so it looks like a Hunter.
                ApplyMaterials(mat_HunterBody, mat_HunterCannon);

                // Revert the tank's speed to its original speed.
                RevertToOriginalSpeed();
                break;


            // In the case that the new personality is the Guard,
            case PersonalityType.Guard:
                // then perform setup for Guard.
                // Set up the materials so it looks like a Guard.
                ApplyMaterials(mat_GuardBody, mat_GuardCannon);

                // Check the waypoints, and set up the Caravan/Guard relationship.
                CheckWaypoints();
                break;


            // In the case that the new personality is the Assassin,
            case PersonalityType.Assassin:
                // then
                // Set up the materials so it looks like a Assassin.
                ApplyMaterials(mat_AssassinBody, mat_AssassinCannon);

                // Revert the tank's speed to its original speed.
                RevertToOriginalSpeed();

                // Ensure the Assassin is in the LayInWait state.
                ChangeState_Assassin(Assassin_AIState.LayInWait);

                break;


            // In the case that the new personality is the Caravan,
            case PersonalityType.Caravan:
                // then
                // Set up the materials so it looks like a Caravan.
                ApplyMaterials(mat_CaravanBody, mat_CaravanCannon);

                // Check the waypoints, and set up the Caravan/Guard relationship.
                CheckWaypoints();

                // Revert the tank's speed to its original speed.
                RevertToOriginalSpeed();
                break;
        }
    }

    // Applies the provided materials to the tank's visuals to help distinguish between Personalities.
    // The body is applied to the shell, and the cannon is applied to the cannon's base and barrel.
    private void ApplyMaterials(Material body, Material cannon)
    {
        // If any of the necessary variables are null,
        if (shell_Visual == null || cannonBase_Visual == null || cannonBarrel_Visual == null ||
            body == null || cannon == null)
        {
            // then log an error and return.
            Debug.LogError("ERROR: Cannot apply materials due to null reference.");
            return;
        }

        // Apply the body material to the tank's shell visual.
        shell_Visual.material = body;

        // Apply the cannon material to the tank's cannon visuals for the base and the barrel.
        cannonBase_Visual.material = cannon;
        cannonBarrel_Visual.material = cannon;
    }


    #region Hunter Methods
    // Perform the FSM for Hunter.
    private void FSM_Hunter()
    {
        // Act depending on the current hunterAIState.
        switch (hunter_AIState)
        {
            // In the case that the state is Chase,
            case Hunter_AIState.Chase:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform Chase protocalls.
                    DoChase();
                }

                // Check for transitions.
                // If the tank's health as fallen to or below the theshhold for fleeing,
                if (data.currentHealth <= (fleeThreshold_Decimal * data.maxHealth))
                {
                    // then change state to CheckForFlee.
                    ChangeState_Hunter(Hunter_AIState.CheckForFlee);
                }
                // Else, tank is above the flee threshold. If target is within firing range,
                else if (Vector3.SqrMagnitude(target.position - tf.position) <= aiSenseRadius_Squared)
                {
                    // then change the state to ChaseAndFire.
                    ChangeState_Hunter(Hunter_AIState.ChaseAndFire);
                }
                break;


            // In the case that the state is ChaseAndFire,
            case Hunter_AIState.ChaseAndFire:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform ChaseAndFire protocalls by chasing and firing.
                    DoChase();
                    // Firing delay automated by the TankCannon itself in conjunction with TankData.
                    cannon.Fire(data.shellSpeed);
                }

                // Check for transition conditions.
                // If the tank's health as fallen to or below the theshhold for fleeing,
                if (data.currentHealth <= (fleeThreshold_Decimal * data.maxHealth))
                {
                    // then change state to CheckForFlee.
                    ChangeState_Hunter(Hunter_AIState.CheckForFlee);
                }
                // Else, tank is above the flee threshold. If target is NOT within firing range,
                else if (Vector3.SqrMagnitude(target.position - tf.position) > aiSenseRadius_Squared)
                {
                    // then change the state to Chase.
                    ChangeState_Hunter(Hunter_AIState.Chase);
                }
                break;


            // In the case that the state is CheckForFlee,
            case Hunter_AIState.CheckForFlee:
                // then perform CheckForFlee protocalls.
                CheckForFlee();

                // Test for transition conditions.
                // If target is within range of the tank's senses,
                if (Vector3.SqrMagnitude(target.position - tf.position) <= aiSenseRadius_Squared)
                {
                    // then change the state to Flee.
                    ChangeState_Hunter(Hunter_AIState.Flee);
                }
                // Else, the tank cannot sense the target.
                else
                {
                    // Change state to Rest.
                    ChangeState_Hunter(Hunter_AIState.Rest);
                }
                break;


            // In the case that the state is Flee,
            case Hunter_AIState.Flee:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform Flee protocalls.
                    DoFlee();
                }

                // Test for transition conditions.
                // If enough time has passed since entering Flee state,
                if ((timeStateEntered + duration_FleeBeforeChecking) <= Time.time)
                {
                    // then change the state back to CheckForFlee.
                    ChangeState_Hunter(Hunter_AIState.CheckForFlee);
                }
                break;


            // In the case that the state is Rest,
            case Hunter_AIState.Rest:
                // then perform Rest protocalls.
                DoRest();

                // Test for transtion conditions.
                // If reached max health while resting,
                if (data.currentHealth >= data.maxHealth)
                {
                    // then change state back to Chase.
                    ChangeState_Hunter(Hunter_AIState.Chase);
                }
                // Else, if the target gets within sense range while resting,
                else if (Vector3.SqrMagnitude(target.position - tf.position) <= aiSenseRadius_Squared)
                {
                    // then set state back to Flee.
                    ChangeState_Hunter(Hunter_AIState.Flee);
                }
                break;
        }
    }

    // Change the Hunter state that the tank AI is in.
    private void ChangeState_Hunter(Hunter_AIState newState)
    {
        // Change the state of this tank's AI accordingly.
        hunter_AIState = newState;

        // Save the time that this change was made.
        timeStateEntered = Time.time;
    }

    // Perform chase protocalls.
    private void DoChase()
    {
        // If target is null,
        if (target == null)
        {
            // then find a new target.
            FindNewTarget();
        }

        // Rotate a bit towards the target.
        motor.RotateTowards(target.position);

        // Check if we can move forward for 1 second without hitting an obstacle.
        // Pass in "data.moveSpeed_Forward" because that is how far the tank can move in 1 second.
        if (CanMoveForward(data.moveSpeed_Forward))
        {
            // If the tank is not already too close to the target,
            if (Vector3.SqrMagnitude(target.position - tf.position) > chasing_CloseEnough_Squared)
            {
                // Move forward.
                motor.Move(data.moveSpeed_Forward);
            }
        }
        // Else, there is an obstacle in the way within 1 second's travel.
        else
        {
            // Enter obstacle avoidance mode.
            avoidanceStage = 1;
        }
    }

    // The tank rests, during which time it will heal its health.
    private void DoRest()
    {
        // If the repair type is set to PerSecond,
        if (repairType == RepairType.PerSecond)
        {
            // then raise the tank's health (repair) by restingHealRate per second.
            data.Repair(restingHealRate * Time.deltaTime);
        }
        // Else, the repair type must be set to PerTick.
        else
        {
            // If enough time has passed since the last tick to tick again,
            if ((timeLastTick + tickLength) <= Time.time)
            {
                // then it is time to tick again.
                // Heal/repair the tank by exactly restingHealRate.
                data.Repair(restingHealRate);

                // Set the new time of last tick.
                timeLastTick = Time.time;
            }
        }
    }
    #endregion Hunter Methods


    #region Guard & Caravan Methods
    // Perform the FSM for Guard.
    private void FSM_Guard()
    {
        // Act according to the current guard_AIState.
        switch (guard_AIState)
        {
            // In the case that the state is Patrol,
            case Guard_AIState.Patrol:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform Patrol protocalls.
                    DoPatrol();
                }

                // Test for transition conditions.
                // If this is a Guard protecting a Caravan,
                if (isGuardProtectingCaravan)
                {
                    // and if that Caravan has died,
                    if (protectedCaravan == null)
                    {
                        // then change personalities to Hunter.
                        ChangePersonality(PersonalityType.Hunter);

                        // Return out of this function.
                        return;
                    }
                }

                // If the Guard can hear a player,
                if (heardPlayer != null)
                {
                    // then change state to RotateTowardSound.
                    ChangeState_Guard(Guard_AIState.RotateTowardSound);
                }
                break;


            // In the case that the state is RotateTowardSound,
            case Guard_AIState.RotateTowardSound:
                // then perform RotateTowardSound protocalls.
                DoRotateTowardSound();

                // Test for transition conditions.
                // If this is a Guard protecting a Caravan,
                if (isGuardProtectingCaravan)
                {
                    // and if that Caravan has died,
                    if (protectedCaravan == null)
                    {
                        // then change personalities to Hunter.
                        ChangePersonality(PersonalityType.Hunter);

                        // Return out of this function.
                        return;
                    }
                }

                // If the Guard no longer hears any players,
                if (heardPlayer == null)
                {
                    // then change state to Patrol.
                    ChangeState_Guard(Guard_AIState.Patrol);
                }
                // Else, if the Guard can see the player,
                else if (CanSee(heardPlayer))
                {
                    // then change state to FireAtIntruder.
                    ChangeState_Guard(Guard_AIState.FireAtIntruder);
                }
                break;


            // In the case that the state is FireAtIntruder,
            case Guard_AIState.FireAtIntruder:
                // then perform FireAtIntruder protocalls.
                DoFireAtIntruder();

                // Test for transition conditions.
                // If this is a Guard protecting a Caravan,
                if (isGuardProtectingCaravan)
                {
                    // and if that Caravan has died,
                    if (protectedCaravan == null)
                    {
                        // then change personalities to Hunter.
                        ChangePersonality(PersonalityType.Hunter);

                        // Return out of this function.
                        return;
                    }
                }

                // If there is no more intruder (either dead or has left the premises),
                if (intruder == null)
                {
                    // and if the intruder can still be heard,
                    if (heardPlayer != null)
                    {
                        // then change back to RotateTowardSound.
                        ChangeState_Guard(Guard_AIState.RotateTowardSound);
                    }
                    // Else, there seems to be no danger.
                    else
                    {
                        // Change back to Patrol.
                        ChangeState_Guard(Guard_AIState.Patrol);
                    }
                }
                break;
        }
    }

    // Perform the FSM for Caravan.
    private void FSM_Caravan()
    {
        // Act according to the current caravan_AIState.
        switch (caravan_AIState)
        {
            // In the case that the state is Transport,
            case Caravan_AIState.Transport:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform Transport protocalls. This is the same as a Guard's Patrol protocalls.
                    DoPatrol();
                }

                // Test for transition conditions.
                // If the Caravan's ecortingGuard has been killed,
                if (escortingGuard == null)
                {
                    // then change to the CheckForFlee state.
                    ChangeState_Caravan(Caravan_AIState.CheckForFlee);
                }
                break;


            // In the case that the state is Flee,
            case Caravan_AIState.CheckForFlee:
                // Perform CheckForFlee protocalls.
                CheckForFlee();

                // Test for transition conditions.
                // If the Caravan can currently hear a player,
                if (heardPlayer != null)
                {
                    // then change state to Flee.
                    ChangeState_Caravan(Caravan_AIState.Flee);
                }
                // Else, the Caravan cannot hear any players.
                else
                {
                    // Switch to Hide state.
                    ChangeState_Caravan(Caravan_AIState.Hide);
                }
                break;


            // In the case that the state is Flee,
            case Caravan_AIState.Flee:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform Transport protocalls. This is the same as a Guard's Patrol protocalls.
                    DoPatrol();
                }

                // Test for transition conditions.
                // If the Caravan has been fleeing long enough,
                if ((timeStateEntered + duration_FleeBeforeChecking) <= Time.time)
                {
                    // then change state to CheckForFlee.
                    ChangeState_Caravan(Caravan_AIState.CheckForFlee);
                }
                break;


            // In the case that the state is Hide,
            case Caravan_AIState.Hide:
                // Perform Hide protocalls.
                DoHide();

                // Test for transition conditions.
                // If the Caravan can currently hear a player,
                if (heardPlayer != null)
                {
                    // then change state to Flee.
                    ChangeState_Caravan(Caravan_AIState.Flee);
                }
                break;
        }
    }

    // Change the Guard state that the tank AI is in.
    private void ChangeState_Guard(Guard_AIState newState)
    {
        // Change the state of this tank's AI accordingly.
        guard_AIState = newState;

        // Save the time that this change was made.
        timeStateEntered = Time.time;

        // If the Guard just switched to FireAtIntruder,
        if (newState == Guard_AIState.FireAtIntruder)
        {
            // then set the intruder equal to the heardPlayer.
            intruder = heardPlayer;
        }
    }

    // Change the Caravan state that the tank AI is in.
    private void ChangeState_Caravan(Caravan_AIState newState)
    {
        // Change the state of this tank's AI accordingly.
        caravan_AIState = newState;

        // Save the time that this change was made.
        timeStateEntered = Time.time;
    }

    // Checks the waypoints on this Guard or Caravan, calling to level them out if necessary
    // and setting up variables for the Caravan/Guard relationship.
    private void CheckWaypoints()
    {
        // If  waypoints has been left empty,
        if (waypoints.Length == 0)
        {
            // then log an error.
            Debug.LogError(gameObject.name + "'s waypoints array is empty! Shutting down the AI.");
            // Turn off this script to prevent errors.
            this.enabled = false;
        }
        // Else, the wapoints array is not empty.
        else
        {
            // If the waypoint gameObjects have not been set up yet (this function called too early),
            if (waypoints[0] == null)
            {
                // then return out to prevent errors.
                return;
            }
            // Else, if the elevation for the next waypoint does not match the tank's elevation,
            else if (waypoints[currentWaypoint].transform.position.y != tf.position.y)
            {
                // then level out all of the waypoints' elevations.
                LevelOutWaypoints();
            }
        }
        
        // If neither of these variables have been set up yet,
        if (protectedCaravan == null && escortingGuard == null)
        {
            // then attempt to get an AI_Controller off of the parent of the next waypoint.
            AI_Controller ai = waypoints[currentWaypoint].gameObject.GetComponentInParent<AI_Controller>();

            // If an AI_Controller was successfully found,
            if (ai != null)
            {
                // and if that ai is a Caravan,
                if (ai.personality == PersonalityType.Caravan)
                {
                    // then this Guard is protecting that Caravan.
                    // Save a reference to the Caravan for this Guard.
                    protectedCaravan = ai.gameObject;

                    // Save a reference to this Guard for the Caravan.
                    ai.escortingGuard = gameObject;

                    // This is a Guard protecting a Caravan. Remember that info.
                    isGuardProtectingCaravan = true;

                    // Change the Guard's speed to 110% of the caravan's speed.
                    data.moveSpeed_Forward =
                        (float)(protectedCaravan.gameObject.GetComponent<TankData>().moveSpeed_Forward * 1.10);
                }
            }
        }
    }

    // Sets all of the appropriate waypoints to the current elevation.
    private void LevelOutWaypoints()
    {
        // Iterate through the waypoints array.
        foreach (Transform waypoint in waypoints)
        {
            // Set each transform's y equal to this tank's y.
            // This prevents the tank from attempting to move into the ground or above it.
            // First, get the position of the waypoint.
            Vector3 p = waypoint.position;

            // Change the y to match.
            p.y = tf.position.y;

            // Set the waypoint's position to the modified value.
            waypoint.position = p;
        }
    }

    // Performs Patrol protocalls (Guard or Caravan).
    private void DoPatrol()
    {
        // If stopped (meaning Tank is set to Stop behavior and has reached the end of the waypoint system),
        if (waypoints_IsStopped)
        {
            // Do nothing.
            // This prevents tank from turning constantly while standing on the final waypoint.
        }
        // else, if the waypoint objects have not been set up yet (this function called too early),
        else if (waypoints[0] == null)
        {
            // then return out to avoid errors.
            return;
        }
        // Else, if we can rotate towards the current waypoint (done during the if call),
        else if (motor.RotateTowards(waypoints[currentWaypoint].position))
        {
            // then the tank was able to rotate towards the waypoint this frame.
            // Do nothing.
        }
        // Else, the tank was unable to rotate towards the waypoint this frame because it is already facing it.
        else
        {
            // If the Guard or Caravan can move forward without hitting and obstacle,
            if (CanMoveForward(data.moveSpeed_Forward))
            {
                // then move forward.
                motor.Move(data.moveSpeed_Forward);
            }
            // Else, there is an obstacle in the way within 1 second's travel.
            else
            {
                // Enter obstacle avoidance mode.
                avoidanceStage = 1;
            }
        }

        // If we are close enough to the waypoint,
        if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) <= waypoints_CloseEnough_Squared)
        {
            // then advance to the next waypoint according to the chosen behavior.
            NextWaypoint_Dispatch();
        }
    }

    // Performs FireAtIntruder protocalls.
    private void DoFireAtIntruder()
    {
        // If the intruder is still non-null,
        if (intruder != null)
        {
            // and if the Guard is locked on to the intruder,
            if (CanLockOn(intruder))
            {
                // then fire at the intruder (fire rate is handled by TankCannon in conjunction with TankData).
                cannon.Fire(data.shellSpeed);
            }
            // Else, if the Guard can still see the intruder,
            else if (CanSee(intruder))
            {
                // then rotate more toward the intruder (attempt to lock on).
                motor.RotateTowards(intruder.position);
            }
            // Else, the intruder is no longer in sight.
            else
            {
                // Set intruder to null.
                intruder = null;

                // If the heardPlayer is still non-null,
                if (heardPlayer != null)
                {
                    // and if that player has left the Guard's hearing range,
                    if (Vector3.SqrMagnitude(heardPlayer.position - tf.position) > hearingDistance_Squared)
                    {
                        // then set heardPlayer to null.
                        heardPlayer = null;
                    }
                }
            }
        }
    }

    // Chooses the next waypoint to head towards based on the chosen behavior (loopType).
    private void NextWaypoint_Dispatch()
    {
        // Based on the chosen loop type, perform a certain action.
        switch (loopType)
        {
            // If set to Stop,
            case LoopType.Stop:
                // then perform the "next waypoint" actions for the LoopType.Stop behavior.
                NextWaypoint_Stop();
                break;

            // If set to Loop,
            case LoopType.Loop:
                // then perform the "next waypoint" actions for the LoopType.Loop behavior.
                NextWaypoint_Loop();
                break;

            // If set to PingPong,
            case LoopType.PingPong:
                // then perform the "next waypoint" actions for the LoopType.PingPong behavior.
                NextWaypoint_PingPong();
                break;
        } // currentWaypoint has been set.

        // If the elevation for the next waypoint does not match the tank's elevation,
        if (waypoints[currentWaypoint].transform.position.y != tf.position.y)
        {
            // then level out all of the waypoints' elevations.
            LevelOutWaypoints();
        }
    }

    // Performs the "next waypoint" actions for the LoopType.Stop behavior.
    private void NextWaypoint_Stop()
    {
        // If NOT already at the end,
        if (currentWaypoint < waypoints.Length - 1)
        {
            // then add 1 to go tot he next waypoint in the list.
            currentWaypoint++;
        }
        // Else, already at the end.
        else
        {
            // Tank should not stop.
            waypoints_IsStopped = true;

            // If this is a Caravan,
            if (personality == PersonalityType.Caravan)
            {
                // then lower its pointValue appropriately.
                data.ChangePointsValue(pointDropPerCircuit);
            }
        }
    }

    // Performs the "next waypoint" actions for the LoopType.Loop behavior.
    private void NextWaypoint_Loop()
    {
        // If NOT already at the end,
        if (currentWaypoint < waypoints.Length - 1)
        {
            // then add 1 to the currentWaypoint.
            currentWaypoint++;
        }
        // Else, already at the end.
        else
        {
            // Reset the current waypoint index to 0.
            currentWaypoint = 0;

            // If this is a Caravan,
            if (personality == PersonalityType.Caravan)
            {
                // then lower its pointValue appropriately.
                data.ChangePointsValue(pointDropPerCircuit);
            }
        }
    }

    // Performs the "next waypoint" actions for the LoopType.PingPong behavior.
    private void NextWaypoint_PingPong()
    {
        // If going forward,
        if (waypoints_IsGoingForward)
        {
            // and if NOT already at the end,
            if (currentWaypoint < waypoints.Length - 1)
            {
                // then add 1 to the currentWaypoint.
                currentWaypoint++;
            }
            // Else, already at the end (going forward).
            else
            {
                // Subtract 1 from the currentWaypoint.
                currentWaypoint--;

                // Flip the direction.
                waypoints_IsGoingForward = false;
            }
        }
        // Else, tank is currently heading backwards through the waypoints.
        else
        {
            // If NOT already at the beginning,
            if (currentWaypoint > 0)
            {
                // then subtract 1 from currenWaypoint.
                currentWaypoint--;
            }
            // Else, already back to the beginning of the array.
            else
            {
                // Add 1 to the index.
                currentWaypoint++;

                // Flip the direction.
                waypoints_IsGoingForward = true;

                // If this is a Caravan,
                if (personality == PersonalityType.Caravan)
                {
                    // then lower its pointValue appropriately.
                    data.ChangePointsValue(pointDropPerCircuit);
                }
            }
        }
    }

    // Performs Hide protocalls for the Caravan.
    private void DoHide()
    {
        // Do nothing.
    }
    #endregion Guard & Caravan Methods


    #region Assassin Methods
    // Perform the FSM for Assassin.
    private void FSM_Assassin()
    {
        // Act according to the current assassin_AIState.
        switch (assassin_AIState)
        {
            // In the case that the state is LayInWait,
            case Assassin_AIState.LayInWait:
                // then perform LayInWait protocalls.
                DoLayInWait();

                // Test for transition conditions.
                // If the Assassin takes damage while in LayInWait state,
                if (data.currentHealth < hiding_UndamagedHealth)
                {
                    // then switch to Hunter personality.
                    ChangePersonality(PersonalityType.Hunter);
                }
                // Else, if a player is heard,
                else if (heardPlayer != null)
                {
                    // then change state to RotateTowardSound.
                    ChangeState_Assassin(Assassin_AIState.TakeAim);
                }
                break;


            // In the case that the state is TakeAim,
            case Assassin_AIState.TakeAim:
                // then perform TakeAim protocalls.
                DoTakeAim();

                // Test for transition conditions.
                // If the victim is now outside of earshot,
                if (!CanHear(victim))
                {
                    // then return to the LayInWait state.
                    ChangeState_Assassin(Assassin_AIState.LayInWait);
                }
                // Else, can still hear the player.
                else
                {
                    // Test if yet aimed directly at player.
                    // This way, motor.RotateTowards() is called twice as often as a normal tank,
                    // so that the Assassin is very effective for its one shot.
                    // If the Assassin is already facing exactly towards the heardPlayer,
                    if (!motor.RotateTowards(victim.position))
                    {
                        // and if the Assassin can clearly see its target,
                        if (CanSee(victim))
                        {
                            // then the Assassin is ready to take its shot. Change to Assassinate state.
                            ChangeState_Assassin(Assassin_AIState.Assassinate);
                        }
                        // Else, the shot is lined up, but the Assassin has no Line of Sight.
                    }
                    // Else, the Assassin still needed to take better aim before firing.
                }
                break;


            // In the case that the state is Assassinate,
            case Assassin_AIState.Assassinate:
                // then perform Assassinate protocalls.
                DoAssassinate();

                // Test for transition conditions.
                // If the shot has been fired,
                if (shotFired)
                {
                    // then change state to VerifyKill.
                    ChangeState_Assassin(Assassin_AIState.VerifyKill);
                }
                break;


            // In the case that the state is VerifyKill,
            case Assassin_AIState.VerifyKill:
                // then perform VerifyKill protocalls.
                DoVerifyKill();

                // Test for transition protocalls.
                // If the target is dead (target should match the intended victim of the attack),
                if (victim == null)
                {
                    // then the attack succeeded. No need to change to Hunter Personality.
                    // Change back to LayInWait state.
                    ChangeState_Assassin(Assassin_AIState.LayInWait);
                }
                // Else, the victim lives. If we have waited long enough to be sure the shot did not kill them,
                else if ((timeStateEntered + duration_VerifyKill) < Time.time)
                {
                    // then null out victim,
                    victim = null;

                    // and change Personality to Hunter.
                    ChangePersonality(PersonalityType.Hunter);
                }
                break;
        }
    }

    // Change the Assassin state that the tank AI is in.
    private void ChangeState_Assassin(Assassin_AIState newState)
    {
        // Change the state of this tank's AI accordingly.
        assassin_AIState = newState;

        // Save the time that this change was made.
        timeStateEntered = Time.time;

        // If the state was changed to LayInWait,
        if (assassin_AIState == Assassin_AIState.LayInWait)
        {
            // then null out victim.
            victim = null;

            // Save the current health (so it knows when it's been damaged).
            hiding_UndamagedHealth = data.currentHealth;
        }
    }

    // Perform the LayInWait protocalls.
    private void DoLayInWait()
    {
        // If heardPlayer is still non-null,
        if (heardPlayer != null)
        {
            // then set that player as the Assassin's victim.
            victim = heardPlayer;
        }

        // If hiding_UndamagedHealth is 0 for some reason,
        if (hiding_UndamagedHealth == 0)
        {
            // then set it equal to current health.
            hiding_UndamagedHealth = data.currentHealth;
        }
    }

    // Perform the TakeAim protocalls.
    private void DoTakeAim()
    {
        // If the victim is still non-null,
        if (victim != null)
        {
            // and if the victim has left the tank's hearing radius,
            if (Vector3.SqrMagnitude(victim.position - tf.position) > hearingDistance_Squared)
            {
                // then set victim back to null.
                victim = null;
            }
            // Else, the victim is still within earshot.
            else
            {
                // Rotate toward that player.
                motor.RotateTowards(victim.position);
            }
        }
    }

    // Perform the Assassinate protocalls.
    private void DoAssassinate()
    {
        // Verify that no round has yet been fired.
        if (!shotFired)
        {
            // Fire a very fast, damaging attack at the target.
            cannon.Fire((data.shellSpeed * sneakAttack_SpeedMultiplier), sneakAttack_DamageMultiplier);

            // The shot has been fired.
            shotFired = true;
        }
    }

    // Perform the VerifyKill protocalls.
    private void DoVerifyKill()
    {

    }
    #endregion Assassin Methods


    // Checks if the tank can move forward to its current target (or if an obstacle is blocking).
    private bool CanMoveForward(float speed)
    {
        // Create a raycast variable to hold the "out" in the later raycast.
        RaycastHit hit;

        // Cast the ray forward to where the tank could move in 1 second's time.
        // If it hit something (an obstacle),
        if (Physics.Raycast(tf.position, tf.forward, out hit, speed))
        {
            // then the raycast hit something. If that something was NOT the player,
            if (!hit.transform.gameObject.GetComponent<InputController>())
            {
                // Get the bounds of the obstacle hit.
                Bounds bounds = hit.transform.GetComponentInChildren<Renderer>().bounds;

                // Determine which direction the tank should turn to most efficiently evade the obstacle.
                avoidance_TurnDirection = DetermineTurnDirection(speed);

                // Return false (we can NOT move forward for 1 second because of an obstacle).
                return false;
            }
            // Else, it was the player.
            else
            {
                // The player is not an obstacle, so the tank can move forward. Return true.
                return true;
            }
        }
        // Else, the way is clear.
        else
        {
            // Return true (we CAN move forward for 1 second without hitting an obstacle).
            return true;
        }
    }

    // Checks if the tank should flee from the target or rest.
    private void CheckForFlee()
    {
        // Do nothing.
    }

    // The tank will flee from the target.
    private void DoFlee()
    {
        // If the tank has been fleeing long enough to care about avoiding obstacles,
        if ((timeStateEntered + duration_FleeBeforeAvoiding) <= Time.time)
        {
            // then perform avoidance protocalls while fleeing.
            // Check if we can move forward for 1 second without hitting an obstacle.
            // Pass in "data.moveSpeed_Forward" because that is how far the tank can move in 1 second.
            if (CanMoveForward(data.moveSpeed_Forward))
            {
                // Flee the current target.
                Flee();
            }
            // Else, there is an obstacle in the way within 1 second's travel.
            else
            {
                // Enter obstacle avoidance mode.
                avoidanceStage = 1;
            }
        }
        // Else, the tank does not yet care about obstacle avoidance.
        else
        {
            // Flee the target.
            Flee();
        }
    }

    // The tank will flee from the target. Always called by DoFlee(), either after checking for obstacles or not.
    private void Flee()
    {
        // If target is null, or if the target is not the same as the most recently heard player,
        if (target == null || target != heardPlayer)
        {
            // then assign a new target. The heard player is the default target.
            FindNewTarget();
        }

        // Find the vector away from the target by finding the vector TO the target * -1.
        Vector3 vectorAwayFromTarget = -1 * (target.position - tf.position);

        // Normalize that vector (gives it a magnitude of 1).
        vectorAwayFromTarget.Normalize();

        // Multiply the normalized vector * the designer-chosen fleeDistance.
        vectorAwayFromTarget *= fleeDistance;

        // Add that value to the tank's current position to determine the place we are traveling to.
        Vector3 fleePosition = tf.position + vectorAwayFromTarget;

        // Now rotate towards that position.
        motor.RotateTowards(fleePosition);

        // Move forward.
        motor.Move(data.moveSpeed_Forward);
    }

    // Perform obstacle avoidance based on which stage the tank is in.
    private void DoAvoidance()
    {
        // If the tank is in stage 1,
        if (avoidanceStage == 1)
        {
            // Then perform stage 1 obstacle avoidance.
            ObstacleAvoidance_Stage1();
        }
        // Else, if tha tank is in stage 2,
        else if (avoidanceStage == 2)
        {
            // Then perform stage 2 obstacle avoidance.
            ObstacleAvoidance_Stage2();
        }
        // Else, we can't move forward.
        else
        {
            // Back to ObstacleAvoidance stage 1.
            avoidanceStage = 1;
        }
    }

    // Perform obstacle avoidance for stage 1, turning until there is no obstacle blocking.
    private void ObstacleAvoidance_Stage1()
    {
        // Turn according to avoidance_TurnDirection. 1 for right, -1 for left.
        motor.Turn(avoidance_TurnDirection * data.turnSpeed);

        // If moving forward 1 second is now possible,
        if (CanMoveForward(data.moveSpeed_Forward))
        {
            // then move on to stage 2.
            avoidanceStage = 2;

            // Set the amount of time the tank will stay in stage 2.
            avoidanceStage2_ExitTime = avoidanceTime;
        }
        // Else, this will be performed again in the next frame.
    }

    // Perform obstacle avoidance for stage 2, moving forward for "avoidanceTime" seconds or until blocked again.
    private void ObstacleAvoidance_Stage2()
    {
        // If moving forward 1 second is now possible,
        if (CanMoveForward(data.moveSpeed_Forward))
        {
            // then move forward.
            motor.Move(data.moveSpeed_Forward);

            // Subtract from the exitTime timer.
            avoidanceStage2_ExitTime -= Time.deltaTime;

            // If the tank has now spent the appropriate amount of time in stage 2,
            if (avoidanceStage2_ExitTime <= 0)
            {
                // then return to chase mode by entering stage 0.
                avoidanceStage = 0;
            }
        }
        // Else, the tank cannot move forward for 1 second (there is an obstacle blocking).
        else
        {
            // Go back to stage 1 of avoidance.
            avoidanceStage = 1;
        }
    }

    private int DetermineTurnDirection(float speed)
    {
        // The distance these rays should cast to. Double the speed of the original ray that found an obstacle.
        float raySpeed = speed * raycastLength_Modifier;

        // Holds the hit info for the raycasts.
        RaycastHit hit;

        // The angle at which the raycast should be cast.
        // First, set it to a 45 degree angle to the tank's left.
        Vector3 rayAngle = (tf.forward - tf.right).normalized;

        // Send out a raycast at that angle. If it hits something,
        if (Physics.Raycast(tf.position, rayAngle, out hit, raySpeed))
        {
            // then turning left might not be faster.
            // Do nothing.
        }
        // Else, the raycast hit nothing.
        else
        {
            // Turning left will be faster.
            // Return -1.
            return -1;
        }

        // The ray must have hit either itself or the original obstacle. Adjust to send new ray to the right.
        // Set the rayAngle to 45 degrees to the right.
        rayAngle = (tf.forward + tf.right).normalized;

        // Send out a raycast at that angle. If it hits something,
        if (Physics.Raycast(tf.position, rayAngle, out hit, raySpeed))
        {
            // then there is an obstacle to the right, as well as the left.
            // Do nothing.
        }
        // Else, the raycast hit nothing.
        else
        {
            // Turning right will be faster.
            // Return 1.
            return 1;
        }

        // Both rays must have hit either the tank or the original obstacle.
        // Return -1 to turn left as default.
        return -1;
    }

    // Checks if the tank can see the passed-in player.
    private bool CanSee(Transform player)
    {
        // Get a vector from this tank to the player.
        Vector3 vectorToPlayer = player.position - tf.position;

        // Get the angle between this tank's "forward" (in local space) and the vector to the player. 
        float angleToPlayer = Vector3.Angle(vectorToPlayer, tf.forward);

        // If the player in within this tank's sight angle,
        if (angleToPlayer <= fov_sightAngle)
        {
            // then we need to check if there are any obstacles blocking sight.
            // Create a raycast hit to store the result of the upcoming raycast.
            RaycastHit hit;

            // If the raycast hits something,
            if (Physics.Raycast(tf.position, vectorToPlayer, out hit, fov_sightRange))
            {
                // and if the first object hit is the player,
                if (hit.transform == player)
                {
                    // then this tank can see the player. Return true.
                    return true;
                }
            }
        }
        // Default of false.
        return false;
    }

    // Checks if the tank can hear the passed-in player.
    private bool CanHear(Transform player)
    {
        if (player == null)
        {
            print("player is null");
        }
        if (tf == null)
        {
            print("tf is null");
        }

        // If the player is close enough to hear,
        if (Vector3.SqrMagnitude(player.position - tf.position) < hearingDistance_Squared)
        {
            // then return true.
            return true;
        }
        // Else, the player is too far away to hear.
        else
        {
            // Return false.
            return false;
        }
    }

    // Checks if the passed-in player is within the lockedOn max angle.
    private bool CanLockOn(Transform player)
    {
        // Get a vector from this tank to the player.
        Vector3 vectorToPlayer = player.position - tf.position;

        // Get the angle between this tank's "forward" (in local space) and the vector to the player. 
        float angleToPlayer = Vector3.Angle(vectorToPlayer, tf.forward);

        // If the player in within this tank's sight angle,
        if (angleToPlayer <= lockedOnAngle)
        {
            // then we need to check if there are any obstacles blocking sight.
            // Create a raycast hit to store the result of the upcoming raycast.
            RaycastHit hit;

            // If the raycast hits something,
            if (Physics.Raycast(tf.position, vectorToPlayer, out hit, fov_sightRange))
            {
                // and if the first object hit is the player,
                if (hit.transform == player)
                {
                    // then this tank can lock on to the player. Return true.
                    return true;
                }
            }
        }
        // Default of false.
        return false;
    }

    // Performs RotateTowardSound protocalls.
    private void DoRotateTowardSound()
    {
        // If the heard player is still non-null,
        if (heardPlayer != null)
        {
            // and if the heardPlayer has left the tank's hearing radius,
            if (Vector3.SqrMagnitude(heardPlayer.position - tf.position) > hearingDistance_Squared)
            {
                // then set heardPlayer back to null.
                heardPlayer = null;
            }
            // Else, the heard player is still within earshot.
            else
            {
                // Rotate toward that player.
                motor.RotateTowards(heardPlayer.position);
            }
        }
    }

    // Find a new target, either from the most recently heard player, or from the playerList.
    private void FindNewTarget()
    {
        // If this tank can currently hear another tank,
        if (heardPlayer != null)
        {
            // then that tank is the new target.
            target = heardPlayer;
        }
        // Else, if the playerList is not empty,
        else if (playerList.Count > 0)
        {
            // then advance to the next player so that all the tanks don't focus on one player.
            NextPlayerToTarget();

            // Get the transform component from the next player tank in the GM's list.
            target = playerList[playerToTarget].gameObject.transform;
        }
    }

    // Advances the int representing the index of the next alive player to target, according to the GM's list.
    private void NextPlayerToTarget()
    {
        // If we're not already at the end of the player list,
        if (playerToTarget < playerList.Count - 1)
        {
            // then add one to the index var.
            playerToTarget++;
        }
        // Else, we are at the end of the player list.
        else
        {
            // Set the var to 0.
            playerToTarget = 0;
        }
    }

    // Reverts the tank to its original forward move speed.
    private void RevertToOriginalSpeed()
    {
        // Set the tank's speed equal to the speed it had at the beginning of the game.
        data.moveSpeed_Forward = data.originalSpeed_Forward;
    }
    #endregion Dev-Defined Methods
}
