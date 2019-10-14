/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using MasterImpl = Neo4Net.Kernel.ha.com.master.MasterImpl;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;

	/// <summary>
	/// Determines when slaves should initialize a transaction on the master. This is particularly relevant for read operations
	/// where we want slaves to be fast, and preferably not go to the master at all.
	/// </summary>
	public class WhenToInitializeTransactionOnMasterFromSlaveIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public ClusterRule ClusterRule = new ClusterRule();

		 private GraphDatabaseService _slave;
		 private ClusterManager.ManagedCluster _cluster;

		 private MasterImpl.Monitor _masterMonitor = mock( typeof( MasterImpl.Monitor ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _cluster = ClusterRule.startCluster();
			  _slave = _cluster.AnySlave;

			  // Create some basic data
			  using ( Transaction tx = _slave.beginTx() )
			  {
					Node node = _slave.createNode( Label.label( "Person" ) );
					node.SetProperty( "name", "Bob" );
					node.CreateRelationshipTo( _slave.createNode(), RelationshipType.withName("KNOWS") );

					tx.Success();
			  }

			  // And now monitor the master for incoming calls
			  _cluster.Master.DependencyResolver.resolveDependency( typeof( Monitors ) ).addMonitorListener( _masterMonitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotInitializeTxOnReadOnlyOpsOnNeoXaDS()
		 public virtual void ShouldNotInitializeTxOnReadOnlyOpsOnNeoXaDS()
		 {
			  long nodeId = 0L;

			  using ( Transaction transaction = _slave.beginTx() )
			  {
					// When
					Node node = _slave.getNodeById( nodeId );

					// Then
					AssertDidntStartMasterTx();

					// When
					count( node.Labels );

					// Then
					AssertDidntStartMasterTx();

					// When
					ReadAllRels( node );

					// Then
					AssertDidntStartMasterTx();

					// When
					ReadEachProperty( node );

					// Then
					AssertDidntStartMasterTx();

					transaction.Success();
			  }

			  // Finally
			  AssertDidntStartMasterTx();
		 }

		 private void AssertDidntStartMasterTx()
		 {
			  verifyNoMoreInteractions( _masterMonitor );
		 }

		 private void ReadAllRels( Node node )
		 {
			  foreach ( Relationship relationship in node.Relationships )
			  {
					ReadEachProperty( relationship );
			  }
		 }

		 private void ReadEachProperty( PropertyContainer entity )
		 {
			  foreach ( string k in entity.PropertyKeys )
			  {
					entity.GetProperty( k );
			  }
		 }

	}

}