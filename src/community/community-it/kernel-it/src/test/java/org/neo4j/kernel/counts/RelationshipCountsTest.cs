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
namespace Neo4Net.Kernel.counts
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using Statement = Neo4Net.Kernel.Api.Statement;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using Barrier = Neo4Net.Test.Barrier;
	using Neo4Net.Test;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.RelationshipType.withName;

	public class RelationshipCountsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.DatabaseRule db = new Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.concurrent.ThreadingRule threading = new Neo4Net.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threading = new ThreadingRule();
		 private System.Func<KernelTransaction> _ktxSupplier;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void exposeGuts()
		 public virtual void ExposeGuts()
		 {
			  _ktxSupplier = () => Db.GraphDatabaseAPI.DependencyResolver.resolveDependency(typeof(ThreadToStatementContextBridge)).getKernelTransactionBoundToThisThread(true);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNumberOfRelationshipsInAnEmptyGraph()
		 public virtual void ShouldReportNumberOfRelationshipsInAnEmptyGraph()
		 {
			  // when
			  long relationshipCount = NumberOfRelationships();

			  // then
			  assertEquals( 0, relationshipCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportTotalNumberOfRelationships()
		 public virtual void ShouldReportTotalNumberOfRelationships()
		 {
			  // given
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  long before = NumberOfRelationships();
			  long during;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Node node = graphDb.CreateNode();
					node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					during = CountsForRelationship( null, null, null );
					tx.Success();
			  }

			  // when
			  long after = NumberOfRelationships();

			  // then
			  assertEquals( 0, before );
			  assertEquals( 3, during );
			  assertEquals( 3, after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccountForDeletedRelationships()
		 public virtual void ShouldAccountForDeletedRelationships()
		 {
			  // given
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  Relationship rel;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Node node = graphDb.CreateNode();
					node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					rel = node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					tx.Success();
			  }
			  long before = NumberOfRelationships();
			  long during;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					rel.Delete();
					during = CountsForRelationship( null, null, null );
					tx.Success();
			  }

			  // when
			  long after = NumberOfRelationships();

			  // then
			  assertEquals( 3, before );
			  assertEquals( 2, during );
			  assertEquals( 2, after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCountRelationshipsCreatedInOtherTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCountRelationshipsCreatedInOtherTransaction()
		 {
			  // given
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.test.Barrier_Control barrier = new Neo4Net.test.Barrier_Control();
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  long before = NumberOfRelationships();
			  Future<long> tx = Threading.execute(new NamedFunctionAnonymousInnerClass(this, graphDb, barrier)
			 , graphDb);
			  barrier.Await();

			  // when
			  long during = NumberOfRelationships();
			  barrier.Release();
			  long whatOtherThreadSees = tx.get();
			  long after = NumberOfRelationships();

			  // then
			  assertEquals( 0, before );
			  assertEquals( 0, during );
			  assertEquals( 2, after );
			  assertEquals( after, whatOtherThreadSees );
		 }

		 private class NamedFunctionAnonymousInnerClass : NamedFunction<GraphDatabaseService, long>
		 {
			 private readonly RelationshipCountsTest _outerInstance;

			 private IGraphDatabaseService _graphDb;
			 private Neo4Net.Test.Barrier_Control _barrier;

			 public NamedFunctionAnonymousInnerClass( RelationshipCountsTest outerInstance, IGraphDatabaseService graphDb, Neo4Net.Test.Barrier_Control barrier ) : base( "create-relationships" )
			 {
				 this.outerInstance = outerInstance;
				 this._graphDb = graphDb;
				 this._barrier = barrier;
			 }

			 public override long? apply( IGraphDatabaseService graphDb )
			 {
				  using ( Transaction tx = graphDb.BeginTx() )
				  {
						Node node = graphDb.CreateNode();
						node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
						node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
						long whatThisThreadSees = outerInstance.countsForRelationship( null, null, null );
						_barrier.reached();
						tx.Success();
						return whatThisThreadSees;
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCountRelationshipsDeletedInOtherTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCountRelationshipsDeletedInOtherTransaction()
		 {
			  // given
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Relationship rel;
			  Relationship rel;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					Node node = graphDb.CreateNode();
					node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					rel = node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					node.CreateRelationshipTo( graphDb.CreateNode(), withName("KNOWS") );
					tx.Success();
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.test.Barrier_Control barrier = new Neo4Net.test.Barrier_Control();
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  long before = NumberOfRelationships();
			  Future<long> tx = Threading.execute(new NamedFunctionAnonymousInnerClass2(this, graphDb, rel, tx, barrier)
			 , graphDb);
			  barrier.Await();

			  // when
			  long during = NumberOfRelationships();
			  barrier.Release();
			  long whatOtherThreadSees = tx.get();
			  long after = NumberOfRelationships();

			  // then
			  assertEquals( 3, before );
			  assertEquals( 3, during );
			  assertEquals( 2, after );
			  assertEquals( after, whatOtherThreadSees );
		 }

		 private class NamedFunctionAnonymousInnerClass2 : NamedFunction<GraphDatabaseService, long>
		 {
			 private readonly RelationshipCountsTest _outerInstance;

			 private IGraphDatabaseService _graphDb;
			 private Relationship _rel;
			 private Transaction _tx;
			 private Neo4Net.Test.Barrier_Control _barrier;

			 public NamedFunctionAnonymousInnerClass2( RelationshipCountsTest outerInstance, IGraphDatabaseService graphDb, Relationship rel, Transaction tx, Neo4Net.Test.Barrier_Control barrier ) : base( "create-relationships" )
			 {
				 this.outerInstance = outerInstance;
				 this._graphDb = graphDb;
				 this._rel = rel;
				 this._tx = tx;
				 this._barrier = barrier;
			 }

			 public override long? apply( IGraphDatabaseService graphDb )
			 {
				  using ( Transaction tx = graphDb.BeginTx() )
				  {
						_rel.delete();
						long whatThisThreadSees = outerInstance.countsForRelationship( null, null, null );
						_barrier.reached();
						tx.Success();
						return whatThisThreadSees;
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountRelationshipsByType()
		 public virtual void ShouldCountRelationshipsByType()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.GraphDatabaseService graphDb = db.getGraphDatabaseAPI();
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.CreateNode().createRelationshipTo(graphDb.CreateNode(), withName("FOO"));
					graphDb.CreateNode().createRelationshipTo(graphDb.CreateNode(), withName("FOO"));
					graphDb.CreateNode().createRelationshipTo(graphDb.CreateNode(), withName("BAR"));
					graphDb.CreateNode().createRelationshipTo(graphDb.CreateNode(), withName("BAR"));
					graphDb.CreateNode().createRelationshipTo(graphDb.CreateNode(), withName("BAR"));
					graphDb.CreateNode().createRelationshipTo(graphDb.CreateNode(), withName("BAZ"));
					tx.Success();
			  }

			  // when
			  long total = NumberOfRelationships();
			  long foo = NumberOfRelationships( withName( "FOO" ) );
			  long bar = NumberOfRelationships( withName( "BAR" ) );
			  long baz = NumberOfRelationships( withName( "BAZ" ) );
			  long qux = NumberOfRelationships( withName( "QUX" ) );

			  // then
			  assertEquals( 2, foo );
			  assertEquals( 3, bar );
			  assertEquals( 1, baz );
			  assertEquals( 0, qux );
			  assertEquals( 6, total );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateRelationshipWithLabelCountsWhenDeletingNodeWithRelationship()
		 public virtual void ShouldUpdateRelationshipWithLabelCountsWhenDeletingNodeWithRelationship()
		 {
			  // given
			  Node foo;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ) );
					Node bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( bar, withName( "BAZ" ) );

					tx.Success();
			  }
			  long before = NumberOfRelationshipsMatching( label( "Foo" ), withName( "BAZ" ), null );

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Relationship relationship in foo.Relationships )
					{
						 relationship.Delete();
					}
					foo.Delete();

					tx.Success();
			  }
			  long after = NumberOfRelationshipsMatching( label( "Foo" ), withName( "BAZ" ), null );

			  // then
			  assertEquals( before - 1, after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateRelationshipWithLabelCountsWhenDeletingNodesWithRelationships()
		 public virtual void ShouldUpdateRelationshipWithLabelCountsWhenDeletingNodesWithRelationships()
		 {
			  // given
			  int numberOfNodes = 2;
			  Node[] nodes = new Node[numberOfNodes];
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < numberOfNodes; i++ )
					{
						 Node foo = Db.createNode( label( "Foo" + i ) );
						 foo.AddLabel( Label.label( "Common" ) );
						 Node bar = Db.createNode( label( "Bar" + i ) );
						 foo.CreateRelationshipTo( bar, withName( "BAZ" + i ) );
						 nodes[i] = foo;
					}

					tx.Success();
			  }

			  long[] beforeCommon = new long[numberOfNodes];
			  long[] before = new long[numberOfNodes];
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					beforeCommon[i] = NumberOfRelationshipsMatching( label( "Common" ), withName( "BAZ" + i ), null );
					before[i] = NumberOfRelationshipsMatching( label( "Foo" + i ), withName( "BAZ" + i ), null );
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in nodes )
					{
						 foreach ( Relationship relationship in node.Relationships )
						 {
							  relationship.Delete();
						 }
						 node.Delete();
					}

					tx.Success();
			  }
			  long[] afterCommon = new long[numberOfNodes];
			  long[] after = new long[numberOfNodes];
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					afterCommon[i] = NumberOfRelationshipsMatching( label( "Common" ), withName( "BAZ" + i ), null );
					after[i] = NumberOfRelationshipsMatching( label( "Foo" + i ), withName( "BAZ" + i ), null );
			  }

			  // then
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					assertEquals( beforeCommon[i] - 1, afterCommon[i] );
					assertEquals( before[i] - 1, after[i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateRelationshipWithLabelCountsWhenRemovingLabelAndDeletingRelationship()
		 public virtual void ShouldUpdateRelationshipWithLabelCountsWhenRemovingLabelAndDeletingRelationship()
		 {
			  // given
			  Node foo;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ) );
					Node bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( bar, withName( "BAZ" ) );

					tx.Success();
			  }
			  long before = NumberOfRelationshipsMatching( label( "Foo" ), withName( "BAZ" ), null );

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Relationship relationship in foo.Relationships )
					{
						 relationship.Delete();
					}
					foo.RemoveLabel( label( "Foo" ) );

					tx.Success();
			  }
			  long after = NumberOfRelationshipsMatching( label( "Foo" ), withName( "BAZ" ), null );

			  // then
			  assertEquals( before - 1, after );
		 }

		 private long NumberOfRelationships( RelationshipType type )
		 {
			  return NumberOfRelationshipsMatching( null, type, null );
		 }

		 private long NumberOfRelationships()
		 {
			  return NumberOfRelationshipsMatching( null, null, null );
		 }

		 /// <summary>
		 /// Transactional version of <seealso cref="countsForRelationship(Label, RelationshipType, Label)"/> </summary>
		 private long NumberOfRelationshipsMatching( Label lhs, RelationshipType type, Label rhs )
		 {
			  using ( Transaction tx = Db.GraphDatabaseAPI.beginTx() )
			  {
					long nodeCount = CountsForRelationship( lhs, type, rhs );
					tx.Success();
					return nodeCount;
			  }
		 }

		 /// <param name="start"> the label of the start node of relationships to get the number of, or {@code null} for "any". </param>
		 /// <param name="type">  the type of the relationships to get the number of, or {@code null} for "any". </param>
		 /// <param name="end">   the label of the end node of relationships to get the number of, or {@code null} for "any". </param>
		 private long CountsForRelationship( Label start, RelationshipType type, Label end )
		 {
			  KernelTransaction ktx = _ktxSupplier.get();
			  using ( Statement ignore = ktx.AcquireStatement() )
			  {
					TokenRead tokenRead = ktx.TokenRead();
					int startId;
					int typeId;
					int endId;
					// start
					if ( start == null )
					{
						 startId = Neo4Net.Kernel.Api.Internal.Read_Fields.ANY_LABEL;
					}
					else
					{
						 if ( Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN == ( startId = tokenRead.NodeLabel( start.Name() ) ) )
						 {
							  return 0;
						 }
					}
					// type
					if ( type == null )
					{
						 typeId = Neo4Net.Kernel.Api.Internal.Read_Fields.ANY_RELATIONSHIP_TYPE;
					}
					else
					{
						 if ( Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN == ( typeId = tokenRead.RelationshipType( type.Name() ) ) )
						 {
							  return 0;
						 }
					}
					// end
					if ( end == null )
					{
						 endId = Neo4Net.Kernel.Api.Internal.Read_Fields.ANY_LABEL;
					}
					else
					{
						 if ( Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN == ( endId = tokenRead.NodeLabel( end.Name() ) ) )
						 {
							  return 0;
						 }
					}
					return ktx.DataRead().countsForRelationship(startId, typeId, endId);
			  }
		 }
	}

}