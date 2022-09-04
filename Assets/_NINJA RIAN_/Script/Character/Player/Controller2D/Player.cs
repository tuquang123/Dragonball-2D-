using UnityEngine;
using System.Collections;

public enum DoggeType { OverObject, HitObject }

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour, ICanTakeDamage, IListener {
    public int ID = 1;
    public Transform centerPoint;

    public bool GodMode;
	[Header("Moving")]
	public float moveSpeed = 3;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;

	[Header("Jump")]
	public float maxJumpHeight = 3;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public int numberOfJumpMax = 1;
	int numberOfJumpLeft;
	public GameObject JumpEffect;
    public GameObject landingFX;

    [Header("Wall Slide")]
  [HideInInspector]  public bool wallSlideJumpUp = false;

    public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;
    public LayerMask wallLayer;
    [Tooltip("When look to other side, sliding with this speed")]
    public float wallSlideSpeedHold = 0.15f;
    [Tooltip("When look to other side, sliding with this speed")]
    public float wallSlideSpeedNoHold = 0.5f;
    [Tooltip("When look to other side, sliding with this speed")]
    public float wallSlideSpeedLookOtherSide = 0.3f;
    public float wallStickTime = .25f;
	float timeToWallUnstick;
    public Transform checkWallUp, checkWallDown;
    [HideInInspector] public bool wallSliding;
    //bool wallSlidingHoldPosition = false;
    int wallDirX;

    [Header("Health")]
	public int maxHealth;
	public int Health{ get; private set;}
	public GameObject HurtEffect;
    public GameObject respawnFX;

    #region FALLING DOWN AND SHAKE ON GROUND
    [Header("FALLING DOWN")]
    public bool fb_useFallingDown = true;
    public float fd_TimeToActive = 1f;
    public float fd_timeIdle = 1f;
    public AudioClip fb_sound;
    float fd_timeCounter = 0;

    void DoIdleAndShake()
    {
        anim.SetTrigger("sitDown");
        SoundManager.PlaySfx(fb_sound);
        CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
        ForceStanding(fd_timeIdle);
    }

    #endregion

    [Header("TAKE DAMAGE")]
    public float rateGetDmg = 0.5f;
    public Color blinkingColor = Color.green;
   [ReadOnly] public bool isBlinking = false;
    public float knockbackForce = 10f;

    [Header("Sound")]
    public AudioClip respawnSound;
	public AudioClip[] jumpSound;
	[Range(0,1)]
	public float jumpSoundVolume = 0.5f;
	public AudioClip landSound;
	[Range(0,1)]
	public float landSoundVolume = 0.5f;
	public AudioClip wallSlideSound;
	[Range(0,1)]
	public float wallSlideSoundVolume = 0.5f;
	public AudioClip[] hurtSound;
	[Range(0,1)]
	public float hurtSoundVolume = 0.5f;
	public AudioClip[] deadSound;
	[Range(0,1)]
	public float deadSoundVolume = 0.5f;
    public AudioClip[] meleeVocalSound;
    [Range(0, 1)]
    public float meleeVocalSoundVolume = 0.8f;
    public AudioClip[] rangeVocalSound;
    [Range(0, 1)]
    public float rangeVocalSoundVolume = 0.8f;
    bool isPlayedLandSound;

    [Header("Option")]
    //public bool animLoopVictory = false;
	public bool allowMeleeAttack;
	public bool allowRangeAttack;
	public bool allowSlideWall;

	protected RangeAttack rangeAttack;
    bool isHoldNormalBullet = false;
    protected MeleeAttack meleeAttack;

	private AudioSource soundFx;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	[HideInInspector]
	public Vector3 velocity;
	float velocityXSmoothing;

	[ReadOnly] public bool isFacingRight;
	

	public Vector2 input;
    bool forceRunning = false;
    bool isDead = false;

    [HideInInspector]
	public Controller2D controller;
	[HideInInspector] public Animator anim;

	public bool isPlaying { get; private set;}
	public bool isFinish { get; set;}
    public GhostSprites ghostSprite { get; set; }
    public bool isGrounded { get { return controller.collisions.below; } }
    bool forceStannding = false;

    void Awake(){
		controller = GetComponent<Controller2D> ();
		anim = GetComponent<Animator> ();
        ghostSprite = GetComponent<GhostSprites>();
        if (ghostSprite)
            ghostSprite.allowGhost = false;
    }

	void Start() {

        CameraFollow.Instance.manualControl = true;
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
//		print ("Gravity: " + gravity + "  Jump Velocity: " + maxJumpVelocity);

		isFacingRight = transform.localScale.x > 0;
		Health = maxHealth;
		numberOfJumpLeft = numberOfJumpMax;

		rangeAttack = GetComponent<RangeAttack> ();
		meleeAttack = GetComponent<MeleeAttack> ();

		soundFx = gameObject.AddComponent<AudioSource> ();
		soundFx.loop = true;
		soundFx.playOnAwake = false;
		soundFx.clip = wallSlideSound;
		soundFx.volume = wallSlideSoundVolume;

        godAudioSource = gameObject.AddComponent<AudioSource>();
        godAudioSource.clip = godSoundKeep;
        godAudioSource.Play();
        godAudioSource.loop = true;
        godAudioSource.volume = 0;
    }

    bool allowCheckWall = true;
    bool firstContactWall = true;
    bool allowGrapNextWall = false;
    void CheckWall()
    {
        wallSliding = false;

        if (isDogging)
            return;

        //wallSlidingHoldPosition = false;

        if (controller.collisions.ClosestHit.collider != null && (controller.collisions.ClosestHit.collider.gameObject.GetComponent<Bridge>() || controller.collisions.ClosestHit.collider.gameObject.GetComponent<SimpleGravityObject>()))
            return;

        if (!allowCheckWall)
            return;
        
        if (controller.collisions.left)
            wallDirX = -1;
        else if (controller.collisions.right)
            wallDirX = 1;
        else
            wallDirX = 0;
        

        //if (timeToWallUnstick > 0 || (allowSlideWall && ((controller.collisions.left && (wallDirX == -1/* || firstContactWall*/))
        //    || (controller.collisions.right && (wallDirX == 1/* || firstContactWall*/))) && (!controller.collisions.below && velocity.y < 0 && firstContactWall)/* && (input.x == wallDirX)*/))
        //{
        if (wallDirX!=0 &&(timeToWallUnstick > 0 || (allowSlideWall && ((controller.collisions.left && !isFacingRight && (input.x == -1 || allowGrapNextWall /* || firstContactWall*/))
            || (controller.collisions.right && isFacingRight && (input.x == 1 || allowGrapNextWall/* || firstContactWall*/))) && (!controller.collisions.below && velocity.y < 0 && firstContactWall)/* && (input.x == wallDirX)*/)))
        {
          
            if (Physics2D.Raycast(checkWallUp.position, wallDirX == 1 ? Vector2.right : Vector2.left, 0.5f, wallLayer) && Physics2D.Raycast(checkWallDown.position, wallDirX == 1 ? Vector2.right : Vector2.left, 0.5f, wallLayer))     //check up and down contact wall or not
            {
            
                wallSliding = true;
                
                firstContactWall = false;
                if (!soundFx.isPlaying)
                    soundFx.Play();     //play the sliding sound
                
                if (timeToWallUnstick > 0)
                {
                  
                    velocityXSmoothing = 0;
                    //velocity.x = 0;

                    if (input.x != wallDirX)
                    {
                        if (input.x == 0)
                        {
                            timeToWallUnstick -= Time.deltaTime;
                            if (timeToWallUnstick <= 0)
                            {
                              
                                wallSliding = false;
                                Invoke("AllowCheckWall", 0.2f);
                                Flip();
                            }

                            if (velocity.y < -wallSlideSpeedNoHold)
                            {
                              
                                velocity.y = -wallSlideSpeedNoHold;
                            }
                        }
                        else
                        {
                            //velocity.y = 0;
                           
                            timeToWallUnstick = wallStickTime;      //
                            //wallSlidingHoldPosition = true;
                            if (velocity.y < -wallSlideSpeedLookOtherSide)
                            {
                                velocity.y = -wallSlideSpeedLookOtherSide;
                            }
                        }
                    }
                    else
                    {
                       
                        timeToWallUnstick = wallStickTime;
                        if (velocity.y < -wallSlideSpeedHold)
                        {
                            velocity.y = -wallSlideSpeedHold;
                        }
                    }
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                wallSliding = false;
                allowCheckWall = true;
                firstContactWall = true;
                timeToWallUnstick = 0;
            }
        }
        else
        {
            if (soundFx.isPlaying)
                soundFx.Stop();
        }

        if ((!controller.collisions.left && controller.collisions.faceDir == -1) || (!controller.collisions.right && controller.collisions.faceDir == 1))
        {
            wallSliding = false;
            allowCheckWall = true;
            firstContactWall = true;
        }  
    }

    void AllowCheckWall()
    {
        allowCheckWall = true;
        firstContactWall = true;
    }

    void Update() {
        
        if (isFrozen/* || forceStannding*/)
            return;

        //		Debug.Log (GameManager.Instance.State);
        //		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
        HandleInput();
		HandleAnimation ();

        //wallDirX = (controller.collisions.left) ? -1 : 1;

        float targetVelocityX = (forceRunning? 1 : input.x) * moveSpeed * mulSpeedc;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        
        
        velocity.y += gravity * Time.deltaTime;
        //if (!isDead)
        //    CheckWall();

        if (controller.collisions.below && !isPlayedLandSound) {
            //CheckBelow();       //check the object below if it have Stand on event
            isPlayedLandSound = true;
			SoundManager.PlaySfx (landSound, landSoundVolume);
            if (landingFX)
                SpawnSystemHelper.GetNextObject(landingFX, true).transform.position = transform.position;
		} else if (!controller.collisions.below && isPlayedLandSound)
			isPlayedLandSound = false;

        if (GodMode || isBlinking || !isPlaying)
        {
            //controller.collisionMask = controller.collisionMask & ~(1 << LayerMask.NameToLayer("Enemies"));
            ;
        }
        else
        {
            if (isDogging)
            {
                if (isDogging && doggeType == DoggeType.HitObject)
                {
                    ;
                }
                else
                {
                    //controller.collisionMask = controller.collisionMask & ~(1 << LayerMask.NameToLayer("Enemies"));
                    if (isDogging)
                        gameObject.layer = LayerMask.NameToLayer("IgnoreAll");
                    else
                        gameObject.layer = LayerMask.NameToLayer("Player");
                }
            }
            else
            {
                //controller.collisionMask = controller.collisionMask | (1 << LayerMask.NameToLayer("Enemies"));    
                gameObject.layer = LayerMask.NameToLayer("Player");
            }
        }

        if (controller.collisions.above)
        {
            CheckBlock();
        }
        
        if (isHoldNormalBullet && DefaultValue.Instance != null && DefaultValue.Instance.normalBulletType == DefaultValue.NormalBulletType.HoldShooting)
        {
            RangeAttack(false);
        }
    }

   void CheckBelow()
    {
        if (controller.collisions.ClosestHit.collider != null)
        {
            var standObj = (IStandOnEvent)controller.collisions.ClosestHit.collider.gameObject.GetComponent(typeof(IStandOnEvent));
            if (standObj != null)
                standObj.StandOnEvent(gameObject);
        }
    }

    void CheckBlock()
    {
        Block isBlock;
        BrokenTreasure isTreasureBlock;
        var bound = controller.boxcollider.bounds;

        //check middle
        var hit = Physics2D.Raycast(new Vector2((bound.min.x + bound.max.x) / 2f, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));

        if (hit)
        {
            isBlock = hit.collider.gameObject.GetComponent<Block>();
            if (isBlock)
            {
                isBlock.BoxHit();
                //return;
            }

            isTreasureBlock = hit.collider.gameObject.GetComponent<BrokenTreasure>();
            if (isTreasureBlock)
            {
                isTreasureBlock.BoxHit();
                //return;
            }
        }

        //check left
        hit = Physics2D.Raycast(new Vector2(bound.min.x, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));
        if (hit)
        {
            isBlock = hit.collider.gameObject.GetComponent<Block>();
            if (isBlock)
            {
                isBlock.BoxHit();
                //return;
            }

            isTreasureBlock = hit.collider.gameObject.GetComponent<BrokenTreasure>();
            if (isTreasureBlock)
            {
                isTreasureBlock.BoxHit();
                //return;
            }
        }

        hit = Physics2D.Raycast(new Vector2(bound.max.x, bound.max.y), Vector2.up, 0.5f, 1 << LayerMask.NameToLayer("Platform"));
        if (hit)
        {
            isBlock = hit.collider.gameObject.GetComponent<Block>();
            if (isBlock)
            {
                isBlock.BoxHit();
                //return;
            }

            isTreasureBlock = hit.collider.gameObject.GetComponent<BrokenTreasure>();
            if (isTreasureBlock)
            {
                isTreasureBlock.BoxHit();
                //return;
            }
        }
    }

	void LateUpdate(){
        if (isFrozen)
            return;
        
        if (isDogging)
        {
            if (doggePowerBar <= 0)
            {
                StopDogge();
                return;
            }

            if ((controller.collisions.right && doggeDirection.x == 1) || (controller.collisions.left && doggeDirection.x == -1))
            {
                
                // set player to the wall to slide wall
                var hit = Physics2D.Raycast(transform.position, doggeDirection, 10, GameManager.Instance.groundLayer);
                if (hit)
                {
                    transform.position = new Vector3(hit.point.x + (transform.position.x - (doggeDirection.x == 1 ? controller.boxcollider.bounds.max.x : controller.boxcollider.bounds.min.x)), transform.position.y, transform.position.z);
                }

                velocity.x = 0;
                StopDogge();
                return; 
            }
            
            velocity.x = speedDogge * doggeDirection.x;
            velocity.y = /*speedDogge * doggeDirection.y;*/0;
            if (!unlimitedDogge)
            {
                doggePowerBar -= doggeConsume * Time.deltaTime;
            }

        }

        if (!isDead)
            CheckWall();

        if (wallSliding && !isDogging)
            velocity.x = 0;

        if ((controller.raycastOrigins.bottomLeft.x < CameraFollow.Instance._min.x && velocity.x<0) || (controller.raycastOrigins.bottomRight.x > CameraFollow.Instance._max.x && velocity.x > 0))
            velocity.x = 0;
        
        if (forceStannding)
            velocity.x = 0;

        if (controller.raycastOrigins.bottomLeft.y < CameraFollow.Instance._min.y)
            GameManager.Instance.GameOver();

        //Debug.LogError(velocity.y);
        controller.Move (velocity * Time.deltaTime, isDogging? doggeDirection : input);
        if(!isDead)
        CameraFollow.Instance.DoFollowPlayer();


        if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
        
        if (controller.collisions.below)
        { 
            timeToWallUnstick = 0;
            numberOfJumpLeft = 0;
            firstContactWall = true;
            allowGrapNextWall = false;
            AllowCheckWall();
            CheckBelow();       //check the object below if it have Stand on event
         
            if (fd_timeCounter >= fd_TimeToActive)
                DoIdleAndShake();
            fd_timeCounter = 0;
        }
        else
        {
            if(fb_useFallingDown)
            {
                if (velocity.y < 0 && !wallSliding && !isDogging && !isGrounded)
                    fd_timeCounter += Time.deltaTime;
                else/* if (velocity.y > 0)*/
                    fd_timeCounter = 0;
            }
        }

        //check if dooge damage
        if (isDogging && doggeCanMakeDamage)
        {
            //var hits = Physics2D.CircleCastAll(centerPoint.position, checkDoggeTargetRadius, Vector2.zero, 0, doggeTargetLayer);
            Vector2 castPos = centerPoint.position - Vector3.right * (transform.position.x - lastPosX) * 0.5f;
            Vector2 castSize = new Vector2(Mathf.Abs(transform.position.x - lastPosX), checkDoggeTargetHeight);
            //Debug.LogError(transform.position.x + "." + lastPosX);
            //Debug.LogError(castPos + " / " + castSize);
            var hits = Physics2D.BoxCastAll(castPos, castSize, 0, Vector2.zero, 0, doggeTargetLayer);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    var damage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
                    if (damage != null)
                    {
                        damage.TakeDamage(doggeDamage, Vector2.zero, gameObject, hit.point);
                        if (playEarthQuakeOnHitDogge)
                        {
                            CameraPlay.EarthQuakeShake(_eqTime, _eqSpeed, _eqSize);
                        }

                    }
                }
            }
        }

        lastPosX = transform.position.x;
        
    }
    //private void CheckBridge()
    //{
    //    if (controller.collisions.ClosestHit.collider == null)
    //        return;
    //    var bridge = controller.collisions.ClosestHit.collider.gameObject.GetComponent<Bridge>();
    //    if (bridge)
    //    {
    //        bridge.Work();
    //    }
    //}

    public void PausePlayer(bool pause)
    {
        StopMove();
        isPlaying = !pause;
    }

    public bool isFrozen { get; set; }  //player will be frozen
    public void Frozen(bool is_enable)
    {
        input = Vector2.zero;
        velocity = Vector2.zero;
        isFrozen = is_enable;
        anim.enabled = !is_enable;
    }

    /// <summary>
    /// Controller	/// </summary>
    /// <param name="pos">Position.</param>


    private void HandleInput(){
		if (Input.GetKey (/*KeyCode.A*/DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyMoveLeft : DefaultValue.Instance.keyMoveLeft) || Input.GetKey(KeyCode.LeftArrow))
			MoveLeft ();
		else if (Input.GetKey (DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyMoveRight : DefaultValue.Instance.keyMoveRight) || Input.GetKey(KeyCode.RightArrow))
			MoveRight ();
//		else if((Input.GetKeyUp (KeyCode.A) || Input.GetKeyUp (KeyCode.D)))
		else if(Input.GetKeyUp (/*KeyCode.A*/DefaultValue.Instance==null? DefaultValueKeyboard.Instance.keyMoveLeft: DefaultValue.Instance.keyMoveLeft) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp (/*KeyCode.D*/DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyMoveRight : DefaultValue.Instance.keyMoveRight) || Input.GetKeyUp(KeyCode.RightArrow))
			StopMove ();

        if (Input.GetKeyDown(DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyMoveDown : DefaultValue.Instance.keyMoveDown) || Input.GetKeyDown(KeyCode.DownArrow))
            FallDown();

        if (Input.GetKeyUp(DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyMoveDown : DefaultValue.Instance.keyMoveDown) || Input.GetKeyUp(KeyCode.DownArrow))
            StopMove();


        if (Input.GetKeyDown (/*KeyCode.Space*/DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyJump : DefaultValue.Instance.keyJump) || Input.GetKeyDown(KeyCode.UpArrow)) {
			Jump ();
		}

		if (Input.GetKeyUp (/*KeyCode.Space*/DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyJump : DefaultValue.Instance.keyJump) || Input.GetKeyUp(KeyCode.UpArrow)) {
			JumpOff ();
		}

        if (Input.GetKeyDown(/*KeyCode.F*/DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyNormalBullet : DefaultValue.Instance.keyNormalBullet))
        {
            HoldingNormalBullet(true);
            RangeAttack(false);
        }

        if (Input.GetKeyUp(/*KeyCode.F*/DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyNormalBullet : DefaultValue.Instance.keyNormalBullet))
        {
            HoldingNormalBullet(false);
        }

        if (Input.GetKeyDown(/*KeyCode.G*/DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyPowerBullet : DefaultValue.Instance.keyPowerBullet))
            RangeAttack(true);


        if (Input.GetKeyDown (/*KeyCode.X*/DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyMelee : DefaultValue.Instance.keyMelee))
			MeleeAttack ();

        if (Input.GetKeyDown(KeyCode.L))
            Dogge();
    }

    public void HoldingNormalBullet(bool holdNormal)
    {
        isHoldNormalBullet = holdNormal;
    }

	private void Flip(){
        if (wallSliding || forceStannding)
            return;
        
        transform.localScale = new Vector3 (transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
		isFacingRight = transform.localScale.x > 0;
	}


	public void MoveLeft(){
        //Debug.LogError("MoveLeft");
		if (isPlaying) {
			input = new Vector2 (-1, 0);
			if (isFacingRight)
				Flip ();
		}
	}


	public void MoveRight(){
		if (isPlaying) {
			input = new Vector2 (1, 0);
			if (!isFacingRight)
				Flip ();
		}
	}


	public void StopMove(){
		input = Vector2.zero;
	}

	public void FallDown(){
		input = new Vector2 (0, -1);
	}


    public void Jump()
    {
        if (!isPlaying)
            return;

        if (forceStannding)
            return;

        if (wallSliding)
        {
            allowCheckWall = false;
            wallSliding = false;
            numberOfJumpLeft = 0;
            timeToWallUnstick = 0;

            if (wallDirX == input.x && wallSlideJumpUp)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
                Invoke("AllowCheckWall", 0.35f);
            }
            else if (input.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
                Flip();
                Invoke("AllowCheckWall", 0.1f);
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
                Flip();
                allowGrapNextWall = true;
                Invoke("AllowCheckWall", 0.05f);
            }
            SoundManager.PlaySfx(jumpSound, jumpSoundVolume);
        }
        else if (controller.collisions.below)
        {
            velocity.y = maxJumpVelocity;

            //if (JumpEffect != null)
            //    Instantiate(JumpEffect, transform.position, transform.rotation);
            if (JumpEffect)
                SpawnSystemHelper.GetNextObject(JumpEffect, true).transform.position = transform.position;

            SoundManager.PlaySfx(jumpSound, jumpSoundVolume);
            numberOfJumpLeft = numberOfJumpMax;
        }
        else
        {
            numberOfJumpLeft--;
            if (numberOfJumpLeft > 0)
            {
                anim.SetTrigger("doubleJump");
                velocity.y = minJumpVelocity;

                //if (JumpEffect != null)
                //    Instantiate(JumpEffect, transform.position, transform.rotation);
                if (JumpEffect)
                    SpawnSystemHelper.GetNextObject(JumpEffect, true).transform.position = transform.position;
                SoundManager.PlaySfx(jumpSound, jumpSoundVolume);
            }
        }
    }
    
	public void JumpOff(){
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}


	public void MeleeAttack(){
		if (!isPlaying)
			return;

        if (wallSliding)
            return;

        if (allowMeleeAttack && meleeAttack != null)
        {
            meleeAttack.Attack();
            anim.SetInteger("combo_", meleeAttack.currentCombo);
            SoundManager.PlaySfx(meleeVocalSound, meleeVocalSoundVolume);
            if (isGrounded)
                ForceStanding(0.2f);
        }
	}

    #region FORCE STANDING
     
    public void ForceStanding(float delay)
    {
        if (ForceStandingCoDo != null)
            StopCoroutine(ForceStandingCoDo);

        ForceStandingCoDo = ForceStandingCo(delay);
        StartCoroutine(ForceStandingCoDo);
    }

    IEnumerator ForceStandingCoDo;

    IEnumerator ForceStandingCo(float delay)
    {
        forceStannding = true;
        input.x = 0;
        velocity.x = 0;
        yield return new WaitForSeconds(delay);
        forceStannding = false;
    }

    #endregion

    public void RangeAttack(bool power){

		if (!isPlaying)
			return;
		
		if (allowRangeAttack && rangeAttack!=null) {

			if (rangeAttack.Fire (power)) {
				anim.SetTrigger ("range_attack");
                SoundManager.PlaySfx(rangeVocalSound, rangeVocalSoundVolume);
                if (rangeAttack.standToFire && isGrounded)
                    ForceStanding(rangeAttack.standTime);
            }
		}
	}


    public void SetForce(Vector2 force, bool springPush = false)
    {
        if (!springPush && isBlinking)
            return;

        if (!springPush && GodMode)
            return;

        if (springPush)
        {
            numberOfJumpLeft = numberOfJumpMax;
            fd_timeCounter = 0;
        }

        velocity = (Vector3)force;
    }

	public void AddForce(Vector2 force){
		velocity += (Vector3) force;
	}


	public void RespawnAt(Vector2 pos){
		transform.position = pos;
        if (respawnFX)
            Instantiate(respawnFX, pos, respawnFX.transform.rotation);
		isPlaying = true;
        isDead = false;
        Health = maxHealth;
        fd_timeCounter = 0;

        SoundManager.PlaySfx(respawnSound, 0.8f);
        godAudioSource.volume = 0;
        GodMode = false;

        mulSpeedc = 1;
        ghostSprite.allowGhost = false;

        ResetAnimation ();

        controller.HandlePhysic = true;

        StartCoroutine(BlinkingCo(1.5f));
	}

	void HandleAnimation(){
		//set animation state
		anim.SetFloat ("speed", Mathf.Abs(velocity.x));
		anim.SetFloat ("height_speed", velocity.y);
		anim.SetBool ("isGrounded", controller.collisions.below);
		anim.SetBool ("isWall", wallSliding);
        anim.SetBool("isDogging" + (doggeCanMakeDamage ? "Skill" : ""), isDogging);
    }

	void ResetAnimation(){
		anim.SetFloat ("speed", 0);
		anim.SetFloat ("height_speed", 0);
		anim.SetBool ("isGrounded", true);
		anim.SetBool ("isWall", false);
		anim.SetTrigger ("reset");
	}

	public void GameFinish(){
		StopMove ();
		isPlaying = false;
		anim.SetTrigger ("finish");
        //anim.SetBool("animLoopVictory", animLoopVictory);
	}

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        if (!isPlaying || (isDogging && doggeType == DoggeType.OverObject) || isBlinking)
            return;

        if (GodMode)
        {
          
            if (instigator.gameObject.layer == LayerMask.NameToLayer("Enemies"))
            {
                if (Time.time > (lastGodDamage + godDamageRate))
                {
                    lastGodDamage = Time.time;
                    var _damage = (ICanTakeDamage)instigator.GetComponent(typeof(ICanTakeDamage));
                    if (_damage != null)
                        _damage.TakeDamage(/*godmodeType == GodmodeType.Kill ? int.MaxValue : */godmodeDamage, Vector2.zero, gameObject, Vector2.zero);        //kill the enemy right away while in godmode
                }
            }

            return;
        }
		
		SoundManager.PlaySfx (hurtSound, hurtSoundVolume);
        //if (HurtEffect != null)
        //	Instantiate (HurtEffect, hitPoint == Vector3.zero? instigator.transform.position : hitPoint, Quaternion.identity);
        if (HurtEffect)
            SpawnSystemHelper.GetNextObject(HurtEffect, true).transform.position = hitPoint == Vector3.zero ? instigator.transform.position : hitPoint;

        Health -= (int)damage;

        if (Health <= 0)
            GameManager.Instance.GameOver();
        else
        {
            anim.SetTrigger("hurt");
            StartCoroutine(BlinkingCo(rateGetDmg));
        }

        
        if (instigator != null)
        {
            int dirKnockBack = (instigator.transform.position.x > transform.position.x) ? -1 : 1;
            SetForce(new Vector2(knockbackForce * dirKnockBack, 0));
        }
    }

    IEnumerator BlinkingCo(float time)
    {
        isBlinking = true;
        int blink = (int)(time * 0.5f / 0.1f);
        for (int i = 0; i < blink; i++)
        {
            imageCharacterSprite.color = godBlinkColor;
            yield return new WaitForSeconds(0.1f);
            imageCharacterSprite.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }

        imageCharacterSprite.color = Color.white;
        isBlinking = false;
    }

	public void GiveHealth(int hearthToGive, GameObject instigator){
		Health = Mathf.Min (Health + hearthToGive, maxHealth);
		//GameManager.Instance.ShowFloatingText ("+" + hearthToGive, transform.position, Color.red);
	}
    
    public void Kill()
    {
        if (isPlaying)
        {
            isPlaying = false;
            forceStannding = false;
            wallSliding = false;
            isDead = true;
            forceRunning = false;
            StopAllCoroutines();
            StopMove();
            SoundManager.PlaySfx(deadSound, deadSoundVolume);
            soundFx.Stop(); //stop the sliding wall sound if it's playing
            anim.SetTrigger("dead");
            SetForce(new Vector2(0, 7f));
            Health = 0;
            //controller.HandlePhysic = false;

            imageCharacterSprite.color = Color.white;
            //GodFX.SetActive(false);
            allowCheckWall = true;
            firstContactWall = true;
            godAudioSource.volume = 0;
            GodMode = false;
            mulSpeedc = 1;
            //isBoostSpeed = false;
            ghostSprite.allowGhost = false;
        }
    }

    #region GOD MODE

    public enum GodType
    {
        Blinking, FX

    }

    //public bool GodMode;        //active this then the player will not be hurt by anything
    public enum GodObstacles { Through, GetKill}
    [Header("GOD MODE")]

    public SpriteRenderer imageCharacterSprite;     //the Image of the character
    public Color godBlinkColor = new Color(0.2f, .2f, .2f, 1f);     //blink colour
    public GodObstacles godObstacles;
    
    //public GameObject GodFX;
    public float godDamageRate = 0.5f;
    float lastGodDamage;
    public AudioClip godSoundKeep;
    AudioSource godAudioSource;

    [Header("GOD DEFAULT")]
    public GodType godEffectType;
    public  float godTimer = 7;     //active the God timer in the given time
  public int godmodeDamage = 50;
    
    public void InitGodmode(/*GodmodeType _type*//*, float useTime, float damage*/)
    {
        if (GodMode)
            return;

        StartCoroutine(GodmodeCo());
    }

    IEnumerator GodmodeCo()
    {

        GodMode = true;
        godAudioSource.volume = /*GlobalValue.isSound ? */1/* : 0*/;
        if (godEffectType == GodType.Blinking)
        {
            int blink = (int)(godTimer * 0.5f / 0.1f);
            for (int i = 0; i < blink; i++)
            {
                imageCharacterSprite.color = godBlinkColor;
                yield return new WaitForSeconds(0.1f);
                imageCharacterSprite.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }

            imageCharacterSprite.color = Color.white;
        }
        else
        {
            //GodFX.SetActive(true);
            //GodFX.GetComponent<ParticleSystem>().gravityModifier = Mathf.Abs(GodFX.GetComponent<ParticleSystem>().gravityModifier) * (GameManager.Instance.Player.transform.localScale.y > 0 ? -1 : 1);
            yield return new WaitForSeconds(godTimer);
            //GodFX.SetActive(false);
        }

        godAudioSource.volume = 0;
        GodMode = false;

    }

    #endregion
    [HideInInspector]
    public float mulSpeedc = 1;
    float XXspeed, XXtime;
    //bool forceShadowFX = false;


    public void SpeedBoost(float Xspeed, float time, bool allowEffect)
    {
        XXspeed = Xspeed;
        XXtime = time;
        StartCoroutine(SpeedBoostCo(allowEffect));
    }


    IEnumerator SpeedBoostCo(bool allowEffect)
    {
       
        mulSpeedc = XXspeed;
        if (ghostSprite)
            ghostSprite.allowGhost = allowEffect;
        
        yield return new WaitForSeconds(XXtime);
        mulSpeedc = 1;
        ghostSprite.allowGhost = false;
    }

    #region DOGGE
    [Header("Dogge")]
    public bool useDoggeGhostFX = true;
    private bool unlimitedDogge = true;
    [Space]
    public bool doggeCanMakeDamage = true;
    public int doggeDamage = 100;
    public float checkDoggeTargetHeight = 1f;
    public LayerMask doggeTargetLayer;
    [Space]
    public float speedDogge = 20;
    private float timeDogge = 0.25f;
    [Range(0, 100)]
    private float doggeConsume = 10;
    private bool allowRechargeDoggeEnegry;
    private float doggeEnegryRecharge = 10;
    public AudioClip soundDogge;
    [Space]
    [Header("HIT EFFECT")]
    public bool playEarthQuakeOnHitDogge = true;
    public float _eqTime = 0.1f;
    public float _eqSpeed = 60;
    public float _eqSize = 1;
    [Space]
    [HideInInspector]
    public float doggePowerBar = 100;
    public bool isDogging { get; set; }
    Vector2 doggeDirection;

    public DoggeType doggeType;
    public LayerMask doggeHitLayer;
    //public bool allowDoggeUp = false;
    float limitDoggeX;
    float lastPosX;

    public void Dogge(bool isDoublePress = false)
    {
        if (isDogging)
            return;

        if (wallSliding && (input.x == wallDirX || input.x == 0))
            return;

        if (input.y == -1)
            return;

        if (input.x != 0)
            doggeDirection = input;
        else
            doggeDirection = isFacingRight ? Vector2.right : Vector2.left;
        
        if (isDogging/* || doggeDirection == Vector2.zero */|| doggePowerBar < 5)
            return;

        if (doggePowerBar <= 0)
        {
            return;
        }

        if ((doggeDirection.x > 0 && controller.collisions.right) || (doggeDirection.x < 0 && controller.collisions.left))
            StopDogge();

        SoundManager.PlaySfx(soundDogge);
        isDogging = true;

        allowGrapNextWall = true;
        if (wallSliding)
        {
            allowCheckWall = false;
            wallSliding = false;
            timeToWallUnstick = 0;
            Invoke("AllowCheckWall", 0.1f);
        }

        if (gameObject.layer == LayerMask.NameToLayer("HidingZone"))
        {
            GameManager.Instance.Player.gameObject.layer = LayerMask.NameToLayer("Player");
        }
        if (ghostSprite)
            ghostSprite.allowGhost = useDoggeGhostFX;

        Invoke("StopDogge", timeDogge);
    }

    public void StopDogge()
    {
        isDogging = false;
        velocity.y = 0;
        ghostSprite.allowGhost = false;
        numberOfJumpLeft = numberOfJumpMax;
    }
    #endregion

    #region TELEPORT
    public void Teleport(Transform newPos, float timer)
    {
        StartCoroutine(TeleportCo(newPos, timer));
    }


    IEnumerator TeleportCo(Transform newPos, float timer)
    {
        StopMove();
        isPlaying = false;
        Color color = imageCharacterSprite.color;

        float transparentSpeed = 3;
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= (Time.deltaTime * transparentSpeed);
            
            color.a = Mathf.Clamp01(alpha);
            imageCharacterSprite.color = color;
            yield return null;
        }

        transform.position = newPos.position;
        yield return new WaitForSeconds(timer);

        isPlaying = true;
        yield return null;
        isPlaying = false;

        alpha = 0;
        while (alpha < 1)
        {
            alpha += (Time.deltaTime * transparentSpeed);
            color.a = Mathf.Clamp01(alpha);
            imageCharacterSprite.color = color;
            yield return null;
        }

        color.a = 1;
        imageCharacterSprite.color = Color.white;

        isPlaying = true;
    }
    #endregion

    public void SetTripleDarts()
    {
        if (rangeAttack)
        {
            rangeAttack.normalSpeadBullet = 3;

        }
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.DrawRay(checkWallUp.position, Vector2.right * 0.5f);
            Gizmos.DrawRay(checkWallDown.position, Vector2.right * 0.5f);

            if (doggeCanMakeDamage)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(centerPoint.position, Vector2.one * checkDoggeTargetHeight);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlaying)
            return;

        var isTriggerEvent = collision.GetComponent<TriggerEvent>();
        if (isTriggerEvent != null)
            isTriggerEvent.OnContactPlayer();

        //var itemType = collision.GetComponent<ItemType>();
        //if (itemType)
        //    itemType.Collect();

        var scrollType = collision.GetComponent<ScrollItem>();
        if (scrollType)
            scrollType.Collect();

        if (collision.CompareTag("Checkpoint"))
        {
            var hitGround = Physics2D.Raycast(collision.transform.position, Vector2.down, 100, GameManager.Instance.groundLayer);
            
            if (hitGround)
                GameManager.Instance.SaveCheckPoint(hitGround.point);
            else
                GameManager.Instance.SaveCheckPoint(collision.transform.position);

        }

        if (collision.CompareTag("DeadZone"))
            GameManager.Instance.GameOver();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isPlaying)
            return;

        var itemType = collision.GetComponent<ItemType>();
        if (itemType)
            itemType.Collect();
    }

    public void IPlay()
    {
        isPlaying = true;
        forceRunning = LevelMapType.Instance.controllerType == CONTROLLER.RUNNER;
    }

    public void ISuccess()
    {
        forceRunning = false;
    }

    public void IPause()
    {
       
    }

    public void IUnPause()
    {
       
    }

    public void IGameOver()
    {
        Kill();
    }

    public void IOnRespawn()
    {
        
    }

    public void IOnStopMovingOn()
    {
       
    }

    public void IOnStopMovingOff()
    {
      
    }
}
