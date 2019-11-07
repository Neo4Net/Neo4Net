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
namespace Neo4Net.Bolt
{
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Config = Neo4Net.driver.v1.Config;
	using Driver = Neo4Net.driver.v1.Driver;
	using GraphDatabase = Neo4Net.driver.v1.GraphDatabase;
	using Record = Neo4Net.driver.v1.Record;
	using Session = Neo4Net.driver.v1.Session;
	using StatementResult = Neo4Net.driver.v1.StatementResult;
	using Value = Neo4Net.driver.v1.Value;
	using Pair = Neo4Net.driver.v1.util.Pair;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using IOUtils = Neo4Net.Io.IOUtils;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class BoltSnapshotQueryExecutionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private Driver _driver;
		 private IGraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
			  IOUtils.closeAllSilently( _driver );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryWithSnapshotEngine()
		 public virtual void ExecuteQueryWithSnapshotEngine()
		 {
			  ExecuteQuery( "withSnapshotEngine", Settings.TRUE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryWithoutSnapshotEngine()
		 public virtual void ExecuteQueryWithoutSnapshotEngine()
		 {
			  ExecuteQuery( "withoutSnapshotEngine", Settings.FALSE );
		 }

		 private void ExecuteQuery( string directory, string useSnapshotEngineSettingValue )
		 {
			  _db = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.directory(directory)).setConfig((new BoltConnector("bolt")).type, "BOLT").setConfig((new BoltConnector("bolt")).enabled, "true").setConfig((new BoltConnector("bolt")).listen_address, "localhost:0").setConfig(GraphDatabaseSettings.snapshot_query, useSnapshotEngineSettingValue).newGraphDatabase();
			  InitDatabase();
			  ConnectDirver();
			  VerifyQueryExecution();
		 }

		 private void VerifyQueryExecution()
		 {
			  using ( Session session = _driver.session() )
			  {
					session.readTransaction(tx =>
					{
					 StatementResult statementResult = tx.run( "MATCH (n) RETURN n.name, n.profession, n.planet, n.city ORDER BY n.name" );
					 IList<string> fields = Arrays.asList( "n.name", "n.profession", "n.planet", "n.city" );
					 Record amy = statementResult.next();
					 assertEquals( amy.keys(), fields );
					 AssertPairs( amy.fields(), "n.name", "Amy", "n.profession", "Student", "n.planet", "Mars", "n.city", "null" );
					 Record fry = statementResult.next();
					 assertEquals( fry.keys(), fields );
					 AssertPairs( fry.fields(), "n.name", "Fry", "n.profession", "Delivery Boy", "n.planet", "Earth", "n.city", "New York" );
					 Record lila = statementResult.next();
					 assertEquals( lila.keys(), fields );
					 AssertPairs( lila.fields(), "n.name", "Lila", "n.profession", "Pilot", "n.planet", "Earth", "n.city", "New York" );
					 assertFalse( statementResult.hasNext() );
					 return null;
					});
			  }
		 }

		 private void ConnectDirver()
		 {
			  _driver = GraphDatabase.driver( BoltURI(), Config.build().withoutEncryption().toConfig() );
		 }

		 private void InitDatabase()
		 {
			  using ( Transaction transaction = _db.beginTx() )
			  {
					Node fry = _db.createNode();
					fry.SetProperty( "name", "Fry" );
					fry.SetProperty( "profession", "Delivery Boy" );
					fry.SetProperty( "planet", "Earth" );
					fry.SetProperty( "city", "New York" );
					Node lila = _db.createNode();
					lila.SetProperty( "name", "Lila" );
					lila.SetProperty( "profession", "Pilot" );
					lila.SetProperty( "planet", "Earth" );
					lila.SetProperty( "city", "New York" );
					Node amy = _db.createNode();
					amy.SetProperty( "name", "Amy" );
					amy.SetProperty( "profession", "Student" );
					amy.SetProperty( "planet", "Mars" );
					transaction.Success();
			  }
		 }

		 private void AssertPairs( IList<Pair<string, Value>> pairs, string key1, string value1, string key2, string value2, string key3, string value3, string key4, string value4 )
		 {
			  assertThat( pairs, Matchers.hasSize( 4 ) );
			  ValidatePair( pairs[0], key1, value1 );
			  ValidatePair( pairs[1], key2, value2 );
			  ValidatePair( pairs[2], key3, value3 );
			  ValidatePair( pairs[3], key4, value4 );
		 }

		 private void ValidatePair( Pair<string, Value> pair, string key, string value )
		 {
			  assertEquals( key, pair.key() );
			  assertEquals( value, pair.value().asString() );
		 }

		 private URI BoltURI()
		 {
			  ConnectorPortRegister connectorPortRegister = ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
			  HostnamePort boltHostNamePort = connectorPortRegister.GetLocalAddress( "bolt" );
			  return URI.create( "bolt://" + boltHostNamePort.Host + ":" + boltHostNamePort.Port );
		 }

	}

}