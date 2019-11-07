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
namespace Neo4Net.GraphDb.store.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Neo4Net.GraphDb;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Neo4Net.Test.rule.EnterpriseDatabaseRule;

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
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.DatabaseRule dbRule = new Neo4Net.test.rule.EnterpriseDatabaseRule().withSetting(Neo4Net.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.idTypesToReuse, Neo4Net.kernel.impl.store.id.IdType.NODE + "," + Neo4Net.kernel.impl.store.id.IdType.RELATIONSHIP).withSetting(Neo4Net.graphdb.factory.GraphDatabaseSettings.record_id_batch_size, "1");
		 public DatabaseRule DbRule = new EnterpriseDatabaseRule().withSetting(EnterpriseEditionSettings.idTypesToReuse, IdType.NODE + "," + IdType.RELATIONSHIP).withSetting(GraphDatabaseSettings.record_id_batch_size, "1");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseNodeIdsFromRolledBackTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReuseNodeIdsFromRolledBackTransaction()
		 {
			  // Given
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
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
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
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
//ORIGINAL LINE: final Neo4Net.kernel.impl.storageengine.impl.recordstorage.id.IdController idMaintenanceController = getIdMaintenanceController();
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
//ORIGINAL LINE: final Neo4Net.kernel.impl.storageengine.impl.recordstorage.id.IdController idMaintenanceController = getIdMaintenanceController();
			  IdController idMaintenanceController = IdMaintenanceController;

			  using ( Transaction transaction = DbRule.beginTx(), IResourceIterator<Node> nodes = DbRule.findNodes(testLabel) )
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
			  using ( Transaction transaction = DbRule.beginTx(), IResourceIterator<Node> nodes = DbRule.findNodes(marker) )
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