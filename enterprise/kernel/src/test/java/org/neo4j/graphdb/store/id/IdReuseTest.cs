﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Graphdb.store.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Org.Neo4j.Graphdb;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using EnterpriseEditionSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Org.Neo4j.Test.rule.EnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class IdReuseTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule dbRule = new org.neo4j.test.rule.EnterpriseDatabaseRule().withSetting(org.neo4j.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.idTypesToReuse, org.neo4j.kernel.impl.store.id.IdType.NODE + "," + org.neo4j.kernel.impl.store.id.IdType.RELATIONSHIP).withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.record_id_batch_size, "1");
		 public DatabaseRule DbRule = new EnterpriseDatabaseRule().withSetting(EnterpriseEditionSettings.idTypesToReuse, IdType.NODE + "," + IdType.RELATIONSHIP).withSetting(GraphDatabaseSettings.record_id_batch_size, "1");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseNodeIdsFromRolledBackTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReuseNodeIdsFromRolledBackTransaction()
		 {
			  // Given
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();

					tx.Failure();
			  }

			  db = DbRule.restartDatabase();

			  // When
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();

					tx.Success();
			  }

			  // Then
			  assertThat( node.Id, equalTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseRelationshipIdsFromRolledBackTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReuseRelationshipIdsFromRolledBackTransaction()
		 {
			  // Given
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  Node node1;
			  Node node2;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node1 = Db.createNode();
					node2 = Db.createNode();

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					node1.CreateRelationshipTo( node2, RelationshipType.withName( "LIKE" ) );

					tx.Failure();
			  }

			  db = DbRule.restartDatabase();

			  // When
			  Relationship relationship;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node1 = Db.getNodeById( node1.Id );
					node2 = Db.getNodeById( node2.Id );
					relationship = node1.CreateRelationshipTo( node2, RelationshipType.withName( "LIKE" ) );

					tx.Success();
			  }

			  // Then
			  assertThat( relationship.Id, equalTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sequentialOperationRelationshipIdReuse()
		 public virtual void SequentialOperationRelationshipIdReuse()
		 {
			  Label marker = Label.label( "marker" );

			  long relationship1 = CreateRelationship( marker );
			  long relationship2 = CreateRelationship( marker );
			  long relationship3 = CreateRelationship( marker );

			  assertEquals( "Ids should be sequential", relationship1 + 1, relationship2 );
			  assertEquals( "Ids should be sequential", relationship2 + 1, relationship3 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.storageengine.impl.recordstorage.id.IdController idMaintenanceController = getIdMaintenanceController();
			  IdController idMaintenanceController = IdMaintenanceController;

			  DeleteRelationshipByLabelAndRelationshipType( marker );

			  idMaintenanceController.Maintenance();

			  assertEquals( "Relationships have reused id", relationship1, CreateRelationship( marker ) );
			  assertEquals( "Relationships have reused id", relationship2, CreateRelationship( marker ) );
			  assertEquals( "Relationships have reused id", relationship3, CreateRelationship( marker ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipIdReusableOnlyAfterTransactionFinish()
		 public virtual void RelationshipIdReusableOnlyAfterTransactionFinish()
		 {
			  Label testLabel = Label.label( "testLabel" );
			  long relationshipId = CreateRelationship( testLabel );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.storageengine.impl.recordstorage.id.IdController idMaintenanceController = getIdMaintenanceController();
			  IdController idMaintenanceController = IdMaintenanceController;

			  using ( Transaction transaction = DbRule.beginTx(), ResourceIterator<Node> nodes = DbRule.findNodes(testLabel) )
			  {
					IList<Node> nodeList = Iterators.asList( nodes );
					foreach ( Node node in nodeList )
					{
						 IEnumerable<Relationship> relationships = node.GetRelationships( TestRelationshipType.Marker );
						 foreach ( Relationship relationship in relationships )
						 {
							  relationship.Delete();
						 }
					}

					idMaintenanceController.Maintenance();

					Node node1 = DbRule.createNode( testLabel );
					Node node2 = DbRule.createNode( testLabel );

					Relationship relationshipTo = node1.CreateRelationshipTo( node2, TestRelationshipType.Marker );

					assertNotEquals( "Relationships should have different ids.", relationshipId, relationshipTo.Id );
					transaction.Success();
			  }
		 }

		 private void DeleteRelationshipByLabelAndRelationshipType( Label marker )
		 {
			  using ( Transaction transaction = DbRule.beginTx(), ResourceIterator<Node> nodes = DbRule.findNodes(marker) )
			  {
					IList<Node> nodeList = Iterators.asList( nodes );
					foreach ( Node node in nodeList )
					{
						 IEnumerable<Relationship> relationships = node.GetRelationships( TestRelationshipType.Marker );
						 foreach ( Relationship relationship in relationships )
						 {
							  relationship.Delete();
						 }
					}
					transaction.Success();
			  }
		 }

		 private IdController IdMaintenanceController
		 {
			 get
			 {
				  return DbRule.DependencyResolver.resolveDependency( typeof( IdController ) );
			 }
		 }

		 private long CreateRelationship( Label label )
		 {
			  using ( Transaction transaction = DbRule.beginTx() )
			  {
					Node node1 = DbRule.createNode( label );
					Node node2 = DbRule.createNode( label );

					Relationship relationshipTo = node1.CreateRelationshipTo( node2, TestRelationshipType.Marker );
					transaction.Success();
					return relationshipTo.Id;
			  }
		 }

		 private enum TestRelationshipType
		 {
			  Marker
		 }
	}

}