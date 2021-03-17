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

		private void OnEnable()
		{
			generateLog = new GenerateLog();
			prevToolbarIndex = -1;
		}

		private void ClearAddLogPage()
		{
			Log_Username = EditorPrefs.GetString( "Log_Username" );
			Log_Title = "Title";
			Log_Message = "Message...";
			Log_Notes = "Notes...";
		}

		private void LoadContent( int currentToolbarIndex )
		{
			switch( currentToolbarIndex )
			{
				case 2:
					generateLog.LoadFile();
					break;

				default:
					break;
			}
		}

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

		private void ShowWelcomePageContents()
		{
			if( EditorPrefs.GetString( "LogbookGenerator_LogPath" ) != "" )
			{
				logPath = EditorPrefs.GetString( "LogbookGenerator_LogPath" );
				PlayerPrefs.SetString( "LogbookGenerator_LogPath", logPath );
			}

			if( GUILayout.Button( "Set Log Folder Path", GUILayout.Height( 32f ) ) )
			{
				logPathRoot = EditorUtility.OpenFilePanel( "Select Root Folder", "This will be the root folder where your logs will be stored.", "" );
				EditorPrefs.SetString( "LogbookGenerator_LogPathRoot", logPathRoot );
				PlayerPrefs.SetString( "LogbookGenerator_LogPathRoot", logPathRoot );
				generateLog.CreateFile( logPathRoot );
			}

			if( GUILayout.Button( "Create New Log File", GUILayout.Height( 32f ) ) )
			{
				//TODO:
				// Set Path.							(Popup file explorer)
				// Set file name.						(Popup file explorer)
				// Set new file as target file.			(generatelog.CreateFile(path + filename))
			}

			if( GUILayout.Button( "Load File", GUILayout.Height( 32f ) ) )
			{
				logPath = EditorUtility.OpenFilePanel( "Select Log File", "This can be a .doc/.docx/.txt etc.", "" );
				Debug.Log( logPath );
				EditorPrefs.SetString( "LogbookGenerator_LogPath", logPath );
				generateLog.LoadFile();
			}

			GUILayout.Label( "Current File:" );
			GUILayout.Label( logPath, EditorStyles.wordWrappedLabel );
		}

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

				generateLog.AddLogToEntries( Log_Username, Log_Title, Log_Message, Log_Notes );
				generateLog.WriteToLog();

				if( Log_Username != EditorPrefs.GetString( "Log_Username" ) )
				{
					EditorPrefs.SetString( "Log_Username", Log_Username );
				}

				ClearAddLogPage();
			}
		}

		private void LoadLogEntryData()
		{
			LogEntry editedLog = generateLog.LogEntryObject.entries[currentLogIndex];

			editedLog_Username = editedLog.Username;
			editedLog_Title = editedLog.Title;
			editedLog_Message = editedLog.Message;
			editedLog_Notes = editedLog.Notes;
		}

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
				LogEntry editedLog = generateLog.LogEntryObject.entries[currentLogIndex];

				editedLog.Username = editedLog_Username;
				editedLog.Title = editedLog_Title;
				editedLog.Message = editedLog_Message;
				editedLog.Notes = editedLog_Notes;
				editedLog.TimeOfLog = generateLog.GetDate();

				generateLog.LogEntryObject.entries[currentLogIndex] = editedLog;

				generateLog.WriteToLog();

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

		private void ShowLogEntriesPage()
		{
			toolbarIndex = 2;
			GUILayout.Label( "Logs: " );

			scrollPos = GUILayout.BeginScrollView( scrollPos, GUILayout.ExpandWidth( true ), GUILayout.Height( position.height - 128f ) );
			for( int i = 0; i < generateLog.LogEntryObject.entries.Count; i++ )
			{
				string buttonTitle = generateLog.LogEntryObject.entries[i].Title + " - " + generateLog.LogEntryObject.entries[i].TimeOfLog;
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

		private void RemoveLogFromEntries()
		{
			if( currentLogIndex <= generateLog.LogEntryObject.entries.Count )
			{
				generateLog.LogEntryObject.entries.RemoveAt( currentLogIndex );
				generateLog.WriteToLog();
				generateLog.LoadFile();
			}
		}
	}
}
#endif