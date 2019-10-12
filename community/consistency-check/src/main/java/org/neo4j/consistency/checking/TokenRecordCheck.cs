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
	using Org.Neo4j.Consistency.store;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using TokenRecord = Org.Neo4j.Kernel.impl.store.record.TokenRecord;

	internal abstract class TokenRecordCheck<RECORD, REPORT> : RecordCheck<RECORD, REPORT>, ComparativeRecordChecker<RECORD, DynamicRecord, REPORT> where RECORD : Org.Neo4j.Kernel.impl.store.record.TokenRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport
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