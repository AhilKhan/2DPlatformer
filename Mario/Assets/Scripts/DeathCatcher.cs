using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCatcher : MonoBehaviour
{
	[SerializeField] private AudioSource DeathSound;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision != null)
		{
			if (collision.GetComponentInParent<Rigidbody2D>() != null)
			{
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
