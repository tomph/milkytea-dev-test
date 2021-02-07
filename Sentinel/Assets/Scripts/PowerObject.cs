using Sentinel.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class PowerObject : MonoBehaviour
{
	protected int power;
	public int defaultPower;

	protected bool fadeOut = false;
	private float fadeOutElapsed = 0.0f;
	public float fadeOutTime = 1.0f;
    private Renderer[] renderers;
    private PO_Sentinel m_sentinel = null;

	private Texture2D _baseTexture;
	private Material _sliceMaterial;

	public IntEvent ON_ABSORBED = new IntEvent();
	public Vector3Event ON_GRID_POSITION_CHANGE = new Vector3Event();

	private Vector3 _lastGridPosition = Vector3.zero;

    private PO_Sentinel Sentinel
    {
        get
        {
            if (m_sentinel == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag("Sentinel");
                if (go != null)
                    m_sentinel = go.GetComponentInChildren<PO_Sentinel>();
            }

            return m_sentinel;
        }
    }

	protected virtual void Awake()
	{
		ObjectManager.instance.AddObject(this);

		renderers = GetComponentsInChildren<Renderer>(true);

		_baseTexture = Resources.Load("trontext") as Texture2D;
		Texture2D tex_static = Resources.Load("static") as Texture2D;
		_sliceMaterial = Resources.Load("SliceMaterial", typeof(Material)) as Material;
		_sliceMaterial.SetTexture("Base (RGB)", _baseTexture);
		_sliceMaterial.SetTexture("Slice Guide (RGB)", tex_static);

		foreach (Transform child in transform)
		{
			if(child.GetComponent<Renderer>())
				child.GetComponent<Renderer>().material = _sliceMaterial;
		}
	}

	public virtual bool SetPositionRandom()
	{
        Vector3 pos;
		if (ObjectManager.instance.GetRandomValidBlock(0, false, false, false, out pos))
		{
			transform.position = pos;
			return true;
		}

		return false;
	}

	public Vector3 gridPosition
    {
        get
        {
			return new Vector3(transform.position.x, 0, transform.position.z);
		}
    }

	// Update is called once per frame
	protected virtual void Update()
	{
		if (fadeOut)
			UpdateFadeOut();

        UpdateAbsorb();    

		if(_lastGridPosition != gridPosition)
        {
			ON_GRID_POSITION_CHANGE.Invoke(gridPosition);
			_lastGridPosition = gridPosition;
        }
    }

    private void UpdateAbsorb()
    {
        if (Sentinel == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].IsVisibleFrom(Sentinel.Camera) && Sentinel.TestView(this))
                return;
        }
    }

    protected void UpdateFadeOut()
	{
		fadeOutElapsed += Time.deltaTime;
		float fade = fadeOutElapsed / fadeOutTime;

		if (fade >= 1.0f)
			KillObject();
		else
		{
			foreach (Transform child in transform)
			{
				if (child.GetComponent<Renderer>() && child.GetComponent<Renderer>().material)
					child.GetComponent<Renderer>().material.SetFloat("_SliceAmount", fade);
			}
		}
	}

	public virtual int GetPower()
	{
		return power;
	}

	public int GetDefaultPower()
	{
		return defaultPower;
	}

	public virtual bool CanAbsorb(PowerObject absorber)
	{
		Vector3 blockPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		ObjectManager.instance.GetTerrainPos(ref blockPos);

		if (fadeOut || blockPos.y > absorber.transform.position.y)
			return false;

		return true;
	}

	public void Absorb()
	{
		if (fadeOut)
			return;

		fadeOut = true;
		fadeOutElapsed = 0.0f;
		power = 0;
	}

	public void KillObject()
	{
		ON_ABSORBED.Invoke(gameObject.GetInstanceID());

		ObjectManager.instance.RemoveObject(this);

        Destroy(this.gameObject);
	}

    public void SetPower(int newPower)
    {
        power = newPower;
    }	

}
