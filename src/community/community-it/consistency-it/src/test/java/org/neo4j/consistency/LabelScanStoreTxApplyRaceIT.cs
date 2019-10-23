using System.Threading;

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
namespace Neo4Net.Consistency
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;
	using Race = Neo4Net.Test.Race;
	using TestLabels = Neo4Net.Test.TestLabels;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.progress.ProgressMonitorFactory.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Config.defaults;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.FormattedLogProvider.toOutputStream;

	/// <summary>
	/// This is a test for triggering a race which was found in and around <seealso cref="RecordStorageEngine.apply(CommandsToApply, TransactionApplicationMode)"/>
	/// where e.g. a transaction A which did CREATE NODE N and transaction B which did DELETE NODE N would have a chance to be applied to the
	/// <seealso cref="LabelScanStore"/> in the reverse order, i.e. transaction B before transaction A, resulting in outdated label data remaining in the label index.
	/// </summary>
	public class LabelScanStoreTxApplyRaceIT
	{
		 // === CONTROL PANEL ===
		 private const int NUMBER_OF_DELETORS = 2;
		 private static readonly int _numberOfCreators = max( 2, Runtime.Runtime.availableProcessors() - NUMBER_OF_DELETORS );
		 private const float CHANCE_LARGE_TX = 0.1f;
		 private const float CHANCE_TO_DELETE_BY_SAME_THREAD = 0.9f;
		 private const int LARGE_TX_SIZE = 3_000;

		 private static readonly Label[] _labels = Enum.GetValues( typeof( TestLabels ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.EmbeddedDatabaseRule db = new org.Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public readonly EmbeddedDatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

		 /// <summary>
		 /// The test case is basically loads of concurrent CREATE/DELETE NODE or sometimes just CREATE, keeping the created node in an array
		 /// for dedicated deleter threads to pick up and delete as fast as they can see them. This concurrently with large creation transactions.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStressIt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStressIt()
		 {
			  // given
			  Race race = ( new Race() ).withMaxDuration(5, TimeUnit.SECONDS);
			  AtomicReferenceArray<Node> nodeHeads = new AtomicReferenceArray<Node>( _numberOfCreators );
			  for ( int i = 0; i < _numberOfCreators; i++ )
			  {
					race.AddContestant( Creator( nodeHeads, i ) );
			  }
			  race.AddContestants( NUMBER_OF_DELETORS, Deleter( nodeHeads ) );

			  // when
			  race.Go();

			  // then
			  DatabaseLayout dbLayout = Db.databaseLayout();
			  Db.shutdownAndKeepStore();
			  assertTrue( ( new ConsistencyCheckService() ).runFullConsistencyCheck(dbLayout, defaults(), NONE, toOutputStream(System.out), false, new ConsistencyFlags(true, true, true, true, false)).Successful );
		 }

		 private ThreadStart Creator( AtomicReferenceArray<Node> nodeHeads, int guy )
		 {
			  return new RunnableAnonymousInnerClass( this, nodeHeads, guy );
		 }

		 private class RunnableAnonymousInnerClass : ThreadStart
		 {
			 private readonly LabelScanStoreTxApplyRaceIT _outerInstance;

			 private AtomicReferenceArray<Node> _nodeHeads;
			 private int _guy;

			 public RunnableAnonymousInnerClass( LabelScanStoreTxApplyRaceIT outerInstance, AtomicReferenceArray<Node> nodeHeads, int guy )
			 {
				 this.outerInstance = outerInstance;
				 this._nodeHeads = nodeHeads;
				 this._guy = guy;
				 random = ThreadLocalRandom.current();
			 }

			 private readonly ThreadLocalRandom random;

			 public void run()
			 {
				  if ( random.nextFloat() < CHANCE_LARGE_TX )
				  {
						// Few large transactions
						using ( Transaction tx = _outerInstance.db.beginTx() )
						{
							 for ( int i = 0; i < LARGE_TX_SIZE; i++ )
							 {
								  // Nodes are created with properties here. Whereas the properties don't have a functional
								  // impact on this test they do affect timings so that the issue is (was) triggered more often
								  // and therefore have a positive effect on this test.
								  _outerInstance.db.createNode( randomLabels() ).setProperty("name", randomUUID().ToString());
							 }
							 tx.Success();
						}
				  }
				  else
				  {
						// Many small create/delete transactions
						Node node;
						using ( Transaction tx = _outerInstance.db.beginTx() )
						{
							 node = _outerInstance.db.createNode( randomLabels() );
							 _nodeHeads.set( _guy, node );
							 tx.Success();
						}
						if ( random.nextFloat() < CHANCE_TO_DELETE_BY_SAME_THREAD )
						{
							 // Most of the time delete in this thread
							 if ( _nodeHeads.getAndSet( _guy, null ) != null )
							 {
								  using ( Transaction tx = _outerInstance.db.beginTx() )
								  {
										node.Delete();
										tx.Success();
								  }
							 }
							 // Otherwise there will be other threads sitting there waiting for these nodes and deletes them if they can
						}
				  }
			 }

			 private Label[] randomLabels()
			 {
				  Label[] labels = new Label[_labels.Length];
				  int cursor = 0;
				  for ( int i = 0; i < labels.Length; i++ )
				  {
						if ( random.nextBoolean() )
						{
							 labels[cursor++] = _labels[i];
						}
				  }
				  if ( cursor == 0 )
				  {
						// at least one
						labels[cursor++] = _labels[0];
				  }
				  return Arrays.copyOf( labels, cursor );
			 }
		 }

		 private ThreadStart Deleter( AtomicReferenceArray<Node> nodeHeads )
		 {
			  return new RunnableAnonymousInnerClass2( this, nodeHeads );
		 }

		 private class RunnableAnonymousInnerClass2 : ThreadStart
		 {
			 private readonly LabelScanStoreTxApplyRaceIT _outerInstance;

			 private AtomicReferenceArray<Node> _nodeHeads;

			 public RunnableAnonymousInnerClass2( LabelScanStoreTxApplyRaceIT outerInstance, AtomicReferenceArray<Node> nodeHeads )
			 {
				 this.outerInstance = outerInstance;
				 this._nodeHeads = nodeHeads;
				 random = ThreadLocalRandom.current();
			 }

			 internal ThreadLocalRandom random;

			 public void run()
			 {
				  int guy = random.Next( _numberOfCreators );
				  Node node = _nodeHeads.getAndSet( guy, null );
				  if ( node != null )
				  {
						try
						{
								using ( Transaction tx = _outerInstance.db.beginTx() )
								{
								 node.Delete();
								 tx.Success();
								}
						}
						catch ( NotFoundException )
						{
							 // This is OK in this test
						}
				  }
			 }
		 }
	}

}