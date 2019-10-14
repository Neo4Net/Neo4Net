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
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;

	public class PropertyKeyTokenRecordCheck : TokenRecordCheck<PropertyKeyTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport>
	{
		 protected internal override RecordReference<DynamicRecord> Name( RecordAccess records, int id )
		 {
			  return records.PropertyKeyName( id );
		 }

		 internal override void NameNotInUse( Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport report, DynamicRecord name )
		 {
			  report.NameBlockNotInUse( name );
		 }

		 internal override void EmptyName( Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport report, DynamicRecord name )
		 {
			  report.EmptyName( name );
		 }
	}

}