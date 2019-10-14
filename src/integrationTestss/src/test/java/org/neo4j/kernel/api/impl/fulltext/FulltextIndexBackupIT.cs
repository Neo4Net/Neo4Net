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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using OnlineBackup = Neo4Net.backup.OnlineBackup;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.NODE_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.QUERY_NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.QUERY_RELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.RELATIONSHIP_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_server;

	public class FulltextIndexBackupIT
	{
		private bool InstanceFieldsInitialized = false;

		public FulltextIndexBackupIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _suppressOutput ).around( _dir ).around( _cleanup );
		}

		 private static readonly Label _label = Label.label( "LABEL" );
		 private const string PROP = "prop";
		 private static readonly RelationshipType _rel = RelationshipType.withName( "REL" );
		 private const string NODE_INDEX = "nodeIndex";
		 private const string REL_INDEX = "relIndex";

		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
		 private readonly TestDirectory _dir = TestDirectory.testDirectory();
		 private readonly CleanupRule _cleanup = new CleanupRule();
		 private long _nodeId1;
		 private long _nodeId2;
		 private long _relId1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(suppressOutput).around(dir).around(cleanup);
		 public RuleChain Rules;

		 private int _backupPort;
		 private GraphDatabaseAPI _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUpPorts()
		 public virtual void SetUpPorts()
		 {
			  _backupPort = PortAuthority.allocatePort();
			  GraphDatabaseFactory factory = new EnterpriseGraphDatabaseFactory();
			  GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( _dir.storeDir() );
			  builder.setConfig( online_backup_enabled, "true" );
			  builder.setConfig( online_backup_server, "127.0.0.1:" + _backupPort );
			  _db = ( GraphDatabaseAPI ) builder.NewGraphDatabase();
			  _cleanup.add( _db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexesMustBeTransferredInBackup()
		 public virtual void FulltextIndexesMustBeTransferredInBackup()
		 {
			  InitializeTestData();
			  VerifyData( _db );
			  File backup = _dir.storeDir( "backup" );
			  OnlineBackup.from( "127.0.0.1", _backupPort ).backup( backup );
			  _db.shutdown();

			  GraphDatabaseAPI backupDb = StartBackupDatabase( backup );
			  VerifyData( backupDb );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexesMustBeUpdatedByIncrementalBackup()
		 public virtual void FulltextIndexesMustBeUpdatedByIncrementalBackup()
		 {
			  InitializeTestData();
			  File backup = _dir.databaseDir( "backup" );
			  OnlineBackup.from( "127.0.0.1", _backupPort ).backup( backup );

			  long nodeId3;
			  long nodeId4;
			  long relId2;
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node3 = _db.createNode( _label );
					node3.SetProperty( PROP, "Additional data." );
					Node node4 = _db.createNode( _label );
					node4.SetProperty( PROP, "Even more additional data." );
					Relationship rel = node3.CreateRelationshipTo( node4, _rel );
					rel.SetProperty( PROP, "Knows of" );
					nodeId3 = node3.Id;
					nodeId4 = node4.Id;
					relId2 = rel.Id;
					tx.Success();
			  }
			  VerifyData( _db );

			  OnlineBackup.from( "127.0.0.1", _backupPort ).backup( backup );
			  _db.shutdown();

			  GraphDatabaseAPI backupDb = StartBackupDatabase( backup );
			  VerifyData( backupDb );

			  using ( Transaction tx = backupDb.BeginTx() )
			  {
					using ( Result nodes = backupDb.Execute( format( QUERY_NODES, NODE_INDEX, "additional" ) ) )
					{
						 IList<long> nodeIds = nodes.Select( m => ( ( Node ) m.get( NODE ) ).Id ).ToList();
						 assertThat( nodeIds, containsInAnyOrder( nodeId3, nodeId4 ) );
					}
					using ( Result relationships = backupDb.Execute( format( QUERY_RELS, REL_INDEX, "knows" ) ) )
					{
						 IList<long> relIds = relationships.Select( m => ( ( Relationship ) m.get( RELATIONSHIP ) ).Id ).ToList();
						 assertThat( relIds, containsInAnyOrder( relId2 ) );
					}
					tx.Success();
			  }
		 }

		 // TODO test that creation and dropping of fulltext indexes is applied through incremental backup.
		 // TODO test that custom analyzer configurations are applied through incremental backup.
		 // TODO test that the eventually_consistent setting is transferred through incremental backup.

		 private void InitializeTestData()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node1 = _db.createNode( _label );
					node1.SetProperty( PROP, "This is an integration test." );
					Node node2 = _db.createNode( _label );
					node2.SetProperty( PROP, "This is a related integration test." );
					Relationship relationship = node1.CreateRelationshipTo( node2, _rel );
					relationship.SetProperty( PROP, "They relate" );
					_nodeId1 = node1.Id;
					_nodeId2 = node2.Id;
					_relId1 = relationship.Id;
					tx.Success();
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( format( NODE_CREATE, NODE_INDEX, array( _label.name() ), array(PROP) ) ).close();
					_db.execute( format( RELATIONSHIP_CREATE, REL_INDEX, array( _rel.name() ), array(PROP) ) ).close();
					tx.Success();
			  }
			  AwaitPopulation( _db );
		 }

		 private static void AwaitPopulation( GraphDatabaseAPI db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }

		 private GraphDatabaseAPI StartBackupDatabase( File backupDatabaseDir )
		 {
			  return ( GraphDatabaseAPI ) _cleanup.add( ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(backupDatabaseDir).newGraphDatabase() );
		 }

		 private void VerifyData( GraphDatabaseAPI db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					AwaitPopulation( db );
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					using ( Result nodes = Db.execute( format( QUERY_NODES, NODE_INDEX, "integration" ) ) )
					{
						 IList<long> nodeIds = nodes.Select( m => ( ( Node ) m.get( NODE ) ).Id ).ToList();
						 assertThat( nodeIds, containsInAnyOrder( _nodeId1, _nodeId2 ) );
					}
					using ( Result relationships = Db.execute( format( QUERY_RELS, REL_INDEX, "relate" ) ) )
					{
						 IList<long> relIds = relationships.Select( m => ( ( Relationship ) m.get( RELATIONSHIP ) ).Id ).ToList();
						 assertThat( relIds, containsInAnyOrder( _relId1 ) );
					}
					tx.Success();
			  }
		 }
	}

}