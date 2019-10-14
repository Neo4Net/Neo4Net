using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.tools.txlog
{

	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;

	/// <summary>
	/// Handler that simply prints given number of inconsistencies to <seealso cref="PrintingInconsistenciesHandler.out"/> and throws
	/// an exception if too many inconsistencies are found.
	/// </summary>
	internal class PrintingInconsistenciesHandler : InconsistenciesHandler
	{
		 private const int DEFAULT_NUMBER_OF_INCONSISTENCIES_TO_PRINT = 1024;

		 private readonly PrintStream @out;

		 private int _seenInconsistencies;

		 internal PrintingInconsistenciesHandler( PrintStream @out )
		 {
			  this.@out = @out;
		 }

		 public override void ReportInconsistentCheckPoint( long logVersion, LogPosition logPosition, long size )
		 {
			  @out.println( "Inconsistent check point found in log with version " + logVersion );
			  long pointedLogVersion = logPosition.LogVersion;
			  @out.println( "\tCheck point claims to recover from " + logPosition.ByteOffset + " in log with version " + pointedLogVersion );
			  if ( size >= 0 )
			  {
					@out.println( "\tLog with version " + pointedLogVersion + " has size " + size );
			  }
			  else
			  {
					@out.println( "\tLog with version " + pointedLogVersion + " does not exist" );
			  }
			  IncrementAndPerhapsThrow();
		 }

		 public override void ReportInconsistentCommand<T1, T2>( RecordInfo<T1> committed, RecordInfo<T2> current )
		 {
			  @out.println( "Inconsistent after and before states:" );
			  @out.println( "\t+" + committed );
			  @out.println( "\t-" + current );
			  IncrementAndPerhapsThrow();
		 }

		 public override void ReportInconsistentTxIdSequence( long lastSeenTxId, long currentTxId )
		 {
			  @out.printf( "Inconsistent in tx id sequence between transactions %d and %d %n", lastSeenTxId, currentTxId );
			  IncrementAndPerhapsThrow();
		 }

		 private void IncrementAndPerhapsThrow()
		 {
			  _seenInconsistencies++;
			  if ( _seenInconsistencies >= DEFAULT_NUMBER_OF_INCONSISTENCIES_TO_PRINT )
			  {
					throw new Exception( "Too many inconsistencies found" );
			  }
		 }
	}

}