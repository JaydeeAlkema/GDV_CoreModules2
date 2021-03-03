using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LogbookGenerator
{
	public static class GenerateLog
	{
		private static List<LogEntry> entries = new List<LogEntry>();

		public static void GetEntriesFromLogFile( string logPath )
		{
			//TODO:
			// Fill the entries list from reading each line of the file.
			string[] lines = System.IO.File.ReadAllLines( logPath );

			foreach( string line in lines )
			{
				Debug.Log( line );
			}
		}

		public static void WriteToLog( string logPath, string user, string title, string message, string notes )
		{
			//Debug.Log( logPath + "\n" + user + "\n" + title + "\n" + message + "\n" + notes );
			LogEntry entry = new LogEntry
			{
				Username = ClearForbiddenCharactersFromString( user ),
				Title = ClearForbiddenCharactersFromString( title ),
				Message = ClearForbiddenCharactersFromString( message ),
				Notes = ClearForbiddenCharactersFromString( notes ),
				TimeOfLog = GetDate()
			};

			entries.Add( entry );

			StringBuilder sb = new StringBuilder();

			// Write File
			sb.AppendLine( entry.TimeOfLog );
			sb.AppendLine( entry.Username );
			sb.AppendLine( entry.Title + "\n" );
			sb.AppendLine( entry.Message + "\n" );
			sb.AppendLine( entry.Notes );
			sb.AppendLine( "\n" + "------------------------------------------------------------------------" + "\n" );

			// Send File
			StreamWriter sw = new StreamWriter( logPath, true );
			sw.Write( sb );
			sw.Flush();
			sw.Close();

			WriteToDataLog( entry.Username, entry.Title, entry.Message, entry.Notes, entry.TimeOfLog );
		}


		private static void WriteToDataLog( string user, string title, string message, string notes, string timeOfLog )
		{
			StringBuilder sb = new StringBuilder();

			string dataPath = Application.persistentDataPath + "_" + user + "_DATALOG" + ".txt";

			// Write File
			sb.AppendLine( timeOfLog + ";" );
			sb.AppendLine( user + ";" );
			sb.AppendLine( title + ";" );
			sb.AppendLine( message + ";" );
			sb.AppendLine( notes + "|" );

			// Send File
			StreamWriter sw = new StreamWriter( dataPath, true );
			sw.Write( sb );
			sw.Flush();
			sw.Close();

			Debug.Log( "Write DataLog to: " + dataPath );
		}

		private static void GetEntriesFromDataLog()
		{
			//TODO:

			// Read complete Data Log File

			// Seperate complete string into substring

			// Convert substrings back into entries and add these to the list for safekeeping.
		}

		private static string GetDate()
		{
			string day = System.DateTime.Now.Day.ToString();
			string month = System.DateTime.Now.Month.ToString();
			string year = System.DateTime.Now.Year.ToString();

			string time = System.DateTime.Now.ToString( "HH:mm:ss" );

			return day + "/" + month + "/" + year + " " + time;
		}
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

	public struct LogEntry
	{
		public string Username { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public string Notes { get; set; }

		public string TimeOfLog { get; set; }
	}
}