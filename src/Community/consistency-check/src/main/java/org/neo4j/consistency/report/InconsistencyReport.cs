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
namespace Neo4Net.Consistency.report
{
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;

	public class InconsistencyReport : InconsistencyLogger
	{
		 private readonly InconsistencyLogger _logger;
		 private readonly ConsistencySummaryStatistics _summary;

		 public InconsistencyReport( InconsistencyLogger logger, ConsistencySummaryStatistics summary )
		 {
			  this._logger = logger;
			  this._summary = summary;
		 }

		 public override void Error( RecordType recordType, AbstractBaseRecord record, string message, object[] args )
		 {
			  _logger.error( recordType, record, message, args );
		 }

		 public override void Error( RecordType recordType, AbstractBaseRecord oldRecord, AbstractBaseRecord newRecord, string message, object[] args )
		 {
			  _logger.error( recordType, oldRecord, newRecord, message, args );
		 }

		 public override void Error( string message )
		 {
			  _logger.error( message );
		 }

		 public override void Warning( RecordType recordType, AbstractBaseRecord record, string message, object[] args )
		 {
			  _logger.warning( recordType, record, message, args );
		 }

		 public override void Warning( RecordType recordType, AbstractBaseRecord oldRecord, AbstractBaseRecord newRecord, string message, object[] args )
		 {
			  _logger.warning( recordType, oldRecord, newRecord, message, args );
		 }

		 public override void Warning( string message )
		 {
			  _logger.warning( message );
		 }

		 internal virtual void UpdateSummary( RecordType type, int errors, int warnings )
		 {
			  _summary.update( type, errors, warnings );
		 }
	}

}