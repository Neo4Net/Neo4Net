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
namespace Neo4Net.causalclustering.helpers
{

	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;

	public class DataCreator
	{
		 private DataCreator()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.causalclustering.discovery.CoreClusterMember createLabelledNodesWithProperty(org.neo4j.causalclustering.discovery.Cluster<?> cluster, int numberOfNodes, org.neo4j.graphdb.Label label, System.Func<org.neo4j.helpers.collection.Pair<String,Object>> propertyPair) throws Exception
		 public static CoreClusterMember CreateLabelledNodesWithProperty<T1>( Cluster<T1> cluster, int numberOfNodes, Label label, System.Func<Pair<string, object>> propertyPair )
		 {
			  CoreClusterMember last = null;
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					last = cluster.CoreTx((db, tx) =>
					{
					 Node node = Db.createNode( label );
					 node.setProperty( propertyPair().first(), propertyPair().other() );
					 tx.success();
					});
			  }
			  return last;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.causalclustering.discovery.CoreClusterMember createEmptyNodes(org.neo4j.causalclustering.discovery.Cluster<?> cluster, int numberOfNodes) throws Exception
		 public static CoreClusterMember CreateEmptyNodes<T1>( Cluster<T1> cluster, int numberOfNodes )
		 {
			  CoreClusterMember last = null;
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					last = cluster.CoreTx((db, tx) =>
					{
					 Db.createNode();
					 tx.success();
					});
			  }
			  return last;
		 }

		 public static long CountNodes( CoreClusterMember member )
		 {
			  CoreGraphDatabase db = member.Database();
			  long count;
			  using ( Transaction tx = Db.beginTx() )
			  {
					count = count( Db.AllNodes );
					tx.Success();
			  }
			  return count;
		 }
	}

}