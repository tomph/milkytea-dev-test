using UnityEngine;
using System.Collections.Generic;

//using System.Runtime.CompilerServices;

public class ObjectManager : MonoBehaviour
{
	private const int GLOBAL_SEED = 5;
	private static ObjectManager s_Instance = null;
	private Dictionary<int, PowerObject> objects;
	public GameObject tree = null;
	public GameObject robot = null;
	public GameObject sentinel = null;
	public GameObject rock = null;
	public GameObject player = null;
	private Terrain terrain;
	public int seed = 0;
	//private int oldSeed = 0;

	public enum PowerObjectType
	{
		Rock,
		Robot,
		Tree,
		Sentinel,
		Unknown
	}
  
	public static ObjectManager instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = FindObjectOfType(typeof(ObjectManager)) as ObjectManager;
			}
      
			// If it is still null, create a new instance
			if (s_Instance == null)
			{
				GameObject obj = new GameObject("ObjectManager");
				s_Instance = obj.AddComponent(typeof(ObjectManager)) as ObjectManager;
				Debug.Log("Could not locate an ObjectManager object. ObjectManager was Generated Automaticly.");
			}
      
			return s_Instance;
		}
	}

	// Ensure that the instance is destroyed when the game is stopped in the editor.
	void OnApplicationQuit()
	{
		s_Instance = null;
	}

	void Awake()
	{
		objects = new Dictionary<int, PowerObject>();

        GameObject terrainBase = new GameObject("TerrainBase");
        terrainBase.transform.Rotate(new Vector3(1,0,0), 90.0f);
		GameObject obj = new GameObject("Terrain");
        obj.transform.parent = terrainBase.transform;
		terrain = obj.AddComponent(typeof(Terrain)) as Terrain;

		if(!tree)
			tree = Resources.Load("Prefabs/TreePrefab", typeof(GameObject)) as GameObject;

		if(!robot)
			robot = Resources.Load("Prefabs/RobotPrefab", typeof(GameObject)) as GameObject;

		if(!sentinel)
			sentinel = Resources.Load("Prefabs/SentinelPrefab", typeof(GameObject)) as GameObject;

		if(!rock)
			rock = Resources.Load("Prefabs/RockPrefab", typeof(GameObject)) as GameObject;

		if(!player)
			player = Resources.Load("Prefabs/PlayerPrefab", typeof(GameObject)) as GameObject;

        Random.InitState(((GLOBAL_SEED << 12) ^ (seed << 8) ^ (seed << 4)));
    }
  

	public void SetSeed(int newSeed)
	{
		Unpopulate();

		//oldSeed = seed;
		seed = newSeed;

		terrain.Initialise();

		PopulateSentinels();
		PopulatePlayer(); //do after sentinels because players start facing the sentinel
		PopulateTrees();
		PopulateRocks();
	}

	//[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetSeed()
	{
		return seed;
	}
  
	public void PopulateLevel()
	{
		PopulateSentinels();
		PopulateTrees();
		PopulateRocks();
	}
  
	public void Unpopulate()
	{
		PowerObject[] itemsToRemove = new PowerObject[objects.Count];
		objects.Values.CopyTo(itemsToRemove, 0);

		foreach (PowerObject value in itemsToRemove)
			//value.Reset();
			value.KillObject();

		objects.Clear();
	}

	private void PopulateSentinels()
	{
		if (sentinel)
			GeneratePowerObject(typeof(PO_Sentinel), null);
	}

	private void PopulateTrees()
	{
		if (tree)
		{
			int trees = Random.Range(1, 20);

			for (int i = 0; i < trees; i++)
				GeneratePowerObject(typeof(PO_Tree), null);
		}
	}

	private void PopulateRocks()
	{
		if (rock)
		{
			int rocks = Random.Range(1, 8);
			
			for (int i = 0; i < rocks; i++)
				GeneratePowerObject(typeof(PO_Rock), null);
		}
	}

	public void PopulatePlayer()
	{
		if (robot && player)
		{
			PowerObject newRobot = GeneratePowerObject(typeof(PO_Robot), null);
			if(newRobot)
			{
				GameObject newObj = InstantiateObject(player, newRobot.transform.root.transform.position, Quaternion.identity);
				if (newObj)
				{
					PO_Player newPlayer = newObj.transform.root.GetComponent<PO_Player>();
					if(newPlayer)
						newPlayer.PosessBody(newRobot);
				}
			}
		}
	}

	private GameObject InstantiateObject(GameObject obj, Vector3 pos, Quaternion rot)
	{
		GameObject newObj = (GameObject)Instantiate(obj, pos, rot);

		return newObj;
	}

    public GameObject InstantiateObjectPrefab(System.Type type, Vector3 position)
    {
        GameObject gameObj = null;

        if (type == typeof(PO_Tree))
            gameObj = tree;
        else if (type == typeof(PO_Robot))
            gameObj = robot;
        else if (type == typeof(PO_Rock))
            gameObj = rock;
        else if (type == typeof(PO_Sentinel))
            gameObj = sentinel;

        if (gameObj == null)
            return null;

        return InstantiateObject(gameObj, position, Quaternion.identity);
    }

	public PowerObject GeneratePowerObject(System.Type type, Vector3? position)
	{
		
		GameObject newObj = null;
		
		if (position != null)
		{
			Vector3 desiredPos = new Vector3(position.Value.x, position.Value.y, position.Value.z);

			if (!CanPositionObject(ref desiredPos))
				return null;

            newObj = InstantiateObjectPrefab(type, desiredPos);
                //InstantiateObject(gameObj, desiredPos, Quaternion.identity);
			if (newObj)
				return newObj.transform.root.GetComponent<PowerObject>();
		} else
		{
			Vector3 farFarAway = new Vector3(99999.0f, 99999.0f, 99999.0f);
            //newObj = InstantiateObject(gameObj, farFarAway, Quaternion.identity);
            newObj = InstantiateObjectPrefab(type, farFarAway);
            if (newObj)
			{
				PowerObject powerObj = newObj.transform.root.GetComponent<PowerObject>();
				if (powerObj)
				{
					if (powerObj.SetPositionRandom())
						return powerObj;
					else
					{
						//failed to find an empty space
						RemoveObject(powerObj);
						Destroy(powerObj.gameObject);
					}
				}
			}
		}

		return null;
	}

    public bool CanPositionObject(ref Vector3 desiredPos)
    {
        if (!IsPosValid(ref desiredPos, false) ||
            !terrain.IsPosFlat(desiredPos))
            return false;

        return true;
    }

	public bool GetHighestValidBlock(bool empty, out Vector3 pos)
	{
		return GetRandomValidBlock(terrain.GetLevels() - 1, empty, true, true, out pos);
	}

	//int targetHeight -aim for a space at this height
	//bool empty -must the space be empty or is it alright to stack onto a block
	//bool lower -if no empty space is found should i look higher or lower
	//bool order -should i look layer to layer or randomise search
	//builds a list of all valid (flat and maybe empty) blocks on target height. 
	//if none are found it will either look up or down
	public bool GetRandomValidBlock(int targetHeight, bool empty, bool lower, bool order, out Vector3 pos)
	{
		int target = targetHeight;
		List<Vector3> posList;
		int count;
		int top = terrain.GetLevels();
		int bottom = -1;
		int loop;
		int targetLoops;

		if (target >= top)
			target = top - 1;
		else if (target <= bottom)
			target = bottom + 1;

		if (lower)
			targetLoops = target;
		else
			targetLoops = top - target;

		int[] targets = new int[targetLoops];
		for (loop = 0; loop < targetLoops; loop++)
		{
			if (lower)
				targets[loop] = target - loop;
			else
				targets[loop] = target + loop;
		}

		if (!order)
		{
			for (loop = 0; loop < targetLoops; loop++)
			{
				int r = Random.Range(0, loop);
				int tmp = targets [loop];
				targets [loop] = targets [r];
				targets [r] = tmp;
			}
		}

		for (loop = 0; loop < targetLoops; loop++)
		{
			posList = GetValidBlocks(targets[loop], empty);
			count = posList.Count;
			if (count > 0)
			{
				if (count == 1)
				{
					pos = GetPosInWorld((int)posList [0].x, (int)posList [0].z);
					return true;
				} else
				{
					int rand = Random.Range(0, count - 1);
					pos = GetPosInWorld((int)posList [rand].x, (int)posList [rand].z);
					return true;
				}
			}
		}

		pos = new Vector3();
		return false;
	}

	public List<Vector3> GetValidBlocks(int targetHeight, bool empty)
	{
		int blocks = terrain.GetBlocks();       
		int height;
		Vector3 bestPos = new Vector3(0, targetHeight, 0);
		List<Vector3> posList = new List<Vector3>();

		for (int x = 0; x < blocks; x++)
		{
			for (int z = 0; z < blocks; z++)
			{
				height = terrain.GetHeight(x, z);
				if (height == targetHeight && 
					terrain.IsBlockFlat(x, z) && IsBlockValid(x, z, empty))
				{
					bestPos.Set(x, height, z);
					posList.Add(bestPos);
				}
			}
		}

		return posList;
	}

	public PowerObject GetObjectAtPos(Vector3 pos)
	{
		PowerObject best = null;

		terrain.ClampPosToGrid(ref pos);

		foreach (PowerObject value in objects.Values)
		{
			if (pos.x == value.transform.root.transform.position.x &&
				pos.z == value.transform.root.transform.position.z &&
				(!best || value.transform.root.transform.position.y > best.transform.root.position.y))
				best = value;
		}

		return best;
	}

	public bool IsPosValid(ref Vector3 pos, bool empty)
	{
		terrain.ClampPosToGrid(ref pos);

		foreach (PowerObject value in objects.Values)
		{
			if (//value.gameObject.activeSelf && 
			    pos.x == value.transform.root.transform.position.x &&
				pos.z == value.transform.root.transform.position.z)
			{
				if (empty || (!empty && value.GetType() != typeof(PO_Rock)))
					return false;
				else if (value.GetType() == typeof(PO_Rock))
				{
					Transform child = value.transform.GetChild(0);
					if (child)
						pos.y += child.GetComponent<Renderer>().bounds.size.y;
				}
			}
		}
		
		return true;
	}

	public bool IsBlockValid(int x, int z, bool empty)
	{
		Vector3 worldPos = terrain.GetPosInWorld(x, z);
		
		return IsPosValid(ref worldPos, empty);
	}

	public Vector3 GetPosInWorld(int x, int z)
	{
		Vector3 pos = terrain.GetPosInWorld(x, z);

		foreach (PowerObject value in objects.Values)
		{
			if (//value.gameObject.activeSelf && 
			    pos.x == value.transform.root.transform.position.x &&
				pos.z == value.transform.root.transform.position.z)
			{
				if (value.GetType() == typeof(PO_Rock))
				{
					Transform child = value.transform.GetChild(0);
					if (child)
						pos.y += child.GetComponent<Renderer>().bounds.size.y;
				}
			}
		}

		return pos;
	}

	public void GetTerrainPos(ref Vector3 pos)
	{
		terrain.ClampPosToGrid(ref pos);
	}

	public void RemoveObject(PowerObject obj)
	{
		objects.Remove(obj.GetInstanceID());
	}

	public void AddObject(PowerObject obj)
	{
		if(objects.ContainsKey(obj.GetInstanceID()) == false)
			objects.Add(obj.GetInstanceID(), obj);
	}

	public PO_Player GetPlayer()
    {
		foreach(PowerObject po in objects.Values)
        {
			GameObject go = po.gameObject;
			PO_Player p = go.GetComponentInChildren<PO_Player>();
			if (p != null) return p;
        }

		return null;
    }

	public PO_Sentinel GetSenitel()
	{
		foreach (PowerObject po in objects.Values)
		{
			GameObject go = po.gameObject;
			PO_Sentinel s = go.GetComponentInChildren<PO_Sentinel>();
			if (s != null) return s;
		}

		return null;
	}
}

