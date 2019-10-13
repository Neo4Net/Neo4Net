using System.Collections.Generic;

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
namespace Neo4Net.Test.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using InstanceId = Neo4Net.cluster.InstanceId;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using WriteOperationsNotAllowedException = Neo4Net.Graphdb.security.WriteOperationsNotAllowedException;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.read_only;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.HaSettings.tx_push_factor;

	/// <summary>
	/// This test ensures that read-only slaves cannot make any modifications.
	/// </summary>
	public class ReadOnlySlaveIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final ClusterRule clusterRule = new ClusterRule().withSharedSetting(tx_push_factor, "2").withInstanceSetting(read_only, oneBasedServerId -> oneBasedServerId == 2 ? org.neo4j.kernel.configuration.Settings.TRUE : null);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withSharedSetting(tx_push_factor, "2").withInstanceSetting(read_only, oneBasedServerId => oneBasedServerId == 2 ? Settings.TRUE : null);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWithReadOnlySlaveWhenWriteTxOnSlaveThenCommitFails()
		 public virtual void GivenClusterWithReadOnlySlaveWhenWriteTxOnSlaveThenCommitFails()
		 {
			  // When
			  ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase readOnlySlave = cluster.GetMemberByServerId( new InstanceId( 2 ) );

			  try
			  {
					  using ( Transaction tx = readOnlySlave.BeginTx() )
					  {
						readOnlySlave.CreateNode();
						tx.Success();
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException )
			  {
					// Then
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWithReadOnlySlaveWhenChangePropertyOnSlaveThenThrowException()
		 public virtual void GivenClusterWithReadOnlySlaveWhenChangePropertyOnSlaveThenThrowException()
		 {
			  // Given
			  ManagedCluster cluster = ClusterRule.startCluster();
			  Node node;
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  using ( Transaction tx = master.BeginTx() )
			  {
					node = master.CreateNode();
					tx.Success();
			  }

			  // When
			  HighlyAvailableGraphDatabase readOnlySlave = cluster.GetMemberByServerId( new InstanceId( 2 ) );

			  try
			  {
					  using ( Transaction tx = readOnlySlave.BeginTx() )
					  {
						Node slaveNode = readOnlySlave.GetNodeById( node.Id );
      
						// Then
						slaveNode.SetProperty( "foo", "bar" );
						tx.Success();
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException )
			  {
					// Ok!
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWithReadOnlySlaveWhenAddNewLabelOnSlaveThenThrowException()
		 public virtual void GivenClusterWithReadOnlySlaveWhenAddNewLabelOnSlaveThenThrowException()
		 {
			  // Given
			  ManagedCluster cluster = ClusterRule.startCluster();
			  Node node;
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  using ( Transaction tx = master.BeginTx() )
			  {
					node = master.CreateNode();
					tx.Success();
			  }

			  // When
			  HighlyAvailableGraphDatabase readOnlySlave = cluster.GetMemberByServerId( new InstanceId( 2 ) );

			  try
			  {
					  using ( Transaction tx = readOnlySlave.BeginTx() )
					  {
						Node slaveNode = readOnlySlave.GetNodeById( node.Id );
      
						// Then
						slaveNode.AddLabel( Label.label( "FOO" ) );
						tx.Success();
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException )
			  {
					// Ok!
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWithReadOnlySlaveWhenAddNewRelTypeOnSlaveThenThrowException()
		 public virtual void GivenClusterWithReadOnlySlaveWhenAddNewRelTypeOnSlaveThenThrowException()
		 {
			  // Given
			  ManagedCluster cluster = ClusterRule.startCluster();
			  Node node;
			  Node node2;
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  using ( Transaction tx = master.BeginTx() )
			  {
					node = master.CreateNode();
					node2 = master.CreateNode();
					tx.Success();
			  }

			  // When
			  HighlyAvailableGraphDatabase readOnlySlave = cluster.GetMemberByServerId( new InstanceId( 2 ) );

			  try
			  {
					  using ( Transaction tx = readOnlySlave.BeginTx() )
					  {
						Node slaveNode = readOnlySlave.GetNodeById( node.Id );
						Node slaveNode2 = readOnlySlave.GetNodeById( node2.Id );
      
						// Then
						slaveNode.CreateRelationshipTo( slaveNode2, RelationshipType.withName( "KNOWS" ) );
						tx.Success();
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException )
			  {
					// Ok!
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWithReadOnlySlaveWhenCreatingNodeOnMasterThenSlaveShouldBeAbleToPullUpdates()
		 public virtual void GivenClusterWithReadOnlySlaveWhenCreatingNodeOnMasterThenSlaveShouldBeAbleToPullUpdates()
		 {
			  ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  Label label = Label.label( "label" );

			  using ( Transaction tx = master.BeginTx() )
			  {
					master.CreateNode( label );
					tx.Success();
			  }

			  IEnumerable<HighlyAvailableGraphDatabase> allMembers = cluster.AllMembers;
			  foreach ( HighlyAvailableGraphDatabase member in allMembers )
			  {
					using ( Transaction tx = member.BeginTx() )
					{
						 long count = count( member.FindNodes( label ) );
						 tx.Success();
						 assertEquals( 1, count );
					}
			  }
		 }
	}

}