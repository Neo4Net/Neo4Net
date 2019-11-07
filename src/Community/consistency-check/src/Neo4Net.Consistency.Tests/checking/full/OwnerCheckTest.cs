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
namespace Neo4Net.Consistency.checking.full
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccessStub = Neo4Net.Consistency.Store.RecordAccessStub;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.DynamicRecordCheckTest.configureDynamicStore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.check;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.dummyDynamicCheck;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.dummyNeoStoreCheck;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.dummyNodeCheck;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.dummyPropertyChecker;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.dummyPropertyKeyCheck;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.dummyRelationshipChecker;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.dummyRelationshipLabelCheck;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.inUse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.notInUse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.propertyBlock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.consistency.checking.RecordCheckTestBase.@string;

	internal class OwnerCheckTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotDecorateCheckerWhenInactive()
		 internal virtual void ShouldNotDecorateCheckerWhenInactive()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( false );
			  PrimitiveRecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> checker = dummyNodeCheck();

			  // when
			  RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> decorated = decorator.DecorateNodeChecker( checker );

			  // then
			  assertSame( checker, decorated );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForNodesWithDifferentPropertyChains()
		 internal virtual void ShouldNotReportAnythingForNodesWithDifferentPropertyChains()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> nodeChecker = decorator.DecorateNodeChecker( dummyNodeCheck() );

			  RecordAccessStub records = new RecordAccessStub();

			  NodeRecord node1 = records.Add( inUse( new NodeRecord( 1, false, NONE, 7 ) ) );
			  NodeRecord node2 = records.Add( inUse( new NodeRecord( 2, false, NONE, 8 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node2, records );

			  // then
			  verifyZeroInteractions( report1 );
			  verifyZeroInteractions( report2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForNodesNotInUse()
		 internal virtual void ShouldNotReportAnythingForNodesNotInUse()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> nodeChecker = decorator.DecorateNodeChecker( dummyNodeCheck() );

			  RecordAccessStub records = new RecordAccessStub();

			  NodeRecord node1 = records.Add( notInUse( new NodeRecord( 1, false, NONE, 6 ) ) );
			  NodeRecord node2 = records.Add( notInUse( new NodeRecord( 2, false, NONE, 6 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node2, records );

			  // then
			  verifyZeroInteractions( report1 );
			  verifyZeroInteractions( report2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportAnythingForRelationshipsWithDifferentPropertyChains()
		 internal virtual void ShouldNotReportAnythingForRelationshipsWithDifferentPropertyChains()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> relationshipChecker = decorator.DecorateRelationshipChecker( dummyRelationshipChecker() );

			  RecordAccessStub records = new RecordAccessStub();

			  RelationshipRecord relationship1 = records.Add( inUse( new RelationshipRecord( 1, 0, 1, 0 ) ) );
			  relationship1.NextProp = 7;
			  RelationshipRecord relationship2 = records.Add( inUse( new RelationshipRecord( 2, 0, 1, 0 ) ) );
			  relationship2.NextProp = 8;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), relationshipChecker, relationship1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), relationshipChecker, relationship2, records );

			  // then
			  verifyZeroInteractions( report1 );
			  verifyZeroInteractions( report2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTwoNodesWithSamePropertyChain()
		 internal virtual void ShouldReportTwoNodesWithSamePropertyChain()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> nodeChecker = decorator.DecorateNodeChecker( dummyNodeCheck() );

			  RecordAccessStub records = new RecordAccessStub();

			  NodeRecord node1 = records.Add( inUse( new NodeRecord( 1, false, NONE, 7 ) ) );
			  NodeRecord node2 = records.Add( inUse( new NodeRecord( 2, false, NONE, 7 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node2, records );

			  // then
			  verifyZeroInteractions( report1 );
			  verify( report2 ).multipleOwners( node1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportTwoRelationshipsWithSamePropertyChain()
		 internal virtual void ShouldReportTwoRelationshipsWithSamePropertyChain()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> relationshipChecker = decorator.DecorateRelationshipChecker( dummyRelationshipChecker() );

			  RecordAccessStub records = new RecordAccessStub();

			  RelationshipRecord relationship1 = records.Add( inUse( new RelationshipRecord( 1, 0, 1, 0 ) ) );
			  relationship1.NextProp = 7;
			  RelationshipRecord relationship2 = records.Add( inUse( new RelationshipRecord( 2, 0, 1, 0 ) ) );
			  relationship2.NextProp = relationship1.NextProp;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), relationshipChecker, relationship1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), relationshipChecker, relationship2, records );

			  // then
			  verifyZeroInteractions( report1 );
			  verify( report2 ).multipleOwners( relationship1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportRelationshipWithSamePropertyChainAsNode()
		 internal virtual void ShouldReportRelationshipWithSamePropertyChainAsNode()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> nodeChecker = decorator.DecorateNodeChecker( dummyNodeCheck() );
			  RecordCheck<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> relationshipChecker = decorator.DecorateRelationshipChecker( dummyRelationshipChecker() );

			  RecordAccessStub records = new RecordAccessStub();

			  NodeRecord node = records.Add( inUse( new NodeRecord( 1, false, NONE, 7 ) ) );
			  RelationshipRecord relationship = records.Add( inUse( new RelationshipRecord( 1, 0, 1, 0 ) ) );
			  relationship.NextProp = node.NextProp;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport nodeReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node, records );
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport relationshipReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), relationshipChecker, relationship, records );

			  // then
			  verifyZeroInteractions( nodeReport );
			  verify( relationshipReport ).multipleOwners( node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportRelationshipWithReferenceToTheGraphGlobalChain()
		 internal virtual void ShouldReportRelationshipWithReferenceToTheGraphGlobalChain()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> relationshipChecker = decorator.DecorateRelationshipChecker( dummyRelationshipChecker() );
			  RecordCheck<NeoStoreRecord, Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport> neoStoreCheck = decorator.DecorateNeoStoreChecker( dummyNeoStoreCheck() );

			  RecordAccessStub records = new RecordAccessStub();

			  NeoStoreRecord master = records.Add( new NeoStoreRecord() );
			  master.NextProp = 7;
			  RelationshipRecord relationship = records.Add( inUse( new RelationshipRecord( 1, 0, 1, 0 ) ) );
			  relationship.NextProp = 7;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport masterReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport ), neoStoreCheck, master, records );
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport relationshipReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), relationshipChecker, relationship, records );

			  // then
			  verifyZeroInteractions( masterReport );
			  verify( relationshipReport ).multipleOwners( master );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeWithSamePropertyChainAsRelationship()
		 internal virtual void ShouldReportNodeWithSamePropertyChainAsRelationship()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> nodeChecker = decorator.DecorateNodeChecker( dummyNodeCheck() );
			  RecordCheck<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> relationshipChecker = decorator.DecorateRelationshipChecker( dummyRelationshipChecker() );

			  RecordAccessStub records = new RecordAccessStub();

			  NodeRecord node = records.Add( inUse( new NodeRecord( 1, false, NONE, 7 ) ) );
			  RelationshipRecord relationship = records.Add( inUse( new RelationshipRecord( 1, 0, 1, 0 ) ) );
			  relationship.NextProp = node.NextProp;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport relationshipReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), relationshipChecker, relationship, records );
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport nodeReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node, records );

			  // then
			  verifyZeroInteractions( relationshipReport );
			  verify( nodeReport ).multipleOwners( relationship );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeWithReferenceToTheGraphGlobalChain()
		 internal virtual void ShouldReportNodeWithReferenceToTheGraphGlobalChain()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> nodeChecker = decorator.DecorateNodeChecker( dummyNodeCheck() );
			  RecordCheck<NeoStoreRecord, Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport> neoStoreCheck = decorator.DecorateNeoStoreChecker( dummyNeoStoreCheck() );

			  RecordAccessStub records = new RecordAccessStub();

			  NodeRecord node = records.Add( inUse( new NodeRecord( 1, false, NONE, 7 ) ) );
			  NeoStoreRecord master = records.Add( new NeoStoreRecord() );
			  master.NextProp = node.NextProp;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport masterReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport ), neoStoreCheck, master, records );
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport nodeReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node, records );

			  // then
			  verifyZeroInteractions( masterReport );
			  verify( nodeReport ).multipleOwners( master );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeStoreReferencingSameChainAsNode()
		 internal virtual void ShouldReportNodeStoreReferencingSameChainAsNode()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> nodeChecker = decorator.DecorateNodeChecker( dummyNodeCheck() );
			  RecordCheck<NeoStoreRecord, Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport> neoStoreCheck = decorator.DecorateNeoStoreChecker( dummyNeoStoreCheck() );

			  RecordAccessStub records = new RecordAccessStub();

			  NodeRecord node = records.Add( inUse( new NodeRecord( 1, false, NONE, 7 ) ) );
			  NeoStoreRecord master = records.Add( new NeoStoreRecord() );
			  master.NextProp = node.NextProp;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport nodeReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), nodeChecker, node, records );
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport masterReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport ), neoStoreCheck, master, records );

			  // then
			  verifyZeroInteractions( nodeReport );
			  verify( masterReport ).multipleOwners( node );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportNodeStoreReferencingSameChainAsRelationship()
		 internal virtual void ShouldReportNodeStoreReferencingSameChainAsRelationship()
		 {
			  // given
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> relationshipChecker = decorator.DecorateRelationshipChecker( dummyRelationshipChecker() );
			  RecordCheck<NeoStoreRecord, Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport> neoStoreCheck = decorator.DecorateNeoStoreChecker( dummyNeoStoreCheck() );

			  RecordAccessStub records = new RecordAccessStub();

			  NeoStoreRecord master = records.Add( new NeoStoreRecord() );
			  master.NextProp = 7;
			  RelationshipRecord relationship = records.Add( inUse( new RelationshipRecord( 1, 0, 1, 0 ) ) );
			  relationship.NextProp = 7;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport relationshipReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), relationshipChecker, relationship, records );
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport masterReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport ), neoStoreCheck, master, records );

			  // then
			  verifyZeroInteractions( relationshipReport );
			  verify( masterReport ).multipleOwners( relationship );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOrphanPropertyChain()
		 internal virtual void ShouldReportOrphanPropertyChain()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true );
			  RecordCheck<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> checker = decorator.DecoratePropertyChecker( dummyPropertyChecker() );

			  PropertyRecord record = inUse( new PropertyRecord( 42 ) );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), checker, record, records );

			  // when
			  decorator.ScanForOrphanChains( ProgressMonitorFactory.NONE );

			  records.CheckDeferred();

			  // then
			  verify( report ).orphanPropertyChain();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportOrphanIfOwnedByNode()
		 internal virtual void ShouldNotReportOrphanIfOwnedByNode()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true );

			  PropertyRecord record = inUse( new PropertyRecord( 42 ) );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), decorator.DecoratePropertyChecker( dummyPropertyChecker() ), record, records );
			  Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport nodeReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport ), decorator.DecorateNodeChecker( dummyNodeCheck() ), inUse(new NodeRecord(10, false, NONE, 42)), records );

			  // when
			  decorator.ScanForOrphanChains( ProgressMonitorFactory.NONE );

			  records.CheckDeferred();

			  // then
			  verifyNoMoreInteractions( report );
			  verifyNoMoreInteractions( nodeReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportOrphanIfOwnedByRelationship()
		 internal virtual void ShouldNotReportOrphanIfOwnedByRelationship()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true );

			  PropertyRecord record = inUse( new PropertyRecord( 42 ) );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), decorator.DecoratePropertyChecker( dummyPropertyChecker() ), record, records );
			  RelationshipRecord relationship = inUse( new RelationshipRecord( 10, 1, 1, 0 ) );
			  relationship.NextProp = 42;
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport relationshipReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport ), decorator.DecorateRelationshipChecker( dummyRelationshipChecker() ), relationship, records );

			  // when
			  decorator.ScanForOrphanChains( ProgressMonitorFactory.NONE );

			  records.CheckDeferred();

			  // then
			  verifyNoMoreInteractions( report );
			  verifyNoMoreInteractions( relationshipReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReportOrphanIfOwnedByNeoStore()
		 internal virtual void ShouldNotReportOrphanIfOwnedByNeoStore()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true );

			  PropertyRecord record = inUse( new PropertyRecord( 42 ) );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), decorator.DecoratePropertyChecker( dummyPropertyChecker() ), record, records );
			  NeoStoreRecord master = inUse( new NeoStoreRecord() );
			  master.NextProp = 42;
			  Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport masterReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport ), decorator.DecorateNeoStoreChecker( dummyNeoStoreCheck() ), master, records );

			  // when
			  decorator.ScanForOrphanChains( ProgressMonitorFactory.NONE );

			  records.CheckDeferred();

			  // then
			  verifyNoMoreInteractions( report );
			  verifyNoMoreInteractions( masterReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicRecordOwnedByTwoOtherDynamicRecords()
		 internal virtual void ShouldReportDynamicRecordOwnedByTwoOtherDynamicRecords()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.STRING );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> checker = decorator.DecorateDynamicChecker( RecordType.STRING_PROPERTY, dummyDynamicCheck( configureDynamicStore( 50 ), DynamicStore.STRING ) );

			  DynamicRecord record1 = records.Add( inUse( @string( new DynamicRecord( 1 ) ) ) );
			  DynamicRecord record2 = records.Add( inUse( @string( new DynamicRecord( 2 ) ) ) );
			  DynamicRecord record3 = records.Add( inUse( @string( new DynamicRecord( 3 ) ) ) );
			  record1.NextBlock = record3.Id;
			  record2.NextBlock = record3.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), checker, record1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), checker, record2, records );

			  // then
			  verifyNoMoreInteractions( report1 );
			  verify( report2 ).nextMultipleOwners( record1 );
			  verifyNoMoreInteractions( report2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicStringRecordOwnedByTwoPropertyRecords()
		 internal virtual void ShouldReportDynamicStringRecordOwnedByTwoPropertyRecords()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.STRING );

			  RecordCheck<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> propChecker = decorator.DecoratePropertyChecker( dummyPropertyChecker() );

			  DynamicRecord dynamic = records.Add( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  PropertyRecord property1 = records.Add( inUse( new PropertyRecord( 1 ) ) );
			  PropertyRecord property2 = records.Add( inUse( new PropertyRecord( 2 ) ) );
			  PropertyKeyTokenRecord key = records.Add( inUse( new PropertyKeyTokenRecord( 10 ) ) );
			  property1.AddPropertyBlock( propertyBlock( key, PropertyType.STRING, dynamic.Id ) );
			  property2.AddPropertyBlock( propertyBlock( key, PropertyType.STRING, dynamic.Id ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), propChecker, property1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), propChecker, property2, records );

			  // then
			  verifyNoMoreInteractions( report1 );
			  verify( report2 ).stringMultipleOwners( property1 );
			  verifyNoMoreInteractions( report2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicArrayRecordOwnedByTwoPropertyRecords()
		 internal virtual void ShouldReportDynamicArrayRecordOwnedByTwoPropertyRecords()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.ARRAY );

			  RecordCheck<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> propChecker = decorator.DecoratePropertyChecker( dummyPropertyChecker() );

			  DynamicRecord dynamic = records.Add( inUse( array( new DynamicRecord( 42 ) ) ) );
			  PropertyRecord property1 = records.Add( inUse( new PropertyRecord( 1 ) ) );
			  PropertyRecord property2 = records.Add( inUse( new PropertyRecord( 2 ) ) );
			  PropertyKeyTokenRecord key = records.Add( inUse( new PropertyKeyTokenRecord( 10 ) ) );
			  property1.AddPropertyBlock( propertyBlock( key, PropertyType.ARRAY, dynamic.Id ) );
			  property2.AddPropertyBlock( propertyBlock( key, PropertyType.ARRAY, dynamic.Id ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), propChecker, property1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), propChecker, property2, records );

			  // then
			  verifyNoMoreInteractions( report1 );
			  verify( report2 ).arrayMultipleOwners( property1 );
			  verifyNoMoreInteractions( report2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicRecordOwnedByPropertyAndOtherDynamic()
		 internal virtual void ShouldReportDynamicRecordOwnedByPropertyAndOtherDynamic()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.STRING );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> dynChecker = decorator.DecorateDynamicChecker( RecordType.STRING_PROPERTY, dummyDynamicCheck( configureDynamicStore( 50 ), DynamicStore.STRING ) );
			  RecordCheck<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> propChecker = decorator.DecoratePropertyChecker( dummyPropertyChecker() );

			  DynamicRecord owned = records.Add( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  DynamicRecord dynamic = records.Add( inUse( @string( new DynamicRecord( 100 ) ) ) );
			  dynamic.NextBlock = owned.Id;
			  PropertyRecord property = records.Add( inUse( new PropertyRecord( 1 ) ) );
			  PropertyKeyTokenRecord key = records.Add( inUse( new PropertyKeyTokenRecord( 10 ) ) );
			  property.AddPropertyBlock( propertyBlock( key, PropertyType.STRING, owned.Id ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport propReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), propChecker, property, records );
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport dynReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), dynChecker, dynamic, records );

			  // then
			  verifyNoMoreInteractions( propReport );
			  verify( dynReport ).nextMultipleOwners( property );
			  verifyNoMoreInteractions( dynReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicStringRecordOwnedByOtherDynamicAndProperty()
		 internal virtual void ShouldReportDynamicStringRecordOwnedByOtherDynamicAndProperty()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.STRING );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> dynChecker = decorator.DecorateDynamicChecker( RecordType.STRING_PROPERTY, dummyDynamicCheck( configureDynamicStore( 50 ), DynamicStore.STRING ) );
			  RecordCheck<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> propChecker = decorator.DecoratePropertyChecker( dummyPropertyChecker() );

			  DynamicRecord owned = records.Add( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  DynamicRecord dynamic = records.Add( inUse( @string( new DynamicRecord( 100 ) ) ) );
			  dynamic.NextBlock = owned.Id;
			  PropertyRecord property = records.Add( inUse( new PropertyRecord( 1 ) ) );
			  PropertyKeyTokenRecord key = records.Add( inUse( new PropertyKeyTokenRecord( 10 ) ) );
			  property.AddPropertyBlock( propertyBlock( key, PropertyType.STRING, owned.Id ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport dynReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), dynChecker, dynamic, records );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport propReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), propChecker, property, records );

			  // then
			  verifyNoMoreInteractions( dynReport );
			  verify( propReport ).stringMultipleOwners( dynamic );
			  verifyNoMoreInteractions( dynReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicArrayRecordOwnedByOtherDynamicAndProperty()
		 internal virtual void ShouldReportDynamicArrayRecordOwnedByOtherDynamicAndProperty()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.ARRAY );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> dynChecker = decorator.DecorateDynamicChecker( RecordType.ARRAY_PROPERTY, dummyDynamicCheck( configureDynamicStore( 50 ), DynamicStore.ARRAY ) );
			  RecordCheck<PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> propChecker = decorator.DecoratePropertyChecker( dummyPropertyChecker() );

			  DynamicRecord owned = records.Add( inUse( array( new DynamicRecord( 42 ) ) ) );
			  DynamicRecord dynamic = records.Add( inUse( array( new DynamicRecord( 100 ) ) ) );
			  dynamic.NextBlock = owned.Id;
			  PropertyRecord property = records.Add( inUse( new PropertyRecord( 1 ) ) );
			  PropertyKeyTokenRecord key = records.Add( inUse( new PropertyKeyTokenRecord( 10 ) ) );
			  property.AddPropertyBlock( propertyBlock( key, PropertyType.ARRAY, owned.Id ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport dynReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), dynChecker, dynamic, records );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport propReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport ), propChecker, property, records );

			  // then
			  verifyNoMoreInteractions( dynReport );
			  verify( propReport ).arrayMultipleOwners( dynamic );
			  verifyNoMoreInteractions( dynReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicRecordOwnedByTwoRelationshipLabels()
		 internal virtual void ShouldReportDynamicRecordOwnedByTwoRelationshipLabels()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.RELATIONSHIP_TYPE );

			  RecordCheck<RelationshipTypeTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport> checker = decorator.DecorateRelationshipTypeTokenChecker( dummyRelationshipLabelCheck() );

			  DynamicRecord dynamic = records.AddRelationshipTypeName( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  RelationshipTypeTokenRecord record1 = records.Add( inUse( new RelationshipTypeTokenRecord( 1 ) ) );
			  RelationshipTypeTokenRecord record2 = records.Add( inUse( new RelationshipTypeTokenRecord( 2 ) ) );
			  record1.NameId = ( int ) dynamic.Id;
			  record2.NameId = ( int ) dynamic.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport ), checker,record1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport ), checker,record2, records );

			  // then
			  verifyNoMoreInteractions( report1 );
			  verify( report2 ).nameMultipleOwners( record1 );
			  verifyNoMoreInteractions( report2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicRecordOwnedByRelationshipLabelAndOtherDynamicRecord()
		 internal virtual void ShouldReportDynamicRecordOwnedByRelationshipLabelAndOtherDynamicRecord()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.RELATIONSHIP_TYPE );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> dynChecker = decorator.DecorateDynamicChecker( RecordType.RELATIONSHIP_TYPE_NAME, dummyDynamicCheck( configureDynamicStore( 50 ), DynamicStore.RELATIONSHIP_TYPE ) );

			  RecordCheck<RelationshipTypeTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport> labelCheck = decorator.DecorateRelationshipTypeTokenChecker( dummyRelationshipLabelCheck() );

			  DynamicRecord owned = records.AddRelationshipTypeName( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  DynamicRecord dynamic = records.AddRelationshipTypeName( inUse( @string( new DynamicRecord( 1 ) ) ) );
			  RelationshipTypeTokenRecord label = records.Add( inUse( new RelationshipTypeTokenRecord( 1 ) ) );
			  dynamic.NextBlock = owned.Id;
			  label.NameId = ( int ) owned.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport labelReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport ), labelCheck, label, records );
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport dynReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), dynChecker, dynamic, records );

			  // then
			  verifyNoMoreInteractions( labelReport );
			  verify( dynReport ).nextMultipleOwners( label );
			  verifyNoMoreInteractions( dynReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicRecordOwnedByOtherDynamicRecordAndRelationshipLabel()
		 internal virtual void ShouldReportDynamicRecordOwnedByOtherDynamicRecordAndRelationshipLabel()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.RELATIONSHIP_TYPE );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> dynChecker = decorator.DecorateDynamicChecker( RecordType.RELATIONSHIP_TYPE_NAME, dummyDynamicCheck( configureDynamicStore( 50 ), DynamicStore.RELATIONSHIP_TYPE ) );

			  RecordCheck<RelationshipTypeTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport> labelCheck = decorator.DecorateRelationshipTypeTokenChecker( dummyRelationshipLabelCheck() );

			  DynamicRecord owned = records.AddRelationshipTypeName( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  DynamicRecord dynamic = records.AddRelationshipTypeName( inUse( @string( new DynamicRecord( 1 ) ) ) );
			  RelationshipTypeTokenRecord label = records.Add( inUse( new RelationshipTypeTokenRecord( 1 ) ) );
			  dynamic.NextBlock = owned.Id;
			  label.NameId = ( int ) owned.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport dynReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), dynChecker, dynamic, records );
			  Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport labelReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport ), labelCheck, label, records );

			  // then
			  verifyNoMoreInteractions( dynReport );
			  verify( labelReport ).nameMultipleOwners( dynamic );
			  verifyNoMoreInteractions( labelReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicRecordOwnedByTwoPropertyKeys()
		 internal virtual void ShouldReportDynamicRecordOwnedByTwoPropertyKeys()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.PROPERTY_KEY );

			  RecordCheck<PropertyKeyTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport> checker = decorator.DecoratePropertyKeyTokenChecker( dummyPropertyKeyCheck() );

			  DynamicRecord dynamic = records.AddPropertyKeyName( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  PropertyKeyTokenRecord record1 = records.Add( inUse( new PropertyKeyTokenRecord( 1 ) ) );
			  PropertyKeyTokenRecord record2 = records.Add( inUse( new PropertyKeyTokenRecord( 2 ) ) );
			  record1.NameId = ( int ) dynamic.Id;
			  record2.NameId = ( int ) dynamic.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport report1 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport ), checker,record1, records );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport report2 = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport ), checker,record2, records );

			  // then
			  verifyNoMoreInteractions( report1 );
			  verify( report2 ).nameMultipleOwners( record1 );
			  verifyNoMoreInteractions( report2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicRecordOwnedByPropertyKeyAndOtherDynamicRecord()
		 internal virtual void ShouldReportDynamicRecordOwnedByPropertyKeyAndOtherDynamicRecord()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.PROPERTY_KEY );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> dynChecker = decorator.DecorateDynamicChecker( RecordType.PROPERTY_KEY_NAME, dummyDynamicCheck( configureDynamicStore( 50 ), DynamicStore.PROPERTY_KEY ) );

			  RecordCheck<PropertyKeyTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport> keyCheck = decorator.DecoratePropertyKeyTokenChecker( dummyPropertyKeyCheck() );

			  DynamicRecord owned = records.AddPropertyKeyName( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  DynamicRecord dynamic = records.AddPropertyKeyName( inUse( @string( new DynamicRecord( 1 ) ) ) );
			  PropertyKeyTokenRecord key = records.Add( inUse( new PropertyKeyTokenRecord( 1 ) ) );
			  dynamic.NextBlock = owned.Id;
			  key.NameId = ( int ) owned.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport keyReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport ), keyCheck, key, records );
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport dynReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), dynChecker, dynamic, records );

			  // then
			  verifyNoMoreInteractions( keyReport );
			  verify( dynReport ).nextMultipleOwners( key );
			  verifyNoMoreInteractions( dynReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportDynamicRecordOwnedByOtherDynamicRecordAndPropertyKey()
		 internal virtual void ShouldReportDynamicRecordOwnedByOtherDynamicRecordAndPropertyKey()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck decorator = new OwnerCheck( true, DynamicStore.PROPERTY_KEY );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> dynChecker = decorator.DecorateDynamicChecker( RecordType.PROPERTY_KEY_NAME, dummyDynamicCheck( configureDynamicStore( 50 ), DynamicStore.PROPERTY_KEY ) );

			  RecordCheck<PropertyKeyTokenRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport> keyCheck = decorator.DecoratePropertyKeyTokenChecker( dummyPropertyKeyCheck() );

			  DynamicRecord owned = records.AddPropertyKeyName( inUse( @string( new DynamicRecord( 42 ) ) ) );
			  DynamicRecord dynamic = records.AddPropertyKeyName( inUse( @string( new DynamicRecord( 1 ) ) ) );
			  PropertyKeyTokenRecord key = records.Add( inUse( new PropertyKeyTokenRecord( 1 ) ) );
			  dynamic.NextBlock = owned.Id;
			  key.NameId = ( int ) owned.Id;

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport dynReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ),dynChecker, dynamic, records );
			  Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport keyReport = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport ), keyCheck, key, records );

			  // then
			  verifyNoMoreInteractions( dynReport );
			  verify( keyReport ).nameMultipleOwners( dynamic );
			  verifyNoMoreInteractions( keyReport );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOrphanedDynamicStringRecord()
		 internal virtual void ShouldReportOrphanedDynamicStringRecord()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck owners = new OwnerCheck( true, DynamicStore.STRING );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> stringCheck = owners.DecorateDynamicChecker( RecordType.STRING_PROPERTY, dummyDynamicCheck( configureDynamicStore( 60 ), DynamicStore.STRING ) );
			  DynamicRecord record = @string( inUse( new DynamicRecord( 42 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), stringCheck, record, records );
			  owners.ScanForOrphanChains( ProgressMonitorFactory.NONE );
			  records.CheckDeferred();

			  // then
			  verify( report ).orphanDynamicRecord();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOrphanedDynamicArrayRecord()
		 internal virtual void ShouldReportOrphanedDynamicArrayRecord()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck owners = new OwnerCheck( true, DynamicStore.ARRAY );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> stringCheck = owners.DecorateDynamicChecker( RecordType.ARRAY_PROPERTY, dummyDynamicCheck( configureDynamicStore( 60 ), DynamicStore.ARRAY ) );
			  DynamicRecord record = @string( inUse( new DynamicRecord( 42 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), stringCheck, record, records );
			  owners.ScanForOrphanChains( ProgressMonitorFactory.NONE );
			  records.CheckDeferred();

			  // then
			  verify( report ).orphanDynamicRecord();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOrphanedDynamicRelationshipLabelRecord()
		 internal virtual void ShouldReportOrphanedDynamicRelationshipLabelRecord()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck owners = new OwnerCheck( true, DynamicStore.RELATIONSHIP_TYPE );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> stringCheck = owners.DecorateDynamicChecker( RecordType.RELATIONSHIP_TYPE_NAME, dummyDynamicCheck( configureDynamicStore( 60 ), DynamicStore.RELATIONSHIP_TYPE ) );
			  DynamicRecord record = @string( inUse( new DynamicRecord( 42 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), stringCheck, record, records );
			  owners.ScanForOrphanChains( ProgressMonitorFactory.NONE );
			  records.CheckDeferred();

			  // then
			  verify( report ).orphanDynamicRecord();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportOrphanedDynamicPropertyKeyRecord()
		 internal virtual void ShouldReportOrphanedDynamicPropertyKeyRecord()
		 {
			  // given
			  RecordAccessStub records = new RecordAccessStub();
			  OwnerCheck owners = new OwnerCheck( true, DynamicStore.PROPERTY_KEY );

			  RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> stringCheck = owners.DecorateDynamicChecker( RecordType.PROPERTY_KEY_NAME, dummyDynamicCheck( configureDynamicStore( 60 ), DynamicStore.PROPERTY_KEY ) );
			  DynamicRecord record = @string( inUse( new DynamicRecord( 42 ) ) );

			  // when
			  Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport report = check( typeof( Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport ), stringCheck, record, records );
			  owners.ScanForOrphanChains( ProgressMonitorFactory.NONE );
			  records.CheckDeferred();

			  // then
			  verify( report ).orphanDynamicRecord();
		 }
	}

}