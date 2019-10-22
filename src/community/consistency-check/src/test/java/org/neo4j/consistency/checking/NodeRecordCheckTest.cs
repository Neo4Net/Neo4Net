using System.Collections.Generic;

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
	using Test = org.junit.jupiter.api.Test;


	using LabelsField = Neo4Net.Consistency.checking.NodeRecordCheck.LabelsField;
	using RelationshipField = Neo4Net.Consistency.checking.NodeRecordCheck.RelationshipField;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using DynamicArrayStore = Neo4Net.Kernel.impl.store.DynamicArrayStore;
	using DynamicNodeLabels = Neo4Net.Kernel.impl.store.DynamicNodeLabels;
	using DynamicRecordAllocator = Neo4Net.Kernel.impl.store.DynamicRecordAllocator;
	using InlineNodeLabels = Neo4Net.Kernel.impl.store.InlineNodeLabels;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using ReusableRecordsAllocator = Neo4Net.Kernel.Impl.Store.Allocators.ReusableRecordsAllocator;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	internal class NodeRecordCheckTest : RecordCheckTestBase<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport, NodeRecordCheck>
	{
		 internal NodeRecordCheckTest() : base(new NodeRecordCheck(RelationshipField.NEXT_REL, LabelsField.LABELS, new PropertyChain<>(from -> null)), typeof(org.Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport), new int[0])
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForNodeNotInUse()
		 internal virtual void ShouldNotReportAnythingForNodeNotInUse()
		 {
			  // given
			  NodeRecord node = NotInUse( new NodeRecord( 42, false, 0, 0 ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForNodeThatDoesNotReferenceOtherRecords()
		 internal virtual void ShouldNotReportAnythingForNodeThatDoesNotReferenceOtherRecords()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForNodeWithConsistentReferences()
		 internal virtual void ShouldNotReportAnythingForNodeWithConsistentReferences()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, 7, 11 ) );
			  Add( InUse( new RelationshipRecord( 7, 42, 0, 0 ) ) );
			  Add( InUse( new PropertyRecord( 11 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportRelationshipNotInUse()
		 internal virtual void ShouldReportRelationshipNotInUse()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, 7, 11 ) );
			  RelationshipRecord relationship = Add( NotInUse( new RelationshipRecord( 7, 0, 0, 0 ) ) );
			  Add( InUse( new PropertyRecord( 11 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).relationshipNotInUse( relationship );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPropertyNotInUse()
		 internal virtual void ShouldReportPropertyNotInUse()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, 11 ) );
			  PropertyRecord property = Add( NotInUse( new PropertyRecord( 11 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).propertyNotInUse( property );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPropertyNotFirstInChain()
		 internal virtual void ShouldReportPropertyNotFirstInChain()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, 11 ) );
			  PropertyRecord property = Add( InUse( new PropertyRecord( 11 ) ) );
			  property.PrevProp = 6;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).propertyNotFirstInChain( property );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportRelationshipForOtherNodes()
		 internal virtual void ShouldReportRelationshipForOtherNodes()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, 7, NONE ) );
			  RelationshipRecord relationship = Add( InUse( new RelationshipRecord( 7, 1, 2, 0 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).relationshipForOtherNode( relationship );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportRelationshipNotFirstInSourceChain()
		 internal virtual void ShouldReportRelationshipNotFirstInSourceChain()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, 7, NONE ) );
			  RelationshipRecord relationship = Add( InUse( new RelationshipRecord( 7, 42, 0, 0 ) ) );
			  relationship.FirstPrevRel = 6;
			  relationship.FirstInFirstChain = false;
			  relationship.SecondPrevRel = 8;
			  relationship.FirstInSecondChain = false;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).relationshipNotFirstInSourceChain( relationship );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportRelationshipNotFirstInTargetChain()
		 internal virtual void ShouldReportRelationshipNotFirstInTargetChain()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, 7, NONE ) );
			  RelationshipRecord relationship = Add( InUse( new RelationshipRecord( 7, 0, 42, 0 ) ) );
			  relationship.FirstPrevRel = 6;
			  relationship.FirstInFirstChain = false;
			  relationship.SecondPrevRel = 8;
			  relationship.FirstInSecondChain = false;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).relationshipNotFirstInTargetChain( relationship );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportLoopRelationshipNotFirstInTargetAndSourceChains()
		 internal virtual void ShouldReportLoopRelationshipNotFirstInTargetAndSourceChains()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, 7, NONE ) );
			  RelationshipRecord relationship = Add( InUse( new RelationshipRecord( 7, 42, 42, 0 ) ) );
			  relationship.FirstPrevRel = 8;
			  relationship.FirstInFirstChain = false;
			  relationship.SecondPrevRel = 8;
			  relationship.FirstInSecondChain = false;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).relationshipNotFirstInSourceChain( relationship );
			  verify( report ).relationshipNotFirstInTargetChain( relationship );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportLabelNotInUse()
		 internal virtual void ShouldReportLabelNotInUse()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );
			  ( new InlineNodeLabels( node ) ).add( 1, null, null );
			  LabelTokenRecord labelRecordNotInUse = NotInUse( new LabelTokenRecord( 1 ) );

			  Add( labelRecordNotInUse );
			  Add( node );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).labelNotInUse( labelRecordNotInUse );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicLabelsNotInUse()
		 internal virtual void ShouldReportDynamicLabelsNotInUse()
		 {
			  // given
			  long[] labelIds = CreateLabels( 100 );

			  LabelTokenRecord labelRecordNotInUse = NotInUse( new LabelTokenRecord( labelIds.Length ) );
			  Add( labelRecordNotInUse );

			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );
			  Add( node );

			  DynamicRecord labelsRecord1 = InUse( Array( new DynamicRecord( 1 ) ) );
			  DynamicRecord labelsRecord2 = InUse( Array( new DynamicRecord( 2 ) ) );
			  ICollection<DynamicRecord> labelRecords = asList( labelsRecord1, labelsRecord2 );

			  labelIds[12] = labelIds.Length;
			  DynamicArrayStore.allocateFromNumbers( new List<DynamicRecord>(), labelIds, new ReusableRecordsAllocator(52, labelRecords) );
			  AssertDynamicRecordChain( labelsRecord1, labelsRecord2 );
			  node.SetLabelField( DynamicNodeLabels.dynamicPointer( labelRecords ), labelRecords );

			  AddNodeDynamicLabels( labelsRecord1 );
			  AddNodeDynamicLabels( labelsRecord2 );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).labelNotInUse( labelRecordNotInUse );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDuplicateLabels()
		 internal virtual void ShouldReportDuplicateLabels()
		 {
			  // given
			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );
			  ( new InlineNodeLabels( node ) ).put( new long[]{ 1, 2, 1 }, null, null );
			  LabelTokenRecord label1 = InUse( new LabelTokenRecord( 1 ) );
			  LabelTokenRecord label2 = InUse( new LabelTokenRecord( 2 ) );

			  Add( label1 );
			  Add( label2 );
			  Add( node );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).labelDuplicate( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDuplicateDynamicLabels()
		 internal virtual void ShouldReportDuplicateDynamicLabels()
		 {
			  // given
			  long[] labelIds = CreateLabels( 100 );

			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );
			  Add( node );

			  DynamicRecord labelsRecord1 = InUse( Array( new DynamicRecord( 1 ) ) );
			  DynamicRecord labelsRecord2 = InUse( Array( new DynamicRecord( 2 ) ) );
			  ICollection<DynamicRecord> labelRecords = asList( labelsRecord1, labelsRecord2 );

			  labelIds[12] = 11;
			  DynamicArrayStore.allocateFromNumbers( new List<DynamicRecord>(), labelIds, new ReusableRecordsAllocator(52, labelRecords) );
			  AssertDynamicRecordChain( labelsRecord1, labelsRecord2 );
			  node.SetLabelField( DynamicNodeLabels.dynamicPointer( labelRecords ), labelRecords );

			  AddNodeDynamicLabels( labelsRecord1 );
			  AddNodeDynamicLabels( labelsRecord2 );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).labelDuplicate( 11 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOutOfOrderLabels()
		 internal virtual void ShouldReportOutOfOrderLabels()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord node = inUse(new org.Neo4Net.kernel.impl.store.record.NodeRecord(42, false, NONE, NONE));
			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );
			  // We need to do this override so we can put the labels unsorted, since InlineNodeLabels always sorts on insert
			  new InlineNodeLabelsAnonymousInnerClass( this, node )
			  .put( new long[]{ 3, 1, 2 }, null, null );
			  LabelTokenRecord label1 = InUse( new LabelTokenRecord( 1 ) );
			  LabelTokenRecord label2 = InUse( new LabelTokenRecord( 2 ) );
			  LabelTokenRecord label3 = InUse( new LabelTokenRecord( 3 ) );

			  Add( label1 );
			  Add( label2 );
			  Add( label3 );
			  Add( node );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).labelsOutOfOrder( 3, 1 );
		 }

		 private class InlineNodeLabelsAnonymousInnerClass : InlineNodeLabels
		 {
			 private readonly NodeRecordCheckTest _outerInstance;

			 private NodeRecord _node;

			 public InlineNodeLabelsAnonymousInnerClass( NodeRecordCheckTest outerInstance, NodeRecord node ) : base( node )
			 {
				 this.outerInstance = outerInstance;
				 this._node = node;
			 }

			 public override ICollection<DynamicRecord> put( long[] labelIds, NodeStore nodeStore, DynamicRecordAllocator allocator )
			 {
				  return putSorted( _node, labelIds, nodeStore, allocator );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProperlyReportOutOfOrderLabelsThatAreFarAway()
		 internal virtual void ShouldProperlyReportOutOfOrderLabelsThatAreFarAway()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord node = inUse(new org.Neo4Net.kernel.impl.store.record.NodeRecord(42, false, NONE, NONE));
			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );
			  // We need to do this override so we can put the labels unsorted, since InlineNodeLabels always sorts on insert
			  new InlineNodeLabelsAnonymousInnerClass2( this, node )
			  .put( new long[]{ 1, 18, 13, 14, 15, 16, 12 }, null, null );
			  LabelTokenRecord label1 = InUse( new LabelTokenRecord( 1 ) );
			  LabelTokenRecord label12 = InUse( new LabelTokenRecord( 12 ) );
			  LabelTokenRecord label13 = InUse( new LabelTokenRecord( 13 ) );
			  LabelTokenRecord label14 = InUse( new LabelTokenRecord( 14 ) );
			  LabelTokenRecord label15 = InUse( new LabelTokenRecord( 15 ) );
			  LabelTokenRecord label16 = InUse( new LabelTokenRecord( 16 ) );
			  LabelTokenRecord label18 = InUse( new LabelTokenRecord( 18 ) );

			  Add( label1 );
			  Add( label12 );
			  Add( label13 );
			  Add( label14 );
			  Add( label15 );
			  Add( label16 );
			  Add( label18 );
			  Add( node );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).labelsOutOfOrder( 18, 13 );
			  verify( report ).labelsOutOfOrder( 16, 12 );
		 }

		 private class InlineNodeLabelsAnonymousInnerClass2 : InlineNodeLabels
		 {
			 private readonly NodeRecordCheckTest _outerInstance;

			 private NodeRecord _node;

			 public InlineNodeLabelsAnonymousInnerClass2( NodeRecordCheckTest outerInstance, NodeRecord node ) : base( node )
			 {
				 this.outerInstance = outerInstance;
				 this._node = node;
			 }

			 public override ICollection<DynamicRecord> put( long[] labelIds, NodeStore nodeStore, DynamicRecordAllocator allocator )
			 {
				  return putSorted( _node, labelIds, nodeStore, allocator );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOutOfOrderDynamicLabels()
		 internal virtual void ShouldReportOutOfOrderDynamicLabels()
		 {
			  // given
			  long[] labelIds = CreateLabels( 100 );

			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );
			  Add( node );

			  DynamicRecord labelsRecord1 = InUse( Array( new DynamicRecord( 1 ) ) );
			  DynamicRecord labelsRecord2 = InUse( Array( new DynamicRecord( 2 ) ) );
			  ICollection<DynamicRecord> labelRecords = asList( labelsRecord1, labelsRecord2 );

			  long temp = labelIds[12];
			  labelIds[12] = labelIds[11];
			  labelIds[11] = temp;
			  DynamicArrayStore.allocateFromNumbers( new List<DynamicRecord>(), labelIds, new ReusableRecordsAllocator(52, labelRecords) );
			  AssertDynamicRecordChain( labelsRecord1, labelsRecord2 );
			  node.SetLabelField( DynamicNodeLabels.dynamicPointer( labelRecords ), labelRecords );

			  AddNodeDynamicLabels( labelsRecord1 );
			  AddNodeDynamicLabels( labelsRecord2 );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).labelsOutOfOrder( labelIds[11], labelIds[12] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDynamicLabelRecordsNotInUse()
		 internal virtual void ShouldDynamicLabelRecordsNotInUse()
		 {
			  // given
			  long[] labelIds = CreateLabels( 100 );

			  NodeRecord node = InUse( new NodeRecord( 42, false, NONE, NONE ) );
			  Add( node );

			  DynamicRecord labelsRecord1 = NotInUse( Array( new DynamicRecord( 1 ) ) );
			  DynamicRecord labelsRecord2 = NotInUse( Array( new DynamicRecord( 2 ) ) );
			  ICollection<DynamicRecord> labelRecords = asList( labelsRecord1, labelsRecord2 );

			  DynamicArrayStore.allocateFromNumbers( new List<DynamicRecord>(), labelIds, new NotUsedReusableRecordsAllocator(this, 52, labelRecords) );
			  AssertDynamicRecordChain( labelsRecord1, labelsRecord2 );
			  node.SetLabelField( DynamicNodeLabels.dynamicPointer( labelRecords ), labelRecords );

			  AddNodeDynamicLabels( labelsRecord1 );
			  AddNodeDynamicLabels( labelsRecord2 );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report = Check( node );

			  // then
			  verify( report ).dynamicLabelRecordNotInUse( labelsRecord1 );
			  verify( report ).dynamicLabelRecordNotInUse( labelsRecord2 );
		 }

		 private long[] CreateLabels( int labelCount )
		 {
			  long[] labelIds = new long[labelCount];
			  for ( int i = 0; i < labelIds.Length; i++ )
			  {
					labelIds[i] = i;
					Add( InUse( new LabelTokenRecord( i ) ) );
			  }
			  return labelIds;
		 }

		 private static void AssertDynamicRecordChain( params DynamicRecord[] records )
		 {
			  if ( records.Length > 0 )
			  {
					for ( int i = 1; i < records.Length; i++ )
					{
						 assertEquals( records[i].Id, records[i - 1].NextBlock );
					}
					assertTrue( Record.NO_NEXT_BLOCK.@is( records[records.Length - 1].NextBlock ) );
			  }
		 }

		 private class NotUsedReusableRecordsAllocator : ReusableRecordsAllocator
		 {
			 private readonly NodeRecordCheckTest _outerInstance;


			  internal NotUsedReusableRecordsAllocator( NodeRecordCheckTest outerInstance, int recordSize, ICollection<DynamicRecord> records ) : base( recordSize, records )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override DynamicRecord NextRecord()
			  {
					DynamicRecord record = base.NextRecord();
					record.InUse = false;
					return record;
			  }
		 }

	}

}