using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InactivityResetController : MonoBehaviour
{
	public static event Action OnResetInactivity;
	private static InactivityResetController Instance;
	
	[SerializeField]
	private int _secondsBeforeReset;
	
	private int _currentSeconds;
	
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		
		DontDestroyOnLoad(gameObject);
		
		_currentSeconds = 0;
		InvokeRepeating(nameof(CheckForInactivity), 1.0f, 1.0f);
	}
	
	private void CheckForInactivity()
	{
		var deltaValue = Mouse.current.delta.value;
		if (deltaValue != Vector2.zero)
		{
			_currentSeconds = 0;
			return;
		}

		if (Keyboard.current.anyKey.isPressed)
		{
			_currentSeconds = 0;
			return;
		}
		
		// at this point, we know there has been no movement of the mouse.
		// or any key on the keyboard has been pressed.
		_currentSeconds++;
        UnityEngine.Debug.Log(_currentSeconds);
		
		if (_currentSeconds >= _secondsBeforeReset)
		{
			_currentSeconds = 0;
			OnResetInactivity?.Invoke();
			return;
		}
	}
}
