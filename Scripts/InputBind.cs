using Godot;
using System;

public class InputBind
{
	public string Action;
	public (Key KeyBind, MouseButton ButtonBind)[] Keys;
	public float DoubleTapTimer = 0f;
	public bool Down = false;
	public bool Hold = false;
	public bool Up = false;
	public bool DoubleTap = false;
	public bool IsInput = false;

	public InputBind(string NewAction, Key NewBind)
	{
		Action = NewAction;
		Keys = new (Key, MouseButton)[] { (NewBind, MouseButton.None), };
	}

	public InputBind(string NewAction, MouseButton NewBind)
	{
		Action = NewAction;
		Keys = new (Key, MouseButton)[] { (Key.None, NewBind), };
	}

	public InputBind AddBind(Key NewBind)
	{
		Array.Resize(ref Keys, Keys.Length + 1);
		Keys[Keys.Length - 1] = (NewBind, MouseButton.None);
		return this;
	}

	public InputBind AddBind(MouseButton NewBind)
	{
		Array.Resize(ref Keys, Keys.Length + 1);
		Keys[Keys.Length - 1] = (Key.None, NewBind);
		return this;
	}
}
