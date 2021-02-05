using UnityEngine;
using System.Collections;

public class PO_Robot : PowerObject
{		
	new void Awake()
	{
		base.Awake();

		power = defaultPower;
	}

	public override bool SetPositionRandom()
	{
		Vector3 pos;
		if(ObjectManager.instance.GetRandomValidBlock(1, false, false, true, out pos))
		{
			transform.position = pos;
			return true;
		}

		return false;
	}
}