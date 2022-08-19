using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	[Serializable]
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

	// character's move speed
	[SerializeField] private float speed;

	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask playerLayer;

	[SerializeField] private Collider2D HitBox;
	[SerializeField] private Collider2D HurtBox;

	[SerializeField] private int points;

	private float CurrentSpeed;
	private Direction MoveDirection;

	public Player player;

	// Start is called before the first frame update
	void Start()
	{
		// if components not assigned in inspector
		if (RigidBody == null)
			RigidBody = GetComponent<Rigidbody2D>();
		if (animator == null)
			animator = GetComponent<Animator>();
		player = FindObjectOfType<Player>();
	}

	// Update is called once per frame
	void Update()
    {
		if (HitBox.IsTouchingLayers(playerLayer))
		{
			if (player == null)
			{
				player = FindObjectOfType<Player>();
			}
			if (player != null)
				player.HurtPlayer();
		}

		if (HurtBox.IsTouchingLayers(playerLayer))
		{
			HurtEnemy();
		}
		RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(RigidBody.position.x - 1, RigidBody.position.y), Vector2.down, 2f, groundLayer);
		RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(RigidBody.position.x + 1, RigidBody.position.y), Vector2.down, 2f, groundLayer);

		CurrentSpeed = 0;
		switch (MoveDirection)
		{
			case Direction.Left:
				if ((!hitLeft) && hitRight)
				{
					MoveDirection = Direction.Right;
				}
				break;
			case Direction.Right:
				if ((!hitRight) && hitLeft)
				{
					MoveDirection = Direction.Left;
				}
				break;
			default:
				MoveDirection = Direction.Stationary;
				break;
		}

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
			transform.rotation = Quaternion.Euler(0, 0, 0);
			CurrentSpeed = 0;
		}

		animator.SetFloat("Speed", Mathf.Abs(CurrentSpeed));
	}

	private void FixedUpdate()
	{
		RigidBody.velocity = new Vector3(CurrentSpeed, RigidBody.velocity.y);
	}

	private void HurtEnemy()
	{
		animator.SetBool("Hit", true);
		if (player != null)
		{
			player.Jump();
			player.AddScore(points);
		}
		RigidBody.velocity = new Vector2(0, 10);
		foreach (Collider2D coll in GetComponents(typeof(Collider2D)))
		{
			coll.enabled = false;
		}
		this.enabled = false;
	}
}
