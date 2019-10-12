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
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;

	public class PropertyKeyTokenRecordCheck : TokenRecordCheck<PropertyKeyTokenRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport>
	{
		 protected internal override RecordReference<DynamicRecord> Name( RecordAccess records, int id )
		 {
			  return records.PropertyKeyName( id );
		 }

		 internal override void NameNotInUse( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport report, DynamicRecord name )
		 {
			  report.NameBlockNotInUse( name );
		 }

		 internal override void EmptyName( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport report, DynamicRecord name )
		 {
			  report.EmptyName( name );
		 }
	}

}