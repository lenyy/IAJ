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
    public const float CAMERA_Y = 72.4f;
    public const float X_WORLD_SIZE = 55;
    public const float Z_WORLD_SIZE = 32.5f;
    public const float AVOID_MARGIN = 4.0f;
    public const float MAX_SPEED = 20.0f;
    public const float MAX_ACCELERATION = 40.0f;
	public const float RADIUS = 20.0f;
    public const float FLEE_RADIUS = 30.0f;
    public const float MAX_LOOK_AHEAD = 7.0f;
    public const float MAX_WHISKERS_LOOK_AHEAD = 4.0f;
    public const float MAX_WHISKERS_SPAN = 45.0f;
    public const float FAN_ANGLE = MathConstants.MATH_PI / 3;
	public const float SEPARATION_FACTOR = 15.0f;
    public const float DRAG = 0.1f;
	public const int MAX_NUMBER_OF_BOIDS = 50;
    public const int MIN_NUMBER_OF_BOIDS = 20;

    private Text RedMovementText { get; set; }

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

	 	var initialCharacter = new DynamicCharacter(redObj)
            	{
                	MaxSpeed = MAX_SPEED,
                	Drag = DRAG
            	};
	    var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
		flock = new List<KinematicData> ();

		this.Characters = this.CloneSecondaryCharacters(redObj, Random.Range(MIN_NUMBER_OF_BOIDS, MAX_NUMBER_OF_BOIDS), obstacles);
		this.Characters.Add(initialCharacter);


		foreach (DynamicCharacter character in this.Characters) {
			this.InitializeCharacters (character, obstacles);
		}
	}

	private void InitializeCharacters(DynamicCharacter character ,GameObject[] obstacles)
    {

        var Blended = new BlendedMovement
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
            Blended.Movements.Add(new MovementWithWeight(avoidObstacleMovement, 7));
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
				
		Blended.Movements.Add(new MovementWithWeight(straightAhead, 3));
        Blended.Movements.Add(new MovementWithWeight(separation, 4));
		Blended.Movements.Add(new MovementWithWeight(flockVelocityMatching, 3));
		Blended.Movements.Add(new MovementWithWeight(cohesion, 3));

		character.Movement = Blended;

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
        Camera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Vector3 PointInWorld = new Vector3();
        var buttonClicked = false;

        if (Input.GetKeyDown (KeyCode.Mouse0)) 
		{
            
            var mousePos = Input.mousePosition;
            mousePos.z = CAMERA_Y;
            PointInWorld = camera.ScreenToWorldPoint(mousePos);
            buttonClicked = true;
            Debug.Log("world point: " + PointInWorld);
		} 

	    foreach (var character in this.Characters)
	    {
            BlendedMovement movement = (BlendedMovement)character.Movement;
            MovementWithWeight fleeClick = movement.Movements.Find(x => x.Movement.Name == "Flee");

            if (buttonClicked)
            {
                var distanceToPoint = (character.KinematicData.position - PointInWorld).magnitude;

                if(distanceToPoint < FLEE_RADIUS)
                {
                    var dynamicFlee = new DynamicFlee()
                    {
                        Character = character.KinematicData,
                        Target = new KinematicData(),
                        MaxAcceleration = MAX_ACCELERATION
                    };

                    if(fleeClick != null)
                    {
                        movement.Movements.Remove(fleeClick);
                    }

                    dynamicFlee.Target.position = PointInWorld;
                    dynamicFlee.Target.position.y = character.KinematicData.position.y;

                    movement.Movements.Add(new MovementWithWeight(dynamicFlee, 7));
                }
            } else
            {
                var distanceToPoint = (character.KinematicData.position - PointInWorld).magnitude;

                if (distanceToPoint > FLEE_RADIUS)
                {
                    if (fleeClick != null)
                    {
                        movement.Movements.Remove(fleeClick);
                    }
                }

            }


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
