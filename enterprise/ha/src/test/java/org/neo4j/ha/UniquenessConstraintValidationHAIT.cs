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
namespace Org.Neo4j.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using Org.Neo4j.Kernel.impl.util;
	using Org.Neo4j.Test;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;
	using Org.Neo4j.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.concurrent.OtherThreadRule.isWaiting;

	public class UniquenessConstraintValidationHAIT
	{
		 private static readonly Label _label = label( "Label1" );
		 private const string PROPERTY_KEY = "key1";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> otherThread = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> OtherThread = new OtherThreadRule<Void>();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withInitialDataset(uniquenessConstraint(LABEL, PROPERTY_KEY));
		 public readonly ClusterRule ClusterRule = new ClusterRule().withInitialDataset(UniquenessConstraint(_label, PROPERTY_KEY));

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowCreationOfNonConflictingDataOnSeparateHosts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowCreationOfNonConflictingDataOnSeparateHosts()
		 {
			  // given
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();

			  HighlyAvailableGraphDatabase slave1 = cluster.AnySlave;
			  HighlyAvailableGraphDatabase slave2 = cluster.GetAnySlave( slave1 );

			  // when
			  Future<bool> created;

			  using ( Transaction tx = slave1.BeginTx() )
			  {
					slave1.CreateNode( _label ).setProperty( PROPERTY_KEY, "value1" );

					created = OtherThread.execute( CreateNode( slave2, _label.name(), PROPERTY_KEY, "value2" ) );
					tx.Success();
			  }

			  // then
			  assertTrue( "creating non-conflicting data should pass", created.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPreventConcurrentCreationOfConflictingDataOnSeparateHosts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPreventConcurrentCreationOfConflictingDataOnSeparateHosts()
		 {
			  // given
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();

			  HighlyAvailableGraphDatabase slave1 = cluster.AnySlave;
			  HighlyAvailableGraphDatabase slave2 = cluster.GetAnySlave( slave1 );

			  // when
			  Future<bool> created;
			  using ( Transaction tx = slave1.BeginTx() )
			  {
					slave1.CreateNode( _label ).setProperty( PROPERTY_KEY, "value3" );

					created = OtherThread.execute( CreateNode( slave2, _label.name(), PROPERTY_KEY, "value3" ) );

					assertThat( OtherThread, Waiting );

					tx.Success();
			  }

			  // then
			  assertFalse( "creating violating data should fail", created.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPreventConcurrentCreationOfConflictingNonStringPropertyOnMasterAndSlave() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPreventConcurrentCreationOfConflictingNonStringPropertyOnMasterAndSlave()
		 {
			  // given
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();

			  HighlyAvailableGraphDatabase master = cluster.Master;
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;

			  // when
			  Future<bool> created;
			  using ( Transaction tx = master.BeginTx() )
			  {
					master.CreateNode( _label ).setProperty( PROPERTY_KEY, 0x0099CC );

					created = OtherThread.execute( CreateNode( slave, _label.name(), PROPERTY_KEY, 0x0099CC ) );

					assertThat( OtherThread, Waiting );

					tx.Success();
			  }

			  // then
			  assertFalse( "creating violating data should fail", created.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOtherHostToCompleteIfFirstHostRollsBackTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowOtherHostToCompleteIfFirstHostRollsBackTransaction()
		 {
			  // given
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();

			  HighlyAvailableGraphDatabase slave1 = cluster.AnySlave;
			  HighlyAvailableGraphDatabase slave2 = cluster.GetAnySlave( slave1 );

			  // when
			  Future<bool> created;

			  using ( Transaction tx = slave1.BeginTx() )
			  {
					slave1.CreateNode( _label ).setProperty( PROPERTY_KEY, "value4" );

					created = OtherThread.execute( CreateNode( slave2, _label.name(), PROPERTY_KEY, "value4" ) );

					assertThat( OtherThread, Waiting );

					tx.Failure();
			  }

			  // then
			  assertTrue( "creating data that conflicts only with rolled back data should pass", created.get() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.kernel.impl.util.Listener<org.neo4j.graphdb.GraphDatabaseService> uniquenessConstraint(final org.neo4j.graphdb.Label label, final String propertyKey)
		 private static Listener<GraphDatabaseService> UniquenessConstraint( Label label, string propertyKey )
		 {
			  return db =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Db.schema().constraintFor(label).assertPropertyIsUnique(propertyKey).create();

					 tx.success();
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.test.OtherThreadExecutor.WorkerCommand<Void, bool> createNode(final org.neo4j.graphdb.GraphDatabaseService db, final String label, final String propertyKey, final Object propertyValue)
		 public static OtherThreadExecutor.WorkerCommand<Void, bool> CreateNode( GraphDatabaseService db, string label, string propertyKey, object propertyValue )
		 {
			  return nothing =>
			  {
				try
				{
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( label( label ) ).setProperty( propertyKey, propertyValue );
   
						 tx.success();
						 return true;
					}
				}
				catch ( ConstraintViolationException )
				{
					 return false;
				}
			  };
		 }
	}

}