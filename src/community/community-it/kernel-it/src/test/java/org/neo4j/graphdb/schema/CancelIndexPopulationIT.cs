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


	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Barrier = Neo4Net.Test.Barrier;
	using TestLabels = Neo4Net.Test.TestLabels;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.first;

	public class CancelIndexPopulationIT
	{
		 private const Label LABEL = TestLabels.LABEL_ONE;
		 private const string KEY = "key";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.EmbeddedDatabaseRule db = new Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public readonly EmbeddedDatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepIndexInPopulatingStateBetweenRestarts() throws InterruptedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepIndexInPopulatingStateBetweenRestarts()
		 {
			  // given
			  Monitors monitors = Db.DependencyResolver.resolveDependency( typeof( Monitors ) );
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  monitors.AddMonitorListener( PopulationCompletionBlocker( barrier ) );

			  // when
			  CreateRelevantNode();
			  CreateIndex();
			  barrier.Await();
			  // This call will eventually make a call to populationCancelled on the monitor below
			  Db.restartDatabase();

			  // then
			  assertEquals( Schema_IndexState.Online, AwaitAndGetIndexState() );
		 }

		 private Schema_IndexState AwaitAndGetIndexState()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IndexDefinition indexDefinition = first( Db.schema().getIndexes(LABEL) );
					Db.schema().awaitIndexOnline(indexDefinition, 1, TimeUnit.MINUTES);
					Schema_IndexState indexState = Db.schema().getIndexState(indexDefinition);
					tx.Success();
					return indexState;
			  }
		 }

		 private void CreateIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(LABEL).on(KEY).create();
					tx.Success();
			  }
		 }

		 private void CreateRelevantNode()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( LABEL ).setProperty( KEY, "value" );
					tx.Success();
			  }
		 }

		 private IndexingService.MonitorAdapter PopulationCompletionBlocker( Neo4Net.Test.Barrier_Control barrier )
		 {
			  return new MonitorAdapterAnonymousInnerClass( this, barrier );
		 }

		 private class MonitorAdapterAnonymousInnerClass : IndexingService.MonitorAdapter
		 {
			 private readonly CancelIndexPopulationIT _outerInstance;

			 private Neo4Net.Test.Barrier_Control _barrier;

			 public MonitorAdapterAnonymousInnerClass( CancelIndexPopulationIT outerInstance, Neo4Net.Test.Barrier_Control barrier )
			 {
				 this.outerInstance = outerInstance;
				 this._barrier = barrier;
			 }

			 public override void indexPopulationScanComplete()
			 {
				  _barrier.reached();
			 }

			 public override void populationCancelled()
			 {
				  // When we get this call we know that the population is still active (due to being blocked in indexPopulationScanComplete())
				  // and have just gotten a call to being cancelled, which should now be known to index populators.
				  _barrier.release();
			 }
		 }
	}

}