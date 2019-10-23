using System;

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
namespace Neo4Net.Kernel.api
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Suite = org.junit.runners.Suite;
	using SuiteClasses = org.junit.runners.Suite.SuiteClasses;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.Schema.ConstraintType;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using InvalidTransactionTypeKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException;
	using NodePropertyExistenceConstraintHaIT = Neo4Net.Kernel.api.ConstraintHaIT.NodePropertyExistenceConstraintHaIT;
	using RelationshipPropertyExistenceConstraintHaIT = Neo4Net.Kernel.api.ConstraintHaIT.RelationshipPropertyExistenceConstraintHaIT;
	using UniquenessConstraintHaIT = Neo4Net.Kernel.api.ConstraintHaIT.UniquenessConstraintHaIT;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using NodePropertyExistenceConstraintDefinition = Neo4Net.Kernel.impl.coreapi.schema.NodePropertyExistenceConstraintDefinition;
	using RelationshipPropertyExistenceConstraintDefinition = Neo4Net.Kernel.impl.coreapi.schema.RelationshipPropertyExistenceConstraintDefinition;
	using UniquenessConstraintDefinition = Neo4Net.Kernel.impl.coreapi.schema.UniquenessConstraintDefinition;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.deleteRecursively;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Suite.class) @SuiteClasses({ NodePropertyExistenceConstraintHaIT.class, RelationshipPropertyExistenceConstraintHaIT.class, UniquenessConstraintHaIT.class }) public class ConstraintHaIT
	public class ConstraintHaIT
	{
		 public class NodePropertyExistenceConstraintHaIT : AbstractConstraintHaIT
		 {
			  protected internal override void CreateConstraint( IGraphDatabaseService db, string type, string value )
			  {
					Db.execute( string.Format( "CREATE CONSTRAINT ON (n:`{0}`) ASSERT exists(n.`{1}`)", type, value ) );
			  }

			  protected internal override ConstraintDefinition GetConstraint( IGraphDatabaseService db, string type, string value )
			  {
					return Iterables.singleOrNull( Db.schema().getConstraints(label(type)) );
			  }

			  protected internal override IndexDefinition GetIndex( IGraphDatabaseService db, string type, string value )
			  {
					return null;
			  }

			  protected internal override void CreateEntityInTx( IGraphDatabaseService db, string type, string propertyKey, string value )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( label( type ) ).setProperty( propertyKey, value );
						 tx.Success();
					}
			  }

			  protected internal override void CreateConstraintViolation( IGraphDatabaseService db, string type, string propertyKey, string value )
			  {
					Db.createNode( label( type ) );
			  }

			  protected internal override Type ConstraintDefinitionClass()
			  {
					return typeof( NodePropertyExistenceConstraintDefinition );
			  }
		 }

		 public class RelationshipPropertyExistenceConstraintHaIT : AbstractConstraintHaIT
		 {
			  protected internal override void CreateConstraint( IGraphDatabaseService db, string type, string value )
			  {
					Db.execute( string.Format( "CREATE CONSTRAINT ON ()-[r:`{0}`]-() ASSERT exists(r.`{1}`)", type, value ) );
			  }

			  protected internal override ConstraintDefinition GetConstraint( IGraphDatabaseService db, string type, string value )
			  {
					return Iterables.singleOrNull( Db.schema().getConstraints(withName(type)) );
			  }

			  protected internal override IndexDefinition GetIndex( IGraphDatabaseService db, string type, string value )
			  {
					return null;
			  }

			  protected internal override void CreateEntityInTx( IGraphDatabaseService db, string type, string propertyKey, string value )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Node start = Db.createNode();
						 Node end = Db.createNode();
						 Relationship relationship = start.CreateRelationshipTo( end, withName( type ) );
						 relationship.SetProperty( propertyKey, value );
						 tx.Success();
					}
			  }

			  protected internal override void CreateConstraintViolation( IGraphDatabaseService db, string type, string propertyKey, string value )
			  {
					Node start = Db.createNode();
					Node end = Db.createNode();
					start.CreateRelationshipTo( end, withName( type ) );
			  }

			  protected internal override Type ConstraintDefinitionClass()
			  {
					return typeof( RelationshipPropertyExistenceConstraintDefinition );
			  }
		 }

		 public class UniquenessConstraintHaIT : AbstractConstraintHaIT
		 {
			  protected internal override void CreateConstraint( IGraphDatabaseService db, string type, string value )
			  {
					Db.execute( string.Format( "CREATE CONSTRAINT ON (n:`{0}`) ASSERT n.`{1}` IS UNIQUE", type, value ) );
			  }

			  protected internal override ConstraintDefinition GetConstraint( IGraphDatabaseService db, string type, string value )
			  {
					return Iterables.singleOrNull( Db.schema().getConstraints(Label.label(type)) );
			  }

			  protected internal override IndexDefinition GetIndex( IGraphDatabaseService db, string type, string value )
			  {
					return Iterables.singleOrNull( Db.schema().getIndexes(Label.label(type)) );
			  }

			  protected internal override void CreateEntityInTx( IGraphDatabaseService db, string type, string propertyKey, string value )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( label( type ) ).setProperty( propertyKey, value );
						 tx.Success();
					}
			  }

			  protected internal override void CreateConstraintViolation( IGraphDatabaseService db, string type, string propertyKey, string value )
			  {
					Db.createNode( label( type ) ).setProperty( propertyKey, value );
			  }

			  protected internal override Type ConstraintDefinitionClass()
			  {
					return typeof( UniquenessConstraintDefinition );
			  }
		 }

		 public abstract class AbstractConstraintHaIT
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withSharedSetting(org.Neo4Net.kernel.ha.HaSettings.read_timeout, "4000s");
			  public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(HaSettings.read_timeout, "4000s");

			  internal const string TYPE = "Type";
			  internal const string PROPERTY_KEY = "name";

			  // These type/key methods are due to the ClusterRule being a ClassRule so that one cluster
			  // is used for all the tests, and so they need to have each their own constraint
			  protected internal virtual string Type( int id )
			  {
					return TYPE + "_" + this.GetType().Name + "_" + id;
			  }

			  protected internal virtual string Key( int id )
			  {
					return PROPERTY_KEY + "_" + this.GetType().Name + "_" + id;
			  }

			  protected internal abstract void CreateConstraint( IGraphDatabaseService db, string type, string value );

			  /// <returns> {@code null} if it has been dropped. </returns>
			  protected internal abstract ConstraintDefinition GetConstraint( IGraphDatabaseService db, string type, string value );

			  /// <returns> {@code null} if it has been dropped. </returns>
			  protected internal abstract IndexDefinition GetIndex( IGraphDatabaseService db, string type, string value );

			  protected internal abstract void CreateEntityInTx( IGraphDatabaseService db, string type, string propertyKey, string value );

			  protected internal abstract void CreateConstraintViolation( IGraphDatabaseService db, string type, string propertyKey, string value );

			  protected internal abstract Type ConstraintDefinitionClass();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateConstraintOnMaster()
			  public virtual void ShouldCreateConstraintOnMaster()
			  {
					// given
					ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
					HighlyAvailableGraphDatabase master = cluster.Master;
					string type = type( 0 );
					string key = key( 0 );

					// when
					using ( Transaction tx = master.BeginTx() )
					{
						 CreateConstraint( master, type, key );
						 tx.Success();
					}

					cluster.Sync();

					// then
					foreach ( HighlyAvailableGraphDatabase clusterMember in cluster.AllMembers )
					{
						 using ( Transaction tx = clusterMember.BeginTx() )
						 {
							  ConstraintDefinition constraint = GetConstraint( clusterMember, type, key );
							  ValidateLabelOrRelationshipType( constraint, type );
							  assertEquals( key, single( constraint.PropertyKeys ) );
							  tx.Success();
						 }
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBePossibleToCreateConstraintsDirectlyOnSlaves()
			  public virtual void ShouldNotBePossibleToCreateConstraintsDirectlyOnSlaves()
			  {
					// given
					ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
					HighlyAvailableGraphDatabase slave = cluster.AnySlave;
					string type = type( 1 );
					string key = key( 1 );

					// when
					try
					{
							using ( Transaction ignored = slave.BeginTx() )
							{
							 CreateConstraint( slave, type, key );
							 fail( "We expected to not be able to create a constraint on a slave in a cluster." );
							}
					}
					catch ( QueryExecutionException e )
					{
						 assertThat( Exceptions.rootCause( e ), instanceOf( typeof( InvalidTransactionTypeKernelException ) ) );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveConstraints()
			  public virtual void ShouldRemoveConstraints()
			  {
					// given
					ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
					HighlyAvailableGraphDatabase master = cluster.Master;
					string type = type( 2 );
					string key = key( 2 );

					long constraintCountBefore;
					long indexCountBefore;
					using ( Transaction tx = master.BeginTx() )
					{
						 constraintCountBefore = count( master.Schema().Constraints );
						 indexCountBefore = count( master.Schema().Indexes );
						 CreateConstraint( master, type, key );
						 tx.Success();
					}
					cluster.Sync();

					// and given I have some data for the constraint
					CreateEntityInTx( cluster.AnySlave, type, key, "Foo" );

					// when
					using ( Transaction tx = master.BeginTx() )
					{
						 GetConstraint( master, type, key ).drop();
						 tx.Success();
					}
					cluster.Sync();

					// then the constraint should be gone, and not be enforced anymore
					foreach ( HighlyAvailableGraphDatabase clusterMember in cluster.AllMembers )
					{
						 using ( Transaction tx = clusterMember.BeginTx() )
						 {
							  assertNull( GetConstraint( clusterMember, type, key ) );
							  assertNull( GetIndex( clusterMember, type, key ) );
							  CreateConstraintViolation( clusterMember, type, key, "Foo" );
							  tx.Success();
						 }
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newSlaveJoiningClusterShouldNotAcceptOperationsUntilConstraintIsOnline() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void NewSlaveJoiningClusterShouldNotAcceptOperationsUntilConstraintIsOnline()
			  {
					// Given
					ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
					string type = type( 4 );
					string key = key( 4 );

					HighlyAvailableGraphDatabase master = cluster.Master;

					HighlyAvailableGraphDatabase slave = cluster.AnySlave;
					File slaveStoreDirectory = cluster.GetDatabaseDir( slave );

					// Crash the slave
					ClusterManager.RepairKit shutdownSlave = cluster.Shutdown( slave );
					deleteRecursively( slaveStoreDirectory );

					using ( Transaction tx = master.BeginTx() )
					{
						 CreateConstraint( master, type, key );
						 tx.Success();
					}

					// When
					slave = shutdownSlave.Repair();

					// Then
					using ( Transaction ignored = slave.BeginTx() )
					{
						 ConstraintDefinition definition = GetConstraint( slave, type, key );
						 assertThat( definition, instanceOf( ConstraintDefinitionClass() ) );
						 assertThat( single( definition.PropertyKeys ), equalTo( key ) );
						 ValidateLabelOrRelationshipType( definition, type );
					}
			  }

			  internal static void ValidateLabelOrRelationshipType( ConstraintDefinition constraint, string type )
			  {
					if ( constraint.IsConstraintType( ConstraintType.RELATIONSHIP_PROPERTY_EXISTENCE ) )
					{
						 assertEquals( type, constraint.RelationshipType.name() );
					}
					else
					{
						 assertEquals( type, constraint.Label.name() );
					}
			  }
		 }
	}

}