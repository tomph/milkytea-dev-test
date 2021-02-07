using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    [System.Serializable]
    public class AudioSnapshotSettings
    {
        public float transitionTime = 0.1f;
        public AudioMixerSnapshot snapshot = null;
    }

    public AudioSnapshotSettings menus;
    public AudioSnapshotSettings launchtitle;

    // Use this for initialization
    void Start () {
        launchtitle.snapshot.TransitionTo(launchtitle.transitionTime);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
