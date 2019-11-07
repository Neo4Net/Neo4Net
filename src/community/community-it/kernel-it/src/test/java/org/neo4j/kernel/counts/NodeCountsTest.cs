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
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using StatementConstants = Neo4Net.Kernel.Api.StatementConstants;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using Barrier = Neo4Net.Test.Barrier;
	using Neo4Net.Test;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class NodeCountsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.DatabaseRule db = new Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.concurrent.ThreadingRule threading = new Neo4Net.test.rule.concurrent.ThreadingRule();
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
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
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
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
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
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
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
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
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
			  IGraphDatabaseService graphDb = Db.GraphDatabaseAPI;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.test.Barrier_Control barrier = new Neo4Net.test.Barrier_Control();
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
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

			 private IGraphDatabaseService _graphDb;
			 private Neo4Net.Test.Barrier_Control _barrier;

			 public NamedFunctionAnonymousInnerClass( NodeCountsTest outerInstance, IGraphDatabaseService graphDb, Neo4Net.Test.Barrier_Control barrier ) : base( "create-nodes" )
			 {
				 this.outerInstance = outerInstance;
				 this._graphDb = graphDb;
				 this._barrier = barrier;
			 }

			 public override long? apply( IGraphDatabaseService graphDb )
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