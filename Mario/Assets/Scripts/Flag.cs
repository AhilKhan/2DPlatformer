using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision != null)
		{
			if (collision.collider.GetComponentInParent<Rigidbody2D>() != null)
			{
				if (collision.collider.GetComponentInParent<Player>() != null)
				{
					FindObjectOfType<LevelDesigner>().GameState = LevelDesigner.States.Victory;
				}
			}
		}
	}
}
