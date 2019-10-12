using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test, but a tool to measure an isolation characteristic where a change will be visible in an index " + "will see changes after being visible in the record store. This test tries to measure how big that gap is") public class MeasureUpdatePullingRecordAndIndexGap
	public class MeasureUpdatePullingRecordAndIndexGap
	{
		 private readonly int _numberOfIndexes = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withSharedSetting(HaSettings.tx_push_factor, "0");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withSharedSetting(HaSettings.TxPushFactor, "0");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMeasureThatGap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMeasureThatGap()
		 {
			  // GIVEN
			  ManagedCluster cluster = ClusterRule.startCluster();
			  CreateIndexes( cluster.Master );
			  cluster.Sync();
			  AwaitIndexes( cluster );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean halter = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean halter = new AtomicBoolean();
			  AtomicLong[] highIdNodes = new AtomicLong[_numberOfIndexes];
			  System.Threading.CountdownEvent endLatch = new System.Threading.CountdownEvent( _numberOfIndexes + 1 );
			  for ( int i = 0; i < highIdNodes.Length; i++ )
			  {
					highIdNodes[i] = new AtomicLong();
			  }
			  StartLoadOn( cluster.Master, halter, highIdNodes, endLatch );
			  GraphDatabaseAPI slave = cluster.AnySlave;
			  StartCatchingUp( slave, halter, endLatch );

			  // WHEN measuring...
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger good = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger good = new AtomicInteger();
			  AtomicInteger bad = new AtomicInteger();
			  AtomicInteger ugly = new AtomicInteger();
			  StartMeasuringTheGap( good, bad, ugly, halter, highIdNodes, slave );
			  for ( long endTime = currentTimeMillis() + SECONDS.toMillis(30); currentTimeMillis() < endTime; )
			  {
					PrintStats( good.get(), bad.get(), ugly.get() );
					try
					{
						 Thread.Sleep( 1000 );
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
			  }
			  halter.set( true );
			  endLatch.await();

			  // THEN
			  PrintStats( good.get(), bad.get(), ugly.get() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void startMeasuringTheGap(final java.util.concurrent.atomic.AtomicInteger good, final java.util.concurrent.atomic.AtomicInteger bad, final java.util.concurrent.atomic.AtomicInteger ugly, final java.util.concurrent.atomic.AtomicBoolean halter, final java.util.concurrent.atomic.AtomicLong[] highIdNodes, final org.neo4j.kernel.internal.GraphDatabaseAPI db)
		 private void StartMeasuringTheGap( AtomicInteger good, AtomicInteger bad, AtomicInteger ugly, AtomicBoolean halter, AtomicLong[] highIdNodes, GraphDatabaseAPI db )
		 {
			  (new Thread(() =>
			  {
			  while ( !halter.get() )
			  {
				  for ( int i = 0; i < _numberOfIndexes; i++ )
				  {
					  long targetNodeId = highIdNodes[i].get();
					  if ( targetNodeId == 0 )
					  {
						  continue;
					  }
					  using ( Transaction tx = Db.beginTx() )
					  {
						  Node nodeFromStorePOV = null;
						  long nodeId = targetNodeId;
						  while ( nodeFromStorePOV == null && !halter.get() )
						  {
							  try
							  {
								  nodeFromStorePOV = Db.getNodeById( nodeId );
							  }
							  catch ( NotFoundException )
							  {
								  nodeId = max( nodeId - 1, 0 );
								  try
								  {
									  Thread.Sleep( 10 );
								  }
								  catch ( InterruptedException e1 )
								  {
									  throw new Exception( e1 );
								  }
							  }
						  }
						  Node nodeFromIndexPOV = Db.findNode( Label( i ), Key( i ), nodeId );
						  tx.success();
						  if ( nodeFromIndexPOV != null )
						  {
							  good.incrementAndGet();
						  }
						  else
						  {
							  bad.incrementAndGet();
						  }
						  if ( nodeId != targetNodeId )
						  {
							  ugly.incrementAndGet();
						  }
					  }
				  }
			  }
			  })).Start();
		 }

		 private void PrintStats( int good, int bad, int ugly )
		 {
			  double total = good + bad;
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: System.out.printf("good: %.1f%%, bad: %.1f%%, ugly: %.1f%% (out of a total of %.0f)%n", 100.0 * good / total, 100.0 * bad / total, 100.0 * ugly / total, total);
			  Console.Write( "good: %.1f%%, bad: %.1f%%, ugly: %.1f%% (out of a total of %.0f)%n", 100.0 * good / total, 100.0 * bad / total, 100.0 * ugly / total, total );
		 }

		 private void AwaitIndexes( ManagedCluster cluster )
		 {
			  foreach ( GraphDatabaseService db in cluster.AllMembers )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().awaitIndexesOnline(1, MINUTES);
						 tx.Success();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void startCatchingUp(final org.neo4j.kernel.internal.GraphDatabaseAPI db, final java.util.concurrent.atomic.AtomicBoolean halter, final java.util.concurrent.CountDownLatch endLatch)
		 private void StartCatchingUp( GraphDatabaseAPI db, AtomicBoolean halter, System.Threading.CountdownEvent endLatch )
		 {
			  (new Thread(() =>
			  {
			  try
			  {
				  while ( !halter.get() )
				  {
					  try
					  {
						  Db.DependencyResolver.resolveDependency( typeof( UpdatePuller ) ).pullUpdates();
					  }
					  catch ( InterruptedException e )
					  {
						  Thread.CurrentThread.Interrupt();
						  throw new Exception( e );
					  }
				  }
			  }
			  finally
			  {
				  endLatch.Signal();
			  }
			  })).Start();
		 }

		 private void CreateIndexes( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < _numberOfIndexes; i++ )
					{
						 Db.schema().indexFor(Label(i)).on(Key(i)).create();
					}
					tx.Success();
			  }
		 }

		 private string Key( int i )
		 {
			  return "key-" + i;
		 }

		 private Label Label( int i )
		 {
			  return Label.label( "Label" + i );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void startLoadOn(final org.neo4j.graphdb.GraphDatabaseService db, final java.util.concurrent.atomic.AtomicBoolean halter, final java.util.concurrent.atomic.AtomicLong[] highIdNodes, final java.util.concurrent.CountDownLatch endLatch)
		 private void StartLoadOn( GraphDatabaseService db, AtomicBoolean halter, AtomicLong[] highIdNodes, System.Threading.CountdownEvent endLatch )
		 {
			  for ( int i = 0; i < _numberOfIndexes; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int x = i;
					int x = i;
					(new Thread(() =>
					{
					try
					{
						while ( !halter.get() )
						{
							long nodeId;
							using ( Transaction tx = Db.beginTx() )
							{
								Node node = Db.createNode( Label( x ) );
								node.setProperty( Key( x ), nodeId = node.Id );
								tx.success();
							}
							highIdNodes[x].set( nodeId );
						}
					}
					finally
					{
						endLatch.Signal();
					}
					})).Start();
			  }
		 }
	}

}