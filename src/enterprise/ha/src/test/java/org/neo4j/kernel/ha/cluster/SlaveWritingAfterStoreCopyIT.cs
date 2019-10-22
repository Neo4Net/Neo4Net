using System.Collections.Generic;

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
namespace Neo4Net.Kernel.ha.cluster
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.deleteRecursively;

	public class SlaveWritingAfterStoreCopyIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule();
		 public ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSlaveWritingFirstAfterStoryCopy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleSlaveWritingFirstAfterStoryCopy()
		 {
			  // Given
			  ISet<long> expected = new HashSet<object>();
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;

			  // When
			  expected.Add( CreateOneNode( master ) );
			  cluster.Sync();

			  // ... crash the slave
			  File slaveStoreDirectory = cluster.GetDatabaseDir( slave );
			  ClusterManager.RepairKit shutdownSlave = cluster.Shutdown( slave );
			  deleteRecursively( slaveStoreDirectory );

			  // ... and slave copy store from master
			  slave = shutdownSlave.Repair();
			  // ... and first write after crash occurs on salve
			  expected.Add( CreateOneNode( slave ) );
			  cluster.Sync();

			  // Then
			  assertEquals( expected, CollectIds( master ) );
			  assertEquals( expected, CollectIds( slave ) );
		 }

		 private ISet<long> CollectIds( HighlyAvailableGraphDatabase db )
		 {
			  ISet<long> result = new HashSet<object>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 result.Add( node.Id );
					}
					tx.Success();
			  }
			  return result;
		 }

		 private long CreateOneNode( HighlyAvailableGraphDatabase db )
		 {
			  long id;
			  using ( Transaction tx = Db.beginTx() )
			  {
					id = Db.createNode().Id;
					tx.Success();
			  }
			  return id;
		 }
	}

}