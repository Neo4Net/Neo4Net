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
namespace Org.Neo4j.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ManagedCluster = Org.Neo4j.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.clusterOfSize;

	/// <summary>
	/// Test for a regression:
	/// 
	/// IndexOutOfBoundsException(-1) when applying a transaction that deletes relationship(s).
	/// Happens when performing transactions in HA, or during recovery.
	/// 
	/// Symptomatic stack trace: (Note that this is from before GCR was renamed to HighPerformanceCache)
	/// 
	/// java.lang.IndexOutOfBoundsException: index -1
	///     at java.util.concurrent.atomic.AtomicReferenceArray.checkedByteOffset(AtomicReferenceArray.java:50)
	///     at java.util.concurrent.atomic.AtomicReferenceArray.get(AtomicReferenceArray.java:95)
	///     at org.neo4j.kernel.impl.cache.GCResistantCache.get(GCResistantCache.java:188)
	///     at org.neo4j.kernel.impl.core.NodeManager.invalidateNode(NodeManager.java:567)
	///     at org.neo4j.kernel.impl.core.NodeManager.patchDeletedRelationshipNodes(NodeManager.java:561)
	///     at org.neo4j.kernel.impl.core.WritableTransactionState.patchDeletedRelationshipNodes(WritableTransactionState.java:558)
	///     at org.neo4j.kernel.impl.nioneo.xa.Command$RelationshipCommand.removeFromCache(Command.java:432)
	///     at org.neo4j.kernel.impl.nioneo.xa.WriteTransaction.executeDeleted(WriteTransaction.java:562)
	///     at org.neo4j.kernel.impl.nioneo.xa.WriteTransaction.applyCommit(WriteTransaction.java:476)
	///     at org.neo4j.kernel.impl.nioneo.xa.WriteTransaction.doCommit(WriteTransaction.java:426)
	/// </summary>
	public class DeletionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withCluster(clusterOfSize(2));
		 public ClusterRule ClusterRule = new ClusterRule().withCluster(clusterOfSize(2));

		 /// <summary>
		 /// The problem would manifest even if the transaction was performed on the Master, it would then occur when the
		 /// Slave pulls updates and tries to apply the transaction. The reason for the test to run transactions against the
		 /// Slave is because it makes guarantees for when the master has to apply the transaction.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteRecords()
		 public virtual void ShouldDeleteRecords()
		 {
			  // given
			  ManagedCluster cluster = ClusterRule.startCluster();

			  HighlyAvailableGraphDatabase master = cluster.Master;
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;

			  Relationship rel;
			  using ( Transaction tx = slave.BeginTx() )
			  {
					rel = slave.CreateNode().createRelationshipTo(slave.CreateNode(), withName("FOO"));
					tx.Success();
			  }

			  using ( Transaction transaction = master.BeginTx() )
			  {
					assertNotNull( master.GetRelationshipById( rel.Id ) );
			  }

			  // when
			  using ( Transaction tx = slave.BeginTx() )
			  {
					rel.Delete();
					tx.Success();
			  }

			  // then - there should have been no exceptions
			  slave.Shutdown();
			  master.Shutdown();
		 }
	}

}