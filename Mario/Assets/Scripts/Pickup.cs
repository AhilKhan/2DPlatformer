using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
	[SerializeField] private int Points;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision != null)
		{
			Player player = collision.gameObject.GetComponent<Player>();
			if (player != null)
			{
				player.AddScore(Points);
				gameObject.SetActive(false);
			}
		}
	}
}
