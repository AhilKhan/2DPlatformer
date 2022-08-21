using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Script to Procedurally Generate levels, Spawn enemies and Pickups, and handle Game States
/// </summary>
public class LevelDesigner : MonoBehaviour
{
	// Possible Game States
	public enum States
	{
		Play,
		Pause,
		Countdown,
		Victory,
		GameOver,
		Start
	}

	// using full Property to manage states
	private States State;
	public States GameState
	{
		get { return State; }
		set
		{
			// handle state before actually changing state
			HandleState(value);
			State = value;
		}
	}

	// Canvas Text Objects to Display Information related to current GameState
	public TMPro.TextMeshProUGUI StateDisplay;
	public TMPro.TextMeshProUGUI InfoDisplay;

	// possible tile positions (not very important just easier)
	private enum TilePosition
	{
		Top,
		Middle,
		Bottom,
		Left,
		Right
	}

	// All Following Variables are assigned in Inspector and used to procedurally generate level

	// Chances of there being a jump block in the next column CHANCES ARE 1 in JUMPCHANCE
	public int JumpChance;

	// Minimum and Maximum number of columns to Jump
	public int MinJumpLength;
	public int MaxJumpLength;

	// Minimum and Maximum Height to make the ground
	public int MinGroundHeight;
	public int MaxGroundHeight;

	// Minimum and Maximum Width to generate the level
	// Level will be somewhere between minimum and maximum number of SCREENS wide
	public int MinWidthMultiplier;
	public int MaxWidthMultiplier;

	// Range of Height to Spawn the Platforms
	public int MinPlatformHeight;
	public int MaxPlatformHeight;

	// Just like jump Chances to spawn a platform
	public int PlatformChance;

	// Minimum and maximum width of the platform
	public int MinPlatformWidth;
	public int MaxPlatformWidth;

	// Following are all Assets/Resources to be used in Generating Level

	// Tilemap to store the Background of the level (this has to be a different object to help handle collisions)
	public Tilemap BackgroundTilemap;
	// all options for background (these tiles will be filled on the entire background tilemap)
	public TileBase[] BackgroundTiles;

	// Main tilemap object to contain all collidable tiles (Ground and platforms)
	public Tilemap ForegroundTilemap;

	// Different types of tiles to be used based on tiles position
	public TileBase[] TopLeftGround;
	public TileBase[] TopMiddleGround;
	public TileBase[] TopRightGround;

	public TileBase[] MiddleLeftGround;
	public TileBase[] MiddleMiddleGround;
	public TileBase[] MiddleRightGround;

	public TileBase[] BottomLeftGround;
	public TileBase[] BottomMiddleGround;
	public TileBase[] BottomRightGround;

	// Platform tiles to be spawned floating in the air
	[Tooltip("Add in sets of 3 (In case sets are smaller leave empty spaces)")]
	public TileBase[] FloatingPlatform;

	// Following are variables and GameObject to be used while Spawning different things (like: Enemies, Pickups, Player)

	// Pickup Related variables
	// all possible pickup Objects
	public GameObject[] Pickups;
	// Chances of spawning a pickup Object
	public int PickupChance;
	// Maximum number of Pickup Objects that can be spawned in a single Column
	// As a personal preference I'm only spawning 1 but the code is ready to spawn multiple
	public int MaxPickupPerColumn;

	// Enemy related variables
	// Enemy Prefabs
	public GameObject[] Enemy;
	// Chances of Spawning an enemy
	public int EnemyChance;

	// Player Prefab
	public Player playerPrefab;
	// UI Object to display player's score (This will be passed as is to the player when it is spawned)
	public TMPro.TextMeshProUGUI ScoreDisplay;

	// Flag Object (to spawn in the end of level)
	public GameObject Flag;

	// Borders to clamp player and enemy's movement to the level and to check when they die
	public GameObject Border;

	// Audio variables

	// Sound to be used at every button press (to change states)
	public AudioSource UISound;
	// Sound to be used by countdown state
	public AudioSource CountDown;
	// Sound to be played at the end of countdown (at the beginning of playstate)
	public AudioSource GO;

	// Main background sound (to be played almost everytime)
	public AudioSource BG;
	// Sound to be played when player wins
	public AudioSource Victory;
	// sound to be played when player loses
	public AudioSource Defeat;

	// Start is called before the first frame update
	void Start()
	{
		// set default gameState and generate a level (to be used as a background)
		GameState = States.Start;
		Generate();
	}

	// Update is called once per frame
	void Update()
    {
		// Change States on keypress (Space and escape)
		// if Escape pressed
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch (GameState)
			{
				// Pause the game if playing
				case States.Play:
					UISound.Play();
					GameState = States.Pause;
					break;
				// Quit the game if Paused
				case States.Pause:
					UISound.Play();
					GameState = States.Start;
					break;
				// Similar to Pause
				case States.Victory:
					UISound.Play();
					GameState = States.Start;
					break;
				// Similar to Pause
				case States.GameOver:
					UISound.Play();
					GameState = States.Start;
					break;
				// Close the Application if at the first screen
				case States.Start:
					UISound.Play();
					Application.Quit();
					break;
				default:
					break;
			}
		}

		// when space pressed
		if (Input.GetKeyDown(KeyCode.Space))
		{
			switch (GameState)
			{
				// Start playing again (after a countdown) if Paused
				case States.Pause:
					UISound.Play();
					GameState = States.Countdown;
					break;
				// Go to main screen if WON
				case States.Victory:
					UISound.Play();
					GameState = States.Start;
					break;
				// Restart Game if LOST (after a countdown)
				case States.GameOver:
					UISound.Play();
					GameState = States.Countdown;
					break;
				// Start a game (after a countdown) if in START state (first state)
				case States.Start:
					UISound.Play();
					GameState = States.Countdown;
					break;
				default:
					break;
			}
		}
	}

	/// <summary>
	/// Main function to genrate level
	/// </summary>
	private void Generate()
	{
		// Getting Screen's width and height
		float ScreenWidth = 2 * (Camera.main.orthographicSize * (Screen.width / Screen.height));
		float ScreenHeight = 2 * Camera.main.orthographicSize;

		// using multipliers to set proper level width (level height is same as screen height)
		int LevelWidth = Random.Range(MinWidthMultiplier * (int)(ScreenWidth / BackgroundTilemap.cellSize.x),
			MaxWidthMultiplier * (int)(ScreenWidth / BackgroundTilemap.cellSize.x));
		int LevelHeight = (int)(ScreenHeight / BackgroundTilemap.cellSize.y);

		// Getting the lowest point in the tilemap (to be used to calculate each cell's position)
		BoundsInt bounds = BackgroundTilemap.cellBounds;
		int minX = bounds.x;
		int minY = bounds.y;

		// Choosing a background tile
		TileBase Background = BackgroundTiles[Random.Range(0, BackgroundTiles.Length)];

		// making sure none of previously generated stuff stays active
		BackgroundTilemap.ClearAllTiles();
		ForegroundTilemap.ClearAllTiles();
		// Spawned objects are stored as child so clearing them is also needed
		for (int i = 0; i < transform.childCount; i++)
		{
			Destroy(transform.GetChild(i).gameObject);
		}

		// as the name suggests generating level in three steps
		GenerateBackground(Background, LevelWidth, LevelHeight, new Vector3Int(minX, minY));
		GenerateForeground(LevelWidth, LevelHeight, new Vector3Int(minX, minY));
		GenerateBorders(LevelWidth, LevelHeight, new Vector3Int(minX, minY));
	}

	/// <summary>
	/// Setting background tile to the whole background tilemap
	/// </summary>
	/// <param name="Background">Tile to be filled</param>
	/// <param name="LevelWidth">Width of the level (in number of Tiles)</param>
	/// <param name="LevelHeight">Height of the level (in number of Tiles)</param>
	/// <param name="Min">Lowest point in the tilemap</param>
	private void GenerateBackground(TileBase Background, int LevelWidth, int LevelHeight, Vector3Int Min)
	{
		// Variable to store position of each tile when spawning
		Vector3Int position = new Vector3Int();
		// Loop for each cell.. Column by Column
		for (int x = 0; x < LevelWidth; x++)
		{
			for (int y = 0; y < LevelHeight; y++)
			{
				// Calculate position and set the tile
				position.x = Min.x + x;
				position.y = Min.y + y;
				BackgroundTilemap.SetTile(position, Background);
			}
		}
	}

	/// <summary>
	/// Setting Ground and Platform tiles to the foreground tilemap
	/// </summary>
	/// <param name="LevelWidth">Width of Level (in number of Tiles)</param>
	/// <param name="LevelHeight">Height of Level (in number of Tiles)</param>
	/// <param name="Min">Lowest point in the tilemap</param>
	private void GenerateForeground(int LevelWidth, int LevelHeight, Vector3Int Min)
	{
		// Picking a variant of the ground (this index will be used in all ground tile arrays) (so fill those properly)
		int SelectedIndex = Random.Range(0, TopLeftGround.Length);
		// Picking a height for the ground
		int GroundHeight = Random.Range(LevelHeight / MinGroundHeight, LevelHeight / MaxGroundHeight);

		// ALL THE FOLLOWING VARIABLES WILL BE SET AND USED INSIDE THE LOOP

		// Jump related variables
		// How long the jump can be
		int JumpDistance = 0;
		// is the jump started
		bool Jumped = false;
		// is the jump currently in progress
		bool Jumping = false;

		// platform related variables
		// Height of platform
		int PlatformHeight = 0;
		// length (or width) of platform
		int PlatformLength = 0;
		// index of platform in the array (this will be used in sets of 3 so it's picked like that as well)
		int PlatformIndex = 0;
		// is a platform being drawn currently
		bool Platform = false;

		// is player already spawned
		bool PlayerSpawned = false;
		// is player supposed to be spawned in this column
		bool SpawnPlayer = false;

		// is enemy supposed to be spawned in this column
		bool SpawnEnemy = false;

		// is pickup supposed to be spawned in this column
		bool Pickup = false;

		// is end of the level approaching
		bool LevelEnd = false;

		// new object made just to contain all spawned enemies
		Transform EnemyContainer = new GameObject("Enemies").transform;
		// making the newly made enemy container a child of level designer and disable the object
		EnemyContainer.SetParent(transform);
		EnemyContainer.gameObject.SetActive(false);

		// similar to enemy container create pickups container object and make level designer it's parent
		Transform PickupContainer = new GameObject("Pickups").transform;
		PickupContainer.SetParent(transform);

		// THIS IS WHERE THE LEVEL DESIGN ACTUALLY STARTS

		for (int x = 0; x < LevelWidth; x++)
		{
			// If Level End is approaching Disable everything and set level end to true
			if (x >= LevelWidth - 7)
			{
				// disable jumps in case any jumps are active
				Jumped = false;
				Jumping = false;
				JumpDistance = 0;

				// similar to jump, disable platforms to ensure there aren't any platforms near the flag 
				PlatformLength = 0;
				PlatformIndex = 0;
				Platform = false;

				// Don't spawn enemies or pickups near level END
				SpawnEnemy = false;
				Pickup = false;

				// set level end variable
				LevelEnd = true;
			}

			// if level end isn't set yet
			if (!LevelEnd)
			{
				// if no platform is active at this column check if one should be generated
				if (!Platform)
				{
					// make sure last platform ended properly (this isn't needed it's just for double checking)
					if (PlatformLength == 0)
					{
						// randomly decide whether to spawn a platform or not
						if (Random.Range(0, PlatformChance) == 0)
						{
							// if decided to spawn randomly calculate platform's length height and choose which platform to spawn
							PlatformLength = Random.Range(MinPlatformWidth, MaxPlatformWidth);
							PlatformHeight = Random.Range(MinPlatformHeight, MaxPlatformHeight);
							// while choosing platform remember platforms are placed in the array in sets of 3
							PlatformIndex = Random.Range(0, (FloatingPlatform.Length / 3) - 1) * 3;

							// set platform active at this column
							Platform = true;
						}
					}
				}

				// if platform is active for this column
				if (Platform)
				{
					// calculate correct coordinates
					int TileX = Min.x + x;
					int TileY = Min.y + PlatformHeight;

					// using a variable to store active tile to be filled in tilemap
					// by default the variable should be left edge of platform
					TileBase PlatformTile = FloatingPlatform[PlatformIndex];
					// if platform has it's own tile (left or middle tile) on the left set current platform tile to middle tile
					if (ForegroundTilemap.GetTile(new Vector3Int(TileX - 1, TileY)) != null)
					{
						if (ForegroundTilemap.GetTile(new Vector3Int(TileX - 1, TileY)) == FloatingPlatform[PlatformIndex] ||
							ForegroundTilemap.GetTile(new Vector3Int(TileX - 1, TileY)) == FloatingPlatform[PlatformIndex + 1])
						{
							PlatformTile = FloatingPlatform[PlatformIndex + 1];
						}
					}
					// if this is platform's last column set platform tile to right edge of platform
					if (PlatformLength == 1)
					{
						PlatformTile = FloatingPlatform[PlatformIndex + 2];
					}
					// set the chosen tile to the tilemap
					ForegroundTilemap.SetTile(new Vector3Int(TileX, TileY), PlatformTile);

					// manage platform length
					PlatformLength--;
					// pretty obvious but still
					// stop making platform and revert to all defaults when platform length reaches 0
					if (PlatformLength == 0)
					{
						PlatformHeight = 0;
						PlatformIndex = 0;
						Platform = false;
					}
				}

				// spawn player randomly somewhere (just in the first 4% of the level)
				if (x < LevelWidth / 25)
				{
					// if player isn't spawned yet and not being spawned in this column
					if (!PlayerSpawned && !SpawnPlayer)
					{
						// 10% chance to spawn player
						if (Random.Range(0, 10) == 0)
						{
							SpawnPlayer = true;
						}
					}

					// if player is spawned stop spawning (To avoid multiple players)
					if (PlayerSpawned)
					{
						SpawnPlayer = false;
					}
				}
				// if level is going more than 4% and player still isn't spawned spawn him immediately
				else
				{
					if (!PlayerSpawned)
					{
						SpawnPlayer = true;
					}
				}

				// if level is more than 10% done start spawning enemies
				if (x >= LevelWidth / 10)
				{
					// only spawn enemies if player is spawned (just to double check) Player's definitely spawned by this time
					if (PlayerSpawned)
					{
						// if not spawning an enemy at this column
						if (!SpawnEnemy)
						{
							// randomly decide whether to spawn an enemy or not
							if (Random.Range(0, EnemyChance) == 0)
							{
								SpawnEnemy = true;
							}
						}
					}
				}

				// start spawning pickups after first 5 tiles
				if (x > 5)
				{
					Pickup = true;
				}

				// if not jumped yet (if not jumping currently)
				if (!Jumped)
				{
					// randomly decide whether to jump or not
					if (Random.Range(0, JumpChance) == 0)
					{
						// if decided to jump decide how long the jump is going to be
						JumpDistance = Random.Range(MinJumpLength, MaxJumpLength);
						Jumped = true;
					}
				}
			}

			// make ground if not jumping this column
			if (!Jumping)
			{
				// loop for ground height (make some extras below ground for looks reasons
				for (int y = -2; y <= GroundHeight; y++)
				{
					// Calculate correct coordinates
					int TileX = Min.x + x;
					int TileY = Min.y + y;

					// is current tile supposed to be on top
					bool Top = false;
					
					// if y coordinate is ground height set top to true otherwise leave it false
					if (y == GroundHeight)
					{
						Top = true;
					}

					// if jumped in this column set jumping to true
					// multiple variables coz:
					// jumping continuously stops ground tile generation
					// jumped sets jumping to true and makes ground tiles have a right side edge
							// to look good with emptiness or jump
					if (Jumped)
					{
						Jumping = true;
					}

					// if tile neighbour function returns true draw it
					if (GetTileNeighbor(TileX, TileY, Jumped, Top, out TilePosition vertical, out TilePosition horizontal))
					{
						DrawGround(TileX, TileY, SelectedIndex, vertical, horizontal);
					}
					// if tile neighbour returned false disable jump completely
					else
					{
						JumpDistance = 0;
						Jumped = false;
						Jumping = false;
					}
				}

				// if player isn't spawned and is set to spawn in this column
				if (SpawnPlayer && !PlayerSpawned)
				{
					// instantiate player from player's prefab and set it's position to current corrdinates
					GameObject obj = Instantiate(playerPrefab.gameObject, new Vector2(Min.x + x,
						Min.y + GroundHeight + Random.Range(1f, 5f)), Quaternion.identity);

					// set camera to follow player
					FindObjectOfType<Move>().target = obj.transform;
					// set it's name to player and parent to level designer
					obj.name = "Player";
					obj.transform.SetParent(transform);

					// also provide score field to player component to display player's score
					obj.GetComponent<Player>().ScoreField = ScoreDisplay;
					// disable player's object and set both booleans properly
					PlayerSpawned = true;
					SpawnPlayer = false;
					obj.SetActive(false);
				}

				// if supposed to spawn enemy in this column
				if (SpawnEnemy)
				{
					// choose enemy from the array
					int index = Random.Range(0, Enemy.Length);
					// if enemy does exist in the array spawn it
					if (Enemy[index] != null)
					{
						// calculate vertical position
						float Height = Random.Range(2f, (float)(LevelHeight - GroundHeight));

						// make sure to avoid platforms
						if (Mathf.Abs((Height + GroundHeight) - PlatformHeight) <= 1)
						{
							Height -= 1;
						}

						// spawn enemy and set it's parent to enemy container object made in script
						GameObject obj = Instantiate(Enemy[index], new Vector2(Min.x + x, Min.y + GroundHeight + Height), Quaternion.identity);
						obj.transform.SetParent(EnemyContainer);
						// stop spawning enemies for now
						SpawnEnemy = false;
					}
				}

				// if pickup spawning is enabled
				if (Pickup)
				{
					// decide whether to spawn one in this column
					if (Random.Range(0, PickupChance) == 0)
					{
						// loop for maximum allowed pickups
						for (int i = 0; i < Random.Range(1, MaxPickupPerColumn); i++)
						{
							// spawn a bit far from closest ground
							int MinHeight = 2;
							int MaxHeight = 3;
							// by default base is ground
							float BaseHeight = GroundHeight;

							// decide 50% chance to spawn over platform (if platform exists at this column)
							if (Random.Range(0, 2) == 0)
							{
								if (Platform)
								{
									BaseHeight = PlatformHeight;
								}
							}

							// randomly decide vertical coordinate for pickup
							float Height = Random.Range(BaseHeight + MinHeight, BaseHeight + MaxHeight);
							// clamp height to keep it inside the level
							Height = Mathf.Min(Height, LevelHeight - 1);
							// if column contains a platform
							if (Platform)
							{
								// adjust pickup's vertical postition to make sure it doesn't interfere with platform
								if (Mathf.Abs(Height - PlatformHeight) <= 1)
								{
									Height -= 1;
								}
							}
							// same as platform just for ground
							if (Mathf.Abs(Height - GroundHeight) <= 1)
							{
								Height += 1;
							}

							// spawn the pickup and set it's position
							GameObject pickupObject = Instantiate(Pickups[Random.Range(0, Pickups.Length)], PickupContainer);
							pickupObject.transform.position = new Vector2(Min.x + x, Min.y + Height);
						}
					}
				}

				// spawn flag at 4th tile before level's end
				if (x == LevelWidth - 4)
				{
					// spawn flag and set it's position
					GameObject flag = Instantiate(Flag, transform);
					flag.transform.position = new Vector2(Min.x + x, Min.y + GroundHeight + 3);
				}
			}
			// this is else of (!Jumping)
			else
			{
				// if jumping this column count it and reduce the jump distance for the next column
				JumpDistance--;
				// when jump distance reaches 0 stop jumping
				if (JumpDistance == 0)
				{
					Jumped = false;
					Jumping = false;
				}
			}
		}
	}

	/// <summary>
	/// Spawning borders of correct sizes at proper positions
	/// </summary>
	/// <param name="LevelWidth">Width of level (in number of Tiles)</param>
	/// <param name="LevelHeight">Height of Level (in number of Tiles)</param>
	/// <param name="Min">Lowest position of Tilemap</param>
	private void GenerateBorders(int LevelWidth, int LevelHeight, Vector3Int Min)
	{
		// to have to border few tiles below the actual bottom of the screen
		int ExtraHeight = 0;

		// Getting total width and height of the borders
		float Width = LevelWidth;
		float Height = LevelHeight + ExtraHeight;

		// Getting different Coordinates for borders
		float MinX = Min.x;
		float MinY = Min.y - ExtraHeight;

		float MidX = MinX + Width / 2;
		float MidY = MinY + Height / 2;

		float MaxX = MinX + Width;
		float MaxY = MinY + Height;

		// Create new Object named "Border" to contain all borders (to be spawned)
		GameObject Container = new GameObject("Border");
		// set Level Designer as Container's parent
		Container.transform.SetParent(transform);

		// Instantiate all 4 borders
		GameObject Top = Instantiate(Border);
		GameObject Bottom = Instantiate(Border);
		GameObject Left = Instantiate(Border);
		GameObject Right = Instantiate(Border);

		// Setting position size and name of Top border
		// also making sure it's collider isn't a trigger meaning it'll stop other collider's motion
		Top.transform.position = new Vector3(MidX, MaxY + 0.5f);
		Top.transform.localScale = new Vector3(Width, 1);
		Top.transform.parent = Container.transform;
		Top.GetComponent<Collider2D>().isTrigger = false;
		Top.name = "Top";

		// Setting position size and name of Bottom border
		// also making sure it's collider is a trigger meaning it'll not stop other collider's motion
		Bottom.transform.position = new Vector3(MidX, MinY - 0.5f);
		Bottom.transform.localScale = new Vector3(Width, 1);
		Bottom.transform.parent = Container.transform;
		Bottom.GetComponent<Collider2D>().isTrigger = true;
		Bottom.name = "Bottom";

		// Setting position size and name of Left border
		// also making sure it's collider isn't a trigger meaning it'll stop other collider's motion
		Left.transform.position = new Vector3(MinX - 0.5f, MidY);
		Left.transform.localScale = new Vector3(1, Height);
		Left.transform.parent = Container.transform;
		Left.GetComponent<Collider2D>().isTrigger = false;
		Left.name = "Left";

		// Setting position size and name of Right border
		// also making sure it's collider isn't a trigger meaning it'll stop other collider's motion
		Right.transform.position = new Vector3(MaxX + 0.5f, MidY);
		Right.transform.localScale = new Vector3(1, Height);
		Right.transform.parent = Container.transform;
		Right.GetComponent<Collider2D>().isTrigger = false;
		Right.name = "Right";
	}

	/// <summary>
	/// Drawing a tile to the foreground tilemap
	/// </summary>
	/// <param name="x">X position of Tile</param>
	/// <param name="y">Y position of Tile</param>
	/// <param name="tileSelection">selected index of ground</param>
	/// <param name="Vertical">Vertical position of the tile</param>
	/// <param name="Horizontal">Horizontal position of tile</param>
	private void DrawGround(int x, int y, int tileSelection, TilePosition Vertical, TilePosition Horizontal)
	{
		// Initialize tile variable for further use
		TileBase tile = null;
		// Get proper tile using a combination of vertical and horizontal tileposition
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
		// set tile to received position (if tile is null let it be)
		ForegroundTilemap.SetTile(new Vector3Int(x, y), tile);
	}

	/// <summary>
	/// Get Vertical and Horizontal positions of tile at coordinates x and y.
	/// </summary>
	/// <param name="x">x coordinates of tile</param>
	/// <param name="y">y coordinates of tile</param>
	/// <param name="Jumping">is the tile just before a jump</param>
	/// <param name="Top">is the tile supposed to be on top</param>
	/// <param name="Vertical">output of vertical position of the tile</param>
	/// <param name="Horizontal">output of horizontal position of the tile</param>
	/// <returns>True if tile is possible, False if for some reason results aren't correct</returns>
	private bool GetTileNeighbor(int x, int y, bool Jumping, bool Top, out TilePosition Vertical, out TilePosition Horizontal)
	{
		// set Base horizontal position to middle
		Horizontal = TilePosition.Middle;
		// if jumping set horizontal position to right
		if (Jumping)
		{
			Horizontal = TilePosition.Right;
		}
		// if Empty on Left set horizontal position to left
		if (ForegroundTilemap.GetTile(new Vector3Int(x - 1, y)) == null)
		{
			Horizontal = TilePosition.Left;
		}

		// similar to horizontal set base vertical position to middle
		Vertical = TilePosition.Middle;
		// if Tile is on Top Layer set vertical position to top
		if (Top)
		{
			Vertical = TilePosition.Top;
		}
		// if tile below this tile is Empty set vertical position to bottom
		if (ForegroundTilemap.GetTile(new Vector3Int(x, y - 1)) == null)
		{
			Vertical = TilePosition.Bottom;
		}

		// Checking edge cases (issues) and returning false for them
		if (Jumping && Horizontal != TilePosition.Right)
		{
			return false;
		}

		if (Top && Vertical != TilePosition.Top)
		{
			return false;
		}

		// returning true otherwise
		return true;
	}

	/// <summary>
	/// Setting proper text to display required information whenever state changes
	/// </summary>
	/// <param name="Value">Next Game state</param>
	private void HandleState(States Value)
	{
		// Stop the time first of all
		Time.timeScale = 0;

		// Stop all Background Audios
		Victory.Stop();
		Defeat.Stop();
		BG.Pause();

		// Switch based on input Game State
		switch (Value)
		{
			case States.Play:
				// In Play State
				// Play main background Sound
				BG.Play();
				// Resume the flow of time
				Time.timeScale = 1;

				// Set correct texts
				StateDisplay.text = "";
				InfoDisplay.text = "";

				// Enable Player and Enemies GameObjects (they were disabled to make level look better in START scene)
				transform.Find("Enemies").gameObject.SetActive(true);
				transform.Find("Player").gameObject.SetActive(true);
				break;
			case States.Pause:
				// In Pause State
				// Pause Background Sound
				BG.Pause();

				// Set Proper Texts
				StateDisplay.text = "PAUSED!";
				InfoDisplay.text = "press SPACE to continue or ESCAPE to quit";
				break;
			case States.Countdown:
				// In Countdown State
				// Set Info Text to EMPTY and let Coroutine handle Main Text
				InfoDisplay.text = "";
				StartCoroutine(Countdown(Current: GameState));
				break;
			case States.Victory:
				// In Victory State
				// Stop Background Sound and Start Playing Victory Music
				BG.Stop();
				Victory.Play();

				// Run Coroutine to Wait For Victory music to end and automatically switch back to Start State
				StartCoroutine(VictoryToStart());

				// Set Correct Texts in both text fields
				StateDisplay.text = "VICTORY";
				InfoDisplay.text = "press SPACE or ESCAPE to return to START screen";
				break;
			case States.GameOver:
				// In Game Over State
				// Stop Background Sound and Start playing Defeat Music
				BG.Stop();
				Defeat.Play();

				// Set Correct Texts in both Text Fields
				StateDisplay.text = "GAME OVER";
				InfoDisplay.text = "press SPACE to try again or ESCAPE to quit";
				break;
			case States.Start:
				// In Start State
				// Play Main Background Music and Generate a Level to Display on Start Scene
				BG.Play();
				Generate();

				// Set Correct Texts in both Text Fields
				StateDisplay.text = "START";
				InfoDisplay.text = "press SPACE to play or ESCAPE to Close Game";
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Wait for Victory Music to end and set the game state to start
	/// </summary>
	/// <returns>nothing just Wait for 9 seconds (that's how long the music is)</returns>
	IEnumerator VictoryToStart()
	{
		// Waiting for 9 seconds before proceeding
		yield return new WaitForSecondsRealtime(9);
		// Changing Game State to Start
		GameState = States.Start;
	}

	/// <summary>
	/// Countdown Coroutine. To Count down from any given number and when done Set gamestate to play
	/// </summary>
	/// <param name="CountDown">Number of seconds to count down</param>
	/// <param name="Current">Scene that initiated the countdown</param>
	/// <returns>Nothing</returns>
	IEnumerator Countdown(int CountDown=3, States Current=States.Start)
	{
		// Loop
		while (true)
		{
			// Set Main Display text to current countdown time
			StateDisplay.text = CountDown.ToString();

			// wait for a second
			yield return new WaitForSecondsRealtime(1);
			// play sound
			this.CountDown.Play();
			// reduce Countdown
			CountDown--;

			// when countdown goes to 0
			if (CountDown == 0)
			{
				// set string to GO (last one second) wait for one second for the last time
				StateDisplay.text = "GO!";
				yield return new WaitForSecondsRealtime(1);

				// play final audio
				this.GO.Play();

				// if countdown was triggered by gameover or start state regenerate level (no point starting the same level again)
				if (Current == States.Start || Current == States.GameOver)
				{
					Generate();
				}

				// resume time to enable update functions
				Time.timeScale = 1;
				// wait for a fixed update (just because) and change game state to play.
				yield return new WaitForFixedUpdate();
				GameState = States.Play;
				break;
			}
		}
	}
}
