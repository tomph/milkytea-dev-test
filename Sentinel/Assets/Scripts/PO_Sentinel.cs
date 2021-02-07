using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public class PO_Sentinel : PowerObject
{
	public Transform neck;
	public Transform head;
	public float headTurnRate = 10.0f;
    private Camera m_camera = null;

    public Camera Camera
    {
        get
        {
            if(m_camera == null)
                m_camera = GetComponentInChildren<Camera>(true);

            return m_camera;
        }
    }

    private float timer = 0.0F;

    private PowerObject absorbTarget = null;

	// Use this for initialization
	new void Awake()
	{
		base.Awake();

		power = defaultPower;
	}

    public bool TestView(PowerObject powerObject)
    {
		//height check
        if (!powerObject.CanAbsorb(this))
            return false;

		//reassignment lock
        if (absorbTarget == powerObject || absorbTarget == null)
        {
            absorbTarget = powerObject;
            if (timer > 1.0F)
            {
                if (absorbTarget.GetPower() > 0)
                {
                    powerObject.SetPower(powerObject.GetPower() - 1);
                    power++;
                    PlantTree();
                }
                else
                {
                    powerObject.Absorb();
                    absorbTarget = null;
                }
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        return true;
    }

	public override bool SetPositionRandom()
	{
		Vector3 pos;
		if(ObjectManager.instance.GetHighestValidBlock(false, out pos))
		{
			transform.position = pos;
			return true;
		}

		return false;
	}
	
	// Update is called once per frame
	new void Update()
	{
		Vector3 forward = neck.forward;
		Vector3 right = neck.right;
		float turnAngle = headTurnRate * Time.deltaTime;
		Vector3 lookPos;

		if (forward.y != 0.0f)
		{
			forward.y = 0.0f;
			forward.Normalize();
		}

		lookPos.y = 0;
		
		RotateHead(turnAngle);

		if (head != null)
			Debug.DrawRay(head.position, head.forward * 5000.0f);

		base.Update(); //;()
	}

	void RotateHead(float angle)
	{
		if (neck == null || absorbTarget != null)
			return;

		neck.Rotate(Vector3.up, angle, Space.World);
	}

	void PlantTrees()
	{
		while(power > defaultPower)
		{
            PlantTree();
		}
	}

    void PlantTree()
    {
        ObjectManager.instance.GeneratePowerObject(typeof(PO_Tree), null);
        power--;
    }

    public override bool CanAbsorb(PowerObject absorber)
    {
		if (absorber.tag != "Player")
			return false;
		else
            return base.CanAbsorb(absorber);
    }
}
