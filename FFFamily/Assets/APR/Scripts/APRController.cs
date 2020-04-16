using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class APRController : MonoBehaviour
{
    [Header("测试玩家")]
    //Enable controls
    public bool useControls;
    [Header("无手柄测试按键")]
    //Player input controls
    public string moveForward = "w";
    public string moveBackward = "s";
    public string turnRight = "d";
    public string turnLeft = "a";
    public string jumpGetup = "space";
    public string punchRight = "e";
    public string punchLeft = "q";
    public string reachRight = "p";
    public string reachLeft = "o";
    public string pickupThrow = "f";
    [Header("设置为几号玩家")]
    public int PlayerNum = 1;
    [SerializeField]
    private PlayerInput input;
    [Header("The Layer Only This Player Is On")]
    //Player Layer
    public string thisPlayerLayer;
    
    [Header("Player Parameters")]
	//Player parameters
	public float MoveSpeed;
	public float turnSpeed;
	public float balanceHeight;
	public float StepDuration;
	public float StepHeight;
	public float jumpForce;
	public float PunchForce;
    public float ThrowForce;
	public float FeetMountForce;
	
	//Actions
    private float timer;
    private float Step_R_timer;
    private float Step_L_timer;
	private bool isKeyDown;
	private bool WalkForward;
	private bool WalkBackward;
	private bool StepRight;
    private bool StepLeft;
    private bool Alert_Leg_Right;
    private bool Alert_Leg_Left;
	private bool balanced = true;
	private bool GettingUp;
	private bool KnockedOut;
	private bool isJumping;
	private bool Jump;
	private bool inAir;
	private bool Landed = true;
	private bool Grounded = true;
    public bool ReachingRight;
    public bool ReachingLeft;
    private bool Punching;
    private bool ResetPose;
    private bool PickedUp;
    private bool Threw;
    
    //Active Ragdoll Player parts
	public GameObject
	//
	Root, Body, Head,
	UpperRightArm, LowerRightArm,
	UpperLeftArm, LowerLeftArm, 
	UpperRightLeg, LowerRightLeg,
	UpperLeftLeg, LowerLeftLeg,
	RightFoot, LeftFoot;
	
	//Active Ragdoll Player Parts Array
	private GameObject[] APR_Parts;
    private Quaternion[] APR_Parts_Orgin;
    //Hands
	public Rigidbody RightHand, LeftHand;
    
	//Center of mass point
	public Transform COMP;
	private Vector3 CenterOfMassPoint;
    
    [Header("Hand Dependancies")]
    //Hand dependancies
    public HandController GrabRight;
    public HandController GrabLeft;
    private Rigidbody GrabbedRight;
    private Rigidbody GrabbedLeft;
    
	[Header("Player Editor Debug Mode")]
	//Debug
	public bool editorDebugMode;
	
	
	
	//Joint Drives on & off
	JointDrive
	//
	BalanceOn, PoseOn, DriveOff;
	
	//Original pose target rotation
	Quaternion
	//
	HeadTarget, BodyTarget,
	UpperRightArmTarget, LowerRightArmTarget,
	UpperLeftArmTarget, LowerLeftArmTarget,
	UpperRightLegTarget, LowerRightLegTarget,
	UpperLeftLegTarget, LowerLeftLegTarget;

    private Dictionary<string, RagdollAnim> anims = new Dictionary<string, RagdollAnim>();
    private string animPath = "RagdollAnim";
	
    void Awake()
	{
        input = new PlayerInput(PlayerNum);
		//Setup joint drives
		BalanceOn = new JointDrive();
        BalanceOn.positionSpring = 5000;
        BalanceOn.positionDamper = 0;
        BalanceOn.maximumForce = Mathf.Infinity;
        
        PoseOn = new JointDrive();
        PoseOn.positionSpring = 500;
        PoseOn.positionDamper = 0;
        PoseOn.maximumForce = Mathf.Infinity;
		
		DriveOff = new JointDrive();
        DriveOff.positionSpring = 15;
        DriveOff.positionDamper = 0;
        DriveOff.maximumForce = Mathf.Infinity;
		
		//Setup/reroute active ragdoll parts to array
		APR_Parts = new GameObject[]
		{
			//array index numbers
			
			//0
			Root,
			//1
			Body,
			//2
			Head,
			//3
			UpperRightArm,
			//4
			LowerRightArm,
			//5
			UpperLeftArm,
			//6
			LowerLeftArm,
			//7
			UpperRightLeg,
			//8
			LowerRightLeg,
			//9
			UpperLeftLeg,
			//10
			LowerLeftLeg,
			//11
			RightFoot,
			//12
			LeftFoot,
            //13
            RightHand.gameObject,
            //14
            LeftHand.gameObject
		};
        APR_Parts_Orgin = new Quaternion[APR_Parts.Length];
        // Setup original Quaternion for joint rotation
        for(int i = 0;i<APR_Parts.Length;i++)
        {
            APR_Parts_Orgin[i] = APR_Parts[i].transform.localRotation;
        }
		//Setup original pose for joint drives
        BodyTarget = APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation;
		HeadTarget = APR_Parts[2].GetComponent<ConfigurableJoint>().targetRotation;
		UpperRightArmTarget = APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation;
        LowerRightArmTarget = APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation;
        UpperLeftArmTarget = APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation;
        LowerLeftArmTarget = APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation;
		UpperRightLegTarget = APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation;
        LowerRightLegTarget = APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation;
        UpperLeftLegTarget = APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation;
        LowerLeftLegTarget = APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation;
	}
	
	//Call Update Functions
    void Update()
    {
    
        if(useControls)
        {
            InputControls();
            
        }
        
        if(!KnockedOut)
        {
            GroundCheck();
            Balance();
            CenterOfMass();
            Posing();
        }
		
    }
	
	//Call FixedUpdate Functions
	void FixedUpdate()
	{
        if(!KnockedOut)
        {
		  Jumping();
		  Walking();
        }
	}
	
	
	////////////////
	//Input Controls
	////////////////
	void InputControls()
	{
        //Walk forward
        if ((Input.GetKey(moveForward)||Input.GetAxis(input.vertical)>0.1f) && balanced && !KnockedOut)
        {
            var v3 = APR_Parts[0].GetComponent<Rigidbody>().transform.forward * MoveSpeed;
            v3.y = APR_Parts[0].GetComponent<Rigidbody>().velocity.y;
            APR_Parts[0].GetComponent<Rigidbody>().velocity = v3;
            WalkForward = true;
            isKeyDown = true;
        }
		
        if(Input.GetKeyUp(moveForward)||(Input.GetAxis(input.vertical)<0.1f&& Input.GetAxis(input.vertical)>=0))
        {
            WalkForward = false;
            isKeyDown = false;
        }
		
        
        
        //Walk backward
        if ((Input.GetKey(moveBackward) || Input.GetAxis(input.vertical) < -0.1f) && balanced && !KnockedOut)
        {
            var v3 = - APR_Parts[0].GetComponent<Rigidbody>().transform.forward * MoveSpeed;
            v3.y = APR_Parts[0].GetComponent<Rigidbody>().velocity.y;
            APR_Parts[0].GetComponent<Rigidbody>().velocity = v3;
            WalkBackward = true;
            isKeyDown = true;
        }
		
        if(Input.GetKeyUp(moveBackward) || (Input.GetAxis(input.vertical) >-0.1f && Input.GetAxis(input.vertical) <= 0))
        {
            WalkBackward = false;
            isKeyDown = false;
        }
		
        
        
        //Turn right
        if ((Input.GetKey(turnRight) || Input.GetAxis(input.horizontal) > 0.1f) && balanced && !KnockedOut)
        {
            APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation, new Quaternion(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.x,APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.y - turnSpeed, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.w), 6 * Time.fixedDeltaTime);
        }
		
        //Turn left
        if ((Input.GetKey(turnLeft) || Input.GetAxis(input.horizontal) < -0.1f) && balanced && !KnockedOut)
        {
            APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation, new Quaternion(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.x,APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.y + turnSpeed, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.w), 6 * Time.fixedDeltaTime);
        }
		
		
        //reset turn upon target rotation limit
        if(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.y < -0.98f)
        {
            APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.x, 0.98f, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.w);
        }
			
        else if(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.y > 0.98f)
        {
            APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.x, -0.98f, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[0].GetComponent<ConfigurableJoint>().targetRotation.w);
        }
        
        
        
        //Get up
        if((Input.GetKeyDown(jumpGetup)||Input.GetKeyDown(input.button[0])) && !balanced && !isJumping)
        {
            GettingUp = true;
            balanced = true;
            
            if(KnockedOut)
            {
                DeactivateRagdoll();
            }
        }

        
        
        //Jump
        else if((Input.GetKeyDown(jumpGetup) || Input.GetKeyDown(input.button[0])) && balanced && !inAir && !Jump && !KnockedOut)
        {
            Jump = true;
            Grounded = false;
			
            APR_Parts[0].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            APR_Parts[0].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
        }
            
            
            
        //Reach Left
        if((Input.GetKeyDown(reachLeft)||Input.GetAxis(input.trigger)>=0.2f) && !KnockedOut)
        {
            ReachingLeft = true;
        }
            
        if((Input.GetKeyUp(reachLeft) || Input.GetAxis(input.trigger) == 0) && !KnockedOut&&ReachingLeft)
        {
            ReachingLeft = false;
            PickedUp = false;
            ResetPose = true;
        }
            
            
        //Reach Right
        if((Input.GetKeyDown(reachRight) || Input.GetAxis(input.trigger) <= -0.9f) && !KnockedOut)
        {
            ReachingRight = true;
        }
            
        if((Input.GetKeyUp(reachRight) || Input.GetAxis(input.trigger) ==0 ) && !KnockedOut&&ReachingRight)
        {
             ReachingRight = false;
             PickedUp = false;
             ResetPose = true;
        }
        
        
        //Pick up left helper
        if(/*Input.GetKey(reachLeft) &&*/ ReachingLeft && GrabLeft.hasJoint && !KnockedOut)
        {
            if(GrabLeft.GetComponent<FixedJoint>() != null)
            {
                if(GrabLeft.GetComponent<FixedJoint>().connectedBody.gameObject.tag == "Player")
                {
                    GrabLeft.GetComponent<Rigidbody>().AddForce(APR_Parts[0].transform.up * GrabLeft.GetComponent<FixedJoint>().connectedBody.mass * 150);
                }
            
                else if(GrabLeft.GetComponent<FixedJoint>().connectedBody.gameObject.tag == "Object")
                {
                    GrabLeft.GetComponent<Rigidbody>().AddForce(APR_Parts[0].transform.up * GrabLeft.GetComponent<FixedJoint>().connectedBody.mass * 5);
                }

            }
        }
        
        //Pick up right helper
        if(/*Input.GetKey(reachRight) &&*/ ReachingRight && GrabRight.hasJoint && !KnockedOut)
        {
            if(GrabRight.GetComponent<FixedJoint>() != null)
            {
                if(GrabRight.GetComponent<FixedJoint>().connectedBody.gameObject.tag == "Player")
                {
                    GrabRight.GetComponent<Rigidbody>().AddForce(APR_Parts[0].transform.up * GrabRight.GetComponent<FixedJoint>().connectedBody.mass * 150);
                }
            
                else if(GrabRight.GetComponent<FixedJoint>().connectedBody.gameObject.tag == "Object")
                {
                    GrabRight.GetComponent<Rigidbody>().AddForce(APR_Parts[0].transform.up * GrabRight.GetComponent<FixedJoint>().connectedBody.mass * 5);
                }
            }
        }
        
        //Pickup and Throw
        if((Input.GetKeyDown(pickupThrow)||Input.GetKeyDown(input.button[1])) && !PickedUp && !KnockedOut)
        {
            PickedUp = true;
            GrabbedLeft = null;
            GrabbedRight = null;
        }
        
        else if((Input.GetKeyDown(pickupThrow) || Input.GetKeyDown(input.button[1])) && PickedUp && !KnockedOut)
        {
            //Let go left
            if(GrabLeft.hasJoint)
            {
                GrabbedLeft = GrabLeft.GetComponent<FixedJoint>().connectedBody;
                GrabLeft.GetComponent<FixedJoint>().breakForce = 0;
                GrabLeft.hasJoint = false;
            }
        
            //Let go right
            if(GrabRight.hasJoint)
            {
                GrabbedRight = GrabRight.GetComponent<FixedJoint>().connectedBody;
                GrabRight.GetComponent<FixedJoint>().breakForce = 0;
                GrabRight.hasJoint = false;
            }
            
            Threw = true;
            
            //Throw left
            if(Threw)
            {        
                if(GrabbedLeft != null && GrabbedLeft.gameObject.tag == "Object")
                {
                    GrabbedLeft.AddForce(APR_Parts[0].transform.forward * ThrowForce * GrabbedLeft.mass, ForceMode.Impulse);
                    GrabbedLeft.AddForce(APR_Parts[0].transform.up * ThrowForce * (GrabbedLeft.mass / 3), ForceMode.Impulse);
                }
                
                else if(GrabbedLeft != null && GrabbedLeft.gameObject.tag == "Player")
                {
                    GrabbedLeft.AddForce(APR_Parts[0].transform.forward * ThrowForce * GrabbedLeft.mass * 5, ForceMode.Impulse);
                    GrabbedLeft.AddForce(APR_Parts[0].transform.up * ThrowForce * GrabbedLeft.mass * 3.5f, ForceMode.Impulse);
                }
            }
            
            //Throw right
            if(Threw)
            {     
                if(GrabbedRight != null && GrabbedRight.gameObject.tag == "Object")
                {
                    GrabbedRight.AddForce(APR_Parts[0].transform.forward * ThrowForce * GrabbedRight.mass, ForceMode.Impulse);
                    GrabbedRight.AddForce(APR_Parts[0].transform.up * ThrowForce * (GrabbedRight.mass / 3), ForceMode.Impulse);
                }
                
                else if(GrabbedRight != null && GrabbedRight.gameObject.tag == "Player")
                {
                    GrabbedRight.AddForce(APR_Parts[0].transform.forward * ThrowForce * GrabbedRight.mass * 5, ForceMode.Impulse);
                    GrabbedRight.AddForce(APR_Parts[0].transform.up * ThrowForce * GrabbedRight.mass * 3.5f, ForceMode.Impulse);
                }
            }
            
            PickedUp = false;
            Threw = false;
        }
            

            
        //punch
        if((Input.GetKeyDown(punchRight)||Input.GetKeyDown(input.button[5])) && !KnockedOut)
        {
            Punching = true;
            PunchRight();
        }
            
        if((Input.GetKeyDown(punchLeft) || Input.GetKeyDown(input.button[4])) && !KnockedOut)
        {
            Punching = true;
            PunchLeft();
        }
	}
	
	
	
	/////////////////
	//Checking Ground
	/////////////////
	void GroundCheck()
	{
		//Raycast to detect ground, Note: the floor object must be tagged "Ground".
		Ray ray = new Ray (APR_Parts[0].transform.position, -APR_Parts[0].transform.up);
		RaycastHit hit;
		
		//Balance when ground detected
        if (Physics.Raycast(ray, out hit, balanceHeight) && Grounded && !balanced && !isJumping)
        {
			if(hit.transform.tag == "Ground")
			{
				balanced = true;
				GettingUp = false;
			}
		}
		
		//Fall when ground is not detected
		else if(!Physics.Raycast(ray, out hit, balanceHeight) && !GettingUp)
		{
            balanced = false;
            Grounded = false;
            inAir = true;
		}

		
		//Balance
		if(balanced)
		{
			APR_Parts[0].GetComponent<ConfigurableJoint>().angularXDrive = BalanceOn;
			APR_Parts[0].GetComponent<ConfigurableJoint>().angularYZDrive = BalanceOn;
		}
		else if(!balanced)
		{
			APR_Parts[0].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
			APR_Parts[0].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		}
	}
	
	
	
	//////////////////
	//Checking Balance
	//////////////////
	void Balance()
	{
		//Reset variables when balanced
		if(!WalkForward && !WalkBackward)
        {
            StepRight = false;
            StepLeft = false;
            Step_R_timer = 0;
            Step_L_timer = 0;
            Alert_Leg_Right = false;
            Alert_Leg_Left = false;
        }
		
		//Check direction to walk when off balance
        //Backwards
		if (COMP.position.z < APR_Parts[11].transform.position.z && COMP.position.z < APR_Parts[12].transform.position.z)
        {
            WalkBackward = true;
        }
        else
        {
			if(!isKeyDown)
			{
				WalkBackward = false;
			}
        }
        
        //Forward
        if (COMP.position.z > APR_Parts[11].transform.position.z && COMP.position.z > APR_Parts[12].transform.position.z)
        {
            WalkForward = true;
        }
        else
        {
            if(!isKeyDown)
			{
				WalkForward = false;
			}
        }
	}
	
	
	
	/////////////////
	//Control Walking
	/////////////////
	void Walking()
	{
		if(Grounded)
		{
			//checking which leg to step with based on direction
			if (WalkForward)
			{
                //right leg
				if (APR_Parts[11].transform.position.z < APR_Parts[12].transform.position.z && !StepLeft && !Alert_Leg_Right)
				{
					StepRight = true;
					Alert_Leg_Right = true;
					Alert_Leg_Left = true;
				}
                
                //left leg
				if (APR_Parts[11].transform.position.z > APR_Parts[12].transform.position.z && !StepRight && !Alert_Leg_Left)
				{
					StepLeft = true;
					Alert_Leg_Left = true;
					Alert_Leg_Right = true;
				}
			}

			if (WalkBackward)
			{
                //right leg
				if (APR_Parts[11].transform.position.z > APR_Parts[12].transform.position.z && !StepLeft && !Alert_Leg_Right)
				{
					StepRight = true;
					Alert_Leg_Right = true;
					Alert_Leg_Left = true;
				}
                
                //left leg
				if (APR_Parts[11].transform.position.z < APR_Parts[12].transform.position.z && !StepRight && !Alert_Leg_Left)
				{
					StepLeft = true;
					Alert_Leg_Left = true;
					Alert_Leg_Right = true;
				}
			}
		
			//Step right
			if (StepRight)
			{
				Step_R_timer += Time.fixedDeltaTime;
			
				APR_Parts[11].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
			
				//forward walk simulation
				if (WalkForward)
				{                
					APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.x + 0.09f * StepHeight, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.w);
					APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.x - 0.09f * StepHeight * 2, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.w);

					APR_Parts[9].GetComponent<ConfigurableJoint>().GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.x - 0.12f * StepHeight / 2, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.w);
				}
                
                //backward walk simulation
				if (WalkBackward)
				{
					APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.x - 0.00f * StepHeight, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.w);
					APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.x - 0.07f * StepHeight * 2, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation.w);

					APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.x + 0.02f * StepHeight / 2, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.w);
				}
                
                
				//step duration
				if (Step_R_timer > StepDuration)
				{
					Step_R_timer = 0;
					StepRight = false;

					if (WalkBackward || WalkForward)
					{
						StepLeft = true;
					}
				}
			}
			else
			{
				//reset to idle
				APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation, UpperRightLegTarget, (8f) * Time.fixedDeltaTime);
				APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation, LowerRightLegTarget, (17f) * Time.fixedDeltaTime);
				
                //feet force down
				APR_Parts[11].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
				APR_Parts[12].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
			}
            
            
            //Step left
			if (StepLeft)
			{
				Step_L_timer += Time.fixedDeltaTime;
			
				APR_Parts[12].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                
                //forward walk simulation
				if (WalkForward)
				{
					APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.x + 0.09f * StepHeight, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.w);
					APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.x - 0.09f * StepHeight * 2, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.w);

					APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.x - 0.12f * StepHeight / 2, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.w);
				}
                
                //backward walk simulation
				if (WalkBackward)
				{
					APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.x - 0.00f * StepHeight, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation.w);
					APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.x - 0.07f * StepHeight * 2, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation.w);

					APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.x + 0.02f * StepHeight / 2, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.y, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.z, APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation.w);
				}
			
			
				//Step duration
				if (Step_L_timer > StepDuration)
				{
					Step_L_timer = 0;
					StepLeft = false;

					if (WalkBackward || WalkForward)
					{
						StepRight = true;
					}
				}
			}
			else
			{
				//reset to idle
				APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation, UpperLeftLegTarget, (7f) * Time.fixedDeltaTime);
				APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation, LowerLeftLegTarget, (18f) * Time.fixedDeltaTime);
				
                //feet force down
				APR_Parts[11].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
				APR_Parts[12].GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
			}
		}
	}
	
	
	
	
	/////////
	//Jumping
	/////////
	void Jumping()
	{
		if (Jump)
			{
				isJumping = true;
				inAir = true;
                Landed = false;
                
				var v3 = APR_Parts[0].GetComponent<Rigidbody>().transform.up * jumpForce;
				v3.x = APR_Parts[0].GetComponent<Rigidbody>().velocity.x;
				v3.z = APR_Parts[0].GetComponent<Rigidbody>().velocity.z;
				APR_Parts[0].GetComponent<Rigidbody>().velocity = v3;
			}

		if (isJumping)
		{
			timer = timer + Time.fixedDeltaTime;
				
			if (timer > 0.2f)
			{
				timer = 0.0f;
				Jump = false;
				isJumping = false;
			}
		}
	}	
	
	
	
	////////
	//Posing
	////////
	void Posing()
	{
		//Jump pose
		if(inAir)
		{
			//upper arms pose
			APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, 0, 1);
			APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, 0, 1);
			//legs pose
			APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 1f, -0.2f, 0, 1);
			APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -2f, 0, 0, 1);
			APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 1f, 0.2f, 0, 1);
			APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -2f, 0, 0, 1);
			//body pose
			APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -0.5f, 0, 0, 1);
		}
		
		//Landed
		if(!Landed && Grounded && !isJumping && !ReachingRight && !ReachingLeft)
		{
			Landed = true;
            ResetPose = true;
		}
        
        //Reaching
        if(ReachingRight)
        {
            if(!PickedUp)
            {
                //upper arms pose
                APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0.58f, -0.88f, 0.67f, 1);
                
                //lower arms pose
                APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, 0.12f, 1);
                
                //Body pose
                 APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, 0, 1);
            }
            
            else if(PickedUp)
            {
                //upper arms pose
                APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 3.3f, -6.5f, 3.4f, 1);
                
                //lower arms pose
                APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, 0.12f, 1);
                
                //Body pose
                 APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0.1f, 0, 0, 1);
            }
        }
        
        if(ReachingLeft)
        {
            if(!PickedUp)
            {
                //upper arms pose
			     APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -0.58f, -0.88f, -0.67f, 1);
            
                //lower arms pose
			     APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, -0.12f, 1);
                 
                 //Body pose
                 APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, 0, 1);
            }
            
            else if(PickedUp)
            {
                //upper arms pose
			     APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -3.3f, -6.5f, -3.4f, 1);
            
                //lower arms pose
			     APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, -0.12f, 1);
                 
                 //Body pose
                 APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0.1f, 0, 0, 1);
            }
        }
        
        
        //Reset pose
        if(ResetPose && !Punching && !Jump)
        {
            APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = BodyTarget;
            APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation = UpperRightArmTarget;
			APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation = LowerRightArmTarget;
            APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = UpperLeftArmTarget;
			APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation = LowerLeftArmTarget;
            
            ResetPose = false;
        }
		
	}
    
    
    
    //////////////////
	//Fighting Actions
	//////////////////
    
    //Punch Right
	void PunchRight()
	{
        APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -0.15f, -0.15f, 0, 1);
        //APR_Parts[3].GetComponent<ConfigurableJoint>().SetTargetRotationLocal(Quaternion.Euler(test),orgin);// new Quaternion( -0.25f, -0.7f, 0.3f, 1);
		APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 1.5f, 0, 0, 1);
		
		StartCoroutine(DelayCoroutine());
			
		IEnumerator DelayCoroutine()
		{
			yield return new WaitForSeconds(0.5f);
			APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -0.15f, 0.15f, 0, 1);
			APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0.12f, -0.8f, 0, 1);
		APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0.15f, 0, 0, 1);
			
            
            //Right hand punch forward
			RightHand.AddForce(APR_Parts[0].transform.forward * PunchForce, ForceMode.Impulse);
 
			APR_Parts[1].GetComponent<Rigidbody>().AddForce(APR_Parts[0].transform.forward * PunchForce, ForceMode.Impulse);
			
			yield return new WaitForSeconds(0.3f);
			APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation = UpperRightArmTarget;
            Punching = false;
		}
	}
    
    //punch Left
    void PunchLeft()
	{
		APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -0.15f, 0.15f, 0, 1);
		APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0.25f, -0.7f, -0.3f, 1);
		APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -1.5f, 0, 0, 1);
		
		StartCoroutine(DelayCoroutine());
			
		IEnumerator DelayCoroutine()
		{
			yield return new WaitForSeconds(0.5f);
			APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -0.15f, -0.15f, 0, 1);
			APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -0.12f, 0.8f, 0, 1);
			APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( -0.15f, 0, 0, 1);
			
            
            //Left hand punch forward
			LeftHand.AddForce(APR_Parts[0].transform.forward * PunchForce, ForceMode.Impulse);
 
			APR_Parts[1].GetComponent<Rigidbody>().AddForce(APR_Parts[0].transform.forward * PunchForce, ForceMode.Impulse);
			
			yield return new WaitForSeconds(0.3f);
			APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = UpperLeftArmTarget;
            Punching = false;
		}
	}
    
    
    
    /////////////////////////
	//Feet Contact With Floor
	/////////////////////////
	public void OnFeetContact()
	{
		if(balanced && !isJumping)
		{
			Grounded = true;
			inAir = false;
			GettingUp = false;
			Landed = false;
			
			//reset body pose after fall
			APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = new Quaternion( 0, 0, 0, 1);
		}
		
		else if(!balanced && !Grounded && !isJumping)
		{
			Grounded = true;
			GettingUp = true;
		}
	}
    
	
	
	//////////////////////
	//Activate Full Ragdoll
	//////////////////////
	public void ActivateRagdoll()
	{
		balanced = false;
		KnockedOut = true;
		
		//Root
		APR_Parts[0].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[0].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		//body
		APR_Parts[1].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[1].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		//head
		APR_Parts[2].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[2].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		//arms
		APR_Parts[3].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[3].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		APR_Parts[4].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[4].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		APR_Parts[5].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[5].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		APR_Parts[6].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[6].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		//legs
		APR_Parts[7].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[7].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		APR_Parts[8].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[8].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		APR_Parts[9].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[9].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		APR_Parts[10].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[10].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		APR_Parts[11].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[11].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
		APR_Parts[12].GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
		APR_Parts[12].GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
	}
	
    
    
    /////////////////////////
	//Deactivate Full Ragdoll
	/////////////////////////
	public void DeactivateRagdoll()
	{
		balanced = true;
		KnockedOut = false;
		
		//Root
		APR_Parts[0].GetComponent<ConfigurableJoint>().angularXDrive = BalanceOn;
		APR_Parts[0].GetComponent<ConfigurableJoint>().angularYZDrive = BalanceOn;
		//body
		APR_Parts[1].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[1].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		//head
		APR_Parts[2].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[2].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		//arms
		APR_Parts[3].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[3].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		APR_Parts[4].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[4].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		APR_Parts[5].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[5].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		APR_Parts[6].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[6].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		//legs
		APR_Parts[7].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[7].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		APR_Parts[8].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[8].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		APR_Parts[9].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[9].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		APR_Parts[10].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[10].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		APR_Parts[11].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[11].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
		APR_Parts[12].GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
		APR_Parts[12].GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
        
        ResetPose = true;
	}
	
	
    
    ////////////////////////////
	//Calculating Center of mass
	////////////////////////////
	void CenterOfMass()
	{
			CenterOfMassPoint =
			
			(APR_Parts[0].GetComponent<Rigidbody>().mass * APR_Parts[0].transform.position + 
            APR_Parts[1].GetComponent<Rigidbody>().mass * APR_Parts[1].transform.position +
            APR_Parts[2].GetComponent<Rigidbody>().mass * APR_Parts[2].transform.position +
            APR_Parts[3].GetComponent<Rigidbody>().mass * APR_Parts[3].transform.position +
            APR_Parts[4].GetComponent<Rigidbody>().mass * APR_Parts[4].transform.position +
            APR_Parts[5].GetComponent<Rigidbody>().mass * APR_Parts[5].transform.position +
            APR_Parts[6].GetComponent<Rigidbody>().mass * APR_Parts[6].transform.position +
			APR_Parts[7].GetComponent<Rigidbody>().mass * APR_Parts[7].transform.position +
			APR_Parts[8].GetComponent<Rigidbody>().mass * APR_Parts[8].transform.position +
			APR_Parts[9].GetComponent<Rigidbody>().mass * APR_Parts[9].transform.position +
			APR_Parts[10].GetComponent<Rigidbody>().mass * APR_Parts[10].transform.position +
			APR_Parts[11].GetComponent<Rigidbody>().mass * APR_Parts[11].transform.position +
			APR_Parts[12].GetComponent<Rigidbody>().mass * APR_Parts[12].transform.position) 
            
            /
			
            (APR_Parts[0].GetComponent<Rigidbody>().mass + APR_Parts[1].GetComponent<Rigidbody>().mass +
            APR_Parts[2].GetComponent<Rigidbody>().mass + APR_Parts[3].GetComponent<Rigidbody>().mass +
            APR_Parts[4].GetComponent<Rigidbody>().mass + APR_Parts[5].GetComponent<Rigidbody>().mass +
            APR_Parts[6].GetComponent<Rigidbody>().mass + APR_Parts[7].GetComponent<Rigidbody>().mass +
			APR_Parts[8].GetComponent<Rigidbody>().mass + APR_Parts[9].GetComponent<Rigidbody>().mass +
			APR_Parts[10].GetComponent<Rigidbody>().mass + APR_Parts[11].GetComponent<Rigidbody>().mass +
			APR_Parts[12].GetComponent<Rigidbody>().mass);
			
			COMP.position = CenterOfMassPoint;
	}
    
    
	
	///////////////////
	//Editor Debug Mode
	///////////////////
	void OnDrawGizmos()
	{
		if(editorDebugMode)
		{
			Debug.DrawRay(Root.transform.position, - Root.transform.up * balanceHeight, Color.green);
			
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(COMP.position, 0.3f);
		}
	}
    public void PlayAnim(string name)
    {
        RagdollAnim anim;
        if(!anims.TryGetValue(name,out anim))
        {
            anim = Resources.Load<RagdollAnim>(animPath + name);
            anims.Add(name, anim);
        }
        PlayAnim(anim);
    }
    public void PlayAnim(RagdollAnim anim)
    {
        if(anim!=null&&anim.animation.Count>=1)
        {
            StartCoroutine(PlayAnims(anim));
        }
    }

    IEnumerator PlayAnims(RagdollAnim anim)
    {
        for (int i = 0; i < anim.animation.Count; i++)
        {
            for (int j = 0; j < APR_Parts.Length; j++)
            {
                print(APR_Parts[j].name+ anim.animation[i].bones[j].rotaThis);
                if (anim.animation[i].bones[j].rotaThis)
                {
                    APR_Parts[j].GetComponent<ConfigurableJoint>().SetTargetRotationLocal(Quaternion.Euler(anim.animation[i].bones[j].targetRotation), APR_Parts_Orgin[j]);
                    print(APR_Parts_Orgin[i].eulerAngles);
                    if (anim.animation[i].bones[j].force != 0)
                    {
                        APR_Parts[j].GetComponent<Rigidbody>().AddForce(APR_Parts[0].transform.forward * anim.animation[i].bones[j].force, ForceMode.Impulse);
                    }
                }
            }
            yield return new WaitForSeconds(anim.animation[i].nextAnim);
        }

            yield return new WaitForSeconds(3);

        APR_Parts[1].GetComponent<ConfigurableJoint>().targetRotation = BodyTarget;
        APR_Parts[2].GetComponent<ConfigurableJoint>().targetRotation = HeadTarget;
        APR_Parts[3].GetComponent<ConfigurableJoint>().targetRotation = UpperRightArmTarget;
        APR_Parts[4].GetComponent<ConfigurableJoint>().targetRotation = LowerRightArmTarget;
        APR_Parts[5].GetComponent<ConfigurableJoint>().targetRotation = UpperLeftArmTarget;
        APR_Parts[6].GetComponent<ConfigurableJoint>().targetRotation = LowerLeftArmTarget;
        APR_Parts[7].GetComponent<ConfigurableJoint>().targetRotation = UpperRightLegTarget;
        APR_Parts[8].GetComponent<ConfigurableJoint>().targetRotation = LowerRightLegTarget;
        APR_Parts[9].GetComponent<ConfigurableJoint>().targetRotation = UpperLeftLegTarget;
        APR_Parts[10].GetComponent<ConfigurableJoint>().targetRotation = LowerLeftLegTarget;
    }
}
