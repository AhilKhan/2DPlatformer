using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
	public Transform target;
	
	public UnityEngine.Tilemaps.TilemapRenderer Tilemap;

	[SerializeField] private Camera cameraObject;

	[SerializeField] private float Margin;

	float MinBound;
	float MaxBound;

	float ScreenHalfWidth;

    // Start is called before the first frame update
    void Start()
    {
		if (cameraObject == null)
			cameraObject = Camera.main;


		MinBound = Tilemap.bounds.min.x;
		MaxBound = Tilemap.bounds.max.x;

		ScreenHalfWidth = cameraObject.orthographicSize * ((float) Screen.width / Screen.height);
	}

	// Update is called once per frame
	void Update()
    {
		if (target != null)
		{
			float x = transform.position.x;
			if (Mathf.Abs(transform.position.x - target.position.x) > Margin)
			{
				x = target.position.x;
			}

			if (MinBound != Tilemap.bounds.min.x)
			{
				MinBound = Tilemap.bounds.min.x;
			}

			if (MaxBound != Tilemap.bounds.max.x)
			{
				MaxBound = Tilemap.bounds.max.x;
			}

			x = Mathf.Clamp(x, MinBound + ScreenHalfWidth, MaxBound - ScreenHalfWidth);
			transform.position = new Vector3(x, transform.position.y, transform.position.z);
		}
	}
}
