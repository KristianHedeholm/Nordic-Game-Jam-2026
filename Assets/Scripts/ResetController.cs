using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetController : MonoBehaviour
{
	public static event Action OnReset;
	private static ResetController _instance;
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}
		_instance = this;
		
		DontDestroyOnLoad(gameObject);
	}
	void Update()
	{
		var currentKeyboard = Keyboard.current;
		// check if we are pressing either the left or right control keys.
		// if not we simply return.
		if (!currentKeyboard.leftCtrlKey.isPressed && !currentKeyboard.rightCtrlKey.isPressed)
		{
			return;
		}

		// check if we are pressing the r key.
		// if not we simply return.
		if (!currentKeyboard.rKey.isPressed)
		{
			return;
		}
	    
		// at this point the player is either pressing the left or right control key
		// and the r key, in this case we should send the Reset signal.
		OnReset?.Invoke();
	}
}