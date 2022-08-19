using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelDesigner : MonoBehaviour
{
	public enum States
	{
		Play,
		Pause,
		Countdown,
		Victory,
		GameOver,
		Start
	}
	private States State;

	public States GameState
	{
		get { return State; }
		set
		{
			HandleState(value);
			State = value;
		}
	}

	public TMPro.TextMeshProUGUI StateDisplay;
	public TMPro.TextMeshProUGUI InfoDisplay;


	private enum TilePosition
	{
		Top,
		Middle,
		Bottom,
		Left,
		Right
	}

	public int JumpChance;

	public int MinJumpLength;
	public int MaxJumpLength;

	public int MinGroundHeight;
	public int MaxGroundHeight;

	public int MinWidthMultiplier;
	public int MaxWidthMultiplier;

	public int MinPlatformHeight;
	public int MaxPlatformHeight;

	public int PlatformChance;

	public int MinPlatformWidth;
	public int MaxPlatformWidth;

	public Tilemap BackgroundTilemap;
	public TileBase[] BackgroundTiles;

	public Tilemap ForegroundTilemap;

	public TileBase[] TopLeftGround;
	public TileBase[] TopMiddleGround;
	public TileBase[] TopRightGround;

	public TileBase[] MiddleLeftGround;
	public TileBase[] MiddleMiddleGround;
	public TileBase[] MiddleRightGround;

	public TileBase[] BottomLeftGround;
	public TileBase[] BottomMiddleGround;
	public TileBase[] BottomRightGround;

	[Tooltip("Add in sets of 3 (In case sets are smaller leave empty spaces)")]
	public TileBase[] FloatingPlatform;

	public GameObject[] Pickups;
	public int PickupChance;
	public int MaxPickupPerColumn;

	public GameObject[] Enemy;
	public int EnemyChance;

	public Player playerPrefab;
	public GameObject Flag;

	public GameObject Border;

	public TMPro.TextMeshProUGUI ScoreDisplay;

	public AudioSource UISound;
	public AudioSource CountDown;
	public AudioSource GO;

	public AudioSource BG;
	public AudioSource Victory;
	public AudioSource Defeat;

	// Start is called before the first frame update
	void Start()
	{
		GameState = States.Start;
		Generate();
	}

	// Update is called once per frame
	void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch (GameState)
			{
				case States.Play:
					UISound.Play();
					GameState = States.Pause;
					break;
				case States.Pause:
					UISound.Play();
					GameState = States.Start;
					break;
				case States.Victory:
					UISound.Play();
					GameState = States.Start;
					break;
				case States.GameOver:
					UISound.Play();
					GameState = States.Start;
					break;
				case States.Start:
					UISound.Play();
					Application.Quit();
					break;
				default:
					break;
			}
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			switch (GameState)
			{
				case States.Pause:
					UISound.Play();
					GameState = States.Countdown;
					break;
				case States.Victory:
					UISound.Play();
					GameState = States.Start;
					break;
				case States.GameOver:
					UISound.Play();
					GameState = States.Countdown;
					break;
				case States.Start:
					UISound.Play();
					GameState = States.Countdown;
					break;
				default:
					break;
			}
		}
	}

	public void Generate()
	{
		float ScreenWidth = 2 * (Camera.main.orthographicSize * (Screen.width / Screen.height));
		float ScreenHeight = 2 * Camera.main.orthographicSize;

		int LevelWidth = Random.Range(MinWidthMultiplier * (int)(ScreenWidth / BackgroundTilemap.cellSize.x), MaxWidthMultiplier * (int)(ScreenWidth / BackgroundTilemap.cellSize.x));
		int LevelHeight = (int)(ScreenHeight / BackgroundTilemap.cellSize.y);

		BoundsInt bounds = BackgroundTilemap.cellBounds;
		int minX = bounds.x;
		int minY = bounds.y;

		TileBase Background = BackgroundTiles[Random.Range(0, BackgroundTiles.Length)];

		BackgroundTilemap.ClearAllTiles();
		ForegroundTilemap.ClearAllTiles();
		for (int i = 0; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}
		GenerateBackground(Background, LevelWidth, LevelHeight, new Vector3Int(minX, minY));
		GenerateForeground(LevelWidth, LevelHeight, new Vector3Int(minX, minY));
		GenerateBorders(LevelWidth, LevelHeight, new Vector3Int(minX, minY));
	}

	private void GenerateBackground(TileBase Background, int LevelWidth, int LevelHeight, Vector3Int Min)
	{
		Vector3Int position = new Vector3Int();
		for (int x = 0; x < LevelWidth; x++)
		{
			for (int y = 0; y < LevelHeight; y++)
			{
				position.x = Min.x + x;
				position.y = Min.y + y;
				BackgroundTilemap.SetTile(position, Background);
			}
		}
	}

	private void GenerateForeground(int LevelWidth, int LevelHeight, Vector3Int Min)
	{
		int SelectedIndex = Random.Range(0, TopLeftGround.Length);
		int GroundHeight = Random.Range(LevelHeight / MinGroundHeight, LevelHeight / MaxGroundHeight);

		int JumpDistance = 0;
		bool Jumped = false;
		bool Jumping = false;

		int PlatformHeight = 0;
		int PlatformLength = 0;
		int PlatformIndex = 0;
		bool Platform = false;

		bool PlayerSpawned = false;
		bool SpawnPlayer = false;
		bool SpawnEnemy = false;

		bool Pickup = false;

		bool LevelEnd = false;

		Transform EnemyContainer = new GameObject("Enemies").transform;
		EnemyContainer.SetParent(transform);
		EnemyContainer.gameObject.SetActive(false);

		Transform PickupContainer = new GameObject("Pickups").transform;
		PickupContainer.SetParent(transform);

		for (int x = 0; x < LevelWidth; x++)
		{
			if (x >= LevelWidth - 7)
			{
				Jumped = false;
				Jumping = false;
				JumpDistance = 0;

				PlatformLength = 0;
				PlatformIndex = 0;
				Platform = false;

				SpawnEnemy = false;

				Pickup = false;

				LevelEnd = true;
			}

			if (!LevelEnd)
			{
				if (!Platform)
				{
					if (PlatformLength == 0)
					{
						if (Random.Range(0, PlatformChance) == 0)
						{
							PlatformLength = Random.Range(MinPlatformWidth, MaxPlatformWidth);
							PlatformHeight = Random.Range(MinPlatformHeight, MaxPlatformHeight);
							PlatformIndex = Random.Range(0, (FloatingPlatform.Length / 3) - 1) * 3;
							Platform = true;
						}
					}
				}

				if (Platform)
				{
					int TileX = Min.x + x;
					int TileY = Min.y + PlatformHeight;
					TileBase PlatformTile = FloatingPlatform[PlatformIndex];
					if (ForegroundTilemap.GetTile(new Vector3Int(TileX - 1, TileY)) != null)
					{
						if (ForegroundTilemap.GetTile(new Vector3Int(TileX - 1, TileY)) == FloatingPlatform[PlatformIndex] ||
							ForegroundTilemap.GetTile(new Vector3Int(TileX - 1, TileY)) == FloatingPlatform[PlatformIndex + 1])
						{
							PlatformTile = FloatingPlatform[PlatformIndex + 1];
						}
					}
					if (PlatformLength == 1)
					{
						PlatformTile = FloatingPlatform[PlatformIndex + 2];
					}
					ForegroundTilemap.SetTile(new Vector3Int(TileX, TileY), PlatformTile);

					PlatformLength--;
					if (PlatformLength == 0)
					{
						PlatformHeight = 0;
						PlatformIndex = 0;
						Platform = false;
					}
				}

				if (x < LevelWidth / 25)
				{
					if (!PlayerSpawned && !SpawnPlayer)
					{
						if (Random.Range(0, 10) == 0)
						{
							SpawnPlayer = true;
						}
					}

					if (PlayerSpawned)
					{
						SpawnPlayer = false;
					}
				}
				else
				{
					if (!PlayerSpawned)
					{
						SpawnPlayer = true;
					}
				}

				if (x >= LevelWidth / 10)
				{
					if (PlayerSpawned)
					{
						if (!SpawnEnemy)
						{
							if (Random.Range(0, EnemyChance) == 0)
							{
								SpawnEnemy = true;
							}
						}
					}
				}

				if (x > 5)
				{
					Pickup = true;
				}

				if (!Jumped)
				{
					if (Random.Range(0, JumpChance) == 0)
					{
						JumpDistance = Random.Range(MinJumpLength, MaxJumpLength);
						Jumped = true;
					}
				}
			}

			if (!Jumping)
			{
				for (int y = -2; y <= GroundHeight; y++)
				{
					int TileX = Min.x + x;
					int TileY = Min.y + y;
					bool Top = false;
					
					if (y == GroundHeight)
					{
						Top = true;
					}

					if (Jumped)
					{
						Jumping = true;
					}

					if (GetTileNeighbor(TileX, TileY, Jumped, Top, out TilePosition vertical, out TilePosition horizontal))
					{
						DrawGround(TileX, TileY, SelectedIndex, vertical, horizontal);
					}
					else
					{
						JumpDistance = 0;
						Jumped = false;
						Jumping = false;
					}
				}

				if (SpawnPlayer && !PlayerSpawned)
				{
					GameObject obj = Instantiate(playerPrefab.gameObject, new Vector2(Min.x + x, Min.y + GroundHeight + Random.Range(1f, 5f)), Quaternion.identity);
					FindObjectOfType<Move>().target = obj.transform;
					obj.name = "Player";
					obj.transform.SetParent(transform);
					obj.GetComponent<Player>().ScoreField = ScoreDisplay;
					PlayerSpawned = true;
					SpawnPlayer = false;
					obj.SetActive(false);
				}

				if (SpawnEnemy)
				{
					int index = Random.Range(0, Enemy.Length);
					if (Enemy[index] != null)
					{
						float Height = Random.Range(2f, (float)(LevelHeight - GroundHeight));
						if (Mathf.Abs((Height + GroundHeight) - PlatformHeight) <= 1)
						{
							Height -= 1;
						}

						GameObject obj = Instantiate(Enemy[index], new Vector2(Min.x + x, Min.y + GroundHeight + Height), Quaternion.identity);
						obj.transform.SetParent(EnemyContainer);
						SpawnEnemy = false;
					}
				}

				if (Pickup)
				{
					if (Random.Range(0, PickupChance) == 0)
					{
						for (int i = 0; i < Random.Range(1, MaxPickupPerColumn); i++)
						{
							int MinHeight = 2;
							int MaxHeight = 3;
							float BaseHeight = GroundHeight;
							if (Random.Range(0, 2) == 0)
							{
								if (Platform)
								{
									BaseHeight = PlatformHeight;
								}
							}
							float Height = Random.Range(BaseHeight + MinHeight, BaseHeight + MaxHeight);
							Height = Mathf.Min(Height, LevelHeight - 1);
							if (Platform)
							{
								if (Mathf.Abs(Height - PlatformHeight) <= 1)
								{
									Height -= 1;
								}
							}
							if (Mathf.Abs(Height - GroundHeight) <= 1)
							{
								Height += 1;
							}

							GameObject pickupObject = Instantiate(Pickups[Random.Range(0, Pickups.Length)], PickupContainer);
							pickupObject.transform.position = new Vector2(Min.x + x, Min.y + Height);
						}
					}
				}

				if (x == LevelWidth - 4)
				{
					GameObject flag = Instantiate(Flag, transform);
					flag.transform.position = new Vector2(Min.x + x, Min.y + GroundHeight + 3);
				}
			}
			else
			{
				JumpDistance--;
				if (JumpDistance == 0)
				{
					Jumped = false;
					Jumping = false;
				}
			}
		}
	}

	private void GenerateBorders(int LevelWidth, int LevelHeight, Vector3Int Min)
	{
		int ExtraHeight = 0;

		float Width = LevelWidth;
		float Height = LevelHeight + ExtraHeight;

		float MinX = Min.x;
		float MinY = Min.y - ExtraHeight;

		float MidX = MinX + Width / 2;
		float MidY = MinY + Height / 2;

		float MaxX = MinX + Width;
		float MaxY = MinY + Height;

		GameObject Container = new GameObject("Border");
		Container.transform.SetParent(transform);
		GameObject Top = Instantiate(Border);
		GameObject Bottom = Instantiate(Border);
		GameObject Left = Instantiate(Border);
		GameObject Right = Instantiate(Border);

		Top.transform.position = new Vector3(MidX, MaxY + 0.5f);
		Top.transform.localScale = new Vector3(Width, 1);
		Top.transform.parent = Container.transform;
		Top.GetComponent<Collider2D>().isTrigger = false;
		Top.name = "Top";

		Bottom.transform.position = new Vector3(MidX, MinY - 0.5f);
		Bottom.transform.localScale = new Vector3(Width, 1);
		Bottom.transform.parent = Container.transform;
		Bottom.GetComponent<Collider2D>().isTrigger = true;
		Bottom.name = "Bottom";

		Left.transform.position = new Vector3(MinX - 0.5f, MidY);
		Left.transform.localScale = new Vector3(1, Height);
		Left.transform.parent = Container.transform;
		Left.GetComponent<Collider2D>().isTrigger = false;
		Left.name = "Left";

		Right.transform.position = new Vector3(MaxX + 0.5f, MidY);
		Right.transform.localScale = new Vector3(1, Height);
		Right.transform.parent = Container.transform;
		Right.GetComponent<Collider2D>().isTrigger = false;
		Right.name = "Right";
	}

	private void DrawGround(int x, int y, int tileSelection, TilePosition Vertical, TilePosition Horizontal)
	{
		TileBase tile = null;
		switch (Vertical)
		{
			case TilePosition.Top:
				switch (Horizontal)
				{
					case TilePosition.Left:
						tile = TopLeftGround[tileSelection];
						break;
					case TilePosition.Middle:
						tile = TopMiddleGround[tileSelection];
						break;
					case TilePosition.Right:
						tile = TopRightGround[tileSelection];
						break;
					default:
						break;
				}
				break;
			case TilePosition.Middle:
				switch (Horizontal)
				{
					case TilePosition.Left:
						tile = MiddleLeftGround[tileSelection];
						break;
					case TilePosition.Middle:
						tile = MiddleMiddleGround[tileSelection];
						break;
					case TilePosition.Right:
						tile = MiddleRightGround[tileSelection];
						break;
					default:
						break;
				}
				break;
			case TilePosition.Bottom:
				switch (Horizontal)
				{
					case TilePosition.Left:
						tile = BottomLeftGround[tileSelection];
						break;
					case TilePosition.Middle:
						tile = BottomMiddleGround[tileSelection];
						break;
					case TilePosition.Right:
						tile = BottomRightGround[tileSelection];
						break;
					default:
						break;
				}
				break;
			default:
				break;
		}
		ForegroundTilemap.SetTile(new Vector3Int(x, y), tile);
	}

	private bool GetTileNeighbor(int x, int y, bool Jumping, bool Top, out TilePosition Vertical, out TilePosition Horizontal)
	{
		Horizontal = TilePosition.Middle;
		if (Jumping)
		{
			Horizontal = TilePosition.Right;
		}
		// Empty on Left
		if (ForegroundTilemap.GetTile(new Vector3Int(x - 1, y)) == null)
		{
			Horizontal = TilePosition.Left;
		}

		Vertical = TilePosition.Middle;
		// Top Layer
		if (Top)
			Vertical = TilePosition.Top;
		// Empty below
		if (ForegroundTilemap.GetTile(new Vector3Int(x, y - 1)) == null)
		{
			Vertical = TilePosition.Bottom;
		}

		if (Jumping && Horizontal != TilePosition.Right)
		{
			return false;
		}

		if (Top && Vertical != TilePosition.Top)
		{
			return false;
		}

		return true;
	}

	private void HandleState(States Value)
	{
		Time.timeScale = 0;
		Victory.Stop();
		Defeat.Stop();
		BG.Pause();
		switch (Value)
		{
			case States.Play:
				BG.Play();
				Time.timeScale = 1;
				StateDisplay.text = "";
				InfoDisplay.text = "";
				transform.Find("Enemies").gameObject.SetActive(true);
				transform.Find("Player").gameObject.SetActive(true);
				break;
			case States.Pause:
				BG.Pause();
				StateDisplay.text = "PAUSED!";
				InfoDisplay.text = "press SPACE to continue or ESCAPE to quit";
				break;
			case States.Countdown:
				InfoDisplay.text = "";
				StartCoroutine(Countdown(Current: GameState));
				break;
			case States.Victory:
				BG.Stop();
				Victory.Play();
				StateDisplay.text = "VICTORY";
				InfoDisplay.text = "press SPACE or ESCAPE to return to START screen";
				break;
			case States.GameOver:
				BG.Stop();
				Defeat.Play();
				StateDisplay.text = "GAME OVER";
				InfoDisplay.text = "press SPACE to try again or ESCAPE to quit";
				break;
			case States.Start:
				BG.Play();
				Generate();
				StateDisplay.text = "MARIO";
				InfoDisplay.text = "press SPACE to play or ESCAPE to Close Game";
				break;
			default:
				break;
		}
	}

	IEnumerator Countdown(int CountDown=3, States Current=States.Start)
	{
		while (true)
		{
			StateDisplay.text = CountDown.ToString();
			yield return new WaitForSecondsRealtime(1);
			this.CountDown.Play();
			CountDown--;
			if (CountDown == 0)
			{
				StateDisplay.text = "GO!";
				yield return new WaitForSecondsRealtime(1);
				this.GO.Play();
				if (Current == States.Start || Current == States.GameOver)
				{
					Generate();
				}
				Time.timeScale = 1;
				yield return new WaitForFixedUpdate();
				GameState = States.Play;
				break;
			}
		}
	}
}
