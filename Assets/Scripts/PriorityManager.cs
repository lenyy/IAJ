using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.IAJ.Unity.Movement.Arbitration;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Util;
using UnityEngine.UI;
using Assets.Scripts.IAJ.Unity.Movement;
using Random = UnityEngine.Random;

public class PriorityManager : MonoBehaviour
{
    public const float X_WORLD_SIZE = 55;
    public const float Z_WORLD_SIZE = 32.5f;
    public const float AVOID_MARGIN = 4.0f;
    public const float MAX_SPEED = 20.0f;
    public const float MAX_ACCELERATION = 40.0f;
	public const float RADIUS = 20.0f;
    public const float MAX_LOOK_AHEAD = 7.0f;
    public const float MAX_WHISKERS_LOOK_AHEAD = 4.0f;
    public const float MAX_WHISKERS_SPAN = 45.0f;
    public const float FAN_ANGLE = MathConstants.MATH_PI / 3;
	public const float SEPARATION_FACTOR = 15.0f;
    public const float DRAG = 0.1f;
	public const int NUMBER_OF_BOIDS = 30;

    private Text RedMovementText { get; set; }

    private BlendedMovement Blended { get; set; }

    private PriorityMovement Priority { get; set; }

    private List<DynamicCharacter> Characters { get; set; }

	private List<KinematicData> flock { get; set; }

	// Use this for initialization
	void Start () 
	{
		var textObj = GameObject.Find ("InstructionsText");
		if (textObj != null) 
		{
			textObj.GetComponent<Text>().text = 
				"Instructions\n\n" +
				"B - Blended\n" +
                "Q - stop"; 
		}
			
		var redObj = GameObject.Find ("Red");

	 
	    var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
		flock = new List<KinematicData> ();
		this.Characters = this.CloneSecondaryCharacters(redObj, NUMBER_OF_BOIDS, obstacles);

		foreach (DynamicCharacter character in this.Characters) {
			this.InitializeCharacters (character, obstacles);
		}
	}

	private void InitializeCharacters(DynamicCharacter character ,GameObject[] obstacles)
    {

        this.Blended = new BlendedMovement
        {
			Character = character.KinematicData
        };
        
	    foreach (var obstacle in obstacles)
	    {
            var avoidObstacleMovement = new DynamicAvoidObstacle(obstacle)
            {
                MaxAcceleration = MAX_ACCELERATION,
                AvoidMargin = AVOID_MARGIN,
                MaxLookAhead = MAX_LOOK_AHEAD,
                Character = character.KinematicData,
                WhiskersLength = MAX_WHISKERS_LOOK_AHEAD,
                WhiskersSpan = MAX_WHISKERS_SPAN,
                MovementDebugColor = Color.magenta
            };
            this.Blended.Movements.Add(new MovementWithWeight(avoidObstacleMovement, 5.0f));
        }

        var separation = new DynamicSeparation () 
		{
			Character = character.KinematicData,
			flock = this.flock,
			maxAcceleration = MAX_ACCELERATION,
			radius = RADIUS,
			separationFactor = SEPARATION_FACTOR
		};

		var flockVelocityMatching = new DynamicFlockVelocityMatching () {
			Character = character.KinematicData,
			flock = this.flock,
			radius = RADIUS,
			fanAngle = FAN_ANGLE,
			MaxAcceleration = MAX_ACCELERATION
        };

        var cohesion = new DynamicCohesion() {
            Character = character.KinematicData,
            flock = this.flock,
            MaxAcceleration = MAX_ACCELERATION,
            radius = RADIUS,
            fanAngle = FAN_ANGLE
		};

		var straightAhead = new DynamicStraightAhead
		{
			MaxAcceleration = MAX_ACCELERATION,
			Character = character.KinematicData,
			MovementDebugColor = Color.yellow
		};
				
		this.Blended.Movements.Add(new MovementWithWeight(straightAhead, 3));
        this.Blended.Movements.Add(new MovementWithWeight(separation, 3));
		this.Blended.Movements.Add(new MovementWithWeight(flockVelocityMatching, 2));
		this.Blended.Movements.Add(new MovementWithWeight(cohesion, 3));

		character.Movement = this.Blended;

    }
		

    private List<DynamicCharacter> CloneSecondaryCharacters(GameObject objectToClone,int numberOfCharacters, GameObject[] obstacles)
    {
        var characters = new List<DynamicCharacter>();
        for (int i = 0; i < numberOfCharacters; i++)
        {
            var clone = GameObject.Instantiate(objectToClone);
            //clone.transform.position = new Vector3(30,0,i*20);
            clone.transform.position = this.GenerateRandomClearPosition(obstacles);
            var character = new DynamicCharacter(clone)
            {
                MaxSpeed = MAX_SPEED,
                Drag = DRAG
            };
            //character.KinematicData.orientation = (float)Math.PI*i;
            characters.Add(character);
			this.flock.Add(character.KinematicData);
        }

        return characters;
    }


    private Vector3 GenerateRandomClearPosition(GameObject[] obstacles)
    {
        Vector3 position = new Vector3();
        var ok = false;
        while (!ok)
        {
            ok = true;

            position = new Vector3(Random.Range(-X_WORLD_SIZE,X_WORLD_SIZE), 0, Random.Range(-Z_WORLD_SIZE,Z_WORLD_SIZE));

            foreach (var obstacle in obstacles)
            {
                var distance = (position - obstacle.transform.position).magnitude;

                //assuming obstacle is a sphere just to simplify the point selection
                if (distance < obstacle.transform.localScale.x + AVOID_MARGIN)
                {
                    ok = false;
                    break;
                }
            }
        }

        return position;
    }

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Q)) 
		{
			
		} 
		else if (Input.GetKeyDown (KeyCode.B))
		{
		    
		}

	    foreach (var character in this.Characters)
	    {
	        this.UpdateMovingGameObject(character);
	    }
			
	}

    private void UpdateMovingGameObject(DynamicCharacter movingCharacter)
    {
        if (movingCharacter.Movement != null)
        {
            movingCharacter.Update();
            movingCharacter.KinematicData.ApplyWorldLimit(X_WORLD_SIZE,Z_WORLD_SIZE);
            movingCharacter.GameObject.transform.position = movingCharacter.Movement.Character.position;
        }
    }
		
}
