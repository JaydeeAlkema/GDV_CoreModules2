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
		private LogEntryObject logEntryObject;

		public GenerateLog()
		{
			logEntryObject = new LogEntryObject();
		}

		public void AddLogToEntries( string user, string title, string message, string notes )
		{
			LogEntry entry = new LogEntry
			{
				Username = ClearForbiddenCharactersFromString( user ),
				Title = ClearForbiddenCharactersFromString( title ),
				Message = ClearForbiddenCharactersFromString( message ),
				Notes = ClearForbiddenCharactersFromString( notes ),
				TimeOfLog = GetDate()
			};

			logEntryObject.AddEntry( entry );
			WriteToLog();
		}

		public void LoadFile()
		{
			if( logEntryObject.EntriesIsEmpty() )
			{
				LogEntryObject _logEntryObject = new LogEntryObject();
			}
		}

		public void WriteToLog()
		{
			try
			{
				string logPath = "C:/UnityPriorityProjects/GDV_CoreModules2/Assets/User Logs/test.txt";
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
	[SerializeField]
	private List<LogEntry> entries = new List<LogEntry>();

	public void AddEntry( LogEntry entry )
	{
		if( entries == null ) entries = new List<LogEntry>();

		entries.Add( entry );
	}

	public bool EntriesIsEmpty()
	{
		return ( entries.Count == 0 ) ? true : false;
	}
}