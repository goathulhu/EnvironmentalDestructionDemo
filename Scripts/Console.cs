using Godot;
using System;

public partial class Console : Node
{
	private float DeltaTime;

	private int CurrentLine;
	
	[Export] private Control ConsoleControl;
	[Export] private RichTextLabel TextLabel;
	[Export] private TextEdit TextField;

	private GameManager GameManager;
	private Player Player;

	private string Content = "";

	private string[] History = {"end of history uwu"};
	private int LastHistoryPos;
	private int HistoryPos;
	private string HistoryTempSave = "";
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
		Player = (Player)GetTree().Root.FindChild("Player", true, false);
		
		Log(Name, "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n", Lt.Raw);
		Log(Name, "[color=darkgray]------------------------------------------", Lt.Raw);
		Log(Name, "[color=darkgray]running", Lt.Raw);
		Log(Name, "[color=darkgray] _____  _____  _____  _____  _____  _____ ", Lt.Raw);
		Log(Name, "[color=darkgray]|   __||  |  ||   __||   __||   __|| __  |", Lt.Raw);
		Log(Name, "[color=darkgray]|__   ||  |  ||   __||   __||   __||    -|", Lt.Raw);
		Log(Name, "[color=darkgray]|_____||_____||__|   |__|   |_____||__|__|", Lt.Raw);
		Log(Name, "[color=darkgray]                            pre-alpha v0.1", Lt.Raw);
		Log(Name, "[color=darkgray]------------------------------------------", Lt.Raw);
		Log(Name, "[color=darkgray]type \"help;\" to get a list of available commands", Lt.Raw);
		
		EndEdit();
		UpdateLog();
	}
	
	public override void _Process(double delta)
	{
		DeltaTime = GameManager.GlobalDeltaTime;
		
		ConsoleIn();
		
		if (GameManager.IsConsoleOpen())
		{
			// scolling
			CurrentLine -= (int)GameManager.InputManager.MouseWheelDelta();
			
			// history
			if (GameManager.InputManager.Down("arrowup"))
			{
				if (HistoryPos == 0) HistoryTempSave = TextField.Text.StripEdges();
				HistoryPos++;
			}
			if (GameManager.InputManager.Down("arrowdown"))
			{
				HistoryPos--;
				if (HistoryPos == 0)
				{
					TextField.Text = HistoryTempSave;
					HistoryTempSave = "";
				}
			}
			HistoryPos = Mathf.Clamp(HistoryPos, 0, History.Length - 1);
			if (HistoryPos != 0)
			{
				if (HistoryPos != LastHistoryPos)
				{
					TextField.Text = History[History.Length - HistoryPos];
					LastHistoryPos = HistoryPos;
				}
			}
		}
		
		CurrentLine = Mathf.Clamp(CurrentLine, 0, Mathf.Clamp(TextLabel.GetLineCount() - 13, 0, 2147483647));
		
		TextLabel.ScrollToLine(CurrentLine);
	}

	private void ConsoleIn()
	{
		if (GameManager.InputManager.Down("return"))
		{
			if (GameManager.IsConsoleOpen())
			{
				if(!RunInput())
				{
					Input.MouseMode = Input.MouseModeEnum.Captured;
					EndEdit();
				}
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
				StartEdit();
			}
		}
		
		if (GameManager.InputManager.Down("esc"))
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			EndEdit();
		}
	}
	
	public enum Lt
	{
		Msg,
		Cmd,
		Raw,
		Err,
	};
	
	private void Log(string Mother, string Message, Lt Type)
	{
		string NewContent = "";
		
		switch (Type)
		{
			case Lt.Msg :
				NewContent = $"[color=white]> [color=dimgray]{Mother}: [color=darkgray]{Message}";
				break;
			case Lt.Cmd :
				NewContent = $"[color=dimgray]  > {Mother} - executed: {Message}";
				break;
			case Lt.Err :
				NewContent = $"[color=red]    > ERROR by {Mother}: [color=darkgray]{Message}";
				break;
			case Lt.Raw :
				NewContent = $"{Message}";
				break;
		}

		Content = Content + "\n" + NewContent;

		UpdateLog();
	}

	public void Log(string Mother, string Message)
	{
		Log(Mother, Message, Lt.Msg);
	}
	
	public void Log(string Message)
	{
		Log("unknown", Message, Lt.Msg);
	}

	private bool RunInput()
	{
		bool ValidInput = TextField.Text.StripEdges() != "";
		if (ValidInput)
		{
			string Message = TextField.Text.StripEdges().ReplaceN("\n", "");

			if (Message.EndsWith(";"))
			{
				Log("Client", Message, Lt.Cmd);
				string[] Parameters = Message.Left(Message.Length - 1).Split(" ");

				switch (Parameters[0])
				{
					case "template" : // command template
						Log(Name, "this is a template command", Lt.Err);
						break; //--------------------------------------------------
					
					case "help" : // displays a list of available commands with a description
						Log(Name, "", Lt.Raw);
						Log(Name, "[color=white]----- commands without paremeters:", Lt.Raw);
						
						Log(Name, "[color=white]               \"help\" : shows this list", Lt.Raw);
						Log(Name, "[color=white]               \"quit\" : quits the game", Lt.Raw);
						Log(Name, "[color=white]             \"reload\" : reloads the game", Lt.Raw);
						Log(Name, "[color=white]               \"test\" : prints command line test output", Lt.Raw);
						
						Log(Name, "", Lt.Raw);
						Log(Name, "[color=white]----- commands with paremeters:", Lt.Raw);
						
						Log(Name, "[color=white]              \"spawn\" : instantiates a given prefab", Lt.Raw);
						Log(Name, "[color=white]               \"drop\" : drops item based on id", Lt.Raw);
						Log(Name, "[color=white]               \"give\" : adds item to players inventory based on id", Lt.Raw);
						Log(Name, "[color=white]                 \"tp\" : teleports the player to a given vector3 position", Lt.Raw);
						
						Log(Name, "", Lt.Raw);
						Log(Name, "[color=white]^^^ here is a list of all available commands ^^^", Lt.Raw);
						
						break; //--------------------------------------------------

					case "quit" : // quits the game session and closes the application without saving
						Log(Name, "closing session", Lt.Err);
						GetTree().Quit();
						break; //--------------------------------------------------

					case "reload" : // reloads the current scene
						Log(Name, "command disabled", Lt.Err);
						break;
						//Log(this.Name, "reloading the scene", Lt.msg);
						//GetTree().ReloadCurrentScene();
						//break; //--------------------------------------------------

					case "test" : // test command had no actual use
						Log("xX_NotConsole_Xx", "test executed", Lt.Msg);
						break; //--------------------------------------------------

					case "spawn" : // will eventually spawn npcs and enemies and stuff
						Log(Name, "command disabled", Lt.Err);
						break; //--------------------------------------------------

					case "drop" : // will drop an item in front of the player
						Log(Name, "command disabled", Lt.Err);
						break; //--------------------------------------------------

					case "give" : // will give the player and item or resource
						Log(Name, "command disabled", Lt.Err);
						break; //--------------------------------------------------
					
					case "car" : // car
						Log(Name, " ╱|、", Lt.Raw);
						Log(Name, "(˚ˎ 。7", Lt.Raw);
						Log(Name, " |、˜〵", Lt.Raw);
						Log(Name, " じしˍ,)ノ", Lt.Raw);
						break;

					case "tp" : // teleports the player to the given coordinates in global/world space
						if (Parameters.Length == 4)
						{
							if (Parameters[1].IsValidFloat() && Parameters[2].IsValidFloat() && Parameters[3].IsValidFloat())
							{
								Vector3 NewPos = new Vector3(Parameters[1].ToFloat(), Parameters[2].ToFloat(), Parameters[3].ToFloat());
								if (Player != null)
								{
									Player.TeleportTo(NewPos);
									Log(Name, $"player teleported to [color=red]{NewPos.X} [color=green]{NewPos.Y} [color=blue]{NewPos.Z}", Lt.Msg);
								}
								else Log(Name, Error("noPlayer"), Lt.Err);
										
							}
							else Log(Name, Error("invalidTeleport"), Lt.Err);
						}
						else if (Parameters.Length == 3)
						{
							if (Parameters[1].IsValidFloat() && Parameters[2].IsValidFloat())
							{
								Vector3 NewPos = 
									new Vector3(
										Parameters[1].ToFloat(), 
										GameManager.CurrentMap.GetTerrainHeightAt("Terrain", 
											new Vector2(
												Parameters[1].ToFloat(), 
												Parameters[2].ToFloat())) + 1f, 
										Parameters[2].ToFloat());
								if (Player != null)
								{
									Player.TeleportTo(NewPos);
									Log(Name, $"player teleported to [color=red]{NewPos.X} [color=green]{NewPos.Y} [color=blue]{NewPos.Z}", Lt.Msg);
								}
								else Log(Name, Error("noPlayer"), Lt.Err);
										
							}
							else Log(Name, Error("invalidTeleport"), Lt.Err);
						}
						else Log(Name, Error("invalidTeleport"), Lt.Err);
						break; //--------------------------------------------------

					default : // no matching command found
						Log(Name, Error("noCommandFound"), Lt.Err);
						break; //--------------------------------------------------
				}
			}
			else Log("Client", Message, Lt.Msg);

			AddHistory(Message);
			HistoryPos = 0;
			LastHistoryPos = 0;
		}

		TextField.Text = "";
		
		return ValidInput;
	}
	
	// x red
	// y green
	// z blue
	
	//string Color(string color, string message)
	//{
	//	return ("[/color][color=" + color + "]" + message + "[/color][color=dark_gray]");
	//}

	private void StartEdit()
	{
		TextField.Text = TextField.Text.StripEdges().ReplaceN("\n", "");
		
		ConsoleControl.Visible = true;
		
		TextField.GrabFocus();
		TextField.SetEditable(true);
		
		GameManager.OpenConsole();
	}

	private void EndEdit()
	{
		TextField.Text = HistoryPos != 0 ? TextField.Text.StripEdges().ReplaceN("\n", "") : "";
		
		HistoryPos = 0;
		LastHistoryPos = 0;
		HistoryTempSave = "";
		
		ConsoleControl.Visible = false;
		TextField.SetEditable(false);
		
		GameManager.CloseConsole();
	}

	private void UpdateLog()
	{
		TextLabel.Text = Content;
		
		CurrentLine = Mathf.Clamp(TextLabel.GetLineCount() - 12, 0, 2147483647);
	}

	private string Error(string ErrorName)
	{
		switch (ErrorName)
		{
			case "template" :
				return "this is a template error message";
			case "noCommandFound" :
				return "invalid command or expecting other parameter(s)";
			case "invalidTeleport" :
				return "need a full Vector3 to teleport(missing or invalid [color=red]x[color=darkgray], [color=green]y [color=darkgray]or [color=blue]z [color=darkgray]position(s))";
			case "noPlayer" :
				return "no player instance found";
			default :
				return "could not find error message";
		}
	}

	private void AddHistory(string Message)
	{
		Array.Resize(ref History, History.Length + 1);
		History[History.Length - 1] = Message;
	}
}
