using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour
{
	public Vector3 rotationVector = Vector3.up;
	public float speed = 10.0f;
	
	// Update is called once per frame
	void Update () {
			transform.Rotate(speed * Time.deltaTime * rotationVector);
	}
}
