using System;

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
namespace Neo4Net.Bolt
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using Timeout = org.junit.rules.Timeout;

	using BoltConnectionMetricsMonitor = Neo4Net.Bolt.runtime.BoltConnectionMetricsMonitor;
	using Config = Neo4Net.driver.v1.Config;
	using Driver = Neo4Net.driver.v1.Driver;
	using GraphDatabase = Neo4Net.driver.v1.GraphDatabase;
	using Session = Neo4Net.driver.v1.Session;
	using Transaction = Neo4Net.driver.v1.Transaction;
	using ServiceUnavailableException = Neo4Net.driver.v1.exceptions.ServiceUnavailableException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IOUtils = Neo4Net.Io.IOUtils;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.Connector.ConnectorType.BOLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.PortUtils.getBoltPort;

	public class BoltFailuresIT
	{
		private bool InstanceFieldsInitialized = false;

		public BoltFailuresIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( Timeout.seconds( TEST_TIMEOUT_SECONDS ) ).around( _dir );
		}

		 private const int TEST_TIMEOUT_SECONDS = 120;

		 private readonly TestDirectory _dir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(org.junit.rules.Timeout.seconds(TEST_TIMEOUT_SECONDS)).around(dir);
		 public RuleChain RuleChain;

		 private GraphDatabaseService _db;
		 private Driver _driver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdownDb()
		 public virtual void ShutdownDb()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
			  IOUtils.closeAllSilently( _driver );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenMonitoredWorkerCreationFails()
		 public virtual void ThrowsWhenMonitoredWorkerCreationFails()
		 {
			  ThrowingSessionMonitor sessionMonitor = new ThrowingSessionMonitor();
			  sessionMonitor.ThrowInConnectionOpened();
			  Monitors monitors = NewMonitorsSpy( sessionMonitor );

			  _db = StartDbWithBolt( ( new GraphDatabaseFactory() ).setMonitors(monitors) );
			  try
			  {
					// attempt to create a driver when server is unavailable
					_driver = CreateDriver( getBoltPort( _db ) );
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( ServiceUnavailableException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenInitMessageReceiveFails()
		 public virtual void ThrowsWhenInitMessageReceiveFails()
		 {
			  ThrowsWhenInitMessageFails( ThrowingSessionMonitor.throwInMessageReceived, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenInitMessageProcessingFailsToStart()
		 public virtual void ThrowsWhenInitMessageProcessingFailsToStart()
		 {
			  ThrowsWhenInitMessageFails( ThrowingSessionMonitor.throwInMessageProcessingStarted, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenInitMessageProcessingFailsToComplete()
		 public virtual void ThrowsWhenInitMessageProcessingFailsToComplete()
		 {
			  ThrowsWhenInitMessageFails( ThrowingSessionMonitor.throwInMessageProcessingCompleted, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenRunMessageReceiveFails()
		 public virtual void ThrowsWhenRunMessageReceiveFails()
		 {
			  ThrowsWhenRunMessageFails( ThrowingSessionMonitor.throwInMessageReceived );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenRunMessageProcessingFailsToStart()
		 public virtual void ThrowsWhenRunMessageProcessingFailsToStart()
		 {
			  ThrowsWhenRunMessageFails( ThrowingSessionMonitor.throwInMessageProcessingStarted );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenRunMessageProcessingFailsToComplete()
		 public virtual void ThrowsWhenRunMessageProcessingFailsToComplete()
		 {
			  ThrowsWhenRunMessageFails( ThrowingSessionMonitor.throwInMessageProcessingCompleted );
		 }

		 private void ThrowsWhenInitMessageFails( System.Action<ThrowingSessionMonitor> monitorSetup, bool shouldBeAbleToBeginTransaction )
		 {
			  ThrowingSessionMonitor sessionMonitor = new ThrowingSessionMonitor();
			  monitorSetup( sessionMonitor );
			  Monitors monitors = NewMonitorsSpy( sessionMonitor );

			  _db = StartTestDb( monitors );

			  try
			  {
					_driver = GraphDatabase.driver( "bolt://localhost:" + getBoltPort( _db ), Config.build().withoutEncryption().toConfig() );
					if ( shouldBeAbleToBeginTransaction )
					{
						 using ( Session session = _driver.session(), Transaction tx = session.beginTransaction() )
						 {
							  tx.run( "CREATE ()" ).consume();
						 }
					}
					else
					{
						 fail( "Exception expected" );
					}
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( ServiceUnavailableException ) ) );
			  }
		 }

		 private void ThrowsWhenRunMessageFails( System.Action<ThrowingSessionMonitor> monitorSetup )
		 {
			  ThrowingSessionMonitor sessionMonitor = new ThrowingSessionMonitor();
			  Monitors monitors = NewMonitorsSpy( sessionMonitor );

			  _db = StartTestDb( monitors );
			  _driver = CreateDriver( getBoltPort( _db ) );

			  // open a session and start a transaction, this will force driver to obtain
			  // a network connection and bind it to the transaction
			  Session session = _driver.session();
			  Transaction tx = session.beginTransaction();

			  // at this point driver holds a valid initialize connection
			  // setup monitor to throw before running the query to make processing of the RUN message fail
			  monitorSetup( sessionMonitor );
			  tx.run( "CREATE ()" );
			  try
			  {
					tx.close();
					session.close();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( ServiceUnavailableException ) ) );
			  }
		 }

		 private GraphDatabaseService StartTestDb( Monitors monitors )
		 {
			  return StartDbWithBolt( NewDbFactory().setMonitors(monitors) );
		 }

		 private GraphDatabaseService StartDbWithBolt( GraphDatabaseFactory dbFactory )
		 {
			  return dbFactory.NewEmbeddedDatabaseBuilder( _dir.storeDir() ).setConfig((new BoltConnector("bolt")).type, BOLT.name()).setConfig((new BoltConnector("bolt")).enabled, TRUE).setConfig((new BoltConnector("bolt")).listen_address, "localhost:0").setConfig(GraphDatabaseSettings.auth_enabled, FALSE).setConfig(OnlineBackupSettings.online_backup_enabled, FALSE).newGraphDatabase();
		 }

		 private static TestEnterpriseGraphDatabaseFactory NewDbFactory()
		 {
			  return new TestEnterpriseGraphDatabaseFactory();
		 }

		 private static Driver CreateDriver( int port )
		 {
			  return GraphDatabase.driver( "bolt://localhost:" + port, Config.build().withoutEncryption().toConfig() );
		 }

		 private static Monitors NewMonitorsSpy( ThrowingSessionMonitor sessionMonitor )
		 {
			  Monitors monitors = spy( new Monitors() );
			  // it is not allowed to throw exceptions from monitors
			  // make the given sessionMonitor be returned as is, without any proxying
			  when( monitors.NewMonitor( typeof( BoltConnectionMetricsMonitor ) ) ).thenReturn( sessionMonitor );
			  when( monitors.HasListeners( typeof( BoltConnectionMetricsMonitor ) ) ).thenReturn( true );
			  return monitors;
		 }

		 private class ThrowingSessionMonitor : BoltConnectionMetricsMonitor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool ThrowInConnectionOpenedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool ThrowInMessageReceivedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool ThrowInMessageProcessingStartedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool ThrowInMessageProcessingCompletedConflict;

			  public override void ConnectionOpened()
			  {
					ThrowIfNeeded( ThrowInConnectionOpenedConflict );
			  }

			  public override void ConnectionActivated()
			  {

			  }

			  public override void ConnectionWaiting()
			  {

			  }

			  public override void MessageReceived()
			  {
					ThrowIfNeeded( ThrowInMessageReceivedConflict );
			  }

			  public override void MessageProcessingStarted( long queueTime )
			  {
					ThrowIfNeeded( ThrowInMessageProcessingStartedConflict );
			  }

			  public override void MessageProcessingCompleted( long processingTime )
			  {
					ThrowIfNeeded( ThrowInMessageProcessingCompletedConflict );
			  }

			  public override void MessageProcessingFailed()
			  {

			  }

			  public override void ConnectionClosed()
			  {

			  }

			  internal virtual void ThrowInConnectionOpened()
			  {
					ThrowInConnectionOpenedConflict = true;
			  }

			  internal virtual void ThrowInMessageReceived()
			  {
					ThrowInMessageReceivedConflict = true;
			  }

			  internal virtual void ThrowInMessageProcessingStarted()
			  {
					ThrowInMessageProcessingStartedConflict = true;
			  }

			  internal virtual void ThrowInMessageProcessingCompleted()
			  {
					ThrowInMessageProcessingCompletedConflict = true;
			  }

			  internal virtual void ThrowIfNeeded( bool shouldThrow )
			  {
					if ( shouldThrow )
					{
						 throw new Exception();
					}
			  }
		 }
	}

}