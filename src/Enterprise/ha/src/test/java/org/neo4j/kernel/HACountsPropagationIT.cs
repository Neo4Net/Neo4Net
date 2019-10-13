﻿/*
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
namespace Neo4Net.Kernel
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;

	public class HACountsPropagationIT
	{
		 private const int PULL_INTERVAL = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withSharedSetting(org.neo4j.kernel.ha.HaSettings.pull_interval, PULL_INTERVAL + "ms");
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(HaSettings.pull_interval, PULL_INTERVAL + "ms");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateNodeCountsInHA() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateNodeCountsInHA()
		 {
			  ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  using ( Transaction tx = master.BeginTx() )
			  {
					master.CreateNode();
					master.CreateNode( Label.label( "A" ) );
					tx.Success();
			  }

			  cluster.Sync();

			  foreach ( HighlyAvailableGraphDatabase db in cluster.AllMembers )
			  {
					using ( Neo4Net.@internal.Kernel.Api.Transaction tx = Db.DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, AUTH_DISABLED ) )
					{
						 assertEquals( 2, tx.dataRead().countsForNode(-1) );
						 assertEquals( 1, tx.dataRead().countsForNode(0) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateRelationshipCountsInHA() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateRelationshipCountsInHA()
		 {
			  ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  using ( Transaction tx = master.BeginTx() )
			  {
					Node left = master.CreateNode();
					Node right = master.CreateNode( Label.label( "A" ) );
					left.CreateRelationshipTo( right, RelationshipType.withName( "Type" ) );
					tx.Success();
			  }

			  cluster.Sync();

			  foreach ( HighlyAvailableGraphDatabase db in cluster.AllMembers )
			  {
					using ( Neo4Net.@internal.Kernel.Api.Transaction tx = Db.DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, AUTH_DISABLED ) )
					{
						 assertEquals( 1, tx.dataRead().countsForRelationship(-1, -1, -1) );
						 assertEquals( 1, tx.dataRead().countsForRelationship(-1, -1, 0) );
						 assertEquals( 1, tx.dataRead().countsForRelationship(-1, 0, -1) );
						 assertEquals( 1, tx.dataRead().countsForRelationship(-1, 0, 0) );
					}
			  }
		 }
	}

}