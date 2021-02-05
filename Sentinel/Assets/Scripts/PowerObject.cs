using UnityEngine;
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

		Texture2D tex_norm = Resources.Load("trontext") as Texture2D;
		Texture2D tex_static = Resources.Load("static") as Texture2D;
		Material newMat = Resources.Load("SliceMaterial", typeof(Material)) as Material;
		newMat.SetTexture("Base (RGB)", tex_norm);
		newMat.SetTexture("Slice Guide (RGB)", tex_static);

		foreach (Transform child in transform)
		{
			if(child.GetComponent<Renderer>())
				child.GetComponent<Renderer>().material = newMat;
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

	// Update is called once per frame
	protected virtual void Update()
	{
		if (fadeOut)
			UpdateFadeOut();

        UpdateAbsorb();    
    }

    private void UpdateAbsorb()
    {
        if (Sentinel == null)
            return;

        renderers = GetComponentsInChildren<Renderer>(true); //TODO: optimise
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

	public int GetPower()
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
		ObjectManager.instance.RemoveObject(this);

        Destroy(this.gameObject);
	}

    public void SetPower(int newPower)
    {
        power = newPower;
    }

    //public virtual void Reset()
    //{
    //	Destroy(this.gameObject);
    //}
}
