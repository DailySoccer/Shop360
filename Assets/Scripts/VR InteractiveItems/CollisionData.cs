using UnityEngine;

public class CollisionData {
	public PointOfInterestIntern item = null;
	public Vector3 point = Vector3.zero;
	public bool isPointOfInterest = false;

	public Vector3 colliderScreenCenter = Vector3.zero;
	public Vector3 collisionScreenPoint = Vector3.zero;

	public float collisionScreenDistance = 0;
	public float maxScreenDistance = 0;

	public float proportion = 0;
}
