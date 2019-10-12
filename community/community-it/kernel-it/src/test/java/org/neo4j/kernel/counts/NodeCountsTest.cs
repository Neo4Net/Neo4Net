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
namespace Org.Neo4j.Kernel.counts
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using StatementConstants = Org.Neo4j.Kernel.api.StatementConstants;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using Barrier = Org.Neo4j.Test.Barrier;
	using Org.Neo4j.Test;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;
	using ThreadingRule = Org.Neo4j.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class NodeCountsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.ThreadingRule threading = new org.neo4j.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threading = new ThreadingRule();

		 private System.Func<KernelTransaction> _kernelTransactionSupplier;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _kernelTransactionSupplier = () => Db.GraphDatabaseAPI.DependencyResolver.resolveDependency(typeof(ThreadToStatementContextBridge)).getKernelTransactionBoundToThisThread(true);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNumberOfNodesInAnEmptyGraph()
		 public virtual void ShouldReportNumberOfNodesInAnEmptyGraph()
		 {
			  // when
			  long nodeCount = NumberOfNodes();

			  // then
			  assertEquals( 0, nodeCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNumberOfNodes()
		 public virtual void ShouldReportNumberOfNodes()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.CreateNode();
					graphDb.CreateNode();
					tx.Success();
			  }

			  // when
			  long nodeCount = NumberOfNodes();

			  // then
			  assertEquals( 2, nodeCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportAccurateNumberOfNodesAfterDeletion()
		 public virtual void ShouldReportAccurateNumberOfNodesAfterDeletion()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  Node one;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					one = graphDb.CreateNode();
					graphDb.CreateNode();
					tx.Success();
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					one.Delete();
					tx.Success();
			  }

			  // when
			  long nodeCount = NumberOfNodes();

			  // then
			  assertEquals( 1, nodeCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeNumberOfNodesAddedInTransaction()
		 public virtual void ShouldIncludeNumberOfNodesAddedInTransaction()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.CreateNode();
					graphDb.CreateNode();
					tx.Success();
			  }
			  long before = NumberOfNodes();

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					// when
					graphDb.CreateNode();
					long nodeCount = CountsForNode();

					// then
					assertEquals( before + 1, nodeCount );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeNumberOfNodesDeletedInTransaction()
		 public virtual void ShouldIncludeNumberOfNodesDeletedInTransaction()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
			  Node one;
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					one = graphDb.CreateNode();
					graphDb.CreateNode();
					tx.Success();
			  }
			  long before = NumberOfNodes();

			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					// when
					one.Delete();
					long nodeCount = CountsForNode();

					// then
					assertEquals( before - 1, nodeCount );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeNodeCountsOfOtherTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeNodeCountsOfOtherTransaction()
		 {
			  // given
			  GraphDatabaseService graphDb = Db.GraphDatabaseAPI;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control barrier = new org.neo4j.test.Barrier_Control();
			  Org.Neo4j.Test.Barrier_Control barrier = new Org.Neo4j.Test.Barrier_Control();
			  long before = NumberOfNodes();
			  Future<long> done = Threading.execute(new NamedFunctionAnonymousInnerClass(this, graphDb, barrier)
			 , graphDb);
			  barrier.Await();

			  // when
			  long during = NumberOfNodes();
			  barrier.Release();
			  long whatOtherThreadSees = done.get();
			  long after = NumberOfNodes();

			  // then
			  assertEquals( 0, before );
			  assertEquals( 0, during );
			  assertEquals( after, whatOtherThreadSees );
			  assertEquals( 2, after );
		 }

		 private class NamedFunctionAnonymousInnerClass : NamedFunction<GraphDatabaseService, long>
		 {
			 private readonly NodeCountsTest _outerInstance;

			 private GraphDatabaseService _graphDb;
			 private Org.Neo4j.Test.Barrier_Control _barrier;

			 public NamedFunctionAnonymousInnerClass( NodeCountsTest outerInstance, GraphDatabaseService graphDb, Org.Neo4j.Test.Barrier_Control barrier ) : base( "create-nodes" )
			 {
				 this.outerInstance = outerInstance;
				 this._graphDb = graphDb;
				 this._barrier = barrier;
			 }

			 public override long? apply( GraphDatabaseService graphDb )
			 {
				  using ( Transaction tx = graphDb.BeginTx() )
				  {
						graphDb.CreateNode();
						graphDb.CreateNode();
						_barrier.reached();
						long whatThisThreadSees = outerInstance.countsForNode();
						tx.Success();
						return whatThisThreadSees;
				  }
			 }
		 }

		 /// <summary>
		 /// Transactional version of <seealso cref="countsForNode()"/> </summary>
		 private long NumberOfNodes()
		 {
			  using ( Transaction tx = Db.GraphDatabaseAPI.beginTx() )
			  {
					long nodeCount = CountsForNode();
					tx.Success();
					return nodeCount;
			  }
		 }

		 private long CountsForNode()
		 {
			  return _kernelTransactionSupplier.get().dataRead().countsForNode(StatementConstants.ANY_LABEL);
		 }
	}

}