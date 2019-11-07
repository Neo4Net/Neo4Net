using System;
using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Consistency.report
{
	using Strings = Neo4Net.Helpers.Strings;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Log = Neo4Net.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Strings.TAB;

	public class InconsistencyMessageLogger : InconsistencyLogger
	{
		 private readonly Log _log;

		 public InconsistencyMessageLogger( Log log )
		 {
			  this._log = log;
		 }

		 public override void Error( RecordType recordType, AbstractBaseRecord record, string message, params object[] args )
		 {
			  _log.error( BuildMessage( message, record, args ) );
		 }

		 public override void Error( RecordType recordType, AbstractBaseRecord oldRecord, AbstractBaseRecord newRecord, string message, params object[] args )
		 {
			  _log.error( BuildMessage( message, oldRecord, newRecord, args ) );
		 }

		 public override void Error( string message )
		 {
			  _log.error( BuildMessage( message ) );
		 }

		 public override void Warning( RecordType recordType, AbstractBaseRecord record, string message, params object[] args )
		 {
			  _log.warn( BuildMessage( message, record, args ) );
		 }

		 public override void Warning( RecordType recordType, AbstractBaseRecord oldRecord, AbstractBaseRecord newRecord, string message, params object[] args )
		 {
			  _log.warn( BuildMessage( message, oldRecord, newRecord, args ) );
		 }

		 public override void Warning( string message )
		 {
			  _log.warn( BuildMessage( message ) );
		 }

		 private static string BuildMessage( string message )
		 {
			  StringBuilder builder = TabAfterLinebreak( message );
			  return builder.ToString();
		 }

		 private static string BuildMessage( string message, AbstractBaseRecord record, object[] args )
		 {
			  StringBuilder builder = JoinLines( message ).Append( Environment.NewLine ).Append( TAB ).Append( record );
			  AppendArgs( builder, args );
			  return builder.ToString();
		 }

		 private static string BuildMessage( string message, AbstractBaseRecord oldRecord, AbstractBaseRecord newRecord, object[] args )
		 {
			  StringBuilder builder = JoinLines( message );
			  builder.Append( Environment.NewLine ).Append( TAB ).Append( "- " ).Append( oldRecord );
			  builder.Append( Environment.NewLine ).Append( TAB ).Append( "+ " ).Append( newRecord );
			  AppendArgs( builder, args );
			  return builder.ToString();
		 }

		 private static StringBuilder TabAfterLinebreak( string message )
		 {
			  string[] lines = message.Split( "\n", true );
			  StringBuilder builder = new StringBuilder( lines[0].Trim() );
			  for ( int i = 1; i < lines.Length; i++ )
			  {
					builder.Append( Environment.NewLine ).Append( TAB ).Append( lines[i].Trim() );
			  }
			  return builder;
		 }

		 private static StringBuilder JoinLines( string message )
		 {
			  string[] lines = message.Split( "\n", true );
			  StringBuilder builder = new StringBuilder( lines[0].Trim() );
			  for ( int i = 1; i < lines.Length; i++ )
			  {
					builder.Append( ' ' ).Append( lines[i].Trim() );
			  }
			  return builder;
		 }

		 private static StringBuilder AppendArgs( StringBuilder builder, object[] args )
		 {
			  if ( args == null || args.Length == 0 )
			  {
					return builder;
			  }
			  builder.Append( Environment.NewLine ).Append( TAB ).Append( "Inconsistent with:" );
			  foreach ( object arg in args )
			  {
					builder.Append( ' ' ).Append( Strings.prettyPrint( arg ) );
			  }
			  return builder;
		 }
	}

}