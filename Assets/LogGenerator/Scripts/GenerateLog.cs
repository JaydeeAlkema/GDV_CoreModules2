using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LogbookGenerator
{
	[InitializeOnLoad]
	public static class GenerateLog
	{
		private static LogEntryObject logEntryObject = new LogEntryObject();  // Active Log Entry Object
		public static string logPath;

		public static LogEntryObject LogEntryObject
		{
			get
			{
				if( logEntryObject == null ) logEntryObject = new LogEntryObject();
				return logEntryObject;
			}
			set { logEntryObject = value; }
		}

		static GenerateLog()
		{
			//Debug.Log( "Constructor..." );
			logEntryObject = new LogEntryObject();
			logPath = PlayerPrefs.GetString( "LogbookGenerator_LogPath" );
		}

		/// <summary>
		/// Adds A log to the entries list.
		/// </summary>
		/// <param name="user"> Name of the User. </param>
		/// <param name="title"> Title of the Log. </param>
		/// <param name="message"> Message of the Log. </param>
		/// <param name="notes"> Notes of the Log. </param>
		public static void AddLogToEntries( string user, string title, string message, string notes )
		{
			LogEntry entry = new LogEntry()
			{
				Username = ClearForbiddenCharactersFromString( user ),
				Title = ClearForbiddenCharactersFromString( title ),
				Message = ClearForbiddenCharactersFromString( message ),
				Notes = ClearForbiddenCharactersFromString( notes ),
				TimeOfLog = GetDate()
			};

			LogEntryObject.AddEntry( entry );
		}

		/// <summary>
		/// Loads data from file. necessary for when you close the unity project and the runtime data is lost.
		/// </summary>
		public static void LoadFile()
		{
			if( !File.Exists( PlayerPrefs.GetString( "LogbookGenerator_LogPath" ) ) )
			{
				Debug.LogWarning( "No such path exists!" );
				//Debug.Log( PlayerPrefs.GetString( "LogbookGenerator_LogPath" ) );
				return;
			}

			StreamReader sr = new StreamReader( PlayerPrefs.GetString( "LogbookGenerator_LogPath" ) );
			string json = sr.ReadToEnd();
			sr.Close();
			sr.Dispose();

			LogEntryObject = JsonUtility.FromJson<LogEntryObject>( json );
		}

		/// <summary>
		/// Writes the LogEntryObject to JSON.
		/// </summary>
		public static void WriteToLog()
		{
			try
			{
				StreamWriter sw = new StreamWriter( logPath );
				sw.Write( JsonUtility.ToJson( LogEntryObject, true ) );

				sw.Flush();
				sw.Close();
			}
			catch( Exception e )
			{
				Debug.LogError( e.Message );
			}
		}

		/// <summary>
		/// Get's the current date and parses it into a neat little string.
		/// </summary>
		/// <returns></returns>
		public static string GetDate()
		{
			string day = System.DateTime.Now.Day.ToString();
			string month = System.DateTime.Now.Month.ToString();
			string year = System.DateTime.Now.Year.ToString();

			string time = System.DateTime.Now.ToString( "HH:mm:ss" );

			return day + "/" + month + "/" + year + " " + time;
		}

		/// <summary>
		/// Clears forbidden characters from the given string
		/// </summary>
		/// <param name="text"> String to clear forbidden messages of.</param>
		/// <returns></returns>
		private static string ClearForbiddenCharactersFromString( string text )
		{
			char[] charsToRemove = { ';', '|' };

			foreach( char c in charsToRemove )
			{
				text = text.Replace( c.ToString(), String.Empty );
			}
			return text;
		}
	}
}

[System.Serializable]
public struct LogEntry
{
	public string TimeOfLog;
	public string Username;
	public string Title;
	public string Message;
	public string Notes;
}

[System.Serializable]
public class LogEntryObject
{
	public List<LogEntry> entries = new List<LogEntry>();

	public void AddEntry( LogEntry entry )
	{
		if( entries == null ) entries = new List<LogEntry>();

		entries.Add( entry );
	}

	public bool EntriesIsEmpty()
	{
		return entries.Count == 0;
	}
}