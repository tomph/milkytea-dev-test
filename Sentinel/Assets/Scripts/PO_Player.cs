using System;
using UnityEngine;
using UnityEngine.UI;


public class PO_Player : PowerObject
{
    [SerializeField]
    private LayerMask _absorbableTargetMask;

	public float grabDistance = 5000.0f;

	private PowerObject m_currentBody = null;

    private bool m_canAbsorb = true;
    private Transform m_transform = null;
    private Transform m_cameraTransform = null;
    private PowerObject m_previewObject = null;
    private RaycastHit m_hit;

    private CursorState _aimState = CursorState.Idle;
    private PadLook hPadLook = null;
    private MouseLook m_MouseLook = null;

    private Transform _cameraParentOrigin;

    private Material _previewMaterial;

    public bool sentinelAbsorbed = true;

    protected override void Awake()
    {
        m_transform = transform;

        m_MouseLook = GetComponent<MouseLook>();
        hPadLook = GetComponent<PadLook>();

		ObjectManager.instance.AddObject(this);

        //used to highlight previews vs real objects
        _previewMaterial = Resources.Load<Material>("Materials/previewmat");

        power = defaultPower;

        //attach camera
        //use the camera placed in the world rather than put cameras into prefabs for now
        m_cameraTransform = Camera.main.transform;
        _cameraParentOrigin = m_cameraTransform.parent;
        m_cameraTransform.parent = m_transform.GetChild(0).transform;
        m_cameraTransform.localPosition = Vector3.zero;
        m_cameraTransform.localRotation = Quaternion.identity;

        ON_ABSORBED.AddListener(OnAbsorbed);

        base.Awake();

	}

    private void OnAbsorbed(int arg0)
    {
        //reset camera
        m_cameraTransform.SetParent(_cameraParentOrigin, true);
    }

    protected override void Update()
    {
        UpdateInput();
        base.Update();
    }

    void UpdateInput()
    {
        if (Input.GetButton("Rock"))
        {
            if (Input.GetButtonDown("Fire1"))
                GenerateObject(typeof(PO_Rock));
                
            else
                PreviewGenerateObject(typeof(PO_Rock));
        }
        else if (Input.GetButton("Tree"))
        {
            if (Input.GetButtonDown("Fire1"))
                GenerateObject(typeof(PO_Tree));
            else
                PreviewGenerateObject(typeof(PO_Tree));
        }
        else if (Input.GetButton("Robot"))
        {
            if (Input.GetButtonDown("Fire1"))
                GenerateObject(typeof(PO_Robot));
            else
                PreviewGenerateObject(typeof(PO_Robot));
        }

        if (Input.GetButtonDown("Fire2"))
            FireAbsorbRay();

        if (Input.GetButtonDown("Fire3"))
            FirePossessRay();

        if (Input.GetButton("Cursor"))
        {
            aimState = FireAimRay();

            hPadLook.sensitivity.x = 1.0F;
            hPadLook.sensitivity.y = 1.0F;

            m_MouseLook.sensitivityX = 7.5F;
            m_MouseLook.sensitivityY = 7.5F;
        }
        else
        {
            aimState = CursorState.Idle;

            hPadLook.sensitivity.x = 2.0F;
            hPadLook.sensitivity.y = 2.0F;

            m_MouseLook.sensitivityX = 15.0F;
            m_MouseLook.sensitivityY = 15.0F;
        }         
    }

    private CursorState FireAimRay()
    {
        RaycastHit hit;
        Ray grabRay = new Ray(m_cameraTransform.position, m_cameraTransform.forward);

        if(Physics.Raycast(grabRay, out hit, grabDistance, _absorbableTargetMask))
        {
            PowerObject obj = hit.transform.root.GetComponentInChildren<PowerObject>();
            if(obj != null)
            {
                return obj.CanAbsorb(this) || obj.transform.root.GetComponentInChildren<PO_Rock>() != null ? CursorState.Hitting : CursorState.OutOfRange;
            }
            else
            {
                return CursorState.OutOfRange;
            }
        }

        return CursorState.Aiming;
    }

    //All items at places, whose base area is visible can be absorbed
    //aim at the base to absorb the object
    //Rocks can be absorbed without seeing the corresponding base area
    private void FireAbsorbRay()
	{
        if (!m_canAbsorb) return;

		RaycastHit hit;
		Ray grabRay = new Ray(m_cameraTransform.position, m_cameraTransform.forward);

		Debug.DrawRay(m_cameraTransform.position, m_cameraTransform.forward * grabDistance);

		if (!Physics.Raycast(grabRay, out hit, grabDistance))
			return;

		PowerObject obj = null;
		switch (hit.collider.transform.root.tag)
		{
			case "Tree":
			case "Rock":
			case "Robot":
			case "Sentinel":
				obj = hit.collider.transform.root.gameObject.GetComponent<PowerObject>();
				break;

			case "Environment":
				obj = ObjectManager.instance.GetObjectAtPos(hit.point);
				break;

		}

        if (obj != null && (obj.GetType() == typeof(PO_Rock) || obj.CanAbsorb(this)))
		{
			power += obj.GetPower();
			obj.Absorb();
        }
	}

	private bool FireRay(out RaycastHit hit)
	{	
		Debug.DrawRay(m_cameraTransform.position, m_cameraTransform.forward * grabDistance);
		
		return Physics.Raycast(m_cameraTransform.position, m_cameraTransform.forward, out hit, Mathf.Infinity);	
	}

    private void PreviewGenerateObject(System.Type type)
    {
        if (!FireRay(out m_hit))
            return;

        string colliderTag = m_hit.collider.tag;
        if (colliderTag != "Environment" && colliderTag != "Rock")
            return;

        Vector3 newPos = m_hit.point;
        if (!ObjectManager.instance.CanPositionObject(ref newPos))
        {
            if (m_previewObject != null && m_previewObject.gameObject.activeSelf)
                m_previewObject.gameObject.SetActive(false);

            return;
        }

        bool generateObject = false;
        if (m_previewObject == null)
            generateObject = true;
        else if (m_previewObject.GetType() != type)
        {
            Destroy(m_previewObject);
            generateObject = true;
        }

        if (generateObject)
        {
            m_previewObject = ObjectManager.instance.InstantiateObjectPrefab(type, newPos).GetComponent<PowerObject>();
            ObjectManager.instance.RemoveObject(m_previewObject);
            foreach (Collider col in m_previewObject.GetComponentsInChildren<Collider>())
                col.enabled = false;
            
            Renderer[] renderers = m_previewObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.material = _previewMaterial;
        }
        else
        {
            m_previewObject.transform.position = newPos;

            if(!m_previewObject.gameObject.activeSelf)
                m_previewObject.gameObject.SetActive(true);
        }

        //Debug.DrawLine(newPos, newPos + Vector3.up, Color.red, 5.0f, false);
    }

	private void GenerateObject(System.Type type)
	{
		RaycastHit hit;
		if (!FireRay(out hit)) 
			return;

		if (hit.collider.tag != "Environment" && hit.collider.tag != "Rock")
			return;

        PowerObject obj = ObjectManager.instance.GeneratePowerObject(type, hit.point + Vector3.up*10);
		if (!obj)
			return;

		int powerRequired = obj.GetPower();
		if (powerRequired > power)
		{
			ObjectManager.instance.RemoveObject(obj);
			Destroy(obj.gameObject);
			return;
		}

		power -= powerRequired;
	}

	private void FirePossessRay()
	{
		RaycastHit hit;
		Ray grabRay = new Ray(m_cameraTransform.position, m_cameraTransform.forward);
		PowerObject obj;
		
		Debug.DrawRay(m_cameraTransform.position, m_cameraTransform.forward * grabDistance);
		
		if (!Physics.Raycast(grabRay, out hit, grabDistance))
			return;

		if (hit.collider.transform.root.tag == "Robot")
			obj = hit.collider.transform.root.gameObject.GetComponent<PowerObject>();
		else if (hit.collider.transform.root.tag == "Environment" ||
			hit.collider.transform.root.tag == "Rock")
			obj = ObjectManager.instance.GetObjectAtPos(hit.point);
		else
			return;

		if (!obj || obj.GetType() != typeof(PO_Robot))
			return;

		PosessBody(obj);
	}

	public void PosessBody(PowerObject obj)
	{
		DebugUtils.Assert(obj != null);
		if (obj == null)
			return;

		if (obj.GetType() != typeof(PO_Robot))
			return;

        if (m_currentBody)
        {
            foreach (Renderer ren in m_currentBody.GetComponentsInChildren<Renderer>(true))
                ren.enabled = true;

            m_currentBody.transform.parent = null;
        }

        m_transform.root.position = obj.transform.position;
        obj.transform.parent = m_transform;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localPosition = Vector3.zero;

        if (m_currentBody)
        {
            //look at the body you came from
            hPadLook = GetComponent<PadLook>();
            hPadLook.LookAt(m_currentBody.transform.position);

            m_MouseLook = GetComponent<MouseLook>();
            
        }

        m_currentBody = obj;

        foreach (Renderer ren in m_currentBody.GetComponentsInChildren<Renderer>(true))
            ren.enabled = false;
    }

    public override bool CanAbsorb(PowerObject absorber)
    {
        return sentinelAbsorbed ? false :  base.CanAbsorb(absorber);
    }

    public CursorState aimState
    {
        get
        {
            return _aimState;
        }

        private set
        {
            _aimState = value;
        }
    }
}
