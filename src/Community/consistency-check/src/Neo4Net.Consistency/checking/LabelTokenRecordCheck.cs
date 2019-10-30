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
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;

	public class LabelTokenRecordCheck : TokenRecordCheck<LabelTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport>
	{
		 protected internal override RecordReference<DynamicRecord> Name( RecordAccess records, int id )
		 {
			  return records.LabelName( id );
		 }

		 internal override void NameNotInUse( Neo4Net.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport report, DynamicRecord name )
		 {
			  report.NameBlockNotInUse( name );
		 }

		 internal override void EmptyName( Neo4Net.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport report, DynamicRecord name )
		 {
			  report.EmptyName( name );
		 }
	}

}