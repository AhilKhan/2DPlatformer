using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[Serializable]
	// types of movement directions for the enemy
	private enum Direction
	{
		Left,
		Right,
		Stationary
	}

	// rigidbody component to control character movements
	[SerializeField] private Rigidbody2D RigidBody;
	// animator component to animate character movements
	[SerializeField] private Animator animator;

	// character's movement speed
	[SerializeField] private float speed;

	// pretty obvious (These layers are used to check collisons)
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask playerLayer;

	// Colliders to hurt player or get hurt by the player
	[SerializeField] private Collider2D HitBox;
	[SerializeField] private Collider2D HurtBox;

	// current Score
	private int points;

	// Variables to help movement
	private float CurrentSpeed;
	private Direction MoveDirection = Direction.Stationary;

	// Reference to Player Object
	private Player player;

	// Start is called before the first frame update
	void Start()
	{
		// if components not assigned in inspector
		if (RigidBody == null)
			RigidBody = GetComponent<Rigidbody2D>();
		if (animator == null)
			animator = GetComponent<Animator>();

		// Find and save player for further reference
		player = FindObjectOfType<Player>();

		// setting default direction
		MoveDirection = Direction.Stationary;
	}

// Update is called once per frame
void Update()
    {
		// Hitting Player if Hitbox is in contact with player
		if (HitBox.IsTouchingLayers(playerLayer))
		{
			// Kill the player (make sure to not get a null reference error)
			if (player == null)
			{
				player = FindObjectOfType<Player>();
			}
			if (player != null)
			{
				player.HurtPlayer();
			}
		}

		// if Hurtbox is in contact with player DIE
		if (HurtBox.IsTouchingLayers(playerLayer))
		{
			HurtEnemy();
		}

		// check ground on left and right
		RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(RigidBody.position.x - 2, RigidBody.position.y), Vector2.down, 2f, groundLayer);
		RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(RigidBody.position.x + 2, RigidBody.position.y), Vector2.down, 2f, groundLayer);

		// Reset speed on every update
		CurrentSpeed = 0;

		// set direction properly
		switch (MoveDirection)
		{
			// if going left but no ground on left while there is ground on right turn right
			case Direction.Left:
				if ((!hitLeft) && hitRight)
				{
					MoveDirection = Direction.Right;
				}
				break;
			// if going right but no ground on right while there is ground on left turn left
			case Direction.Right:
				if ((!hitRight) && hitLeft)
				{
					MoveDirection = Direction.Left;
				}
				break;
			case Direction.Stationary:
				// if not moving and there is ground on left turn left and walk
				if (hitLeft)
				{
					MoveDirection = Direction.Left;
				}
				// if not moving and there is ground on right turn right and walk
				else if (hitRight)
				{
					MoveDirection = Direction.Right;
				}
				break;
			default:
				break;
		}

		// if no land on both sides stay still
		if (!hitLeft && !hitRight)
		{
			MoveDirection = Direction.Stationary;
		}

		// set rotation and speed according to chosen movement direction
		if (MoveDirection == Direction.Left)
		{
			transform.rotation = Quaternion.Euler(0, 0, 0);
			CurrentSpeed = -speed;
		}
		else if (MoveDirection == Direction.Right)
		{
			transform.rotation = Quaternion.Euler(0, 180, 0);
			CurrentSpeed = speed;
		}
		else
		{
			CurrentSpeed = 0;
		}

		// setting proper animation
		animator.SetFloat("Speed", Mathf.Abs(CurrentSpeed));
	}

	private void FixedUpdate()
	{
		// set proper velocity (speed calculated in update)
		RigidBody.velocity = new Vector3(CurrentSpeed, RigidBody.velocity.y);
	}

	private void HurtEnemy()
	{
		// set animator as hit (obviously)
		animator.SetBool("Hit", true);
		// pop player a bit in the air and add score
		if (player != null)
		{
			player.Jump();
			player.AddScore(points);
		}
		// jump a bit and fall through everything
		RigidBody.velocity = new Vector2(0, 10);

		// turning all colliders off will make enemy fall through the ground
		// to ensure deathcatcher can catch it enemy has a child object in a different layer (DeadBodies) deathcatcher's layer is Hell
		// DeadBodies and Hell layers don't collide with any other layer except each other
		foreach (Collider2D coll in GetComponents(typeof(Collider2D)))
		{
			coll.enabled = false;
		}
		this.enabled = false;
	}
}
