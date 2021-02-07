using UnityEngine;
using UnityEngine.Networking;

[RequireComponent (typeof(NetworkIdentity))]
public class Terrain : MonoBehaviour
{
	public int targetResolution = 30;
	private const float GLOBAL_SCALE = 1.0f;
	private int blocks = 1;       // number of blocks accross
	public const float SIZE = GLOBAL_SCALE * 50f; // size in real world units in the x&z direction
	public const float Y_SCALE = 1f;
	private const int GLOBAL_SEED = 5;
	private Vector3 blockSize;
	MeshFilter mesh_filter;
	MeshRenderer mesh_renderer;
	MeshCollider mesh_collider;
	int[,] heights;
	public string levelString = "0";
	private int levels = 0;
	public int maxLevels = 5;
	public float maxHilly = 8.0f;


	void Awake() {
		mesh_filter = gameObject.AddComponent<MeshFilter>();
		mesh_renderer = gameObject.AddComponent<MeshRenderer>();
		mesh_collider = gameObject.AddComponent<MeshCollider>();

		this.tag = "Environment";
		
		// put in correct position
		gameObject.transform.Translate(SIZE * -0.5f, 0, SIZE * -0.5f);
	}

//		void OnGUI ()
//		{
//				levelString = GUI.TextField (new Rect (10, 10, 200, 20), levelString, 25);
//
//				if (GUI.changed) {
//						int temp;
//						if (int.TryParse (levelString, out temp)) {
//								seed = temp;
//						} else if (levelString == "")
//								seed = 0;
//				}
//		}

	public void Initialise()
	{	
		blocks = targetResolution;
		
		// initialize properties
		heights = new int[blocks + 1, blocks + 1];

		blockSize.x = SIZE / blocks;
		blockSize.y = SIZE / blocks;//Y_SCALE * blockSize.x;
		blockSize.z = SIZE / blocks;	
		Randomise();
		// create game object
		SetGameObject();
	}

	private void Randomise()
	{
		if (maxHilly < 1.0f)
			maxHilly = 1.0f;
		float hills = Random.Range(1.0f, maxHilly);

		levels = Random.Range(5, maxLevels);

		for (int x = 0; x < blocks; x++)
		{
			for (int z = 0; z < blocks; z++)
			{
//				//https://code.google.com/p/fractalterraingeneration/wiki/Fractional_Brownian_Motion
//				//float total = 0;
//				float lacunarity = 2.0f;
//				float gain = 0.65f;
//				int octaves = 50;
//				float amplitude = gain;
//				float frequency = 1.0f / blocks;
//				heights [x, z] = (int)(levels * Mathf.PerlinNoise(((float)x / (float)blocks) * hills, ((float)z / (float)blocks) * hills));
//
//				for (int i = 0; i < octaves; ++i)
//				{
//					heights [x, z] += (int)(levels * (Mathf.PerlinNoise( ((float)x / (float)blocks) * frequency, ((float)z / (float)blocks) * frequency ) * amplitude));
//					//total += noise((float)x * frequency, (float)y * frequency) * amplitude;         
//					frequency *= lacunarity;
//					amplitude *= gain;
//				}

				 //(int)(levels * total);
				heights [x, z] = (int)(levels * Mathf.PerlinNoise(((float)x / (float)blocks) * hills, ((float)z / (float)blocks) * hills));
			}
		}
	
	}

	public void SetGameObject()
	{
		// remove any existing game object
		Mesh mesh = mesh_filter.mesh;
		GenerateMesh(ref mesh);
		// add texture coordinates:
		Vector2[] uv = new Vector2[(blocks * blockScale + 3) * (blocks * blockScale + 3)];
		for (int x = 0; x < blocks * blockScale + 3; x++)
		{
			int x_idx = x * (blocks * blockScale + 3);
			for (int z = 0; z < blocks * blockScale + 3; z++)
			{
//				float fU = x / (float)blockScale;
//
//				if(fU > 1.0f)
//				{
//					int sub = 1 - fU;
//				}
				uv [x_idx + z] = new Vector2(x / (float)blockScale, (z / (float)blockScale));
				
			}
		}		
		mesh.uv = uv;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		//Destroy(go.GetComponent<MeshCollider);
		//mesh_collider.sharedMesh = mesh;
		// shade!
		//MeshRenderer renderer = gameObject.GetComponent<MeshRenderer> ();
		mesh_collider.sharedMesh = null; //if you dont null it first it still uses the old one!
		mesh_collider.sharedMesh = mesh_filter.mesh;
		//Shader shader = Resources.Load ("TronShader") as Shader;
		//mesh_renderer.material = new Material (shader);
		//mesh_renderer.material.color = new Color(0.5f,0.5f,1.0f);
		mesh_renderer.material.mainTexture = Resources.Load("trontext2") as Texture2D;
		
	}

	/* We create a mesh that is one column greater on all sides
	 * we then fold that column over to create an 'edge'
	 */
	private void GenerateMesh(ref Mesh mesh)
	{
		// create vertices
		Vector3[] vertices = GenerateVertices();
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = GenerateTriangles(ref vertices);
	}

	private int blockScale = 1;
	private Vector3[] GenerateVertices()
	{
		Vector3[] vertices = new Vector3[(blocks * blockScale + 3) * (blocks * blockScale + 3)];
		for (int x = 0; x < blocks * blockScale + 3; x++)
		{
			int x_idx = x * (blocks * blockScale + 3);
			for (int z = 0; z < blocks * blockScale + 3; z++)
			{
				int _x = x - 1;
				int _z = z - 1;
				if (_x < 0)
					_x = 0; 
				if (_x >= blocks * blockScale + 1)
					_x = blocks * blockScale;
				if (_z < 0)
					_z = 0; 
				if (_z >= blocks * blockScale + 1)
					_z = blocks * blockScale;
				if (_x != x - 1 || _z != z - 1)
				{
					//this is the lip around the edge
					//vertices[x_idx + z] = new Vector3(_x*BLOCK_SIZE_X, (heights[_x,_z]-1)*BLOCK_SIZE_Y, _z*BLOCK_SIZE_Z + _x*0.577350269189626f*BLOCK_SIZE_X); //skew
					vertices [x_idx + z] = new Vector3(_x * (blockSize.x / blockScale), (0/*heights [_x, _z] - 1*/) * (blockSize.y / blockScale), _z * (blockSize.z / blockScale));
				} else
				{ 
					//vertices[x_idx + z] = new Vector3(_x*BLOCK_SIZE_X, heights[_x,_z]*BLOCK_SIZE_Y, _z*BLOCK_SIZE_Z + _x*0.577350269189626f*BLOCK_SIZE_X);
					vertices [x_idx + z] = new Vector3(_x * (blockSize.x / blockScale), 
					                                   heights [_x / blockScale, _z / blockScale] * blockSize.y,// / blockScale), 
					                                   _z * (blockSize.z / blockScale));
				}
			}
		}		
		return vertices;
	}
	
	private int[] GenerateTriangles(ref Vector3[] vertices)
	{
		// there are blocks*blocks squares
		// each square has 2 triangles
		// each triangle has 3 corners...
		int[] triangles = new int[(blocks * blockScale + 2) * (blocks * blockScale + 2) * 2 * 3];
		// loop over each square
		for (int x = 0; x < blocks * blockScale + 2; x++)
		{
			int x_idx = x * (blocks * blockScale + 3);
			for (int z = 0; z < blocks * blockScale + 2; z++)
			{
				// calculate the indexes into the vertex 
				// array for the 4 corners of this square
				int v_00 = x_idx + z;
				int v_01 = x_idx + z + 1;
				int v_10 = x_idx + (blocks * blockScale + 3) + z;
				int v_11 = x_idx + (blocks * blockScale + 3) + z + 1;
				// starting index into the triangles array
				int t_idx = ((x * (blocks * blockScale + 2)) + z) * 6;
				// alternate between triangle styles
				//if(z % 2 == 0){
				// lower left triangle
				triangles [t_idx + 0] = v_00;
				triangles [t_idx + 1] = v_01;
				triangles [t_idx + 2] = v_10;
				// upper right triangle
				triangles [t_idx + 3] = v_10;
				triangles [t_idx + 4] = v_01;
				triangles [t_idx + 5] = v_11;
			}
		}	
		return triangles;
	}

	//returns world space from row/col space
	public Vector3 GetPosInWorld(int x, int z)
	{
		if (x > blocks || z > blocks)
		{
			return new Vector3(0,0,0);
		}

		//position in middle of block!
		return new Vector3( ((x * blockSize.x) + (blockSize.x * 0.5f)) - (SIZE * 0.5f), 
		        heights [x, z] * blockSize.y, 
		        ((z * blockSize.z) + (blockSize.z * 0.5f)) - (SIZE * 0.5f));

	}

	public void ClampPosToGrid(ref Vector3 pos) 
	{
		pos = GetPosInWorld((int)(((pos.x + (SIZE * 0.5f)) / SIZE) * blocks), 
		                    (int)(((pos.z + (SIZE * 0.5f)) / SIZE) * blocks));
	}

	public bool IsPosFlat(Vector3 pos)
	{
		return IsBlockFlat((int)(((pos.x + (SIZE * 0.5f)) / SIZE) * blocks), 
		                   (int)(((pos.z + (SIZE * 0.5f)) / SIZE) * blocks));
	}

	public bool IsBlockFlat(int x, int z)
	{
		int height = heights [x, z];
		
		if (x < blocks && z < blocks &&
			heights [x + 1, z] == height &&
			heights [x, z + 1] == height &&
			heights [x + 1, z + 1] == height)
			return true;
		
		return false;
	}

	public int GetBlocks()
	{
		return blocks;
	}

	public int GetHeight(int x, int z)
	{
		return heights[x, z];
	}

	public int GetLevels()
	{
		return levels;
	}
}
