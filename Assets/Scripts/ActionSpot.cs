using UnityEngine;
using UnityEngine.Events;

public class ActionSpot : MonoBehaviour {

	public bool multifire = false;

	public UnityEvent onFireAction = new UnityEvent();
	public UnityEvent onLookAction = new UnityEvent();
	public UnityEvent onLeaveAction = new UnityEvent();

	public void fireAction() {
		Debug.LogFormat("{0} fired Action", name);
		onFireAction.Invoke();
	}
	public void Leave() {
		onLeaveAction.Invoke();
	}
	public void Look() {
		onLookAction.Invoke();
	}
}
