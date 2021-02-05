using UnityEngine;
using System.Collections;

public class PO_Rock : PowerObject
{		
	// Use this for initialization
	new void Awake()
	{
		base.Awake();

		power = defaultPower;
	}
}