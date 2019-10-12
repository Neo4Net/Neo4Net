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
namespace Org.Neo4j.Consistency
{
	using ConsistencySummaryStatistics = Org.Neo4j.Consistency.report.ConsistencySummaryStatistics;
	using DataInconsistencyError = Org.Neo4j.Kernel.impl.store.DataInconsistencyError;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;

	public class ConsistencyCheckingError : DataInconsistencyError
	{
		 private readonly ConsistencySummaryStatistics _summary;

		 public ConsistencyCheckingError( LogEntryStart startEntry, LogEntryCommit commitEntry, ConsistencySummaryStatistics summary ) : base( string.Format( "Inconsistencies in transaction:\n\t{0}\n\t{1}\n\t{2}", startEntry == null ? "NO START ENTRY" : startEntry.ToString(), commitEntry == null ? "NO COMMIT ENTRY" : commitEntry.ToString(), summary ) )
		 {
			  this._summary = summary;
		 }

		 public virtual int GetInconsistencyCountForRecordType( RecordType recordType )
		 {
			  return _summary.getInconsistencyCountForRecordType( recordType );
		 }

		 public virtual int TotalInconsistencyCount
		 {
			 get
			 {
				  return _summary.TotalInconsistencyCount;
			 }
		 }
	}

}