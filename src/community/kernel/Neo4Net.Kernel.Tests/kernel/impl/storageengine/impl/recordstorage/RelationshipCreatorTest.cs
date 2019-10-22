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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Test = org.junit.Test;

	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using NoOpClient = Neo4Net.Kernel.impl.locking.NoOpClient;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RecordChangeSet = Neo4Net.Kernel.impl.transaction.state.RecordChangeSet;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.filterType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.firstIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.firstOut;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.from;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.group;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.firstLoop;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.newChangeSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.newRelGroupGetter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.nextRel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.node;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.owningNode;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.rel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.relGroup;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.sCount;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.sNext;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.sPrev;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.tCount;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.tNext;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.tPrev;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordBuilders.to;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.storageengine.impl.recordstorage.RecordMatchers.containsChanges;

	public class RelationshipCreatorTest
	{
		 private AbstractBaseRecord[] _givenState;
		 private RecordChangeSet _changeset;
		 private int _denseNodeThreshold = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newRelWithNoPriorRels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NewRelWithNoPriorRels()
		 {
			  GivenState( node( 0 ), node( 1 ) );

			  CreateRelationshipBetween( 0, 1 );

			  assertThat( _changeset, containsChanges( node( 0, nextRel( 0 ) ), node( 1, nextRel( 0 ) ), rel( 0, from( 0 ), to( 1 ), sCount( 1 ), tCount( 1 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selfRelWithNoPriorRels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelfRelWithNoPriorRels()
		 {
			  GivenState( node( 0 ) );

			  CreateRelationshipBetween( 0, 0 );

			  assertThat( _changeset, containsChanges( node( 0, nextRel( 0 ) ), rel( 0, from( 0 ), to( 0 ), sCount( 1 ), tCount( 1 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sourceHas1PriorRel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SourceHas1PriorRel()
		 {
			  GivenState( node( 0, nextRel( 0 ) ), node( 1, nextRel( 0 ) ), node( 2 ), rel( 0, from( 0 ), to( 1 ), sCount( 1 ), tCount( 1 ) ) );

			  CreateRelationshipBetween( 0, 2 );

			  assertThat( _changeset, containsChanges( node( 0, nextRel( 1 ) ), node( 2, nextRel( 1 ) ), rel( 0, from( 0 ), to( 1 ), sPrev( 1 ), tCount( 1 ) ), rel( 1, from( 0 ), to( 2 ), sCount( 2 ), sNext( 0 ), tCount( 1 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void targetHas1PriorRel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TargetHas1PriorRel()
		 {
			  GivenState( node( 0, nextRel( 0 ) ), node( 1, nextRel( 0 ) ), node( 2 ), rel( 0, from( 0 ), to( 1 ), sCount( 1 ), tCount( 1 ) ) );

			  CreateRelationshipBetween( 2, 0 );

			  assertThat( _changeset, containsChanges( node( 0, nextRel( 1 ) ), node( 2, nextRel( 1 ) ), rel( 0, from( 0 ), to( 1 ), sPrev( 1 ), tCount( 1 ) ), rel( 1, from( 2 ), to( 0 ), sCount( 1 ), tNext( 0 ), tCount( 2 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sourceAndTargetShare1PriorRel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SourceAndTargetShare1PriorRel()
		 {
			  GivenState( node( 0, nextRel( 0 ) ), node( 1, nextRel( 0 ) ), rel( 0, from( 0 ), to( 1 ), sCount( 1 ), tCount( 1 ) ) );

			  CreateRelationshipBetween( 0, 1 );

			  assertThat( _changeset, containsChanges( node( 0, nextRel( 1 ) ), node( 1, nextRel( 1 ) ), rel( 0, from( 0 ), to( 1 ), sPrev( 1 ), tPrev( 1 ) ), rel( 1, from( 0 ), to( 1 ), sCount( 2 ), sNext( 0 ), tCount( 2 ), tNext( 0 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selfRelWith1PriorRel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelfRelWith1PriorRel()
		 {
			  GivenState( node( 0, nextRel( 0 ) ), rel( 0, from( 0 ), to( 0 ), sCount( 1 ), tCount( 1 ) ) );

			  CreateRelationshipBetween( 0, 0 );

			  assertThat( _changeset, containsChanges( node( 0, nextRel( 1 ) ), rel( 0, from( 0 ), to( 0 ), sPrev( 1 ), tPrev( 1 ) ), rel( 1, from( 0 ), to( 0 ), sCount( 2 ), sNext( 0 ), tCount( 2 ), tNext( 0 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void selfRelUpgradesToDense() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SelfRelUpgradesToDense()
		 {
			  GivenState( node( 0, nextRel( 0 ) ), rel( 0, from( 0 ), to( 0 ), sCount( 1 ), tCount( 1 ) ) );

			  _denseNodeThreshold = 1;
			  CreateRelationshipBetween( 0, 0 );

			  assertThat( _changeset, containsChanges( node( 0, group( 0 ) ), relGroup( 0, owningNode( 0 ), firstLoop( 1 ) ), rel( 0, from( 0 ), to( 0 ), sPrev( 1 ), tPrev( 1 ) ), rel( 1, from( 0 ), to( 0 ), sCount( 2 ), sNext( 0 ), tCount( 2 ), tNext( 0 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sourceNodeUpdatesToDense() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SourceNodeUpdatesToDense()
		 {
			  GivenState( node( 0, nextRel( 0 ) ), node( 1 ), rel( 0, from( 0 ), to( 0 ), sCount( 1 ), tCount( 1 ) ) );

			  _denseNodeThreshold = 1;
			  CreateRelationshipBetween( 0, 1 );

			  assertThat( _changeset, containsChanges( node( 0, group( 0 ) ), node( 1, nextRel( 1 ) ), relGroup( 0, owningNode( 0 ), firstLoop( 0 ), firstOut( 1 ) ), rel( 0, from( 0 ), to( 0 ), sCount( 1 ), tCount( 1 ) ), rel( 1, from( 0 ), to( 1 ), sCount( 1 ), tCount( 1 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void targetNodeUpdatesToDense() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TargetNodeUpdatesToDense()
		 {
			  GivenState( node( 0, nextRel( 0 ) ), node( 1 ), rel( 0, from( 0 ), to( 0 ), sCount( 1 ), tCount( 1 ) ) );

			  _denseNodeThreshold = 1;
			  CreateRelationshipBetween( 1, 0 );

			  assertThat( _changeset, containsChanges( node( 0, group( 0 ) ), node( 1, nextRel( 1 ) ), relGroup( 0, owningNode( 0 ), firstLoop( 0 ), firstIn( 1 ) ), rel( 0, from( 0 ), to( 0 ), sCount( 1 ), tCount( 1 ) ), rel( 1, from( 1 ), to( 0 ), sCount( 1 ), tCount( 1 ) ) ) );
		 }

		 private void GivenState( params AbstractBaseRecord[] records )
		 {
			  _givenState = records;
			  _changeset = newChangeSet( _givenState );
		 }

		 private void CreateRelationshipBetween( long fromNode, long toNode )
		 {
			  Neo4Net.Kernel.impl.locking.Locks_Client locks = new NoOpClient();
			  RelationshipCreator logic = new RelationshipCreator( newRelGroupGetter( _givenState ), _denseNodeThreshold );

			  logic.RelationshipCreate( NextRelId( _givenState ), 0, fromNode, toNode, _changeset, locks );
		 }

		 private long NextRelId( AbstractBaseRecord[] existingRecords )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return filterType( existingRecords, typeof( RelationshipRecord ) ).map( AbstractBaseRecord::getId ).max( long?.compareTo ).orElse( -1L ) + 1;
		 }
	}

}