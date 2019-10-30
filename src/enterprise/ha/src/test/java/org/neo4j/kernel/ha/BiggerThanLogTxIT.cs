using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using TransactionTemplate = Neo4Net.Helpers.TransactionTemplate;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.parseLongWithUnit;

	public class BiggerThanLogTxIT
	{
		 private const string ROTATION_THRESHOLD = "1M";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withSharedSetting(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.logical_log_rotation_threshold, ROTATION_THRESHOLD);
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(GraphDatabaseSettings.logical_log_rotation_threshold, ROTATION_THRESHOLD);

		 protected internal ClusterManager.ManagedCluster Cluster;

		 private readonly TransactionTemplate _template = new TransactionTemplate().retries(10).backoff(3, TimeUnit.SECONDS);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSlaveCommittingLargeTx()
		 public virtual void ShouldHandleSlaveCommittingLargeTx()
		 {
			  // GIVEN
			  IGraphDatabaseService slave = Cluster.AnySlave;
			  long initialNodeCount = NodeCount( slave );

			  // WHEN
			  Cluster.info( "Before commit large" );
			  int nodeCount = CommitLargeTx( slave );
			  Cluster.info( "Before sync" );
			  Cluster.sync();
			  Cluster.info( "After sync" );

			  // THEN all should have that tx
			  AssertAllMembersHasNodeCount( initialNodeCount + nodeCount );
			  // and if then master commits something, they should all get that too
			  Cluster.info( "Before commit small" );
			  CommitSmallTx( Cluster.Master );
			  Cluster.info( "Before sync small" );
			  Cluster.sync();
			  Cluster.info( "After sync small" );
			  AssertAllMembersHasNodeCount( initialNodeCount + nodeCount + 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMasterCommittingLargeTx()
		 public virtual void ShouldHandleMasterCommittingLargeTx()
		 {
			  // GIVEN
			  IGraphDatabaseService slave = Cluster.AnySlave;
			  long initialNodeCount = NodeCount( slave );

			  // WHEN
			  int nodeCount = CommitLargeTx( Cluster.Master );
			  Cluster.sync();

			  // THEN all should have that tx
			  AssertAllMembersHasNodeCount( initialNodeCount + nodeCount );
			  // and if then master commits something, they should all get that too
			  CommitSmallTx( Cluster.Master );
			  Cluster.sync();
			  AssertAllMembersHasNodeCount( initialNodeCount + nodeCount + 1 );
		 }

		 private void CommitSmallTx( IGraphDatabaseService db )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					Db.createNode();
					transaction.Success();
			  }
		 }

		 private long NodeCount( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					long count = Iterables.count( Db.AllNodes );
					tx.Success();
					return count;
			  }
		 }

		 private void AssertAllMembersHasNodeCount( long expectedNodeCount )
		 {
			  foreach ( IGraphDatabaseService db in Cluster.AllMembers )
			  {
					// Try again with sync, it will clear up...
					if ( expectedNodeCount != NodeCount( db ) )
					{
						 for ( int i = 0; i < 100; i++ )
						 {
							  try
							  {
									Thread.Sleep( 1000 );
							  }
							  catch ( InterruptedException e )
							  {
									throw new Exception( e );
							  }

							  long count = NodeCount( db );
							  if ( expectedNodeCount == count )
							  {
									break;
							  }

							  Cluster.sync();
						 }
					}

					assertEquals( expectedNodeCount, NodeCount( db ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private int commitLargeTx(final org.Neo4Net.graphdb.GraphDatabaseService db)
		 private int CommitLargeTx( IGraphDatabaseService db )
		 {
			  return _template.with( db ).execute(transaction =>
			  {
				// We're not actually asserting that this transaction produces log data
				// bigger than the threshold.
				long rotationThreshold = parseLongWithUnit( ROTATION_THRESHOLD );
				int nodeCount = 100;
				sbyte[] arrayProperty = new sbyte[( int )( rotationThreshold / nodeCount )];
				for ( int i = 0; i < nodeCount; i++ )
				{
					 Node node = Db.createNode();
					 node.setProperty( "name", "big" + i );
					 node.setProperty( "data", arrayProperty );
				}
				return nodeCount;
			  });
		 }
	}

}