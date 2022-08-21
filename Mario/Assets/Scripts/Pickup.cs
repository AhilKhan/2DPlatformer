using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickup Component. To be attached to a gameobject with a trigger type Collider2d
/// </summary>
public class Pickup : MonoBehaviour
{
	// Points to be added to player's score when this object get's picked up
	[SerializeField] private int Points;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		// if Collided with a player
		if (collision != null)
		{
			Player player = collision.gameObject.GetComponent<Player>();
			if (player != null)
			{
				// add score to player's score and disable this gameObject
				player.AddScore(Points);
				gameObject.SetActive(false);
			}
		}
	}
}
