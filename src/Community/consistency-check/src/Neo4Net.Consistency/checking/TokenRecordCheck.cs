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
namespace Neo4Net.Consistency.checking
{
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.Store.RecordAccess;
	using Neo4Net.Consistency.Store;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;

	internal abstract class TokenRecordCheck<RECORD, REPORT> : RecordCheck<RECORD, REPORT>, ComparativeRecordChecker<RECORD, DynamicRecord, REPORT> where RECORD : Neo4Net.Kernel.Impl.Store.Records.TokenRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport
	{
		public abstract void CheckReference( RECORD record, REFERRED referred, CheckerEngine<RECORD, REPORT> engine, RecordAccess records );
		 public override void Check( RECORD record, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  if ( !record.inUse() )
			  {
					return;
			  }
			  if ( !Record.NO_NEXT_BLOCK.@is( record.NameId ) )
			  {
					engine.ComparativeCheck( Name( records, record.NameId ), this );
			  }
		 }

		 public override void CheckReference( RECORD record, DynamicRecord name, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  if ( !name.InUse() )
			  {
					NameNotInUse( engine.Report(), name );
			  }
			  else
			  {
					if ( name.Length <= 0 )
					{
						 EmptyName( engine.Report(), name );
					}
			  }
		 }

		 internal abstract RecordReference<DynamicRecord> Name( RecordAccess records, int id );

		 internal abstract void NameNotInUse( REPORT report, DynamicRecord name );

		 internal abstract void EmptyName( REPORT report, DynamicRecord name );
	}

}