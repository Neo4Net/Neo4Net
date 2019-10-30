/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.tools.txlog
{
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;

	/// <summary>
	/// Handler of inconsistencies discovered by <seealso cref="CheckTxLogs"/> tool.
	/// </summary>
	internal interface IInconsistenciesHandler
	{
		 /// <summary>
		 /// For reporting of invalid check points. </summary>
		 /// <param name="logVersion"> the log file version where the check point is located in </param>
		 /// <param name="logPosition"> the invalid logPosition stored in the check point entry </param>
		 /// <param name="size"> the size of file pointed by the check point entry </param>
		 void ReportInconsistentCheckPoint( long logVersion, LogPosition logPosition, long size );

		 /// <summary>
		 /// For reporting of inconsistencies found between before and after state of commands. </summary>
		 /// <param name="committed"> the record seen previously during transaction log scan and considered valid </param>
		 /// <param name="current"> the record met during transaction log scan and considered inconsistent with committed </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void reportInconsistentCommand(RecordInfo<?> committed, RecordInfo<?> current);
		 void reportInconsistentCommand<T1, T2>( RecordInfo<T1> committed, RecordInfo<T2> current );

		 /// <summary>
		 /// For reporting of inconsistencies found about tx id sequences </summary>
		 /// <param name="lastSeenTxId"> last seen tx id before processing the current commit </param>
		 /// <param name="currentTxId"> the transaction id of the process commit entry </param>
		 void ReportInconsistentTxIdSequence( long lastSeenTxId, long currentTxId );
	}

}