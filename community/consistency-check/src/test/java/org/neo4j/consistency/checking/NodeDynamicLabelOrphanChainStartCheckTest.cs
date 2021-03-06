﻿using System.Collections.Generic;

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
	using Test = org.junit.jupiter.api.Test;


	using ConsistencyReport_DynamicLabelConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_DynamicLabelConsistencyReport;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using ReusableRecordsAllocator = Org.Neo4j.Kernel.impl.store.allocator.ReusableRecordsAllocator;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicArrayStore.allocateFromNumbers;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicNodeLabels.dynamicPointer;

	internal class NodeDynamicLabelOrphanChainStartCheckTest : RecordCheckTestBase<DynamicRecord, ConsistencyReport_DynamicLabelConsistencyReport, NodeDynamicLabelOrphanChainStartCheck>
	{

		 internal NodeDynamicLabelOrphanChainStartCheckTest() : base(new NodeDynamicLabelOrphanChainStartCheck(), typeof(ConsistencyReport_DynamicLabelConsistencyReport), new int[0])
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportMissingOwnerId()
		 internal virtual void ShouldReportMissingOwnerId()
		 {
			  // given
			  DynamicRecord record = new DynamicRecord( 0 );
			  InUse( record );
			  allocateFromNumbers( new List<>(), new long[] { }, new ReusableRecordsAllocator(66, record) );

			  // when
			  ConsistencyReport_DynamicLabelConsistencyReport report = Check( record );

			  // then
			  verify( report ).orphanDynamicLabelRecord();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOwningNodeRecordNotInUse()
		 internal virtual void ShouldReportOwningNodeRecordNotInUse()
		 {
			  // given
			  NodeRecord nodeRecord = NotInUse( new NodeRecord( 12L, false, -1, -1 ) );
			  Add( nodeRecord );

			  DynamicRecord nodeDynamicLabelRecord = InUse( new DynamicRecord( 0 ) );
			  allocateFromNumbers( new List<>(), new long[]{ 12L }, new ReusableRecordsAllocator(66, nodeDynamicLabelRecord) );

			  // when
			  ConsistencyReport_DynamicLabelConsistencyReport report = Check( nodeDynamicLabelRecord );

			  // then
			  verify( report ).orphanDynamicLabelRecordDueToInvalidOwner( nodeRecord );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOwningNodeRecordNotPointingBack()
		 internal virtual void ShouldReportOwningNodeRecordNotPointingBack()
		 {
			  // given
			  long nodeId = 12L;

			  ICollection<DynamicRecord> validLabelRecords = new List<DynamicRecord>();
			  DynamicRecord dynamicRecord = InUse( new DynamicRecord( 0 ) );
			  allocateFromNumbers( validLabelRecords, new long[] { nodeId }, new ReusableRecordsAllocator( 66, dynamicRecord ) );

			  ICollection<DynamicRecord> fakePointedToRecords = new List<DynamicRecord>();
			  DynamicRecord dynamicRecord1 = InUse( new DynamicRecord( 1 ) );
			  allocateFromNumbers( fakePointedToRecords, new long[] { nodeId }, new ReusableRecordsAllocator( 66, dynamicRecord1 ) );

			  NodeRecord nodeRecord = InUse( new NodeRecord( nodeId, false, -1, -1 ) );
			  nodeRecord.SetLabelField( dynamicPointer( fakePointedToRecords ), fakePointedToRecords );
			  Add( nodeRecord );

			  // when
			  ConsistencyReport_DynamicLabelConsistencyReport report = Check( Iterators.single( validLabelRecords.GetEnumerator() ) );

			  // then
			  verify( report ).orphanDynamicLabelRecordDueToInvalidOwner( nodeRecord );
		 }
	}

}