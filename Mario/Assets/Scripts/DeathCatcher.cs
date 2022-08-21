using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to Handle Death by falling.
/// Component to be added to a Collider that is trigger
/// </summary>
public class DeathCatcher : MonoBehaviour
{
	// Sound Effect to be Played when PLAYER dies
	[SerializeField] private AudioSource DeathSound;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		// Destroy every RigidBody that falls through
		if (collision != null)
		{
			if (collision.GetComponentInParent<Rigidbody2D>() != null)
			{
				// If RigidBody is a player Set gamestate to game Over
				if (collision.GetComponentInParent<Player>() != null)
				{
					DeathSound.Play();
					FindObjectOfType<LevelDesigner>().GameState = LevelDesigner.States.GameOver;
				}
				Destroy(collision.transform.parent.gameObject);
			}
		}
	}
}
