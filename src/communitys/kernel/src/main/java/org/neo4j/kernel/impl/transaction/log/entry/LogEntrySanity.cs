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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	/// <summary>
	/// Sanity checking for read <seealso cref="LogEntry log entries"/>.
	/// </summary>
	internal class LogEntrySanity
	{
		 private static readonly long _unreasonablyLongTime = TimeUnit.DAYS.toMillis( 30 * 365 );
		 private const int UNREASONABLY_HIGH_SERVER_ID = 10_000_000;

		 private LogEntrySanity()
		 {
			  throw new AssertionError();
		 }

		 internal static bool LogEntryMakesSense( LogEntry entry )
		 {
			  if ( entry == null )
			  {
					return false;
			  }
			  if ( entry is LogEntryStart )
			  {
					return StartEntryMakesSense( ( LogEntryStart ) entry );
			  }
			  else if ( entry is LogEntryCommit )
			  {
					return CommitEntryMakesSense( ( LogEntryCommit ) entry );
			  }
			  return true;
		 }

		 private static bool CommitEntryMakesSense( LogEntryCommit entry )
		 {
			  return TimeMakesSense( entry.TimeWritten ) && TransactionIdMakesSense( entry );
		 }

		 private static bool TransactionIdMakesSense( LogEntryCommit entry )
		 {
			  return entry.TxId > Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
		 }

		 private static bool StartEntryMakesSense( LogEntryStart entry )
		 {
			  return ServerIdMakesSense( entry.LocalId ) && ServerIdMakesSense( entry.MasterId ) && TimeMakesSense( entry.TimeWritten );
		 }

		 private static bool ServerIdMakesSense( int serverId )
		 {
			  return serverId >= 0 && serverId < UNREASONABLY_HIGH_SERVER_ID;
		 }

		 private static bool TimeMakesSense( long time )
		 {
			  return abs( currentTimeMillis() - time ) < _unreasonablyLongTime;
		 }
	}

}