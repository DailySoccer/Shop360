﻿using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class RoomMenu : MonoBehaviour
{
	

	#region Public Fields


	#endregion

	//=========================================================================

	#region Mono	

	private void Awake()
	{
		_canvas = GetComponent<Canvas>();
	}

	private void OnDestroy()
	{
		_canvas = null;
	}

	#endregion

	//=================================================================

	#region Events

	public void OnRoomClick(RoomButton button)
	{
		_canvas.enabled = false;

		var game = MetaManager.Instance.Get<GameManager>();
		game.Room = button.TargetRoomId;

		if (button.IsBlocked)
			SceneLoader.Instance.GoToQRUnlock("SelectionMenu");
		else if (game.IsReadyToPlay)
			game.Play();
		else
			Camera.main.GetComponent<Animator>().SetTrigger("ShowBoomons");		  
	}




	#endregion

	//===============================================================

	#region Private Fields

	private Canvas _canvas;
	#endregion
}

