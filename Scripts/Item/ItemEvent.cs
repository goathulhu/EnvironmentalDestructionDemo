using Godot;
using System;

public class ItemEvent
{
	public string Name = "";
	public float TriggerTime = 0f;
	public bool Triggered = false;
	
	public ItemEvent(string NewName, float NewTriggerTime)
	{
		Name = NewName;
		TriggerTime = NewTriggerTime;
	}
}
