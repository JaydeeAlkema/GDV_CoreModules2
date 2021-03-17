using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LogbookGenerator
{
	public class GenerateLog
	{
		private LogEntryObject logEntryObject;  // Active Log Entry Object
		private string logPath;

		public LogEntryObject LogEntryObject { get => logEntryObject; set => logEntryObject = value; }

		public GenerateLog()
		{
			logEntryObject = new LogEntryObject();
			logPath = PlayerPrefs.GetString( "LogbookGenerator_LogPath" );
		}

		public void CreateFile( string path )
		{
			string fileName = path + "/";
		}

		public void AddLogToEntries( string user, string title, string message, string notes )
		{
			LogEntry entry = new LogEntry()
			{
				Username = ClearForbiddenCharactersFromString( user ),
				Title = ClearForbiddenCharactersFromString( title ),
				Message = ClearForbiddenCharactersFromString( message ),
				Notes = ClearForbiddenCharactersFromString( notes ),
				TimeOfLog = GetDate()
			};

			logEntryObject.AddEntry( entry );
		}

		public void LoadFile()
		{
			StreamReader sr = new StreamReader( PlayerPrefs.GetString( "LogbookGenerator_LogPath" ) );
			string json = sr.ReadToEnd();
			sr.Close();
			sr.Dispose();

			if( logEntryObject == null ) logEntryObject = new LogEntryObject();

			logEntryObject = JsonUtility.FromJson<LogEntryObject>( json );
		}

		public void WriteToLog()
		{
			try
			{
				StreamWriter sw = new StreamWriter( logPath );
				sw.Write( JsonUtility.ToJson( logEntryObject, true ) );

				sw.Flush();
				sw.Close();
			}
			catch( Exception e )
			{
				Debug.LogError( e.Message );
			}
		}

		public string GetDate()
		{
			string day = System.DateTime.Now.Day.ToString();
			string month = System.DateTime.Now.Month.ToString();
			string year = System.DateTime.Now.Year.ToString();

			string time = System.DateTime.Now.ToString( "HH:mm:ss" );

			return day + "/" + month + "/" + year + " " + time;
		}

		private string ClearForbiddenCharactersFromString( string text )
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