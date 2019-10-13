﻿/*
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
namespace Neo4Net.Consistency.checking
{
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReport_NeoStoreConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport;
	using NeoStoreRecord = Neo4Net.Kernel.impl.store.record.NeoStoreRecord;

	internal class NeoStoreCheck : PrimitiveRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs NeoStoreCheck(RecordField<org.neo4j.kernel.impl.store.record.NeoStoreRecord,org.neo4j.consistency.report.ConsistencyReport_NeoStoreConsistencyReport>...fields)
		 internal NeoStoreCheck( params RecordField<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport>[] fields ) : base( fields )
		 {
		 }
		 // nothing added over PrimitiveRecordCheck
	}

}