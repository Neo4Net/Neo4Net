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
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

	public abstract class PrimitiveRecordCheck <RECORD, REPORT> : OwningRecordCheck<RECORD, REPORT> where RECORD : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
	{
		 private readonly RecordField<RECORD, REPORT>[] _fields;
		 private readonly ComparativeRecordChecker<RECORD, PrimitiveRecord, REPORT> _ownerCheck = ( record, other, engine, records ) =>
		 {
					 if ( record.Id == other.Id && record.GetType() == other.GetType() )
					 {
						  // Owner identities match. Things are as they should be.
						  return;
					 }

					 if ( other is NodeRecord )
					 {
						  engine.report().multipleOwners((NodeRecord) other);
					 }
					 else if ( other is RelationshipRecord )
					 {
						  engine.report().multipleOwners((RelationshipRecord) other);
					 }
					 else if ( other is NeoStoreRecord )
					 {
						  engine.report().multipleOwners((NeoStoreRecord) other);
					 }
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs PrimitiveRecordCheck(RecordField<RECORD,REPORT>... fields)
		 internal PrimitiveRecordCheck( params RecordField<RECORD, REPORT>[] fields )
		 {
			  this._fields = Arrays.copyOf( fields, fields.Length );
		 }

		 public override void Check( RECORD record, CheckerEngine<RECORD, REPORT> engine, RecordAccess records )
		 {
			  if ( !record.inUse() )
			  {
					return;
			  }
			  foreach ( RecordField<RECORD, REPORT> field in _fields )
			  {
					field.CheckConsistency( record, engine, records );
			  }
		 }

		 public override ComparativeRecordChecker<RECORD, PrimitiveRecord, REPORT> OwnerCheck()
		 {
			  return _ownerCheck;
		 }
	}

}