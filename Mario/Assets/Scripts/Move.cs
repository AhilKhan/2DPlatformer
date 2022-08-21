using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Move a Camera object to follow a target
/// </summary>
public class Move : MonoBehaviour
{
	// Target transform to follow
	public Transform target;
	
	// Background tilemap (to adjust the position of camera object
	public UnityEngine.Tilemaps.TilemapRenderer Tilemap;

	// main camera object to get screen dimensions
	[SerializeField] private Camera cameraObject;

	// ignore too slight movements
	[SerializeField] private float Margin;

	// Variables for minimum and maximum possible x coordinates of camera object
	float MinBound;
	float MaxBound;

	// half of screen width
	float ScreenHalfWidth;

    // Start is called before the first frame update
    void Start()
    {
		// setting camera in case it's not assigned in script
		if (cameraObject == null)
			cameraObject = Camera.main;


		// calculating all variables
		MinBound = Tilemap.bounds.min.x;
		MaxBound = Tilemap.bounds.max.x;

		ScreenHalfWidth = cameraObject.orthographicSize * ((float) Screen.width / Screen.height);
	}

	// Update is called once per frame
	void Update()
    {
		// if target is provided follow it
		if (target != null)
		{
			// set current position as default
			float x = transform.position.x;
			// if target has moved enough update the position
			if (Mathf.Abs(transform.position.x - target.position.x) > Margin)
			{
				x = target.position.x;
			}

			// if minimum and maximum changes update the variables
			if (MinBound != Tilemap.bounds.min.x)
			{
				MinBound = Tilemap.bounds.min.x;
			}

			if (MaxBound != Tilemap.bounds.max.x)
			{
				MaxBound = Tilemap.bounds.max.x;
			}

			// clamp the x coordinate
			x = Mathf.Clamp(x, MinBound + ScreenHalfWidth, MaxBound - ScreenHalfWidth);
			// set position
			transform.position = new Vector3(x, transform.position.y, transform.position.z);
		}
	}
}
