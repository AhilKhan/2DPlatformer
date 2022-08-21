using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Script to make player win the game when it collides with flag
/// </summary>
public class Flag : MonoBehaviour
{
	private void OnCollisionEnter2D(Collision2D collision)
	{
		// check if collided object is a player
		if (collision != null)
		{
			if (collision.collider.GetComponentInParent<Player>() != null)
			{
				// if it is announce the victory
				FindObjectOfType<LevelDesigner>().GameState = LevelDesigner.States.Victory;
			}
		}
	}
}
