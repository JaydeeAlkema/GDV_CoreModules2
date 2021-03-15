#if UNITY_EDITOR
using System.Collections;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//TODO:
// Add comments.
// Improve UI layout.

namespace LogbookGenerator
{
	public class GenerateLogEditorWindow : EditorWindow
	{
		private static GenerateLogEditorWindow window;
		private GenerateLog generateLog;
		private LogEntryObject logEntryObject;

		private string logPath;
		private string logPathRoot;
		private int toolbarInt = 0;
		private int prevToolbarInt = -1;
		string[] toolbarTabTitles = { "Home", "Add Log", "Entries" };

		private string Log_Username = "Username";
		private string Log_Title = "Title";
		private string Log_Message = "Message...";
		private string Log_Notes = "Notes...";

		Vector2 scrollPos;
		private int currentLogIndex = -1;

		GUIStyle activeEntryButtonStyle;

		[MenuItem( "Window/Logbook Generator" )]
		static void ShowWindow()
		{
			window = GetWindow<GenerateLogEditorWindow>( "Logbook Generator" );
			window.Show();
		}
		void OnGUI()
		{
			toolbarInt = GUILayout.Toolbar( toolbarInt, toolbarTabTitles, GUILayout.Height( 32f ) );

			activeEntryButtonStyle = new GUIStyle( GUI.skin.button );
			activeEntryButtonStyle.active.textColor = Color.green;
			activeEntryButtonStyle.normal.textColor = Color.white;
			activeEntryButtonStyle.fixedHeight = 32f;


			if( prevToolbarInt != toolbarInt )
			{
				LoadContent( toolbarInt );
			}

			prevToolbarInt = toolbarInt;

			GUILayout.Space( 15 );
			ShowContentsDependingOnToolbarInt();
		}
		private void OnEnable()
		{
			generateLog = new GenerateLog();
			prevToolbarInt = -1;
		}

		private void ClearAddLogPage()
		{
			Log_Username = "Username";
			Log_Title = "Title";
			Log_Message = "Message...";
			Log_Notes = "Notes...";
		}

		private void LoadContent( int currentToolbarInt )
		{
			switch( currentToolbarInt )
			{
				case 0:

				case 2:
					generateLog.LoadFile();
					break;

				default:
					break;
			}
		}

		private void ShowContentsDependingOnToolbarInt()
		{
			switch( toolbarInt )
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

		private void ShowWelcomePageContents()
		{
			if( EditorPrefs.GetString( "LogbookGenerator_LogPath" ) != "" )
			{
				logPath = EditorPrefs.GetString( "LogbookGenerator_LogPath" );
				PlayerPrefs.SetString( "LogbookGenerator_LogPath", logPath );
			}

			if( GUILayout.Button( "Set Log Folder Path" ) )
			{
				logPathRoot = EditorUtility.OpenFilePanel( "Select Root Folder", "This will be the root folder where your logs will be stored.", "" );
				EditorPrefs.SetString( "LogbookGenerator_LogPathRoot", logPathRoot );
				PlayerPrefs.SetString( "LogbookGenerator_LogPathRoot", logPathRoot );
				generateLog.CreateFile( logPathRoot );
			}

			if( GUILayout.Button( "Create New Log File" ) )
			{

			}

			if( GUILayout.Button( "Load File" ) )
			{
				logPath = EditorUtility.OpenFilePanel( "Select Log File", "This can be a .doc/.docx/.txt etc.", "" );
				Debug.Log( logPath );
				EditorPrefs.SetString( "LogbookGenerator_LogPath", logPath );
				generateLog.LoadFile();
			}

			if( logPath != "" )
			{
				GUILayout.Label( "Current Path:" );
				GUILayout.Label( logPath, EditorStyles.wordWrappedLabel );
			}
		}
		private void ShowAddLogPageContents()
		{
			//TODO:
			// Fix formating and make this look beter!

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
				EditorPrefs.SetString( "Log_DataPath", Application.persistentDataPath + "_" + Log_Username + "_DATALOG" + ".txt" );

				string dataPath = EditorPrefs.GetString( "Log_DataPath" );

				generateLog.AddLogToEntries( Log_Username, Log_Title, Log_Message, Log_Notes );
				generateLog.WriteToLog();

				if( Log_Username != EditorPrefs.GetString( "Log_Username" ) )
				{
					EditorPrefs.SetString( "Log_Username", Log_Username );
				}

				ClearAddLogPage();
			}
		}

		private void ShowEditLogPageContents()
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
			if( GUILayout.Button( "Finish Editing" ) )
			{
				LogEntry editedLog = generateLog.LogEntryObject.entries[currentLogIndex];

				editedLog.Username = Log_Username;
				editedLog.Title = Log_Title;
				editedLog.Message = Log_Message;
				editedLog.Notes = Log_Notes;
				editedLog.TimeOfLog = generateLog.GetDate();

				generateLog.LogEntryObject.entries[currentLogIndex] = editedLog;

				generateLog.WriteToLog();

				ClearAddLogPage();
				ShowLogEntriesPage();
			}
		}

		private void RemoveLogFromEntries()
		{
			generateLog.LogEntryObject.entries.RemoveAt( currentLogIndex );
			generateLog.WriteToLog();
			generateLog.LoadFile();
		}

		private void ShowLogEntriesPage()
		{
			toolbarInt = 2;
			GUILayout.Label( "Logs: " );

			scrollPos = GUILayout.BeginScrollView( scrollPos, GUILayout.ExpandWidth( true ), GUILayout.Height( position.height - 128f ) );
			for( int i = 0; i < generateLog.LogEntryObject.entries.Count; i++ )
			{
				string buttonTitle = generateLog.LogEntryObject.entries[i].Title + " - " + generateLog.LogEntryObject.entries[i].TimeOfLog;
				if( GUILayout.Button( buttonTitle, activeEntryButtonStyle ) )
				{
					// Do something
					currentLogIndex = i;
				}
			}
			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			if( GUILayout.Button( "Edit" ) )
			{
				toolbarInt = 3;
				ShowEditLogPageContents();
			}
			if( GUILayout.Button( "Delete" ) )
			{
				RemoveLogFromEntries();
			}
			GUILayout.EndHorizontal();
		}
	}
}
#endif