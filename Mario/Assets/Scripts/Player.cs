using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Custom Character controller
/// </summary>
public class Player : MonoBehaviour
{
	// rigidbody component to control character movements
	[SerializeField] private Rigidbody2D RigidBody;

	// animator component to animate character movements
	[SerializeField] private Animator animator;

	// character's move speed
	[SerializeField] private float speed;
	// force applied to make the character jump
	[SerializeField] private float jumpSpeed;
	// force applied mid-air for a double jump
	[SerializeField] private float doubleJumpSpeed;
	// ground layer containing all collidable terrain tiles
	[SerializeField] private LayerMask GroundLayer;

	[SerializeField] private Collider2D GroundCheckCollider;

	[SerializeField] private AudioSource JumpSound;
	[SerializeField] private AudioSource DeathSound;
	[SerializeField] private AudioSource PickupSound;

	// variables to keep track of what's happening in script
	private float currentSpeed;
	private bool jump;
	private bool doubleJump;
	private bool onGround;
	private bool jumpAllowed;
	private Vector2 LastPosition;

	private int PlayerScore = 0;
	
	public TMPro.TextMeshProUGUI ScoreField;

	private int Score
	{
		get { return PlayerScore; }
		set 
		{
			PlayerScore = value;
			ScoreField.text = PlayerScore.ToString();
		}
	}

	public void AddScore(int points)
	{
		Score += points;
		PickupSound.Play();
	}

	public void HurtPlayer()
	{
		animator.SetBool("Hit", true);
		jump = true;
		RigidBody.velocity = new Vector2(RigidBody.velocity.x, jumpSpeed);
		foreach (Collider2D coll in GetComponents(typeof(Collider2D)))
		{
			coll.enabled = false;
		}
		DeathSound.Play();
		this.enabled = false;
	}

	public void Jump(bool State=true)
	{
		jump = State;
	}

	// Start is called before the first frame update
	void Start()
    {
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
		if (FindObjectOfType<LevelDesigner>().GameState == LevelDesigner.States.Play)
		{
			// reset speed on every update
			currentSpeed = 0;

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
		// JUMP
		float VerticalVelocity = RigidBody.velocity.y;
		if (jump)
		{
			VerticalVelocity = jumpSpeed;
			//RigidBody.AddForce(new Vector2(0, jumpForce));
			jump = false;
		}

		// DOUBLE JUMP
		if (doubleJump)
		{
			if (doubleJumpSpeed > 0)
			{
				VerticalVelocity = doubleJumpSpeed;
			}
			else
			{
				VerticalVelocity = jumpSpeed;
			}
			//RigidBody.AddForce(new Vector2(0, doubleJumpForce));
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

	private void SetAnimatorParameters()
	{
		animator.SetBool("Hit", false);
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
}
