using DungeonGenerator.Delaunay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Collections;

namespace DungeonGenerator
{

    public enum GridType
    {
        None = 0,
        MainRoom,
        HallWay,
        TmpHallWay = 500,
        Cellular = 600
    }


    public enum RandomSpawnType
    {
        Oval = 0,
        Rectangle,
		Cross,
		LineH,
		LineV
    }

	public enum LevelType
	{
		Start = 0,
		Red = 1,
		Green = 2,
		Blue = 3
	}




    [RequireComponent(typeof(AutoTiling))]
    public class MapGenerator : MonoBehaviour
    {

		public struct RoomDescription
		{
			public int ID {get;}
			public Vector2 Size {get;}
			public RoomType Type {get; set;}
			public Vector3 Position {get; set;}

			public RoomDescription(int id, Vector2 size, RoomType type, Vector3 position)
			{
				this.ID = id;
				this.Size = size;
				this.Type = type;
				Position = position;
			}
			public (int, Vector2, RoomType, Vector3) GetValues()
			{
				return (ID, Size, Type, Position);
			}
		}



		public enum RoomType
		{
			Undefined,
			Start,
			End,
			Generic
		}

        [Header("Map Generate Variables")]
		[SerializeField] private int SEED;
        [SerializeField] private GameObject gridPrefab;
        [SerializeField] private RandomSpawnType randomSpawnType;   // Shape of the Region for randomly selected room position
        [SerializeField] private Vector2Int spawnRegionSize;    // Size of Region
        [SerializeField] private int generateRoomCnt;           // Count of Rooms to spawn
        [SerializeField] private int selectRoomCnt;             // Count of Rooms to select
        [SerializeField] private int minRoomSize;               // Size boundary for big rooms
        [SerializeField] private int maxRoomSize;
        [SerializeField] private int smallMinRoomSize;          // Size boundary for small rooms
        [SerializeField] private int smallMaxRoomSize;
        [SerializeField] private int overlapWidth;              // Width to determine corridor ('L' shape or straight )
        [SerializeField] private int hallwayWidth;              // Width of hallway ( Recommend : lesser than 'maxRoomSize' )
        [SerializeField, Range(1, 9)] private int smoothLevel;  // 9 : Disable smooth
		[SerializeField] private bool isExplodeOnRuntime;

		[Header("Room Generate Variables")]
		[SerializeField] private float chestBorderFix;
		[SerializeField] private float enemyCentreDelimiter;
		[SerializeField] public int tightPassageRadius;



        [Header("Visualize Generating Progress")]
        [SerializeField] private bool isVisualizeProgress;      // On/Off
        [SerializeField] private float roomSpawnTerm;
        [SerializeField] private Color32 roomColor;             // Color of all rooms ( include hallway grids )
        [SerializeField] private Color32 delaunayLineColor;     // Line Color when Bowyer-Watson in execution
        [SerializeField] private Color32 mstLineColor;          // Line Color when Kruskal in excution
        [SerializeField] private float lineWidth;               // Width of all lines
        [SerializeField] private float lineDrawTerm;            // Time between creating and deleting a LineRenderer.

        [Header("Player Reference")]
        [SerializeField] private Transform playerTransform;

        [Header("Objects Spawn")]
        [SerializeField] private GameObject playerPrefab;
		[SerializeField] private GameObject endPrefab;
		[SerializeField] private GameObject enemyPrefab;
		[SerializeField] private GameObject chestPrefab;
        
        private List<GameObject> rooms;
        private HashSet<Delaunay.Vertex> vertices;
        private List<Edge> hallwayEdges;
        private List<GameObject> lineRenderers;
        private List<GameObject> gridsList;

		public List<RoomDescription> selectedRoomsDescriptions {get; private set;}
		public NativeHashMap<int, RoomDescription> indexToRoomDescription ;


        private int[,] map;
        private int minX = int.MaxValue, minY = int.MaxValue;
        private int maxX = int.MinValue, maxY = int.MinValue;

        public int MinX { get => minX; }
        public int MinY { get => minY; }
        public Vector2 StartPosition { get; private set; }
		public Vector2 EndPosition { get; private set; }

		private GameObject PlayerObject;
		private GameObject EndObject;
		private List<GameObject> ObjectsToClear;


		private int currentRoomSeed;
		
		private int LevelCount = 1;
		private LevelType currentLevelType = LevelType.Start;


		private List<Vector3> NoNoCoords;


		private LevelType GetNextLevelType(LevelType inputLevelType){
			switch (inputLevelType){

				case (LevelType.Start):
					return LevelType.Red;
				case (LevelType.Red):
					return LevelType.Green;
				case (LevelType.Green):
					return LevelType.Blue;
				case (LevelType.Blue):
					return LevelType.Red;
			}
			return LevelType.Red;
		}

		
		private void ResetVars() {
			rooms = new List<GameObject>();
			vertices = new HashSet<Delaunay.Vertex>();
			lineRenderers = new List<GameObject>();
			selectedRoomsDescriptions = new List<RoomDescription>();
			indexToRoomDescription = new NativeHashMap<int, RoomDescription>(selectRoomCnt, Allocator.Persistent);
			gridsList = new List<GameObject>();
			NoNoCoords = new List<Vector3>();
			ObjectsToClear = new List<GameObject>();
			minX = int.MaxValue;
			minY = int.MaxValue;
			maxX = int.MinValue;
			maxY = int.MinValue;
			Random.InitState(currentRoomSeed);
		}



		///<summary>
		///	clear objects and tilesets, to prepare for new level generation
		///</summary>
		private void Reset()
		{
			LevelCount++;
			currentRoomSeed += LevelCount;
			ClearAll();
			GetComponent<AutoTiling>().ClearTiles();
			ResetVars();
			currentLevelType = GetNextLevelType(currentLevelType);

			// Телепортируем игрока в безопасное место на время генерации
			if (playerTransform != null)
				playerTransform.position = new Vector3(-100, -100, 0);

			StartCoroutine(MapGenerateCoroutine());
		}

		///<summary>
		///	Destroy all objects on scene
		///</summary>
		private void ClearAll(){ 
			Debug.Log("reached");
			ClearObjects();
			foreach (GameObject _object in ObjectsToClear) Destroy(_object);

		}

		public List<GameObject> GetRooms(){ 
			return rooms;
		}

        private void Start()
        {
			if (SEED == 0) SEED = Random.Range(int.MinValue, int.MaxValue);
			if (!isVisualizeProgress)roomSpawnTerm = 0;
			Debug.Log(LevelCount);
			currentRoomSeed = SEED;
			ResetVars();
			StartCoroutine(MapGenerateCoroutine());
        }
		
        private IEnumerator MapGenerateCoroutine()
        {
            yield return new WaitForSeconds(1f);
            Time.timeScale = 2.0f;

            yield return StartCoroutine(SpawnRooms());
            yield return new WaitForSeconds(8f);                // Wait for Physics Update
            FindMainRooms(selectRoomCnt);

            GenerateMapArr();                                   // 
            MainRoomFraming();                                  // Frame Main rooms with walls
            yield return StartCoroutine(ConnectRooms());        // Connect Rooms ( Delaunay Triangulation and MST )
            yield return StartCoroutine(GenerateHallways());
            CellularAutomata(smoothLevel);                      // Smooth ( I think it isn't necessary )

		
            MapArrNormalization();                              // Normalize Map for auto tiling
            OnMapGenComplete();    
			
                             // Transfer Map Data for auto tiling
            if (isVisualizeProgress) yield return new WaitForSeconds(1f);

            GetComponent<AutoTiling>().TilingMap();
            GetComponent<AutoTiling>().AddCollidersAfterGeneration();
			UnityEngine.Tilemaps.Tilemap nonoTilemap = GetComponent<AutoTiling>().nonoTilemap;
			foreach( Vector3Int tileCoord in nonoTilemap.cellBounds.allPositionsWithin.GetEnumerator())
			{
				if (GetComponent<AutoTiling>().IsNoNoCoord(tileCoord)) NoNoCoords.Add(nonoTilemap.CellToWorld(tileCoord));
			}
			Debug.Log("NoNoCoords count: " + NoNoCoords.Count.ToString());

			foreach (RoomDescription room in selectedRoomsDescriptions) GenerateRoomContent(room);

			EndObject.GetComponent<EndScript>().event_on_interaction.AddListener(Reset);
            ClearObjects();

            Time.timeScale = 1.0f;
            
        }

		private void GenerateRoomContent(RoomDescription room)
		{
			RoomType roomType = room.Type;
			switch (roomType)
			{
				case RoomType.Start:
					SpawnPlayer();
					break;
				case RoomType.End:
					SpawnEnd();
					break;
				default:
					SpawnGeneric(room);
					break;
			}
				

		}
		private void SpawnGeneric(RoomDescription room)
		{
			(float left_fix, float right_fix) = (-room.Size.x/2, room.Size.x/2);
			(float top_fix, float bottom_fix) = (-room.Size.y/2, room.Size.y/2);

			float[,] bounds = new float[2,2];
			bounds[0,0] = room.Position.x + left_fix;
			bounds[0,1] = room.Position.x + right_fix;
			bounds[1,0] = room.Position.y + top_fix;
			bounds[1,1] = room.Position.y + bottom_fix;
			(float left_bound_enemy, float right_bound_enemy) = (bounds[0,0] + enemyCentreDelimiter, bounds[0,1] - enemyCentreDelimiter);
			(float top_bound_enemy, float bottom_bound_enemy) = (bounds[1,0] + enemyCentreDelimiter, bounds[1,1] - enemyCentreDelimiter);

			float coordinate_x = (right_bound_enemy - left_bound_enemy) * Random.value + left_bound_enemy;
			float coordinate_y = (bottom_bound_enemy - top_bound_enemy) * Random.value + top_bound_enemy;
			ObjectsToClear.Add(Instantiate(enemyPrefab, new Vector2(coordinate_x, coordinate_y), Quaternion.identity));

			int randomWall = (int)Random.Range(0,3);

			bounds[0,0] += chestBorderFix;
			bounds[0,1] -= chestBorderFix;
			bounds[1,0] += chestBorderFix;
			bounds[1,1] -= chestBorderFix;

			bool isNoNoBound(Bounds bounds)
			{
				foreach (Vector3 coord in NoNoCoords)
					if (bounds.Contains(coord)) 
						return true;
				return false;
			}

			float chestX = 0;
			float chestY = 0;
			Vector3Int positionInTilemap = GetComponent<AutoTiling>().nonoTilemap.WorldToCell(new Vector3Int((int)chestX, (int)chestY, 0));
			GameObject chest = null;
			Bounds chestBounds;
			int counter = 0;
			do 
			{
				if (chest)
				{
					Destroy(chest);
					Random.InitState(currentRoomSeed++);
				}
				switch (randomWall) {
					case 0:
						chestX =(bounds[0,1] - bounds[0,0]) * Random.value + bounds[0,0];			
						chestY = bounds[1,0];
						break;
					case 1:
						chestX = bounds[0,1];
						chestY = (bounds[1,1] - bounds[1,0]) * Random.value + bounds[1,0];
						break;
					case 2:
						chestX =(bounds[0,1] - bounds[0,0]) * Random.value + bounds[0,0];			
						chestY = bounds[1,1];
						break;
					case 3:
						chestX = bounds[0,0];
						chestY = (bounds[1,1] - bounds[1,0]) * Random.value + bounds[1,0];
						break;
				}

				chest = Instantiate(chestPrefab, new Vector2(chestX, chestY+1), Quaternion.identity);
				chestBounds = chest.GetComponent<BoxCollider2D>().bounds;
				BoxCollider2D checkCollider =  chest.AddComponent<BoxCollider2D>();
				float boundSideY = chestBounds.size.y;
				// HACK: '4' is to be replaced with variable
				chestBounds.Expand(boundSideY*4 - boundSideY);
				chest.transform.position.Set(chest.transform.position.x, chest.transform.position.y + boundSideY/2, 0);


				Vector3 chestCoords = new Vector3(chestX, chestY, 0);

				positionInTilemap = GetComponent<AutoTiling>().nonoTilemap.WorldToCell(chestCoords);
				Debug.Log("positionInTilemap: "+ positionInTilemap.ToString());
				Debug.Log("positionGlobal: "+ chestCoords.ToString());
				Debug.Log("isNoNoBound: " +  isNoNoBound(chestBounds).ToString());
				counter ++;
			} while (isNoNoBound(chestBounds) && counter < 100);

			if (counter >= 100) Destroy(chest);
			else ObjectsToClear.Add(chest);
			
		}


		private void SpawnPlayer(){
			if (playerTransform != null)
			{
				playerTransform.position = StartPosition;
				Debug.Log($"Player teleported to: {StartPosition}");

				// Обновление камеры
				CameraController cameraController = Camera.main.GetComponent<CameraController>();
				if (cameraController != null)
				{
					cameraController.SetPlayerTarget(playerTransform);
				}
			}
			else
			{
				Debug.LogError("Player transform is not assigned in MapGenerator!");
			}
         }
		private void SpawnEnd()
        {
            if (endPrefab != null)
            {
                EndObject = Instantiate(endPrefab, EndPosition, Quaternion.identity);
				 ObjectsToClear.Add(EndObject);
            }
            else
            {
                Debug.LogError("End prefab is not assigned in MapGenerator!");
            }
        }



		private void TurnOnRoomGravity(GameObject room)
		{
			room.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
			room.GetComponent<Rigidbody2D>().gravityScale = 0f;
		}
		private GameObject InstatiateNewRoom(GameObject room_prefab, System.Func<Vector2Int, Vector3> SpawnFunction, Vector2Int size)
		{
			GameObject newRoom = Instantiate(gridPrefab, SpawnFunction(spawnRegionSize), Quaternion.identity);
			newRoom.transform.localScale = GetRandomScale(size[0], size[1]);
			return newRoom;

		}
		private GameObject InstatiateNewRoom(GameObject room_prefab, System.Func<Vector2Int, Vector3> SpawnFunction, Vector2Int size, bool explodeOnRuntime)
		{
			GameObject newRoom = Instantiate(gridPrefab, SpawnFunction(spawnRegionSize), Quaternion.identity);
			if (explodeOnRuntime) TurnOnRoomGravity(newRoom);
			newRoom.transform.localScale = GetRandomScale(size[0], size[1]);
			return newRoom;

		}

		private void PopulateList(List<GameObject> listToAdd, Vector2Int size, int nToAdd,
				GameObject room_prefab, System.Func<Vector2Int, Vector3> SpawnFunction)
		{

		}

		

        /// <summary>Randomly Spawn Rooms</summary>
        private IEnumerator SpawnRooms()
		{

			System.Func<Vector2Int, Vector3> SpawnFunction = GetRandomPointInRect;

			switch (randomSpawnType)
			{
				case RandomSpawnType.Oval:
					SpawnFunction = GetRandomPointInOval;
					break;
				case RandomSpawnType.Rectangle:
					SpawnFunction = GetRandomPointInRect;
					break;
				case RandomSpawnType.Cross:
					SpawnFunction = GetRandomPointInCross;
					break;
				case RandomSpawnType.LineH:
					SpawnFunction = GetRandomPointInLineH;
					break;
				case RandomSpawnType.LineV:
					SpawnFunction = GetRandomPointInLineV;
					break;
			}


			// Randomly spawn rooms
			for (int i = 0; i < selectRoomCnt; i++)
			{
				Random.InitState(currentRoomSeed ++);

				GameObject newRoom = InstatiateNewRoom(gridPrefab, SpawnFunction, new Vector2Int(minRoomSize, maxRoomSize), isExplodeOnRuntime);
				rooms.Add(newRoom);
				yield return new WaitForSeconds(roomSpawnTerm);
			}
			for (int i = 0; i < generateRoomCnt - selectRoomCnt; i++)
			{
				Random.InitState(currentRoomSeed ++);

				GameObject newRoom = InstatiateNewRoom(gridPrefab, SpawnFunction, new Vector2Int(smallMinRoomSize, smallMaxRoomSize), isExplodeOnRuntime);
				rooms.Add(newRoom);
				yield return new WaitForSeconds(roomSpawnTerm);
			}
			
			if (!isExplodeOnRuntime) foreach (GameObject room in rooms) TurnOnRoomGravity(room);
				
		


			// Dynamic for Physics Interaction
		}
		

        private Vector3 GetRandomPointInOval(Vector2Int size)
        {
            float theta = Random.Range(0, 2 * Mathf.PI);
            float rad = Mathf.Sqrt(Random.Range(0, 1f));

            return new Vector3(size.x * rad * Mathf.Cos(theta), size.y * rad * Mathf.Sin(theta));
        }

        private Vector3 GetRandomPointInRect(Vector2Int size)
        {
            float width = Random.Range(-size.x, size.x);
            float height = Random.Range(-size.y, size.y);
            return new Vector3(width, height, 0);
        }

        private Vector3 GetRandomPointInCross(Vector2Int size)
        {
			bool isVertical = Random.value < 0.5f;
            float width = Random.Range(-size.x, size.x);
            float height = Random.Range(-size.y, size.y);
			if (isVertical) return new Vector3(0, height, 0);
			return new Vector3(width, 0, 0);
        }

        private Vector3 GetRandomPointInLineV(Vector2Int size)
        {
            float width = Random.Range(-size.x, size.x);
            float height = Random.Range(-size.y, size.y);
			return new Vector3(0, height, 0);
        }

        private Vector3 GetRandomPointInLineH(Vector2Int size)
        {
            float width = Random.Range(-size.x, size.x);
            float height = Random.Range(-size.y, size.y);
			return new Vector3(width, 0, 0);
        }

        private Vector3 GetRandomScale(int minS, int maxS)
        {
            int x = Random.Range(minS, maxS) * 2;
            int y = Random.Range(minS, maxS) * 2;

            return new Vector3(x, y, 1);
        }
        private int RoundPos(float n, int m)
        {
            return Mathf.FloorToInt(((n + m - 1) / m)) * m;
        }

        /** Select Main Rooms**/
        private void FindMainRooms(int roomCount)
        {
            // Temporaray store each room's Size, Ratio, Index
            List<(float size, int index)> tmpRooms = new List<(float size, int index)>();

            for (int i = 0; i < rooms.Count; i++)
            {
                rooms[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                rooms[i].GetComponent<BoxCollider2D>().isTrigger = true;
                rooms[i].transform.position = new Vector3(RoundPos(rooms[i].transform.position.x, 1), RoundPos(rooms[i].transform.position.y, 1), 1);


                Vector3 scale = rooms[i].transform.localScale;
                float size = scale.x * scale.y;                 // Calculate size of room
                float ratio = scale.x / scale.y;                // 
                if (ratio > 2f || ratio < 0.5f) continue;       // Ignore unbalance rooms
                tmpRooms.Add((size, i));
            }

            // Order by Room size
            var sortedRooms = tmpRooms.OrderByDescending(room => room.size).ToList();

            foreach (var room in rooms)
            {
                room.SetActive(false);
            }

            // Select Rooms ( Except narrow room )
            int count = 0;
            foreach (var roomInfo in sortedRooms)
            {
                if (count >= roomCount) break;
                GameObject room = rooms[roomInfo.index];
                room.GetComponent<SpriteRenderer>().color = roomColor;
                room.SetActive(true);
				RoomDescription roomDescription = new RoomDescription(roomInfo.index, new Vector2(room.transform.localScale.x, room.transform.localScale.y), RoomType.Generic, room.transform.position);
				indexToRoomDescription.Add(roomDescription.ID, roomDescription);

                vertices.Add(new Delaunay.Vertex((int)room.transform.position.x, (int)room.transform.position.y));
				selectedRoomsDescriptions.Add(roomDescription);
                count++;
            }

            if (selectedRoomsDescriptions.Count > 1)
            {
				Debug.Log("Selected {selectedRoomsDescriptions.Count} rooms");

				int randomRoomIndex = Random.Range(0,selectedRoomsDescriptions.Count);
				RoomDescription startRoom = selectedRoomsDescriptions[randomRoomIndex];
				startRoom.Type = RoomType.Start;
				selectedRoomsDescriptions[randomRoomIndex] = startRoom;
				int startRoomID = startRoom.ID;

				int endRoomID; RoomDescription endRoom;
				do {
					randomRoomIndex = Random.Range(0,selectedRoomsDescriptions.Count);
					endRoom = selectedRoomsDescriptions[randomRoomIndex];
					endRoomID = endRoom.ID;
				} while (endRoomID== startRoomID);
				endRoom.Type = RoomType.End;
				selectedRoomsDescriptions[randomRoomIndex] = endRoom;
			
				StartPosition = rooms[startRoomID].transform.position;
				EndPosition = rooms[endRoomID].transform.position;
            }
			else Debug.Log("Too few rooms!");

        }

        /** Rasterize rooms into 2D Array Data **/
        private void GenerateMapArr()
        {
            foreach (var room in rooms)
            {
                Vector3 pos = room.transform.position;
                Vector3 scale = room.transform.localScale;

                minX = Mathf.Min(minX, Mathf.FloorToInt(pos.x - scale.x));
                minY = Mathf.Min(minY, Mathf.FloorToInt(pos.y - scale.y));
                maxX = Mathf.Max(maxX, Mathf.CeilToInt(pos.x + scale.x));
                maxY = Mathf.Max(maxY, Mathf.CeilToInt(pos.y + scale.y));
            }

            int width = maxX - minX;
            int height = maxY - minY;
            map = new int[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    map[i, j] = -1;

            for (int i = 0; i < rooms.Count; i++)
            {
                Vector3 pos = rooms[i].transform.position;
                Vector3 scale = rooms[i].transform.localScale;

                for (int x = (int)-scale.x / 2; x < scale.x / 2; x++)       // Store grid info in map arr
                {
                    for (int y = (int)-scale.y / 2; y < scale.y / 2; y++)
                    {
                        int mapX = Mathf.FloorToInt(pos.x - minX + x);
                        int mapY = Mathf.FloorToInt(pos.y - minY + y);
                        map[mapY, mapX] = i;
                    }
                }
            }
        }
        private void MainRoomFraming()
        {
            foreach (var roomDescription in selectedRoomsDescriptions)
            {
				int index = roomDescription.ID;
				Vector2Int size = new Vector2Int((int)roomDescription.Size.x, (int)roomDescription.Size.y);
                int selectedId = index;

                rooms[selectedId].GetComponent<SpriteRenderer>().color = roomColor;

                int minIx = int.MaxValue, minIy = int.MaxValue;
                int maxIx = int.MinValue, maxIy = int.MinValue;

                for (int y = 0; y < map.GetLength(0); y++)
                {
                    for (int x = 0; x < map.GetLength(1); x++)
                    {
                        if (map[y, x] == selectedId)
                        {
                            minIx = Mathf.Min(minIx, x);
                            maxIx = Mathf.Max(maxIx, x);
                            minIy = Mathf.Min(minIy, y);
                            maxIy = Mathf.Max(maxIy, y);
                        }
                    }
                }

                for (int y = minIy; y <= maxIy; y++)
                {
                    for (int x = minIx; x <= maxIx; x++)
                    {
                        if (x == minIx || x == maxIx || y == minIy || y == maxIy)
                        {
                            map[y, x] = -1;
                        }
                    }
                }
            }
        }

        /** Connect Rooms **/
        private IEnumerator ConnectRooms()
        {
            yield return StartCoroutine(DelaunayTriangulation.TriangulateCoroutine(vertices, isVisualizeProgress, delaunayLineColor, lineWidth, lineDrawTerm));
            var triangles = DelaunayTriangulation.RetTriangles;
            var graph = new HashSet<Delaunay.Edge>();

            foreach (var triangle in triangles)
            {
                if (isVisualizeProgress)
                {
                    var lines = triangle.GetVisualLines();
                    foreach (var line in lines) Destroy(line);
                }
                graph.UnionWith(triangle.edges);
            }

            if (isVisualizeProgress) yield return new WaitForSeconds(1f);

            hallwayEdges = Kruskal.MinimumSpanningTree(graph);

            if (isVisualizeProgress)
            {
                foreach (var edge in hallwayEdges)
                {
                    lineRenderers.Add(Visualization.Instance.SpawnLineRenderer(edge.a.ToVector3(), edge.b.ToVector3(), lineWidth, mstLineColor));
                }
            }
        }

        /** Generate Hallways **/
        private IEnumerator GenerateHallways()
        {
            Vector2Int size1 = new Vector2Int(2, 2);
            Vector2Int size2 = new Vector2Int(2, 2);

            foreach (Delaunay.Edge edge in hallwayEdges)
            {
                Delaunay.Vertex start = edge.a;
                Delaunay.Vertex end = edge.b;

                size1 = new Vector2Int((int)rooms[map[start.y - minY, start.x - minX]].transform.localScale.x, (int)rooms[map[start.y - minY, start.x - minX]].transform.localScale.y);
                size2 = new Vector2Int((int)rooms[map[end.y - minY, end.x - minX]].transform.localScale.x, (int)rooms[map[end.y - minY, end.x - minX]].transform.localScale.y);

                CreateHallwayLine(start, end, size1, size2);
                yield return new WaitForSeconds(lineDrawTerm * 2);
            }

            foreach (Delaunay.Edge edge in hallwayEdges)
            {
                Delaunay.Vertex start = edge.a;
                Delaunay.Vertex end = edge.b;

                size1 = new Vector2Int((int)rooms[map[start.y - minY, start.x - minX]].transform.localScale.x, (int)rooms[map[start.y - minY, start.x - minX]].transform.localScale.y);
                size2 = new Vector2Int((int)rooms[map[end.y - minY, end.x - minX]].transform.localScale.x, (int)rooms[map[end.y - minY, end.x - minX]].transform.localScale.y);

                CreateHallwayWidth(start, end, size1, size2);
                yield return new WaitForSeconds(lineDrawTerm * 2);
            }

        }
        private void CreateHallwayLine(Delaunay.Vertex start, Delaunay.Vertex end, Vector2Int startSize, Vector2Int endSize)
        {
            bool isHorizontalOverlap = Mathf.Abs(start.x - end.x) < ((startSize.x + endSize.x) / 2f - overlapWidth);
            bool isVerticalOverlap = Mathf.Abs(start.y - end.y) < ((startSize.y + endSize.y) / 2f - overlapWidth);


            if (isVerticalOverlap)          // Generate Horizontal Corridor
            {
                int startY = Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2) + Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2);
                startY /= 2;
                for (int x = Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2); x <= Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2); x++)
                {
                    InstantiateGrid(x, startY);
                }
            }
            else if (isHorizontalOverlap)   // Generate Vertical Corridor
            {
                int startX = Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
                startX /= 2;
                for (int y = Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2); y <= Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2); y++)
                {
                    InstantiateGrid(startX, y);
                }
            }
            else                            //  Generate 'L' shaped corridor
            {
                // Get center of vertices
                int mapCenterX = map.GetLength(0) / 2;
                int mapCenterY = map.GetLength(1) / 2;

                int midX = (start.x + end.x) / 2;
                int midY = (start.y + end.y) / 2;

                int quadrant = DetermineQuadrant(midX - mapCenterX - minX, midY - mapCenterY - minY);

                // Determine hallway ('L' or flipped 'L')
                if (quadrant == 2 || quadrant == 3)
                {
                    CreateStraightHallway(start.x, start.y, end.x, start.y);    // Generate horizontal hallway first
                    CreateStraightHallway(end.x, start.y, end.x, end.y);       // Then vertical hallway
                }
                else if (quadrant == 1 || quadrant == 4)
                {
                    CreateStraightHallway(start.x, start.y, start.x, end.y);   // Generate ertical hallway first
                    CreateStraightHallway(start.x, end.y, end.x, end.y);       // Then horizontal hallway
                }
            }
        }
        private void CreateHallwayWidth(Delaunay.Vertex start, Delaunay.Vertex end, Vector2Int startSize, Vector2Int endSize)
        {
            bool isHorizontalOverlap = Mathf.Abs(start.x - end.x) < ((startSize.x + endSize.x) / 2f - overlapWidth);
            bool isVerticalOverlap = Mathf.Abs(start.y - end.y) < ((startSize.y + endSize.y) / 2f - overlapWidth);

            if (isVerticalOverlap)
            {
                int startY = Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2) + Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2);
                startY /= 2;
                for (int x = (int)Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2); x <= (int)Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2); x++)
                {
                    AddHallwayWidth(x, startY);
                }
            }
            else if (isHorizontalOverlap)
            {
                int startX = Mathf.Min(start.x + startSize.x / 2, end.x + endSize.x / 2) + Mathf.Max(start.x - startSize.x / 2, end.x - endSize.x / 2);
                startX /= 2;
                for (int y = (int)Mathf.Min(start.y + startSize.y / 2, end.y + endSize.y / 2); y <= (int)Mathf.Max(start.y - startSize.y / 2, end.y - endSize.y / 2); y++)
                {
                    AddHallwayWidth(startX, y);
                }
            }
            else
            {
                int mapCenterX = map.GetLength(0) / 2;
                int mapCenterY = map.GetLength(1) / 2;

                int midX = (start.x + end.x) / 2;
                int midY = (start.y + end.y) / 2;

                int quadrant = DetermineQuadrant(midX - mapCenterX - minX, midY - mapCenterY - minY);

                if (quadrant == 2 || quadrant == 3)
                {
                    CreateStraightHallwayWidth(start.x, start.y, end.x, start.y);
                    CreateStraightHallwayWidth(end.x, start.y, end.x, end.y);
                }
                else if (quadrant == 1 || quadrant == 4)
                {
                    CreateStraightHallwayWidth(start.x, start.y, start.x, end.y);
                    CreateStraightHallwayWidth(start.x, end.y, end.x, end.y);
                }
            }
        }
        private void CreateStraightHallway(int startX, int startY, int endX, int endY)
        {
            for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
            {
                for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++)
                {
                    InstantiateGrid(x, y);
                }
            }
        }
        private void CreateStraightHallwayWidth(int startX, int startY, int endX, int endY)
        {

            for (int x = Mathf.Min(startX, endX); x <= Mathf.Max(startX, endX); x++)
            {
                for (int y = Mathf.Min(startY, endY); y <= Mathf.Max(startY, endY); y++)
                {
                    AddHallwayWidth(x, y);
                }
            }
        }
        private int DetermineQuadrant(int x, int y)
        {
            if (x >= 0 && y >= 0) return 1;
            if (x < 0 && y >= 0) return 2;
            if (x < 0 && y < 0) return 3;
            if (x >= 0 && y < 0) return 4;

            return -1;  // Never happen 
        }
        private void AddHallwayWidth(int x, int y)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int px = x + i; int py = y + j;
                    if (px < minX || py < minY || py >= maxY || px >= maxX) continue;
                    if (map[py - minY, px - minX] == (int)GridType.TmpHallWay) continue;


                    if (map[py - minY, px - minX] == -1 || !rooms[map[py - minY, px - minX]].activeSelf)
                    {
                        map[py - minY, px - minX] = (int)GridType.TmpHallWay;
                        InstantiateGrid(px, py);
                    }
                }
            }
        }

        /** Smoothing ( Additional ) **/
        private void CellularAutomata(int n)
        {
            for (int x = 0; x < maxX - minX; x++)
            {
                for (int y = 0; y < maxY - minY; y++)
                {
                    if (map[y, x] == (int)GridType.TmpHallWay) continue;
                    if ((map[y, x] != -1 && map[y, x] != (int)GridType.TmpHallWay && rooms[map[y, x]].activeSelf)) continue;


                    int nonWallCount = 0;

                    // Check around grids
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        for (int offsetY = -1; offsetY <= 1; offsetY++)
                        {
                            int checkX = x + offsetX;
                            int checkY = y + offsetY;

                            if (checkX < 0 || checkX >= maxX - minX || checkY < 0 || checkY >= maxY - minY)
                            {
                                continue;
                            }
                            else if (map[checkY, checkX] == -1) continue;
                            else if (map[checkY, checkX] == (int)GridType.Cellular) continue;
                            else if (map[checkY, checkX] == (int)GridType.TmpHallWay || rooms[map[checkY, checkX]].activeSelf)
                            {
                                nonWallCount++;
                            }
                        }
                    }

                    if (nonWallCount >= n)
                    {
                        map[y, x] = (int)GridType.Cellular;
                    }
                }
            }


            for (int x = 0; x < maxX - minX; x++)
            {
                for (int y = 0; y < maxY - minY; y++)
                {
                    if (map[y, x] == (int)GridType.Cellular) map[y, x] = (int)GridType.TmpHallWay;
                }
            }
        }

        private void InstantiateGrid(int x, int y)
        {
            if (map[y - minY, x - minX] == -1)
            {
                if (isVisualizeProgress)
                {
                    GameObject grid = Instantiate(gridPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
                    grid.GetComponent<SpriteRenderer>().color = roomColor;
                    gridsList.Add(grid);
                }
                map[y - minY, x - minX] = (int)GridType.TmpHallWay;
            }
            else if (map[y - minY, x - minX] != (int)GridType.TmpHallWay)
            {
                if (rooms[map[y - minY, x - minX]].activeSelf) return;

                rooms[map[y - minY, x - minX]].SetActive(true);
                rooms[map[y - minY, x - minX]].GetComponent<SpriteRenderer>().color = roomColor;
            }
        }
        private void MapArrNormalization()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] < 0 || (map[i, j] != (int)GridType.TmpHallWay && !rooms[map[i, j]].activeSelf))
                    {
                        map[i, j] = (int)GridType.None;
                    }
                    else if (map[i, j] == (int)GridType.TmpHallWay) map[i, j] = (int)GridType.HallWay;
                    else map[i, j] = (int)GridType.MainRoom;
                }
            }
        }
        private void ClearObjects()
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                Destroy(rooms[i]);
            }
            rooms.Clear();

            foreach (GameObject obj in gridsList)
            {
                Destroy(obj);
            }
            gridsList.Clear();

            if (GetComponent<AutoTiling>() != null)
                GetComponent<AutoTiling>().Clear();
        }
        private void OnMapGenComplete()
        {
            if (GetComponent<AutoTiling>() != null)

			{
				int rowLength = map.GetLength(0);
				int colLength = map.GetLength(1);

				for (int i = 0; i < rowLength; i++)
				{
					string printRow = "";
					for (int j = 0; j < colLength; j++)
					{
						printRow += string.Format("{0} ", map[i, j]);
						if ((j+1) %4 == 0) printRow += '\t';
					}
					// Debug.Log(printRow);
				}
				GetComponent<AutoTiling>().SetMapInfos(ref map);
			}

            if (isVisualizeProgress)
            {
                foreach (var lineRenderer in lineRenderers)
                {
                    Destroy(lineRenderer);
                }
                lineRenderers.Clear();
            }
        }

        /** Getter Functions **/
        public GridType GetGridType(int x, int y)
        {
            if (y < 0 || x < 0 || (maxX - minX) <= x || (maxY - minY) <= y) return GridType.None;

            return (GridType)map[y, x];
        }


    }

}
