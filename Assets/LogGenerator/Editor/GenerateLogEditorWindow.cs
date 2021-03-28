#if UNITY_EDITOR
using System.Collections;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace LogbookGenerator
{
	[InitializeOnLoadAttribute]
	public class GenerateLogEditorWindow : EditorWindow
	{
		private static GenerateLogEditorWindow window;

		private string logPath;
		private string logPathRoot;
		private int toolbarIndex = 0;
		private int prevToolbarIndex = -1;
		string[] toolbarTabTitles = { "Home", "Add Log", "Logs" };

		private string Log_Username = "Username";
		private string Log_Title = "Title";
		private string Log_Message = "Message...";
		private string Log_Notes = "Notes...";

		private string editedLog_Username = "";
		private string editedLog_Title = "";
		private string editedLog_Message = "";
		private string editedLog_Notes = "";

		Vector2 scrollPos;
		private int currentLogIndex = -1;

		GUIStyle activeEntryButtonStyle;

		private static bool reloadRequired = false;

		[MenuItem( "Window/Log Generator" )]
		static void ShowWindow()
		{
			window = GetWindow<GenerateLogEditorWindow>( "Log Generator" );
			window.minSize = new Vector2( 250, 400 );

			window.Show();
		}

		void OnGUI()
		{
			toolbarIndex = GUILayout.Toolbar( toolbarIndex, toolbarTabTitles, GUILayout.Height( 32f ) );

			activeEntryButtonStyle = new GUIStyle( GUI.skin.button );
			activeEntryButtonStyle.active.textColor = Color.green;
			activeEntryButtonStyle.normal.textColor = Color.white;
			activeEntryButtonStyle.fixedHeight = 32f;


			if( prevToolbarIndex != toolbarIndex )
			{
				LoadContent( toolbarIndex );
			}

			prevToolbarIndex = toolbarIndex;

			GUILayout.Space( 15 );
			ShowContentsDependingOnToolbarIndex();
		}

		static void OnToolUpdate()
		{
			if( reloadRequired )
			{
				GenerateLog.LoadFile();

				reloadRequired = false;
			}
		}

		private void OnEnable()
		{
			EditorApplication.projectChanged += OnToolUpdate;

			prevToolbarIndex = -1;
		}

		/// <summary>
		/// Clears the Add Log page 
		/// </summary>
		private void ClearAddLogPage()
		{
			Log_Username = EditorPrefs.GetString( "Log_Username" );
			Log_Title = "Title";
			Log_Message = "Message...";
			Log_Notes = "Notes...";
		}

		/// <summary>
		/// Loads the content depending on which tab selected.
		/// </summary>
		/// <param name="currentToolbarIndex"></param>
		private void LoadContent( int currentToolbarIndex )
		{
			switch( currentToolbarIndex )
			{
				case 2:
					GenerateLog.LoadFile();
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Shows the content depending on which toolbar index is selected.
		/// </summary>
		private void ShowContentsDependingOnToolbarIndex()
		{
			switch( toolbarIndex )
			{
				// Welcome Page
				case 0:
					ShowWelcomePageContents();
					break;

				// Add Log Page
				case 1:
					ShowAddLogPageContents();
					break;

				// Edit/Remove Log Page
				case 2:
					ShowLogEntriesPage();
					break;

				case 3:
					ShowEditLogPageContents();
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Shows the welcome page contents
		/// </summary>
		private void ShowWelcomePageContents()
		{
			GUILayout.BeginHorizontal();

			EditorGUILayout.LabelField( "Username: ", GUILayout.Width( 64f ) );
			Log_Username = EditorGUILayout.TextField( "", Log_Username );
			EditorPrefs.SetString( "Log_Username", Log_Username );

			GUILayout.EndHorizontal();
			GUILayout.Space( 15 );

			if( EditorPrefs.GetString( "LogbookGenerator_LogPath" ) != "" )
			{
				logPath = EditorPrefs.GetString( "LogbookGenerator_LogPath" );
				PlayerPrefs.SetString( "LogbookGenerator_LogPath", logPath );
				EditorPrefs.SetString( "LogbookGenerator_LogPath", logPath );
			}

			if( GUILayout.Button( "Create New Log File", GUILayout.Height( 32f ) ) )
			{
				if( !Directory.Exists( Application.dataPath + "/User Logs" ) )
				{
					string guid = AssetDatabase.CreateFolder( "Assets", "User Logs" );
					string newFolderPath = AssetDatabase.GUIDToAssetPath( guid );
				}

				logPath = Path.Combine( Application.dataPath + "/User Logs/" + Log_Username + ".json" );
				PlayerPrefs.SetString( "LogbookGenerator_LogPath", logPath );
				EditorPrefs.SetString( "LogbookGenerator_LogPath", logPath );

				FileStream fileStream = new FileStream( logPath, FileMode.Append );
				StreamWriter sr = new StreamWriter( fileStream );
				sr.Flush();
				sr.Close();

				AssetDatabase.Refresh();
				reloadRequired = true;
			}

			if( GUILayout.Button( "Load File", GUILayout.Height( 32f ) ) )
			{
				logPath = EditorUtility.OpenFilePanel( "Select Log File", "", "" );
				//Debug.Log( logPath );
				EditorPrefs.SetString( "LogbookGenerator_LogPath", logPath );
				GenerateLog.LoadFile();
			}

			GUILayout.Label( "Current File:" );
			GUILayout.Label( logPath, EditorStyles.wordWrappedLabel );
		}

		/// <summary>
		/// Shows the Add Log Page contents
		/// </summary>
		private void ShowAddLogPageContents()
		{
			Log_Username = EditorGUILayout.TextField( "", Log_Username );
			GUILayout.Space( 2 );
			Log_Title = EditorGUILayout.TextField( "", Log_Title );
			GUILayout.Space( 2 );
			Log_Message = GUILayout.TextArea( Log_Message, GUILayout.ExpandHeight( true ), GUILayout.Height( 128 ) );
			GUILayout.Space( 5 );
			Log_Notes = GUILayout.TextArea( Log_Notes, GUILayout.ExpandHeight( true ), GUILayout.Height( 64 ) );

			GUILayout.FlexibleSpace();
			GUI.backgroundColor = Color.green;
			if( GUILayout.Button( "Complete" ) )
			{
				string dataPath = EditorPrefs.GetString( "Log_DataPath" );

				GenerateLog.AddLogToEntries( Log_Username, Log_Title, Log_Message, Log_Notes );
				GenerateLog.WriteToLog();

				if( Log_Username != EditorPrefs.GetString( "Log_Username" ) )
				{
					EditorPrefs.SetString( "Log_Username", Log_Username );
				}

				toolbarIndex = 0;
				ClearAddLogPage();
			}
		}

		/// <summary>
		/// Laods the entry log data before use.
		/// </summary>
		private void LoadLogEntryData()
		{
			LogEntry editedLog = GenerateLog.LogEntryObject.entries[currentLogIndex];

			editedLog_Username = editedLog.Username;
			editedLog_Title = editedLog.Title;
			editedLog_Message = editedLog.Message;
			editedLog_Notes = editedLog.Notes;
		}

		/// <summary>
		/// Shows the Edit Log Page Contents. Which is the same layout as the Add Log Page 
		/// </summary>
		private void ShowEditLogPageContents()
		{
			editedLog_Username = EditorGUILayout.TextField( "", editedLog_Username );
			GUILayout.Space( 2 );
			editedLog_Title = EditorGUILayout.TextField( "", editedLog_Title );
			GUILayout.Space( 2 );
			editedLog_Message = GUILayout.TextArea( editedLog_Message, GUILayout.ExpandHeight( true ), GUILayout.Height( 128 ) );
			GUILayout.Space( 5 );
			editedLog_Notes = GUILayout.TextArea( editedLog_Notes, GUILayout.ExpandHeight( true ), GUILayout.Height( 64 ) );

			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.green;
			if( GUILayout.Button( "Finish Editing", GUILayout.Height( 32 ) ) )
			{
				LogEntry editedLog = GenerateLog.LogEntryObject.entries[currentLogIndex];

				editedLog.Username = editedLog_Username;
				editedLog.Title = editedLog_Title;
				editedLog.Message = editedLog_Message;
				editedLog.Notes = editedLog_Notes;
				editedLog.TimeOfLog = GenerateLog.GetDate();

				GenerateLog.LogEntryObject.entries[currentLogIndex] = editedLog;

				GenerateLog.WriteToLog();

				ClearAddLogPage();
				ShowLogEntriesPage();
			}
			GUI.backgroundColor = Color.red;
			if( GUILayout.Button( "Cancel", GUILayout.Height( 32 ) ) )
			{
				toolbarIndex = 2;
			}

			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Shows a list of all the logs in the list
		/// </summary>
		private void ShowLogEntriesPage()
		{
			// Failsafe
			if( GenerateLog.LogEntryObject.EntriesIsEmpty() )
			{
				Debug.LogWarning( "Entry Object is either Empty! Could not load logs. Add a log first" );
				return;
			}

			toolbarIndex = 2;
			GUILayout.Label( "Logs: " );

			scrollPos = GUILayout.BeginScrollView( scrollPos, GUILayout.ExpandWidth( true ), GUILayout.Height( position.height - 128f ) );
			for( int i = 0; i < GenerateLog.LogEntryObject.entries.Count; i++ )
			{
				string buttonTitle = GenerateLog.LogEntryObject.entries[i].Title + " - " + GenerateLog.LogEntryObject.entries[i].TimeOfLog;
				if( GUILayout.Button( buttonTitle, activeEntryButtonStyle ) )
				{
					currentLogIndex = i;
				}
			}
			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			if( GUILayout.Button( "Edit", GUILayout.Height( 32f ) ) )
			{
				toolbarIndex = 3;
				LoadLogEntryData();
				ShowEditLogPageContents();
			}
			if( GUILayout.Button( "Delete", GUILayout.Height( 32f ) ) )
			{
				RemoveLogFromEntries();
			}
			GUILayout.EndHorizontal();
		}

		/// <summary>
		///  Removes a Log from the entries.
		/// </summary>
		private void RemoveLogFromEntries()
		{
			if( currentLogIndex <= GenerateLog.LogEntryObject.entries.Count )
			{
				GenerateLog.LogEntryObject.entries.RemoveAt( currentLogIndex );
				GenerateLog.WriteToLog();
				GenerateLog.LoadFile();
			}
		}
	}
}
#endif