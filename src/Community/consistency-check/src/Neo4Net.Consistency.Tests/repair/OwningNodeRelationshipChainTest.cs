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
namespace Neo4Net.Consistency.repair
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.jupiter.api.Test;

	using Neo4Net.Kernel.impl.store;
	using ReadNodeAnswer = Neo4Net.Kernel.impl.store.RecordStoreUtil.ReadNodeAnswer;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.Record.NO_NEXT_PROPERTY;

	internal class OwningNodeRelationshipChainTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFindBothChainsThatTheRelationshipRecordShouldBelongTo()
		 internal virtual void ShouldFindBothChainsThatTheRelationshipRecordShouldBelongTo()
		 {
			  // given
			  long node1 = 101;
			  long node1Rel = 1001;
			  long node2 = 201;
			  long node2Rel = 2001;
			  long sharedRel = 1000;
			  int relType = 0;

			  RecordSet<RelationshipRecord> node1RelChain = AsSet( new RelationshipRecord( node1Rel, node1, node1 - 1, relType ), new RelationshipRecord( sharedRel, node1, node2, relType ), new RelationshipRecord( node1Rel + 1, node1 + 1, node1, relType ) );
			  RecordSet<RelationshipRecord> node2RelChain = AsSet( new RelationshipRecord( node2Rel, node2 - 1, node2, relType ), new RelationshipRecord( sharedRel, node1, node2, relType ), new RelationshipRecord( node2Rel + 1, node2, node2 + 1, relType ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Neo4Net.kernel.impl.store.RecordStore<Neo4Net.kernel.impl.store.record.NodeRecord> recordStore = mock(Neo4Net.kernel.impl.store.RecordStore.class);
			  RecordStore<NodeRecord> recordStore = mock( typeof( RecordStore ) );
			  when( recordStore.GetRecord( eq( node1 ), any( typeof( NodeRecord ) ), any( typeof( RecordLoad ) ) ) ).thenAnswer( new ReadNodeAnswer( false, node1Rel, NO_NEXT_PROPERTY.intValue() ) );
			  when( recordStore.GetRecord( eq( node2 ), any( typeof( NodeRecord ) ), any( typeof( RecordLoad ) ) ) ).thenAnswer( new ReadNodeAnswer( false, node2Rel, NO_NEXT_PROPERTY.intValue() ) );
			  when( recordStore.NewRecord() ).thenReturn(new NodeRecord(-1));

			  RelationshipChainExplorer relationshipChainExplorer = mock( typeof( RelationshipChainExplorer ) );
			  when( relationshipChainExplorer.FollowChainFromNode( node1, node1Rel ) ).thenReturn( node1RelChain );
			  when( relationshipChainExplorer.FollowChainFromNode( node2, node2Rel ) ).thenReturn( node2RelChain );

			  OwningNodeRelationshipChain owningChainFinder = new OwningNodeRelationshipChain( relationshipChainExplorer, recordStore );

			  // when
			  RecordSet<RelationshipRecord> recordsInChains = owningChainFinder.FindRelationshipChainsThatThisRecordShouldBelongTo( new RelationshipRecord( sharedRel, node1, node2, relType ) );

			  // then
			  assertThat( recordsInChains, ContainsAllRecords( node1RelChain ) );
			  assertThat( recordsInChains, ContainsAllRecords( node2RelChain ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.hamcrest.Matcher<RecordSet<Neo4Net.kernel.impl.store.record.RelationshipRecord>> containsAllRecords(final RecordSet<Neo4Net.kernel.impl.store.record.RelationshipRecord> expectedSet)
		 private static Matcher<RecordSet<RelationshipRecord>> ContainsAllRecords( RecordSet<RelationshipRecord> expectedSet )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( expectedSet );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<RecordSet<RelationshipRecord>>
		 {
			 private Neo4Net.Consistency.repair.RecordSet<RelationshipRecord> _expectedSet;

			 public TypeSafeMatcherAnonymousInnerClass( Neo4Net.Consistency.repair.RecordSet<RelationshipRecord> expectedSet )
			 {
				 this._expectedSet = expectedSet;
			 }

			 public override bool matchesSafely( RecordSet<RelationshipRecord> actualSet )
			 {
				  return actualSet.ContainsAll( _expectedSet );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "RecordSet containing " ).appendValueList( "[", ",", "]", _expectedSet );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static <R extends Neo4Net.kernel.impl.store.record.AbstractBaseRecord> RecordSet<R> asSet(R... records)
		 private static RecordSet<R> AsSet<R>( params R[] records ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  RecordSet<R> set = new RecordSet<R>();
			  foreach ( R record in records )
			  {
					set.Add( record );
			  }
			  return set;
		 }
	}

}