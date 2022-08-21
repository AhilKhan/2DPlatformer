using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom Player Character controller
/// </summary>
public class Player : MonoBehaviour
{
	// rigidbody component to control character movements
	[SerializeField] private Rigidbody2D RigidBody;

	// animator component to animate character movements
	[SerializeField] private Animator animator;

	// character's move speed
	[SerializeField] private float speed;
	// speed applied to make the character jump
	[SerializeField] private float jumpSpeed;
	// speed applied mid-air for a double jump
	// jumpSpeed is used if doubleJumpSpeed is not provided
	[SerializeField] private float doubleJumpSpeed;
	// ground layer containing all collidable terrain tiles
	[SerializeField] private LayerMask GroundLayer;

	// player's ground facing collider
	[SerializeField] private Collider2D GroundCheckCollider;

	// Audio Variables (Pretty self explanatory)
	[SerializeField] private AudioSource JumpSound;
	[SerializeField] private AudioSource DeathSound;
	[SerializeField] private AudioSource PickupSound;

	// variables to keep track of what's happening in script

	// speed variables
	private float currentSpeed;
	private bool jump;
	private bool doubleJump;

	// jump and ground possibility variables
	private bool onGround;
	private bool jumpAllowed;

	// positions in last update
	private Vector2 LastPosition;

	// score related variables
	private int PlayerScore = 0;
	public TMPro.TextMeshProUGUI ScoreField;
	private int Score
	{
		get { return PlayerScore; }
		set 
		{
			// everytime score variable changes update the score field to show the new value
			PlayerScore = value;
			ScoreField.text = PlayerScore.ToString();
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		// reset score variable
		Score = 0;
		// if components not assigned in inspector
        if (RigidBody == null)
			RigidBody = GetComponent<Rigidbody2D>();
		if (animator == null)
			animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
		// only update in play state
		if (FindObjectOfType<LevelDesigner>().GameState == LevelDesigner.States.Play)
		{
			// reset speed on every update
			currentSpeed = 0;

			// reset onGround state
			onGround = false;

			// recheck if character is on ground
			if (GroundCheckCollider.IsTouchingLayers(GroundLayer))
			{
				// if character is on ground allow jumps
				onGround = true;
				doubleJump = false;
				jumpAllowed = true;
			}


			// set speed to move left or right (actual movement takes place in fixed update)
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
				currentSpeed = speed;
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
				currentSpeed = -speed;

			// set bool variables to manage jumping
			if (Input.GetKeyDown(KeyCode.Space))
			{
				// make first jump if on ground
				if (onGround)
				{
					JumpSound.Play();
					jump = true;
				}
				// if jumping isn't disables yet and character isn't going too fast go for a double jump
				else if (jumpAllowed && Mathf.Abs(RigidBody.velocity.y)! > 0.4)
				{
					JumpSound.Play();
					doubleJump = true;
					jumpAllowed = false;
				}
			}
		}
	}

	private void FixedUpdate()
	{
		// separate function to handle animator
		SetAnimatorParameters();

		// store player's current vertical velocity
		float VerticalVelocity = RigidBody.velocity.y;
		// JUMP
		if (jump)
		{
			// update the vertical velocity
			VerticalVelocity = jumpSpeed;
			jump = false;
		}

		// DOUBLE JUMP
		if (doubleJump)
		{
			// properly update the vertical velocity
			if (doubleJumpSpeed > 0)
			{
				VerticalVelocity = doubleJumpSpeed;
			}
			else
			{
				VerticalVelocity = jumpSpeed;
			}
			doubleJump = false;
		}

		// TURN TO FACE THE CORRECT WAY
		if (currentSpeed > 0)
			transform.rotation = Quaternion.Euler(Vector3.zero);
		else if (currentSpeed < 0)
			transform.rotation = Quaternion.Euler(new Vector3(0, 180));

		// MOVE
		RigidBody.velocity = new Vector2(currentSpeed, VerticalVelocity);
	}

	/// <summary>
	/// Set animator parameters to correct values to show correct animation
	/// </summary>
	private void SetAnimatorParameters()
	{
		// animator.SetBool("Hit", false);
		// get current position
		Vector2 CurrentPosition = RigidBody.position;

		// manage idle and running animation if on ground
		if (onGround)
		{
			// set all airborne animations to false
			animator.SetBool("Jump", false);
			animator.SetBool("DoubleJump", false);
			animator.SetBool("Fall", false);

			if (LastPosition.x == CurrentPosition.x)
			{
				// Object isn't moving horizontally
				// Animator Speed = 0
				animator.SetFloat("Speed", 0);
			}
			else
			{
				// Object is moving
				// Animator speed = 1 (or something higher)
				animator.SetFloat("Speed", 1);
			}
		}
		else
		{
			// setting airborne animation parameters
			if (LastPosition.y > CurrentPosition.y)
			{
				// character moving DOWNWARDS, so set falling animation
				animator.SetBool("Jump", false);
				animator.SetBool("DoubleJump", false);
				animator.SetBool("Fall", true);
			}
			if (RigidBody.velocity.y >= jumpSpeed / 10)
			{
				if (jumpAllowed)
				{
					// character moving UPWARD but there's still a chance to jump
					// set simple jump animation
					animator.SetBool("Jump", true);
					animator.SetBool("DoubleJump", false);
					animator.SetBool("Fall", false);
				}
				else
				{
					// character moving UPWARD but no more chances to jump
					// set double jump animation
					animator.SetBool("Jump", false);
					animator.SetBool("DoubleJump", true);
					animator.SetBool("Fall", false);
				}
			}
		}

		// store position for next update
		LastPosition = CurrentPosition;
	}

	/// <summary>
	/// Add points to player's score
	/// </summary>
	/// <param name="points">number of points to be added to score</param>
	public void AddScore(int points)
	{
		// play sound and update score
		Score += points;
		PickupSound.Play();
	}

	/// <summary>
	/// Function to handle death of the player
	/// </summary>
	public void HurtPlayer()
	{
		// activate hit animation
		animator.SetBool("Hit", true);
		// jump once (last time)
		jump = true;
		// update player's velocity for the jump
		RigidBody.velocity = new Vector2(RigidBody.velocity.x, jumpSpeed);

		// disable all colliders on the main player object
		foreach (Collider2D coll in GetComponents(typeof(Collider2D)))
		{
			coll.enabled = false;
		}
		// play death sound and disable player script
		DeathSound.Play();
		this.enabled = false;
	}

	/// <summary>
	/// Setting Jump Variable from outside of player script
	/// </summary>
	/// <param name="State">state to put the jump to True or false</param>
	public void Jump(bool State = true)
	{
		// update the jump variable
		jump = State;
	}
}
