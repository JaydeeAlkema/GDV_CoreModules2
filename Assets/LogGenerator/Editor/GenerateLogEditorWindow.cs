#if UNITY_EDITOR
using System.Collections;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LogbookGenerator
{
	public class GenerateLogEditorWindow : EditorWindow
	{
		private static GenerateLogEditorWindow window;

		private string logPath;
		private int toolbarInt = 0;
		string[] toolbarTabTitles = { "Home", "Add Log", "Edit Log", "Remove Log" };

		private string Log_Username = "Username";
		private string Log_Title = "Title";
		private string Log_Message = "Message...";
		private string Log_Notes = "Notes...";

		[MenuItem( "Window/Logbook Generator" )]
		static void ShowWindow()
		{
			window = GetWindow<GenerateLogEditorWindow>( "Logbook Generator" );
			window.Show();
		}
		void OnGUI()
		{
			toolbarInt = GUILayout.Toolbar( toolbarInt, toolbarTabTitles );

			GUILayout.Space( 15 );
			ShowContentsDependingOnToolbarInt();
		}

		private void ShowContentsDependingOnToolbarInt()
		{
			switch( toolbarInt )
			{
				// Welcome Page
				case 0:
					ShowWelcomeContents();
					break;

				// Add Log Page
				case 1:
					ShowAddLogContents();
					break;

				// Edit Log Page
				case 2:
					ShowEditLogContents();
					break;

				// Remove Log Page
				case 3:
					ShowRemoveLogContents();
					break;

				default:
					break;
			}
		}

		private void ShowWelcomeContents()
		{
			if( EditorPrefs.GetString( "LogbookGenerator_LogPath" ) != "" )
			{
				logPath = EditorPrefs.GetString( "LogbookGenerator_LogPath" );
			}

			if( GUILayout.Button( "Select Log File" ) )
			{
				logPath = EditorUtility.OpenFilePanel( "Select Log File", "This can be a .doc/.docx/.txt etc.", "" );
				Debug.Log( logPath );
				EditorPrefs.SetString( "LogbookGenerator_LogPath", logPath );
			}

			GUILayout.Label( "Current Path:" );
			GUILayout.Label( logPath, EditorStyles.wordWrappedLabel );

			//if( logPath != "" )
			//{
			//	GenerateLog.GetEntriesFromLogFile( logPath );
			//}
		}
		private void ShowAddLogContents()
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
				GenerateLog.WriteToLog( logPath, Log_Username, Log_Title, Log_Message, Log_Notes );

				if( Log_Username != EditorPrefs.GetString( "Log_Username" ) )
				{
					EditorPrefs.SetString( "Log_Username", Log_Username );
				}

				Log_Username = EditorPrefs.GetString( "Log_Username" );
				Log_Title = "Title";
				Log_Message = "Message";
				Log_Notes = "Notes";
			}
		}
		private void ShowEditLogContents()
		{
			GUILayout.Label( "Edit Log" );

		}
		private void ShowRemoveLogContents()
		{
			GUILayout.Label( "Remove Log" );

		}
	}
}
#endif