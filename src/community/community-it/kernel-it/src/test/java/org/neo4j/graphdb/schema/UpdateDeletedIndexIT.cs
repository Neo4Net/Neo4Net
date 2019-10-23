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
namespace Neo4Net.GraphDb.Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using Race = Neo4Net.Test.Race;
	using TestLabels = Neo4Net.Test.TestLabels;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.Race.throwing;

	public class UpdateDeletedIndexIT
	{
		 private const TestLabels LABEL = TestLabels.LABEL_ONE;
		 private const string KEY = "key";
		 private const int NODES = 100;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUpdateRemovalOfLabelConcurrentlyWithIndexDrop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleUpdateRemovalOfLabelConcurrentlyWithIndexDrop()
		 {
			  ShouldHandleIndexDropConcurrentlyWithOperation( nodeId => Db.getNodeById( nodeId ).removeLabel( LABEL ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDeleteNodeConcurrentlyWithIndexDrop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDeleteNodeConcurrentlyWithIndexDrop()
		 {
			  ShouldHandleIndexDropConcurrentlyWithOperation( nodeId => Db.getNodeById( nodeId ).delete() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRemovePropertyConcurrentlyWithIndexDrop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleRemovePropertyConcurrentlyWithIndexDrop()
		 {
			  ShouldHandleIndexDropConcurrentlyWithOperation( nodeId => Db.getNodeById( nodeId ).removeProperty( KEY ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNodeDetachDeleteConcurrentlyWithIndexDrop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleNodeDetachDeleteConcurrentlyWithIndexDrop()
		 {
			  ShouldHandleIndexDropConcurrentlyWithOperation(nodeId =>
			  {
				ThreadToStatementContextBridge txBridge = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
				txBridge.getKernelTransactionBoundToThisThread( true ).dataWrite().nodeDetachDelete(nodeId);
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldHandleIndexDropConcurrentlyWithOperation(NodeOperation operation) throws Throwable
		 private void ShouldHandleIndexDropConcurrentlyWithOperation( NodeOperation operation )
		 {
			  // given
			  long[] nodes = CreateNodes();
			  IndexDefinition indexDefinition = CreateIndex();

			  // when
			  Race race = new Race();
			  race.AddContestant(() =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 indexDefinition.Drop();
					 tx.Success();
				}
			  }, 1);
			  for ( int i = 0; i < NODES; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId = nodes[i];
					long nodeId = nodes[i];
					race.AddContestant(throwing(() =>
					{
					 using ( Transaction tx = Db.beginTx() )
					 {
						  operation.Run( nodeId );
						  tx.Success();
					 }
					}));
			  }

			  // then
			  race.Go();
		 }

		 private long[] CreateNodes()
		 {
			  long[] nodes = new long[NODES];
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < NODES; i++ )
					{
						 Node node = Db.createNode( LABEL );
						 node.SetProperty( KEY, i );
						 nodes[i] = node.Id;
					}
					tx.Success();
			  }
			  return nodes;
		 }

		 private IndexDefinition CreateIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < NODES; i++ )
					{
						 Db.createNode( LABEL ).setProperty( KEY, i );
					}
					tx.Success();
			  }
			  IndexDefinition indexDefinition;
			  using ( Transaction tx = Db.beginTx() )
			  {
					indexDefinition = Db.schema().indexFor(LABEL).on(KEY).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }
			  return indexDefinition;
		 }

		 private interface NodeOperation
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void run(long nodeId) throws Exception;
			  void Run( long nodeId );
		 }
	}

}