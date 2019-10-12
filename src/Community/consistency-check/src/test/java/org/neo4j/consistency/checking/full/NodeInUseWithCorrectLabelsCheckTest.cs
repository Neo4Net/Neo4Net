using System.Collections.Generic;

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
namespace Neo4Net.Consistency.checking.full
{
	using Test = org.junit.jupiter.api.Test;


	using Neo4Net.Consistency.checking;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccessStub = Neo4Net.Consistency.store.RecordAccessStub;
	using LabelScanDocument = Neo4Net.Consistency.store.synthetic.LabelScanDocument;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using DynamicArrayStore = Neo4Net.Kernel.impl.store.DynamicArrayStore;
	using InlineNodeLabels = Neo4Net.Kernel.impl.store.InlineNodeLabels;
	using ReusableRecordsAllocator = Neo4Net.Kernel.impl.store.allocator.ReusableRecordsAllocator;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.RecordCheckTestBase.inUse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.RecordCheckTestBase.notInUse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.schema.SchemaDescriptor_PropertySchemaType.COMPLETE_ALL_TOKENS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicNodeLabels.dynamicPointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.LabelIdArray.prependNodeId;

	internal class NodeInUseWithCorrectLabelsCheckTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeNotInUse()
		 internal virtual void ShouldReportNodeNotInUse()
		 {
			  // given
			  int nodeId = 42;
			  long labelId = 7;

			  Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ) );
			  NodeRecord node = notInUse( new NodeRecord( nodeId, false, 0, 0 ) );

			  // when
			  Checker( new long[]{ labelId }, true ).checkReference( null, node, EngineFor( report ), null );

			  // then
			  verify( report ).nodeNotInUse( node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeWithoutExpectedLabelWhenLabelsAreInlineBothDirections()
		 internal virtual void ShouldReportNodeWithoutExpectedLabelWhenLabelsAreInlineBothDirections()
		 {
			  // given
			  int nodeId = 42;
			  long[] storeLabelIds = new long[] { 7, 9 };
			  long[] indexLabelIds = new long[] { 9, 10 };

			  NodeRecord node = inUse( WithInlineLabels( new NodeRecord( nodeId, false, 0, 0 ), storeLabelIds ) );

			  Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ) );

			  // when
			  Checker( indexLabelIds, true ).checkReference( null, node, EngineFor( report ), null );

			  // then
			  verify( report ).nodeDoesNotHaveExpectedLabel( node, 10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeWithoutExpectedLabelWhenLabelsAreInlineIndexToStore()
		 internal virtual void ShouldReportNodeWithoutExpectedLabelWhenLabelsAreInlineIndexToStore()
		 {
			  // given
			  int nodeId = 42;
			  long[] storeLabelIds = new long[] { 7, 9 };
			  long[] indexLabelIds = new long[] { 9, 10 };

			  NodeRecord node = inUse( WithInlineLabels( new NodeRecord( nodeId, false, 0, 0 ), storeLabelIds ) );

			  Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ) );

			  // when
			  Checker( indexLabelIds, false ).checkReference( null, node, EngineFor( report ), null );

			  // then
			  verify( report ).nodeDoesNotHaveExpectedLabel( node, 10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeWithoutExpectedLabelWhenLabelsAreDynamicBothDirections()
		 internal virtual void ShouldReportNodeWithoutExpectedLabelWhenLabelsAreDynamicBothDirections()
		 {
			  // given
			  int nodeId = 42;
			  long[] indexLabelIds = new long[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			  long[] storeLabelIds = new long[] { 1, 2, 3, 4, 5, 6, 8, 9, 10, 11 };

			  RecordAccessStub recordAccess = new RecordAccessStub();
			  NodeRecord node = inUse( WithDynamicLabels( recordAccess, new NodeRecord( nodeId, false, 0, 0 ), storeLabelIds ) );

			  Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ) );

			  // when
			  CheckerEngine<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport> engine = recordAccess.Engine( null, report );
			  Checker( indexLabelIds, true ).checkReference( null, node, engine, recordAccess );
			  recordAccess.CheckDeferred();

			  // then
			  verify( report ).nodeDoesNotHaveExpectedLabel( node, 7 );
			  verify( report ).nodeLabelNotInIndex( node, 11 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeWithoutExpectedLabelWhenLabelsAreDynamicIndexToStore()
		 internal virtual void ShouldReportNodeWithoutExpectedLabelWhenLabelsAreDynamicIndexToStore()
		 {
			  // given
			  int nodeId = 42;
			  long[] indexLabelIds = new long[] { 3, 7, 9, 10 };
			  long[] storeLabelIds = new long[] { 1, 2, 3, 4, 5, 6, 8, 9, 10 };
			  long missingLabelId = 7;

			  RecordAccessStub recordAccess = new RecordAccessStub();
			  NodeRecord node = inUse( WithDynamicLabels( recordAccess, new NodeRecord( nodeId, false, 0, 0 ), storeLabelIds ) );

			  Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ) );

			  // when
			  CheckerEngine<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport> engine = recordAccess.Engine( null, report );
			  Checker( indexLabelIds, true ).checkReference( null, node, engine, recordAccess );
			  recordAccess.CheckDeferred();

			  // then
			  verify( report ).nodeDoesNotHaveExpectedLabel( node, missingLabelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void reportNodeWithoutLabelsWhenLabelsAreInlined()
		 internal virtual void ReportNodeWithoutLabelsWhenLabelsAreInlined()
		 {
			  int nodeId = 42;
			  long[] indexLabelIds = new long[] { 3 };
			  long[] storeLabelIds = new long[] {};
			  long missingLabelId = 3;

			  RecordAccessStub recordAccess = new RecordAccessStub();
			  NodeRecord node = inUse( WithInlineLabels( new NodeRecord( nodeId, false, 0, 0 ), storeLabelIds ) );

			  Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ) );

			  // when
			  CheckerEngine<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport> engine = recordAccess.Engine( null, report );
			  Checker( indexLabelIds, true ).checkReference( null, node, engine, recordAccess );
			  recordAccess.CheckDeferred();

			  // then
			  verify( report ).nodeDoesNotHaveExpectedLabel( node, missingLabelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void reportNodeWithoutLabelsWhenLabelsAreDynamic()
		 internal virtual void ReportNodeWithoutLabelsWhenLabelsAreDynamic()
		 {
			  int nodeId = 42;
			  long[] indexLabelIds = new long[] { 3, 7, 9, 10 };
			  long[] storeLabelIds = new long[] {};
			  long[] missingLabelIds = new long[] { 3, 7, 9, 10 };

			  RecordAccessStub recordAccess = new RecordAccessStub();
			  NodeRecord node = inUse( WithDynamicLabels( recordAccess, new NodeRecord( nodeId, false, 0, 0 ), storeLabelIds ) );

			  Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ) );

			  // when
			  CheckerEngine<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport> engine = recordAccess.Engine( null, report );
			  Checker( indexLabelIds, true ).checkReference( null, node, engine, recordAccess );
			  recordAccess.CheckDeferred();

			  // then
			  foreach ( long missingLabelId in missingLabelIds )
			  {
					verify( report ).nodeDoesNotHaveExpectedLabel( node, missingLabelId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRemainSilentWhenEverythingIsInOrder()
		 internal virtual void ShouldRemainSilentWhenEverythingIsInOrder()
		 {
			  // given
			  int nodeId = 42;
			  int labelId = 7;

			  NodeRecord node = WithInlineLabels( inUse( new NodeRecord( nodeId, false, 0, 0 ) ), labelId );

			  Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport ) );

			  // when
			  Checker( new long[]{ labelId }, true ).checkReference( null, node, EngineFor( report ), null );

			  // then
			  verifyNoMoreInteractions( report );
		 }

		 private static NodeRecord WithInlineLabels( NodeRecord nodeRecord, params long[] labelIds )
		 {
			  ( new InlineNodeLabels( nodeRecord ) ).put( labelIds, null, null );
			  return nodeRecord;
		 }

		 private static NodeRecord WithDynamicLabels( RecordAccessStub recordAccess, NodeRecord nodeRecord, params long[] labelIds )
		 {
			  IList<DynamicRecord> preAllocatedRecords = new List<DynamicRecord>();
			  for ( int i = 0; i < 10; i++ )
			  {
					preAllocatedRecords.Add( inUse( new DynamicRecord( i ) ) );
			  }
			  ICollection<DynamicRecord> dynamicRecords = new List<DynamicRecord>();
			  DynamicArrayStore.allocateFromNumbers( dynamicRecords, prependNodeId( nodeRecord.Id, labelIds ), new ReusableRecordsAllocator( 4, preAllocatedRecords ) );
			  foreach ( DynamicRecord dynamicRecord in dynamicRecords )
			  {
					recordAccess.AddNodeDynamicLabels( dynamicRecord );
			  }

			  nodeRecord.SetLabelField( dynamicPointer( dynamicRecords ), dynamicRecords );
			  return nodeRecord;
		 }

		 private static Engine EngineFor( Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport report )
		 {
			  Engine engine = mock( typeof( Engine ) );
			  when( engine.report() ).thenReturn(report);
			  return engine;
		 }

		 private static NodeInUseWithCorrectLabelsCheck<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport> Checker( long[] expectedLabels, bool checkStoreToIndex )
		 {
			  return new NodeInUseWithCorrectLabelsCheck<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport>( expectedLabels, COMPLETE_ALL_TOKENS, checkStoreToIndex );
		 }

		 internal interface Engine : CheckerEngine<LabelScanDocument, Neo4Net.Consistency.report.ConsistencyReport_LabelScanConsistencyReport>
		 {
		 }
	}

}