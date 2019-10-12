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

	using RelationshipField = Org.Neo4j.Consistency.checking.RelationshipRecordCheck.RelationshipField;
	using RelationshipTypeField = Org.Neo4j.Consistency.checking.RelationshipRecordCheck.RelationshipTypeField;
	using CheckStage = Org.Neo4j.Consistency.checking.full.CheckStage;
	using MultiPassStore = Org.Neo4j.Consistency.checking.full.MultiPassStore;
	using ConsistencyReport_RelationshipConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.MultiPassStore.RELATIONSHIPS;

	internal class RelationshipRecordCheckTest : RecordCheckTestBase<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport, RelationshipRecordCheck>
	{
		 private bool _checkSingleDirection;

		 internal RelationshipRecordCheckTest() : base(new RelationshipRecordCheck(RelationshipTypeField.RELATIONSHIP_TYPE, NodeField.Source, RelationshipField.SOURCE_PREV, RelationshipField.SOURCE_NEXT, NodeField.Target, RelationshipField.TARGET_PREV, RelationshipField.TARGET_NEXT, new PropertyChain<>(from -> null)), typeof(ConsistencyReport_RelationshipConsistencyReport), CheckStage.Stage6_RS_Forward.CacheSlotSizes, MultiPassStore.RELATIONSHIPS)
		 {
		 }

		 private void CheckSingleDirection()
		 {
			  this._checkSingleDirection = true;
		 }

		 internal override ConsistencyReport_RelationshipConsistencyReport Check( RelationshipRecord record )
		 {
			  // Make sure the cache is properly populated
			  Records.populateCache();
			  ConsistencyReport_RelationshipConsistencyReport report = mock( typeof( ConsistencyReport_RelationshipConsistencyReport ) );
			  Records.cacheAccess().CacheSlotSizes = CheckStage.Stage6_RS_Forward.CacheSlotSizes;
			  base.Check( report, record );
			  if ( !_checkSingleDirection )
			  {
					Records.cacheAccess().Forward = !Records.cacheAccess().Forward;
					base.Check( report, record );
			  }
			  return report;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRelationshipNotInUse()
		 internal virtual void ShouldNotReportAnythingForRelationshipNotInUse()
		 {
			  // given
			  RelationshipRecord relationship = NotInUse( new RelationshipRecord( 42, 0, 0, 0 ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRelationshipThatDoesNotReferenceOtherRecords()
		 internal virtual void ShouldNotReportAnythingForRelationshipThatDoesNotReferenceOtherRecords()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRelationshipWithConsistentReferences()
		 internal virtual void ShouldNotReportAnythingForRelationshipWithConsistentReferences()
		 {
			  // given
			  /*
			   * (1) --> (3) <==> (2)
			   */
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, relationship.Id, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 53, NONE ) ) );
			  Add( InUse( new NodeRecord( 3, false, NONE, NONE ) ) );
			  Add( InUse( new PropertyRecord( 101 ) ) );
			  relationship.NextProp = 101;
			  RelationshipRecord sNext = Add( InUse( new RelationshipRecord( 51, 1, 3, 4 ) ) );
			  RelationshipRecord tNext = Add( InUse( new RelationshipRecord( 52, 2, 3, 4 ) ) );
			  RelationshipRecord tPrev = Add( InUse( new RelationshipRecord( 53, 3, 2, 4 ) ) );

			  relationship.FirstNextRel = sNext.Id;
			  sNext.FirstPrevRel = relationship.Id;
			  sNext.FirstInFirstChain = false;
			  relationship.SecondNextRel = tNext.Id;
			  tNext.FirstPrevRel = relationship.Id;
			  tNext.FirstInFirstChain = false;
			  relationship.SecondPrevRel = tPrev.Id;
			  relationship.FirstInSecondChain = false;
			  tPrev.SecondNextRel = relationship.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportIllegalRelationshipType()
		 internal virtual void ShouldReportIllegalRelationshipType()
		 {
			  // given
			  CheckSingleDirection();
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, NONE ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).illegalRelationshipType();
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportRelationshipTypeNotInUse()
		 internal virtual void ShouldReportRelationshipTypeNotInUse()
		 {
			  // given
			  CheckSingleDirection();
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  RelationshipTypeTokenRecord relationshipType = Add( NotInUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).relationshipTypeNotInUse( relationshipType );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportIllegalSourceNode()
		 internal virtual void ShouldReportIllegalSourceNode()
		 {
			  // given
			  CheckSingleDirection();
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, NONE, 1, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).illegalSourceNode();
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourceNodeNotInUse()
		 internal virtual void ShouldReportSourceNodeNotInUse()
		 {
			  // given
			  CheckSingleDirection();
			  Initialize( RELATIONSHIPS, NODES );
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  NodeRecord node = Add( NotInUse( new NodeRecord( 1, false, NONE, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourceNodeNotInUse( node );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportIllegalTargetNode()
		 internal virtual void ShouldReportIllegalTargetNode()
		 {
			  // given
			  CheckSingleDirection();
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, NONE, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).illegalTargetNode();
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetNodeNotInUse()
		 internal virtual void ShouldReportTargetNodeNotInUse()
		 {
			  // given
			  CheckSingleDirection();
			  Initialize( RELATIONSHIPS, NODES );
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  NodeRecord node = Add( NotInUse( new NodeRecord( 2, false, NONE, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetNodeNotInUse( node );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPropertyNotInUse()
		 internal virtual void ShouldReportPropertyNotInUse()
		 {
			  // given
			  CheckSingleDirection();
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  relationship.NextProp = 11;
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  PropertyRecord property = Add( NotInUse( new PropertyRecord( 11 ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).propertyNotInUse( property );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportPropertyNotFirstInChain()
		 internal virtual void ShouldReportPropertyNotFirstInChain()
		 {
			  // given
			  CheckSingleDirection();
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  relationship.NextProp = 11;
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  PropertyRecord property = Add( InUse( new PropertyRecord( 11 ) ) );
			  property.PrevProp = 6;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).propertyNotFirstInChain( property );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourceNodeNotReferencingBackForFirstRelationshipInSourceChain()
		 internal virtual void ShouldReportSourceNodeNotReferencingBackForFirstRelationshipInSourceChain()
		 {
			  // given
			  CheckSingleDirection();
			  Initialize( RELATIONSHIPS, NODES );
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  NodeRecord source = Add( InUse( new NodeRecord( 1, false, 7, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourceNodeDoesNotReferenceBack( source );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetNodeNotReferencingBackForFirstRelationshipInTargetChain()
		 internal virtual void ShouldReportTargetNodeNotReferencingBackForFirstRelationshipInTargetChain()
		 {
			  // given
			  CheckSingleDirection();
			  Initialize( RELATIONSHIPS, NODES );
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  NodeRecord target = Add( InUse( new NodeRecord( 2, false, 7, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetNodeDoesNotReferenceBack( target );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourceAndTargetNodeNotReferencingBackForFirstRelationshipInChains()
		 internal virtual void ShouldReportSourceAndTargetNodeNotReferencingBackForFirstRelationshipInChains()
		 {
			  // given
			  CheckSingleDirection();
			  Initialize( RELATIONSHIPS, NODES );
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  NodeRecord source = Add( InUse( new NodeRecord( 1, false, NONE, NONE ) ) );
			  NodeRecord target = Add( InUse( new NodeRecord( 2, false, NONE, NONE ) ) );

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourceNodeDoesNotReferenceBack( source );
			  verify( report ).targetNodeDoesNotReferenceBack( target );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourceNodeWithoutChainForRelationshipInTheMiddleOfChain()
		 internal virtual void ShouldReportSourceNodeWithoutChainForRelationshipInTheMiddleOfChain()
		 {
			  // given
			  CheckSingleDirection();
			  Initialize( RELATIONSHIPS, NODES );
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  NodeRecord source = Add( InUse( new NodeRecord( 1, false, NONE, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord sPrev = Add( InUse( new RelationshipRecord( 51, 1, 0, 0 ) ) );
			  relationship.FirstPrevRel = sPrev.Id;
			  relationship.FirstInFirstChain = false;
			  sPrev.FirstNextRel = relationship.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourceNodeHasNoRelationships( source );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetNodeWithoutChainForRelationshipInTheMiddleOfChain()
		 internal virtual void ShouldReportTargetNodeWithoutChainForRelationshipInTheMiddleOfChain()
		 {
			  // given
			  CheckSingleDirection();
			  Initialize( RELATIONSHIPS, NODES );
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  NodeRecord target = Add( InUse( new NodeRecord( 2, false, NONE, NONE ) ) );
			  RelationshipRecord tPrev = Add( InUse( new RelationshipRecord( 51, 0, 2, 0 ) ) );
			  relationship.SecondPrevRel = tPrev.Id;
			  relationship.FirstInSecondChain = false;
			  tPrev.SecondNextRel = relationship.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetNodeHasNoRelationships( target );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourcePrevReferencingOtherNodes()
		 internal virtual void ShouldReportSourcePrevReferencingOtherNodes()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 0, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord sPrev = Add( InUse( new RelationshipRecord( 51, 8, 9, 0 ) ) );
			  relationship.FirstPrevRel = sPrev.Id;
			  relationship.FirstInFirstChain = false;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourcePrevReferencesOtherNodes( sPrev );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetPrevReferencingOtherNodes()
		 internal virtual void ShouldReportTargetPrevReferencingOtherNodes()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 0, NONE ) ) );
			  RelationshipRecord tPrev = Add( InUse( new RelationshipRecord( 51, 8, 9, 0 ) ) );
			  relationship.SecondPrevRel = tPrev.Id;
			  relationship.FirstInSecondChain = false;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetPrevReferencesOtherNodes( tPrev );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourceNextReferencingOtherNodes()
		 internal virtual void ShouldReportSourceNextReferencingOtherNodes()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord sNext = Add( InUse( new RelationshipRecord( 51, 8, 9, 0 ) ) );
			  relationship.FirstNextRel = sNext.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourceNextReferencesOtherNodes( sNext );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetNextReferencingOtherNodes()
		 internal virtual void ShouldReportTargetNextReferencingOtherNodes()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord tNext = Add( InUse( new RelationshipRecord( 51, 8, 9, 0 ) ) );
			  relationship.SecondNextRel = tNext.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetNextReferencesOtherNodes( tNext );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourcePrevReferencingOtherNodesWhenReferencingTargetNode()
		 internal virtual void ShouldReportSourcePrevReferencingOtherNodesWhenReferencingTargetNode()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 0, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord sPrev = Add( InUse( new RelationshipRecord( 51, 2, 0, 0 ) ) );
			  relationship.FirstPrevRel = sPrev.Id;
			  relationship.FirstInFirstChain = false;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourcePrevReferencesOtherNodes( sPrev );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetPrevReferencingOtherNodesWhenReferencingSourceNode()
		 internal virtual void ShouldReportTargetPrevReferencingOtherNodesWhenReferencingSourceNode()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 0, NONE ) ) );
			  RelationshipRecord tPrev = Add( InUse( new RelationshipRecord( 51, 1, 0, 0 ) ) );
			  relationship.SecondPrevRel = tPrev.Id;
			  relationship.FirstInSecondChain = false;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetPrevReferencesOtherNodes( tPrev );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourceNextReferencingOtherNodesWhenReferencingTargetNode()
		 internal virtual void ShouldReportSourceNextReferencingOtherNodesWhenReferencingTargetNode()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord sNext = Add( InUse( new RelationshipRecord( 51, 2, 0, 0 ) ) );
			  relationship.FirstNextRel = sNext.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourceNextReferencesOtherNodes( sNext );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetNextReferencingOtherNodesWhenReferencingSourceNode()
		 internal virtual void ShouldReportTargetNextReferencingOtherNodesWhenReferencingSourceNode()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord tNext = Add( InUse( new RelationshipRecord( 51, 1, 0, 0 ) ) );
			  relationship.SecondNextRel = tNext.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetNextReferencesOtherNodes( tNext );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourcePrevNotReferencingBack()
		 internal virtual void ShouldReportSourcePrevNotReferencingBack()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 0, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord sPrev = Add( InUse( new RelationshipRecord( 51, 1, 3, 0 ) ) );
			  relationship.FirstPrevRel = sPrev.Id;
			  relationship.FirstInFirstChain = false;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourcePrevDoesNotReferenceBack( sPrev );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetPrevNotReferencingBack()
		 internal virtual void ShouldReportTargetPrevNotReferencingBack()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 0, NONE ) ) );
			  RelationshipRecord tPrev = Add( InUse( new RelationshipRecord( 51, 2, 3, 0 ) ) );
			  relationship.SecondPrevRel = tPrev.Id;
			  relationship.FirstInSecondChain = false;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetPrevDoesNotReferenceBack( tPrev );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportSourceNextNotReferencingBack()
		 internal virtual void ShouldReportSourceNextNotReferencingBack()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord sNext = Add( InUse( new RelationshipRecord( 51, 3, 1, 0 ) ) );
			  relationship.FirstNextRel = sNext.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).sourceNextDoesNotReferenceBack( sNext );
			  verifyNoMoreInteractions( report );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTargetNextNotReferencingBack()
		 internal virtual void ShouldReportTargetNextNotReferencingBack()
		 {
			  // given
			  RelationshipRecord relationship = InUse( new RelationshipRecord( 42, 1, 2, 4 ) );
			  Add( InUse( new RelationshipTypeTokenRecord( 4 ) ) );
			  Add( InUse( new NodeRecord( 1, false, 42, NONE ) ) );
			  Add( InUse( new NodeRecord( 2, false, 42, NONE ) ) );
			  RelationshipRecord tNext = Add( InUse( new RelationshipRecord( 51, 3, 2, 0 ) ) );
			  relationship.SecondNextRel = tNext.Id;

			  // when
			  ConsistencyReport_RelationshipConsistencyReport report = Check( relationship );

			  // then
			  verify( report ).targetNextDoesNotReferenceBack( tNext );
			  verifyNoMoreInteractions( report );
		 }
	}

}