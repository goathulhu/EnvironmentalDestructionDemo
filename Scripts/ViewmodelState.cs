using Godot;
using System;

public class ViewmodelState
{
	private string[] HandBones =
	{
		"hand",
		"palm.01",
		"palm.02",
		"palm.03",
		"palm.04",
		"f_index.01",
		"f_index.02",
		"f_index.03",
		"thumb.01",
		"thumb.02",
		"thumb.03",
		"f_middle.01",
		"f_middle.02",
		"f_middle.03",
		"f_ring.01",
		"f_ring.02",
		"f_ring.03",
		"f_pinky.01",
		"f_pinky.02",
		"f_pinky.03",
	};
	
	public (float Control, 
		(Vector3 Position, Vector3 Rotation) RigTransform, 
		(Vector3 Position, Vector3 Rotation) InnerTransform, 
		(Vector3 Position, Vector3 Rotation) OuterTransform, 
		(Vector3 Pole, Vector3 Target) Ik, 
		(Vector3 Position, Quaternion Rotation)[] Bones) Left;
	public (float Control, 
		(Vector3 Position, Vector3 Rotation) RigTransform, 
		(Vector3 Position, Vector3 Rotation) InnerTransform, 
		(Vector3 Position, Vector3 Rotation) OuterTransform, 
		(Vector3 Pole, Vector3 Target) Ik, 
		(Vector3 Position, Quaternion Rotation)[] Bones) Right;
	
	public ViewmodelState InterpolateWith(ViewmodelState OtherState)
	{
		ViewmodelState NewState = this;
		
		// bones
		for (int I = 0; I < HandBones.Length; I++)
		{
			NewState.Left.Bones[I].Position = NewState.Left.Bones[I].Position.Lerp(OtherState.Left.Bones[I].Position, OtherState.Left.Control);
			NewState.Left.Bones[I].Rotation = NewState.Left.Bones[I].Rotation.Slerpni(OtherState.Left.Bones[I].Rotation, OtherState.Left.Control);
			
			NewState.Right.Bones[I].Position = NewState.Right.Bones[I].Position.Lerp(OtherState.Right.Bones[I].Position, OtherState.Right.Control);
			NewState.Right.Bones[I].Rotation = NewState.Right.Bones[I].Rotation.Slerpni(OtherState.Right.Bones[I].Rotation, OtherState.Right.Control);
		}
			
		// ik
		NewState.Left.Ik.Pole = NewState.Left.Ik.Pole.Lerp(OtherState.Left.Ik.Pole, OtherState.Left.Control);
		NewState.Left.Ik.Target = NewState.Left.Ik.Target.Lerp(OtherState.Left.Ik.Target, OtherState.Left.Control);
		
		NewState.Right.Ik.Pole = NewState.Right.Ik.Pole.Lerp(OtherState.Right.Ik.Pole, OtherState.Right.Control);
		NewState.Right.Ik.Target = NewState.Right.Ik.Target.Lerp(OtherState.Right.Ik.Target, OtherState.Right.Control);
		
		// orientation
		NewState.Left.RigTransform.Position = NewState.Left.RigTransform.Position.Lerp(OtherState.Left.RigTransform.Position, OtherState.Left.Control);
		NewState.Left.RigTransform.Rotation = NewState.Left.RigTransform.Rotation.Lerp(OtherState.Left.RigTransform.Rotation, OtherState.Left.Control);
		NewState.Left.InnerTransform.Position = NewState.Left.InnerTransform.Position.Lerp(OtherState.Left.InnerTransform.Position, OtherState.Left.Control);
		NewState.Left.InnerTransform.Rotation = NewState.Left.InnerTransform.Rotation.Lerp(OtherState.Left.InnerTransform.Rotation, OtherState.Left.Control);
		NewState.Left.OuterTransform.Position = NewState.Left.OuterTransform.Position.Lerp(OtherState.Left.OuterTransform.Position, OtherState.Left.Control);
		NewState.Left.OuterTransform.Rotation = NewState.Left.OuterTransform.Rotation.Lerp(OtherState.Left.OuterTransform.Rotation, OtherState.Left.Control);
		
		NewState.Right.RigTransform.Position = NewState.Right.RigTransform.Position.Lerp(OtherState.Right.RigTransform.Position, OtherState.Right.Control);
		NewState.Right.RigTransform.Rotation = NewState.Right.RigTransform.Rotation.Lerp(OtherState.Right.RigTransform.Rotation, OtherState.Right.Control);
		NewState.Right.InnerTransform.Position = NewState.Right.InnerTransform.Position.Lerp(OtherState.Right.InnerTransform.Position, OtherState.Right.Control);
		NewState.Right.InnerTransform.Rotation = NewState.Right.InnerTransform.Rotation.Lerp(OtherState.Right.InnerTransform.Rotation, OtherState.Right.Control);
		NewState.Right.OuterTransform.Position = NewState.Right.OuterTransform.Position.Lerp(OtherState.Right.OuterTransform.Position, OtherState.Right.Control);
		NewState.Right.OuterTransform.Rotation = NewState.Right.OuterTransform.Rotation.Lerp(OtherState.Right.OuterTransform.Rotation, OtherState.Right.Control);
		
		return NewState;
	}
}
