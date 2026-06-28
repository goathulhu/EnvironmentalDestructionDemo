using Godot;
using System;

public class ItemAction
{
	public string Name = "";
	public float Duration = 0f;
	public ViewmodelOverride Override = null;
	public string Animation = "";
	public ItemEvent[] Events = new ItemEvent[0];
	
	// full
	public ItemAction(string NewName, float NewDuration, ViewmodelOverride NewOverride, string NewAnimation, ItemEvent[] NewEvents)
	{
		Name = NewName;
		Duration = NewDuration;
		Override = NewOverride;
		Animation = NewAnimation;
		Events = NewEvents;
	}
	
	// event only
	public ItemAction(string NewName, float NewDuration, ItemEvent[] NewEvents)
	{
		Name = NewName;
		Duration = NewDuration;
		Events = NewEvents;
	}
	
	// animation only
	public ItemAction(string NewName, float NewDuration, ViewmodelOverride NewOverride, string NewAnimation)
	{
		Name = NewName;
		Duration = NewDuration;
		Override = NewOverride;
		Animation = NewAnimation;
	}
}
