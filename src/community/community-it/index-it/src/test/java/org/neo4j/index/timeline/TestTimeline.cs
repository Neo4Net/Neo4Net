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
namespace Neo4Net.Index.timeline
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.Index;
	using RelationshipIndex = Neo4Net.GraphDb.Index.RelationshipIndex;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Index.lucene;
	using Neo4Net.Index.lucene;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asCollection;

	public class TestTimeline
	{
		 private IGraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

		 private interface IEntityCreator<T> where T : Neo4Net.GraphDb.PropertyContainer
		 {
			  T Create();
		 }

		 private IEntityCreator<PropertyContainer> nodeCreator = new IEntityCreatorAnonymousInnerClass();

		 private class IEntityCreatorAnonymousInnerClass : IEntityCreator<PropertyContainer>
		 {
			 public Node create()
			 {
				  return outerInstance.db.createNode();
			 }
		 }

		 private IEntityCreator<PropertyContainer> relationshipCreator = new IEntityCreatorAnonymousInnerClass2();

		 private class IEntityCreatorAnonymousInnerClass2 : IEntityCreator<PropertyContainer>
		 {
			 private readonly RelationshipType type = RelationshipType.withName( "whatever" );

			 public Relationship create()
			 {
				  return outerInstance.db.createNode().createRelationshipTo(outerInstance.db.createNode(), type);
			 }
		 }

		 private TimelineIndex<PropertyContainer> NodeTimeline()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Index<Node> nodeIndex = _db.index().forNodes("timeline");
					tx.Success();
					return new LuceneTimeline( _db, nodeIndex );
			  }
		 }

		 private TimelineIndex<PropertyContainer> RelationshipTimeline()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					RelationshipIndex relationshipIndex = _db.index().forRelationships("timeline");
					tx.Success();
					return new LuceneTimeline( _db, relationshipIndex );
			  }
		 }

		 private LinkedList<Pair<PropertyContainer, long>> CreateTimestamps( IEntityCreator<PropertyContainer> creator, TimelineIndex<PropertyContainer> timeline, params long[] timestamps )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					LinkedList<Pair<PropertyContainer, long>> result = new LinkedList<Pair<PropertyContainer, long>>();
					foreach ( long timestamp in timestamps )
					{
						 result.AddLast( CreateTimestampedEntity( creator, timeline, timestamp ) );
					}
					tx.Success();
					return result;
			  }
		 }

		 private Pair<PropertyContainer, long> CreateTimestampedEntity( IEntityCreator<PropertyContainer> creator, TimelineIndex<PropertyContainer> timeline, long timestamp )
		 {
			  IPropertyContainer IEntity = creator.Create();
			  timeline.Add( IEntity, timestamp );
			  return Pair.of( IEntity, timestamp );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.util.List<Neo4Net.graphdb.PropertyContainer> sortedEntities(java.util.LinkedList<Neo4Net.helpers.collection.Pair<Neo4Net.graphdb.PropertyContainer, long>> timestamps, final boolean reversed)
		 private IList<PropertyContainer> SortedEntities( LinkedList<Pair<PropertyContainer, long>> timestamps, bool reversed )
		 {
			  IList<Pair<PropertyContainer, long>> sorted = new List<Pair<PropertyContainer, long>>( timestamps );
			  sorted.sort( ( o1, o2 ) => !reversed ? o1.other().compareTo(o2.other()) : o2.other().compareTo(o1.other()) );

			  IList<PropertyContainer> result = new List<PropertyContainer>();
			  foreach ( Pair<PropertyContainer, long> timestamp in sorted )
			  {
					result.Add( timestamp.First() );
			  }
			  return result;
		 }

		 // ======== Tests, although private so that we can create two versions of each,
		 // ======== one for nodes and one for relationships

		 private void MakeSureFirstAndLastAreReturnedCorrectly( IEntityCreator<PropertyContainer> creator, TimelineIndex<PropertyContainer> timeline )
		 {
			  LinkedList<Pair<PropertyContainer, long>> timestamps = CreateTimestamps( creator, timeline, 223456, 12345, 432234 );
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( timestamps.get( 1 ).first(), timeline.First );
					assertEquals( timestamps.Last.Value.first(), timeline.Last );
					tx.Success();
			  }
		 }

		 private void MakeSureRangesAreReturnedInCorrectOrder( IEntityCreator<PropertyContainer> creator, TimelineIndex<PropertyContainer> timeline )
		 {
			  LinkedList<Pair<PropertyContainer, long>> timestamps = CreateTimestamps( creator, timeline, 300000, 200000, 400000, 100000, 500000, 600000, 900000, 800000 );
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( SortedEntities( timestamps, false ), asCollection( timeline.GetBetween( null, null ).GetEnumerator() ) );
					tx.Success();
			  }
		 }

		 private void MakeSureRangesAreReturnedInCorrectReversedOrder( IEntityCreator<PropertyContainer> creator, TimelineIndex<PropertyContainer> timeline )
		 {
			  LinkedList<Pair<PropertyContainer, long>> timestamps = CreateTimestamps( creator, timeline, 300000, 200000, 199999, 400000, 100000, 500000, 600000, 900000, 800000 );
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( SortedEntities( timestamps, true ), asCollection( timeline.GetBetween( null, null, true ).GetEnumerator() ) );
					tx.Success();
			  }
		 }

		 private void MakeSureWeCanQueryLowerDefaultThan1970( IEntityCreator<PropertyContainer> creator, TimelineIndex<PropertyContainer> timeline )
		 {
			  LinkedList<Pair<PropertyContainer, long>> timestamps = CreateTimestamps( creator, timeline, -10000, 0, 10000 );
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( SortedEntities( timestamps, true ), asCollection( timeline.GetBetween( null, 10000L, true ).GetEnumerator() ) );
					tx.Success();
			  }
		 }

		 private void MakeSureUncommittedChangesAreSortedCorrectly( IEntityCreator<PropertyContainer> creator, TimelineIndex<PropertyContainer> timeline )
		 {
			  LinkedList<Pair<PropertyContainer, long>> timestamps = CreateTimestamps( creator, timeline, 300000, 100000, 500000, 900000, 800000 );

			  using ( Transaction tx = _db.beginTx() )
			  {
					timestamps.addAll( CreateTimestamps( creator, timeline, 40000, 70000, 20000 ) );
					assertEquals( SortedEntities( timestamps, false ), asCollection( timeline.GetBetween( null, null ).GetEnumerator() ) );
					tx.Success();
			  }

			  using ( Transaction ignore = _db.beginTx() )
			  {
					assertEquals( SortedEntities( timestamps, false ), asCollection( timeline.GetBetween( null, null ).GetEnumerator() ) );
			  }
		 }

		 // ======== The tests

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureFirstAndLastAreReturnedCorrectlyNode()
		 public virtual void MakeSureFirstAndLastAreReturnedCorrectlyNode()
		 {
			  MakeSureFirstAndLastAreReturnedCorrectly( nodeCreator, NodeTimeline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureFirstAndLastAreReturnedCorrectlyRelationship()
		 public virtual void MakeSureFirstAndLastAreReturnedCorrectlyRelationship()
		 {
			  MakeSureFirstAndLastAreReturnedCorrectly( relationshipCreator, RelationshipTimeline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureRangesAreReturnedInCorrectOrderNode()
		 public virtual void MakeSureRangesAreReturnedInCorrectOrderNode()
		 {
			  MakeSureRangesAreReturnedInCorrectOrder( nodeCreator, NodeTimeline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureRangesAreReturnedInCorrectOrderRelationship()
		 public virtual void MakeSureRangesAreReturnedInCorrectOrderRelationship()
		 {
			  MakeSureRangesAreReturnedInCorrectOrder( relationshipCreator, RelationshipTimeline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureRangesAreReturnedInCorrectReversedOrderNode()
		 public virtual void MakeSureRangesAreReturnedInCorrectReversedOrderNode()
		 {
			  MakeSureRangesAreReturnedInCorrectReversedOrder( nodeCreator, NodeTimeline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureRangesAreReturnedInCorrectReversedOrderRelationship()
		 public virtual void MakeSureRangesAreReturnedInCorrectReversedOrderRelationship()
		 {
			  MakeSureRangesAreReturnedInCorrectReversedOrder( relationshipCreator, RelationshipTimeline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureUncommittedChangesAreSortedCorrectlyNode()
		 public virtual void MakeSureUncommittedChangesAreSortedCorrectlyNode()
		 {
			  MakeSureUncommittedChangesAreSortedCorrectly( nodeCreator, NodeTimeline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureUncommittedChangesAreSortedCorrectlyRelationship()
		 public virtual void MakeSureUncommittedChangesAreSortedCorrectlyRelationship()
		 {
			  MakeSureUncommittedChangesAreSortedCorrectly( relationshipCreator, RelationshipTimeline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureWeCanQueryLowerDefaultThan1970Node()
		 public virtual void MakeSureWeCanQueryLowerDefaultThan1970Node()
		 {
			  MakeSureWeCanQueryLowerDefaultThan1970( nodeCreator, NodeTimeline() );
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureWeCanQueryLowerDefaultThan1970Relationship()
		 public virtual void MakeSureWeCanQueryLowerDefaultThan1970Relationship()
		 {
			  MakeSureWeCanQueryLowerDefaultThan1970( relationshipCreator, RelationshipTimeline() );
		 }
	}

}