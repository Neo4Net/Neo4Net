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
namespace Org.Neo4j.Consistency.checking
{
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using Org.Neo4j.Kernel.impl.store;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;

	public class DynamicRecordCheck : RecordCheck<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_DynamicConsistencyReport>, ComparativeRecordChecker<DynamicRecord, DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_DynamicConsistencyReport>
	{
		 private readonly int _blockSize;
		 private readonly DynamicStore _dereference;

		 public DynamicRecordCheck( RecordStore<DynamicRecord> store, DynamicStore dereference )
		 {
			  this._blockSize = store.RecordDataSize;
			  this._dereference = dereference;
		 }

		 public override void Check( DynamicRecord record, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_DynamicConsistencyReport> engine, RecordAccess records )
		 {
			  if ( !record.InUse() )
			  {
					return;
			  }
			  if ( record.Length == 0 )
			  {
					engine.Report().emptyBlock();
			  }
			  else if ( record.Length < 0 )
			  {
					engine.Report().invalidLength();
			  }
			  if ( !Record.NO_NEXT_BLOCK.@is( record.NextBlock ) )
			  {
					if ( record.NextBlock == record.Id )
					{
						 engine.Report().selfReferentialNext();
					}
					else
					{
						 engine.ComparativeCheck( _dereference.lookup( records, record.NextBlock ), this );
					}
					if ( record.Length < _blockSize )
					{
						 engine.Report().recordNotFullReferencesNext();
					}
			  }
		 }

		 public override void CheckReference( DynamicRecord record, DynamicRecord next, CheckerEngine<DynamicRecord, Org.Neo4j.Consistency.report.ConsistencyReport_DynamicConsistencyReport> engine, RecordAccess records )
		 {
			  if ( !next.InUse() )
			  {
					engine.Report().nextNotInUse(next);
			  }
			  else
			  {
					if ( next.Length <= 0 )
					{
						 engine.Report().emptyNextBlock(next);
					}
			  }
		 }
	}

}