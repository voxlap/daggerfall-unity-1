using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingUI : MonoBehaviour {
    public GameObject whatToFollow;

	void Start () {
		
	}
	
	void Update () {
        if (!whatToFollow) return;

        transform.position = whatToFollow.transform.position;
        transform.rotation = Quaternion.identity;
        transform.Rotate(0, whatToFollow.transform.rotation.eulerAngles.y, 0);
	}
}
