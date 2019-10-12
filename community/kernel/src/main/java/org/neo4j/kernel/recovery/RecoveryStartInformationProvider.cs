using System;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.recovery
{

	using Org.Neo4j.Function;
	using UnderlyingStorageException = Org.Neo4j.Kernel.impl.store.UnderlyingStorageException;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using CheckPoint = Org.Neo4j.Kernel.impl.transaction.log.entry.CheckPoint;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogVersionRepository_Fields.INITIAL_LOG_VERSION;

	/// <summary>
	/// Utility class to find the log position to start recovery from
	/// </summary>
	public class RecoveryStartInformationProvider : ThrowingSupplier<RecoveryStartInformation, IOException>
	{
		 public interface Monitor
		 {
			  /// <summary>
			  /// There's a check point log entry as the last entry in the transaction log.
			  /// </summary>
			  /// <param name="logPosition"> <seealso cref="LogPosition"/> of the last check point. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void noCommitsAfterLastCheckPoint(org.neo4j.kernel.impl.transaction.log.LogPosition logPosition)
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// There's a check point log entry, but there are other log entries after it.
			  /// </summary>
			  /// <param name="logPosition"> <seealso cref="LogPosition"/> pointing to the first log entry after the last
			  /// check pointed transaction. </param>
			  /// <param name="firstTxIdAfterLastCheckPoint"> transaction id of the first transaction after the last check point. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void commitsAfterLastCheckPoint(org.neo4j.kernel.impl.transaction.log.LogPosition logPosition, long firstTxIdAfterLastCheckPoint)
	//		  { // no-op by default
	//		  }

			  /// <summary>
			  /// No check point log entry found in the transaction log.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void noCheckPointFound()
	//		  { // no-op by default
	//		  }
		 }

		 public static readonly Monitor NO_MONITOR = new MonitorAnonymousInnerClass();

		 private class MonitorAnonymousInnerClass : Monitor
		 {
		 }

		 private readonly LogTailScanner _logTailScanner;
		 private readonly Monitor _monitor;

		 public RecoveryStartInformationProvider( LogTailScanner logTailScanner, Monitor monitor )
		 {
			  this._logTailScanner = logTailScanner;
			  this._monitor = monitor;
		 }

		 /// <summary>
		 /// Find the log position to start recovery from
		 /// </summary>
		 /// <returns> <seealso cref="LogPosition.UNSPECIFIED"/> if there is no need to recover otherwise the <seealso cref="LogPosition"/> to
		 /// start recovery from </returns>
		 /// <exception cref="IOException"> if log files cannot be read </exception>
		 public override RecoveryStartInformation Get()
		 {
			  LogTailScanner.LogTailInformation logTailInformation = _logTailScanner.TailInformation;
			  CheckPoint lastCheckPoint = logTailInformation.LastCheckPoint;
			  long txIdAfterLastCheckPoint = logTailInformation.FirstTxIdAfterLastCheckPoint;
			  if ( !logTailInformation.CommitsAfterLastCheckpoint() )
			  {
					_monitor.noCommitsAfterLastCheckPoint( lastCheckPoint != null ? lastCheckPoint.LogPosition : null );
					return CreateRecoveryInformation( LogPosition.UNSPECIFIED, txIdAfterLastCheckPoint );
			  }

			  if ( lastCheckPoint != null )
			  {
					_monitor.commitsAfterLastCheckPoint( lastCheckPoint.LogPosition, txIdAfterLastCheckPoint );
					return CreateRecoveryInformation( lastCheckPoint.LogPosition, txIdAfterLastCheckPoint );
			  }
			  else
			  {
					if ( logTailInformation.OldestLogVersionFound != INITIAL_LOG_VERSION )
					{
						 long fromLogVersion = Math.Max( INITIAL_LOG_VERSION, logTailInformation.OldestLogVersionFound );
						 throw new UnderlyingStorageException( "No check point found in any log file from version " + fromLogVersion + " to " + logTailInformation.CurrentLogVersion );
					}
					_monitor.noCheckPointFound();
					return CreateRecoveryInformation( LogPosition.start( 0 ), txIdAfterLastCheckPoint );
			  }
		 }

		 private RecoveryStartInformation CreateRecoveryInformation( LogPosition logPosition, long firstTxId )
		 {
			  return new RecoveryStartInformation( logPosition, firstTxId );
		 }
	}

}