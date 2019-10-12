using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.api
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;
	using Timeout = org.junit.rules.Timeout;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using NodeValueIndexCursor = Neo4Net.@internal.Kernel.Api.NodeValueIndexCursor;
	using Write = Neo4Net.@internal.Kernel.Api.Write;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptor.Type.UNIQUE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CompositeIndexingIT
	public class CompositeIndexingIT
	{
		 private const int LABEL_ID = 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public readonly TestName TestName = new TestName();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.Timeout globalTimeout = org.junit.rules.Timeout.seconds(200);
		 public Timeout GlobalTimeout = Timeout.seconds( 200 );

		 private readonly IndexDescriptor _index;
		 private GraphDatabaseAPI _graphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _graphDatabaseAPI = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					if ( _index.type() == UNIQUE )
					{
						 ktx.SchemaWrite().uniquePropertyConstraintCreate(_index.schema());
					}
					else
					{
						 ktx.SchemaWrite().indexCreate(_index.schema());
					}
					tx.Success();
			  }

			  using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					while ( ktx.SchemaRead().indexGetState(_index) != InternalIndexState.ONLINE )
					{
						 Thread.Sleep( 10 );
					} // Will break loop on test timeout
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void clean() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Clean()
		 {
			  using ( Transaction tx = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					if ( _index.type() == UNIQUE )
					{
						 ktx.SchemaWrite().constraintDrop(ConstraintDescriptorFactory.uniqueForSchema(_index.schema()));
					}
					else
					{
						 ktx.SchemaWrite().indexDrop(_index);
					}
					tx.Success();
			  }

			  using ( Transaction tx = _graphDatabaseAPI.beginTx() )
			  {
					foreach ( Node node in _graphDatabaseAPI.AllNodes )
					{
						 node.Delete();
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "Index: {0}") public static Iterable<Object[]> parameterValues()
		 public static IEnumerable<object[]> ParameterValues()
		 {
			  return Arrays.asList( Iterators.array( TestIndexDescriptorFactory.forLabel( LABEL_ID, 1 ) ), Iterators.array( TestIndexDescriptorFactory.forLabel( LABEL_ID, 1, 2 ) ), Iterators.array( TestIndexDescriptorFactory.forLabel( LABEL_ID, 1, 2, 3, 4 ) ), Iterators.array( TestIndexDescriptorFactory.forLabel( LABEL_ID, 1, 2, 3, 4, 5, 6, 7 ) ), Iterators.array( TestIndexDescriptorFactory.uniqueForLabel( LABEL_ID, 1 ) ), Iterators.array( TestIndexDescriptorFactory.uniqueForLabel( LABEL_ID, 1, 2 ) ), Iterators.array( TestIndexDescriptorFactory.uniqueForLabel( LABEL_ID, 1, 2, 3, 4, 5, 6, 7 ) ) );
		 }

		 public CompositeIndexingIT( IndexDescriptor nodeDescriptor )
		 {
			  this._index = nodeDescriptor;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNodeAddedByPropertyToIndexInTranslation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNodeAddedByPropertyToIndexInTranslation()
		 {
			  using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					Write write = ktx.DataWrite();
					long nodeID = write.NodeCreate();
					write.NodeAddLabel( nodeID, LABEL_ID );
					foreach ( int propID in _index.schema().PropertyIds )
					{
						 write.NodeSetProperty( nodeID, propID, Values.intValue( propID ) );
					}
					using ( NodeValueIndexCursor cursor = Seek( ktx ) )
					{
						 assertTrue( cursor.Next() );
						 assertThat( cursor.NodeReference(), equalTo(nodeID) );
						 assertFalse( cursor.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNodeAddedToByLabelIndexInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNodeAddedToByLabelIndexInTransaction()
		 {
			  using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					Write write = ktx.DataWrite();
					long nodeID = write.NodeCreate();
					foreach ( int propID in _index.schema().PropertyIds )
					{
						 write.NodeSetProperty( nodeID, propID, Values.intValue( propID ) );
					}
					write.NodeAddLabel( nodeID, LABEL_ID );
					using ( NodeValueIndexCursor cursor = Seek( ktx ) )
					{
						 assertTrue( cursor.Next() );
						 assertThat( cursor.NodeReference(), equalTo(nodeID) );
						 assertFalse( cursor.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeNodeThatWasDeletedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeNodeThatWasDeletedInTransaction()
		 {
			  long nodeID = CreateNode();
			  using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					ktx.DataWrite().nodeDelete(nodeID);
					using ( NodeValueIndexCursor cursor = Seek( ktx ) )
					{
						 assertFalse( cursor.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeNodeThatHasItsLabelRemovedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeNodeThatHasItsLabelRemovedInTransaction()
		 {
			  long nodeID = CreateNode();
			  using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					ktx.DataWrite().nodeRemoveLabel(nodeID, LABEL_ID);
					using ( NodeValueIndexCursor cursor = Seek( ktx ) )
					{
						 assertFalse( cursor.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeNodeThatHasAPropertyRemovedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeNodeThatHasAPropertyRemovedInTransaction()
		 {
			  long nodeID = CreateNode();
			  using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					ktx.DataWrite().nodeRemoveProperty(nodeID, _index.schema().PropertyIds[0]);
					using ( NodeValueIndexCursor cursor = Seek( ktx ) )
					{
						 assertFalse( cursor.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeAllNodesAddedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeAllNodesAddedInTransaction()
		 {
			  if ( _index.type() != UNIQUE ) // this test does not make any sense for UNIQUE indexes
			  {
					using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
					{
						 long nodeID1 = CreateNode();
						 long nodeID2 = CreateNode();
						 long nodeID3 = CreateNode();
						 KernelTransaction ktx = ktx();
						 ISet<long> result = new HashSet<long>();
						 using ( NodeValueIndexCursor cursor = Seek( ktx ) )
						 {
							  while ( cursor.Next() )
							  {
									result.Add( cursor.NodeReference() );
							  }
						 }
						 assertThat( result, containsInAnyOrder( nodeID1, nodeID2, nodeID3 ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeAllNodesAddedBeforeTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeAllNodesAddedBeforeTransaction()
		 {
			  if ( _index.type() != UNIQUE ) // this test does not make any sense for UNIQUE indexes
			  {
					long nodeID1 = CreateNode();
					long nodeID2 = CreateNode();
					long nodeID3 = CreateNode();
					using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
					{
						 KernelTransaction ktx = ktx();
						 ISet<long> result = new HashSet<long>();
						 using ( NodeValueIndexCursor cursor = Seek( ktx ) )
						 {
							  while ( cursor.Next() )
							  {
									result.Add( cursor.NodeReference() );
							  }
						 }
						 assertThat( result, containsInAnyOrder( nodeID1, nodeID2, nodeID3 ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeNodesLackingOneProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeNodesLackingOneProperty()
		 {
			  long nodeID1 = CreateNode();
			  using ( Transaction ignore = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					Write write = ktx.DataWrite();
					long irrelevantNodeID = write.NodeCreate();
					write.NodeAddLabel( irrelevantNodeID, LABEL_ID );
					int[] propertyIds = _index.schema().PropertyIds;
					for ( int i = 0; i < propertyIds.Length - 1; i++ )
					{
						 int propID = propertyIds[i];
						 write.NodeSetProperty( irrelevantNodeID, propID, Values.intValue( propID ) );
					}
					ISet<long> result = new HashSet<long>();
					using ( NodeValueIndexCursor cursor = Seek( ktx ) )
					{
						 while ( cursor.Next() )
						 {
							  result.Add( cursor.NodeReference() );
						 }
					}
					assertThat( result, contains( nodeID1 ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNode() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreateNode()
		 {
			  long nodeID;
			  using ( Transaction tx = _graphDatabaseAPI.beginTx() )
			  {
					KernelTransaction ktx = ktx();
					Write write = ktx.DataWrite();
					nodeID = write.NodeCreate();
					write.NodeAddLabel( nodeID, LABEL_ID );
					foreach ( int propID in _index.schema().PropertyIds )
					{
						 write.NodeSetProperty( nodeID, propID, Values.intValue( propID ) );
					}
					tx.Success();
			  }
			  return nodeID;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.NodeValueIndexCursor seek(KernelTransaction transaction) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private NodeValueIndexCursor Seek( KernelTransaction transaction )
		 {
			  NodeValueIndexCursor cursor = transaction.Cursors().allocateNodeValueIndexCursor();
			  transaction.DataRead().nodeIndexSeek(_index, cursor, IndexOrder.NONE, false, ExactQuery());
			  return cursor;
		 }

		 private IndexQuery[] ExactQuery()
		 {
			  int[] propertyIds = _index.schema().PropertyIds;
			  IndexQuery[] query = new IndexQuery[propertyIds.Length];
			  for ( int i = 0; i < query.Length; i++ )
			  {
					int propID = propertyIds[i];
					query[i] = IndexQuery.exact( propID, Values.of( propID ) );
			  }
			  return query;
		 }

		 private KernelTransaction Ktx()
		 {
			  return _graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
		 }
	}

}