using System;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.enterprise.store.id
{
	using ReflectionToStringBuilder = org.apache.commons.lang3.builder.ReflectionToStringBuilder;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Race = Neo4Net.Test.Race;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EnterpriseDatabaseRule = Neo4Net.Test.rule.EnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class NodeIdReuseStressIT
	{
		 private const int CONTESTANTS_COUNT = 12;
		 private const int INITIAL_NODE_COUNT = 10_000;
		 private const int OPERATIONS_COUNT = 10_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.EnterpriseDatabaseRule().withSetting(org.Neo4Net.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.idTypesToReuse, org.Neo4Net.kernel.impl.store.id.IdType.NODE.name());
		 public readonly DatabaseRule Db = new EnterpriseDatabaseRule().withSetting(EnterpriseEditionSettings.idTypesToReuse, IdType.NODE.name());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void verifyParams()
		 public virtual void VerifyParams()
		 {
			  assertThat( CONTESTANTS_COUNT, greaterThan( 0 ) );
			  assertThat( CONTESTANTS_COUNT % 2, equalTo( 0 ) );
			  assertThat( INITIAL_NODE_COUNT, greaterThan( 0 ) );
			  assertThat( OPERATIONS_COUNT, greaterThan( 1_000 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeIdsReused() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NodeIdsReused()
		 {
			  CreateInitialNodes( Db );
			  long initialHighestNodeId = HighestNodeId( Db );

			  Race race = new Race();

			  for ( int i = 0; i < CONTESTANTS_COUNT; i++ )
			  {
					if ( i % 2 == 0 )
					{
						 race.AddContestant( new NodeCreator( Db ) );
					}
					else
					{
						 race.AddContestant( new NodeRemover( Db ) );
					}
			  }

			  race.Go();

			  int writeContestants = CONTESTANTS_COUNT / 2;
			  int createdNodes = writeContestants * OPERATIONS_COUNT;
			  long highestNodeIdWithoutReuse = initialHighestNodeId + createdNodes;

			  long currentHighestNodeId = HighestNodeId( Db );

			  assertThat( currentHighestNodeId, lessThan( highestNodeIdWithoutReuse ) );
		 }

		 private static void CreateInitialNodes( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < INITIAL_NODE_COUNT; i++ )
					{
						 Db.createNode();
					}
					tx.Success();
			  }
		 }

		 private static long HighestNodeId( IGraphDatabaseService db )
		 {
			  DependencyResolver resolver = DependencyResolver( db );
			  NeoStores neoStores = resolver.ResolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
			  NodeStore nodeStore = neoStores.NodeStore;
			  return nodeStore.HighestPossibleIdInUse;
		 }

		 private static void MaybeRunIdMaintenance( IGraphDatabaseService db, int iteration )
		 {
			  if ( iteration % 100 == 0 && ThreadLocalRandom.current().nextBoolean() )
			  {
					DependencyResolver resolver = DependencyResolver( db );
					IdController idController = resolver.ResolveDependency( typeof( IdController ) );
					if ( idController != null )
					{
						 idController.Maintenance();
					}
					else
					{
						 Console.WriteLine( "Id controller is null. Dumping resolver content." );
						 Console.WriteLine( "Resolver: " + ReflectionToStringBuilder.ToString( resolver ) );
						 throw new System.InvalidOperationException( "Id controller not found" );
					}
			  }
		 }

		 private static DependencyResolver DependencyResolver( IGraphDatabaseService db )
		 {
			  return ( ( GraphDatabaseAPI ) db ).DependencyResolver;
		 }

		 private class NodeCreator : ThreadStart
		 {
			  internal readonly IGraphDatabaseService Db;

			  internal NodeCreator( IGraphDatabaseService db )
			  {
					this.Db = db;
			  }

			  public override void Run()
			  {
					for ( int i = 0; i < OPERATIONS_COUNT; i++ )
					{
						 using ( Transaction tx = Db.beginTx() )
						 {
							  Db.createNode();
							  tx.Success();
						 }

						 MaybeRunIdMaintenance( Db, i );
					}
			  }
		 }

		 private class NodeRemover : ThreadStart
		 {
			  internal readonly IGraphDatabaseService Db;

			  internal NodeRemover( IGraphDatabaseService db )
			  {
					this.Db = db;
			  }

			  public override void Run()
			  {
					for ( int i = 0; i < OPERATIONS_COUNT; i++ )
					{
						 long highestId = HighestNodeId( Db );
						 if ( highestId > 0 )
						 {
							  long id = ThreadLocalRandom.current().nextLong(highestId);

							  try
							  {
									  using ( Transaction tx = Db.beginTx() )
									  {
										Db.getNodeById( id ).delete();
										tx.Success();
									  }
							  }
							  catch ( NotFoundException )
							  {
									// same node was removed concurrently
							  }
						 }

						 MaybeRunIdMaintenance( Db, i );
					}
			  }
		 }
	}

}