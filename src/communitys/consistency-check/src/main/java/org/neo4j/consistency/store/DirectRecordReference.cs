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
namespace Neo4Net.Consistency.store
{
	using Neo4Net.Consistency.report;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;

	public class DirectRecordReference<RECORD> : RecordReference<RECORD> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly RECORD RecordConflict;
		 internal readonly RecordAccess Records;

		 public DirectRecordReference( RECORD record, RecordAccess records )
		 {
			  this.RecordConflict = record;
			  this.Records = records;
		 }

		 public override void Dispatch( PendingReferenceCheck<RECORD> reporter )
		 {
			  reporter.CheckReference( RecordConflict, Records );
		 }

		 public virtual RECORD Record()
		 {
			  return RecordConflict;
		 }
	}

}