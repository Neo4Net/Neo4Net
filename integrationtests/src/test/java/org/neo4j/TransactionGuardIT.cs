using System;
using System.Collections.Generic;

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
namespace Org.Neo4j
{
	using After = org.junit.After;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;


	using OnlineBackupSettings = Org.Neo4j.backup.OnlineBackupSettings;
	using Driver = Org.Neo4j.driver.v1.Driver;
	using GraphDatabase = Org.Neo4j.driver.v1.GraphDatabase;
	using Session = Org.Neo4j.driver.v1.Session;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using Org.Neo4j.Graphdb.config;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using IdContextFactory = Org.Neo4j.Graphdb.factory.module.id.IdContextFactory;
	using IdContextFactoryBuilder = Org.Neo4j.Graphdb.factory.module.id.IdContextFactoryBuilder;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using HttpConnector = Org.Neo4j.Kernel.configuration.HttpConnector;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using KernelTransactionMonitor = Org.Neo4j.Kernel.Impl.Api.transaciton.monitor.KernelTransactionMonitor;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;
	using EnterpriseIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGenerator = Org.Neo4j.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using CommunityNeoServer = Org.Neo4j.Server.CommunityNeoServer;
	using SimpleGraphFactory = Org.Neo4j.Server.database.SimpleGraphFactory;
	using OpenEnterpriseNeoServer = Org.Neo4j.Server.enterprise.OpenEnterpriseNeoServer;
	using EnterpriseServerBuilder = Org.Neo4j.Server.enterprise.helpers.EnterpriseServerBuilder;
	using HttpHeaderUtils = Org.Neo4j.Server.web.HttpHeaderUtils;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestGraphDatabaseFactoryState = Org.Neo4j.Test.TestGraphDatabaseFactoryState;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using HTTP = Org.Neo4j.Test.server.HTTP;
	using Clocks = Org.Neo4j.Time.Clocks;
	using FakeClock = Org.Neo4j.Time.FakeClock;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.transaction_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.TransactionNotFound;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class TransactionGuardIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.CleanupRule cleanupRule = new org.neo4j.test.rule.CleanupRule();
		 public static readonly CleanupRule CleanupRule = new CleanupRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public static readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private const string BOLT_CONNECTOR_KEY = "bolt";

		 private static readonly FakeClock _fakeClock = Clocks.fakeClock();
		 private static GraphDatabaseAPI _databaseWithTimeout;
		 private static GraphDatabaseAPI _databaseWithoutTimeout;
		 private static OpenEnterpriseNeoServer _neoServer;
		 private static int _boltPortDatabaseWithTimeout;
		 private const string DEFAULT_TIMEOUT = "2s";
		 private static readonly KernelTransactionTimeoutMonitorSupplier _monitorSupplier = new KernelTransactionTimeoutMonitorSupplier();
		 private static readonly IdInjectionFunctionAction _getIdInjectionFunction = new IdInjectionFunctionAction( _monitorSupplier );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _monitorSupplier.clear();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateLongRunningTransaction()
		 public virtual void TerminateLongRunningTransaction()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  try
			  {
					  using ( Transaction transaction = database.BeginTx() )
					  {
						_fakeClock.forward( 3, TimeUnit.SECONDS );
						transaction.Success();
						timeoutMonitor.Run();
						database.CreateNode();
						fail( "Transaction should be already terminated." );
					  }
			  }
			  catch ( TransactionTerminatedException e )
			  {
					assertThat( e.Message, startsWith( "The transaction has been terminated." ) );
					assertEquals( e.Status(), Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.TransactionTimedOut );
			  }

			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateLongRunningTransactionWithPeriodicCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateLongRunningTransactionWithPeriodicCommit()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  _monitorSupplier.TransactionTimeoutMonitor = timeoutMonitor;
			  try
			  {
					URL url = PrepareTestImportFile( 8 );
					database.Execute( "USING PERIODIC COMMIT 5 LOAD CSV FROM '" + url + "' AS line CREATE ();" );
					fail( "Transaction should be already terminated." );
			  }
			  catch ( TransactionTerminatedException )
			  {
			  }
			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateTransactionWithCustomTimeoutWithoutConfiguredDefault()
		 public virtual void TerminateTransactionWithCustomTimeoutWithoutConfiguredDefault()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithoutTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  using ( Transaction transaction = database.BeginTx( 27, TimeUnit.SECONDS ) )
			  {
					_fakeClock.forward( 26, TimeUnit.SECONDS );
					timeoutMonitor.Run();
					database.CreateNode();
					transaction.Failure();
			  }

			  try
			  {
					  using ( Transaction transaction = database.BeginTx( 27, TimeUnit.SECONDS ) )
					  {
						_fakeClock.forward( 28, TimeUnit.SECONDS );
						timeoutMonitor.Run();
						database.CreateNode();
						fail( "Transaction should be already terminated." );
					  }
			  }
			  catch ( TransactionTerminatedException e )
			  {
					assertThat( e.Message, startsWith( "The transaction has been terminated." ) );
			  }

			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateLongRunningQueryTransaction()
		 public virtual void TerminateLongRunningQueryTransaction()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  _monitorSupplier.TransactionTimeoutMonitor = timeoutMonitor;

			  try
			  {
					  using ( Transaction transaction = database.BeginTx() )
					  {
						_fakeClock.forward( 3, TimeUnit.SECONDS );
						timeoutMonitor.Run();
						transaction.Success();
						database.Execute( "create (n)" );
						fail( "Transaction should be already terminated." );
					  }
			  }
			  catch ( TransactionTerminatedException e )
			  {
					assertThat( e.Message, startsWith( "The transaction has been terminated." ) );
			  }

			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateLongRunningQueryWithCustomTimeoutWithoutConfiguredDefault()
		 public virtual void TerminateLongRunningQueryWithCustomTimeoutWithoutConfiguredDefault()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithoutTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  using ( Transaction transaction = database.BeginTx( 5, TimeUnit.SECONDS ) )
			  {
					_fakeClock.forward( 4, TimeUnit.SECONDS );
					timeoutMonitor.Run();
					database.Execute( "create (n)" );
					transaction.Failure();
			  }

			  try
			  {
					  using ( Transaction transaction = database.BeginTx( 6, TimeUnit.SECONDS ) )
					  {
						_fakeClock.forward( 7, TimeUnit.SECONDS );
						timeoutMonitor.Run();
						transaction.Success();
						database.Execute( "create (n)" );
						fail( "Transaction should be already terminated." );
					  }
			  }
			  catch ( TransactionTerminatedException e )
			  {
					assertThat( e.Message, startsWith( "The transaction has been terminated." ) );
			  }

			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateLongRunningRestTransactionalEndpointQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateLongRunningRestTransactionalEndpointQuery()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  OpenEnterpriseNeoServer neoServer = StartNeoServer( ( GraphDatabaseFacade ) database );
			  string transactionEndPoint = HTTP.POST( TransactionUri( neoServer ) ).location();

			  _fakeClock.forward( 3, TimeUnit.SECONDS );
			  timeoutMonitor.Run();

			  HTTP.Response response = HTTP.POST( transactionEndPoint, quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n)' } ] }" ) );
			  assertEquals( "Response should be successful.", 200, response.Status() );

			  HTTP.Response commitResponse = HTTP.POST( transactionEndPoint + "/commit" );
			  assertEquals( "Transaction should be already closed and not found.", 404, commitResponse.Status() );

			  assertEquals( "Transaction should be forcefully closed.", TransactionNotFound.code().serialize(), commitResponse.Get("errors").findValue("code").asText() );
			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateLongRunningRestTransactionalEndpointWithCustomTimeoutQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateLongRunningRestTransactionalEndpointWithCustomTimeoutQuery()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  OpenEnterpriseNeoServer neoServer = StartNeoServer( ( GraphDatabaseFacade ) database );
			  long customTimeout = TimeUnit.SECONDS.toMillis( 10 );
			  HTTP.Response beginResponse = HTTP.withHeaders( HttpHeaderUtils.MAX_EXECUTION_TIME_HEADER, customTimeout.ToString() ).POST(TransactionUri(neoServer), quotedJson("{ 'statements': [ { 'statement': 'CREATE (n)' } ] }"));
			  assertEquals( "Response should be successful.", 201, beginResponse.Status() );

			  string transactionEndPoint = beginResponse.Location();
			  _fakeClock.forward( 3, TimeUnit.SECONDS );

			  HTTP.Response response = HTTP.POST( transactionEndPoint, quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n)' } ] }" ) );
			  assertEquals( "Response should be successful.", 200, response.Status() );

			  _fakeClock.forward( 11, TimeUnit.SECONDS );
			  timeoutMonitor.Run();

			  response = HTTP.POST( transactionEndPoint, quotedJson( "{ 'statements': [ { 'statement': 'CREATE (n)' } ] }" ) );
			  assertEquals( "Response should be successful.", 200, response.Status() );

			  HTTP.Response commitResponse = HTTP.POST( transactionEndPoint + "/commit" );
			  assertEquals( "Transaction should be already closed and not found.", 404, commitResponse.Status() );

			  assertEquals( "Transaction should be forcefully closed.", TransactionNotFound.code().serialize(), commitResponse.Get("errors").findValue("code").asText() );
			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateLongRunningDriverQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateLongRunningDriverQuery()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  OpenEnterpriseNeoServer neoServer = StartNeoServer( ( GraphDatabaseFacade ) database );

			  Org.Neo4j.driver.v1.Config driverConfig = DriverConfig;

			  using ( Driver driver = GraphDatabase.driver( "bolt://localhost:" + _boltPortDatabaseWithTimeout, driverConfig ), Session session = driver.session() )
			  {
					Org.Neo4j.driver.v1.Transaction transaction = session.beginTransaction();
					transaction.run( "create (n)" ).consume();
					transaction.success();
					_fakeClock.forward( 3, TimeUnit.SECONDS );
					timeoutMonitor.Run();
					try
					{
						 transaction.run( "create (n)" ).consume();
						 fail( "Transaction should be already terminated by execution guard." );
					}
					catch ( Exception )
					{
						 // ignored
					}
			  }
			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminateLongRunningDriverPeriodicCommitQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TerminateLongRunningDriverPeriodicCommitQuery()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  _monitorSupplier.TransactionTimeoutMonitor = timeoutMonitor;
			  OpenEnterpriseNeoServer neoServer = StartNeoServer( ( GraphDatabaseFacade ) database );

			  Org.Neo4j.driver.v1.Config driverConfig = DriverConfig;

			  try
			  {
					  using ( Driver driver = GraphDatabase.driver( "bolt://localhost:" + _boltPortDatabaseWithTimeout, driverConfig ), Session session = driver.session() )
					  {
						URL url = PrepareTestImportFile( 8 );
						session.run( "USING PERIODIC COMMIT 5 LOAD CSV FROM '" + url + "' AS line CREATE ();" ).consume();
						fail( "Transaction should be already terminated by execution guard." );
					  }
			  }
			  catch ( Exception )
			  {
					//
			  }
			  AssertDatabaseDoesNotHaveNodes( database );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changeTimeoutAtRuntime()
		 public virtual void ChangeTimeoutAtRuntime()
		 {
			  GraphDatabaseAPI database = StartDatabaseWithTimeout();
			  KernelTransactionMonitor timeoutMonitor = database.DependencyResolver.resolveDependency( typeof( KernelTransactionMonitor ) );
			  try
			  {
					  using ( Transaction transaction = database.BeginTx() )
					  {
						_fakeClock.forward( 3, TimeUnit.SECONDS );
						timeoutMonitor.Run();
						transaction.Success();
						database.Execute( "create (n)" );
						fail( "Transaction should be already terminated." );
					  }
			  }
			  catch ( TransactionTerminatedException e )
			  {
					assertThat( e.Message, startsWith( "The transaction has been terminated." ) );
			  }

			  AssertDatabaseDoesNotHaveNodes( database );

			  // Increase timeout
			  using ( Transaction transaction = database.BeginTx() )
			  {
					database.Execute( "CALL dbms.setConfigValue('" + transaction_timeout.name() + "', '5s')" );
					transaction.Success();
			  }

			  using ( Transaction transaction = database.BeginTx() )
			  {
					_fakeClock.forward( 3, TimeUnit.SECONDS );
					timeoutMonitor.Run();
					transaction.Success();
					database.Execute( "create (n)" );
			  }

			  // Assert node successfully created
			  using ( Transaction ignored = database.BeginTx() )
			  {
					assertEquals( 1, database.AllNodes.Count() );
			  }

			  // Reset timeout and cleanup
			  using ( Transaction transaction = database.BeginTx() )
			  {
					database.Execute( "CALL dbms.setConfigValue('" + transaction_timeout.name() + "', '" + DEFAULT_TIMEOUT + "')" );
					using ( Stream<Node> stream = database.AllNodes.stream() )
					{
						 stream.findFirst().map(node =>
						 {
						  node.delete();
						  return node;
						 });
					}
					transaction.Success();
			  }
		 }

		 private GraphDatabaseAPI StartDatabaseWithTimeout()
		 {
			  if ( _databaseWithTimeout == null )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> configMap = getSettingsWithTimeoutAndBolt();
					IDictionary<Setting<object>, string> configMap = SettingsWithTimeoutAndBolt;
					_databaseWithTimeout = StartCustomDatabase( TestDirectory.directory( "dbWithTimeout" ), configMap );
					_boltPortDatabaseWithTimeout = GetBoltConnectorPort( _databaseWithTimeout );
			  }
			  return _databaseWithTimeout;
		 }

		 private int GetBoltConnectorPort( GraphDatabaseAPI databaseAPI )
		 {
			  ConnectorPortRegister connectorPortRegister = databaseAPI.DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
			  return connectorPortRegister.GetLocalAddress( BOLT_CONNECTOR_KEY ).Port;
		 }

		 private GraphDatabaseAPI StartDatabaseWithoutTimeout()
		 {
			  if ( _databaseWithoutTimeout == null )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> configMap = getSettingsWithoutTransactionTimeout();
					IDictionary<Setting<object>, string> configMap = SettingsWithoutTransactionTimeout;
					_databaseWithoutTimeout = StartCustomDatabase( TestDirectory.directory( "dbWithoutTimeout" ), configMap );
			  }
			  return _databaseWithoutTimeout;
		 }

		 private Org.Neo4j.driver.v1.Config DriverConfig
		 {
			 get
			 {
				  return Org.Neo4j.driver.v1.Config.build().withEncryptionLevel(Org.Neo4j.driver.v1.Config.EncryptionLevel.NONE).toConfig();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.server.enterprise.OpenEnterpriseNeoServer startNeoServer(org.neo4j.kernel.impl.factory.GraphDatabaseFacade database) throws java.io.IOException
		 private OpenEnterpriseNeoServer StartNeoServer( GraphDatabaseFacade database )
		 {
			  if ( _neoServer == null )
			  {
					GuardingServerBuilder serverBuilder = new GuardingServerBuilder( this, database );
					BoltConnector boltConnector = new BoltConnector( BOLT_CONNECTOR_KEY );
					serverBuilder.WithProperty( boltConnector.Type.name(), "BOLT" ).withProperty(boltConnector.Enabled.name(), Settings.TRUE).withProperty(boltConnector.EncryptionLevel.name(), BoltConnector.EncryptionLevel.DISABLED.name()).withProperty(GraphDatabaseSettings.auth_enabled.name(), Settings.FALSE);
					serverBuilder.WithProperty( ( new HttpConnector( "http" ) ).listen_address.name(), "localhost:0" );
					_neoServer = serverBuilder.Build();
					CleanupRule.add( _neoServer );
					_neoServer.start();
			  }
			  return _neoServer;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> getSettingsWithTimeoutAndBolt()
		 private IDictionary<Setting<object>, string> SettingsWithTimeoutAndBolt
		 {
			 get
			 {
				  BoltConnector boltConnector = new BoltConnector( BOLT_CONNECTOR_KEY );
				  return MapUtil.genericMap( transaction_timeout, DEFAULT_TIMEOUT, boltConnector.Address, "localhost:0", boltConnector.Type, "BOLT", boltConnector.Enabled, "true", boltConnector.EncryptionLevel, BoltConnector.EncryptionLevel.DISABLED.name(), GraphDatabaseSettings.auth_enabled, "false" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Map<org.neo4j.graphdb.config.Setting<?>,String> getSettingsWithoutTransactionTimeout()
		 private IDictionary<Setting<object>, string> SettingsWithoutTransactionTimeout
		 {
			 get
			 {
				  return MapUtil.genericMap();
			 }
		 }

		 private string TransactionUri( OpenEnterpriseNeoServer neoServer )
		 {
			  return neoServer.BaseUri().ToString() + "db/data/transaction";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URL prepareTestImportFile(int lines) throws java.io.IOException
		 private URL PrepareTestImportFile( int lines )
		 {
			  File tempFile = File.createTempFile( "testImport", ".csv" );
			  using ( PrintWriter writer = FileUtils.newFilePrintWriter( tempFile, StandardCharsets.UTF_8 ) )
			  {
					for ( int i = 0; i < lines; i++ )
					{
						 writer.println( "a,b,c" );
					}
			  }
			  return tempFile.toURI().toURL();
		 }

		 private void AssertDatabaseDoesNotHaveNodes( GraphDatabaseAPI database )
		 {
			  using ( Transaction ignored = database.BeginTx() )
			  {
					assertEquals( 0, database.AllNodes.Count() );
			  }
		 }

		 private GraphDatabaseAPI StartCustomDatabase<T1>( File storeDir, IDictionary<T1> configMap )
		 {
			  CustomClockEnterpriseFacadeFactory customClockEnterpriseFacadeFactory = new CustomClockEnterpriseFacadeFactory( this );
			  GraphDatabaseBuilder databaseBuilder = ( new CustomGuardTestTestGraphDatabaseFactory( this, customClockEnterpriseFacadeFactory ) ).NewImpermanentDatabaseBuilder( storeDir );
			  configMap.forEach( databaseBuilder.setConfig );
			  databaseBuilder.SetConfig( GraphDatabaseSettings.record_id_batch_size, "1" );

			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) databaseBuilder.SetConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE ).newGraphDatabase();
			  CleanupRule.add( database );
			  return database;
		 }

		 private class KernelTransactionTimeoutMonitorSupplier : System.Func<KernelTransactionMonitor>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile KernelTransactionMonitor TransactionTimeoutMonitorConflict;

			  internal virtual KernelTransactionMonitor TransactionTimeoutMonitor
			  {
				  set
				  {
						this.TransactionTimeoutMonitorConflict = value;
				  }
			  }

			  public override KernelTransactionMonitor Get()
			  {
					return TransactionTimeoutMonitorConflict;
			  }

			  public virtual void Clear()
			  {
					TransactionTimeoutMonitor = null;
			  }
		 }

		 private class IdInjectionFunctionAction
		 {
			  internal readonly System.Func<KernelTransactionMonitor> MonitorSupplier;

			  internal IdInjectionFunctionAction( System.Func<KernelTransactionMonitor> monitorSupplier )
			  {
					this.MonitorSupplier = monitorSupplier;
			  }

			  internal virtual void TickAndCheck()
			  {
					KernelTransactionMonitor timeoutMonitor = MonitorSupplier.get();
					if ( timeoutMonitor != null )
					{
						 _fakeClock.forward( 1, TimeUnit.SECONDS );
						 timeoutMonitor.Run();
					}
			  }
		 }

		 private class GuardingServerBuilder : EnterpriseServerBuilder
		 {
			 private readonly TransactionGuardIT _outerInstance;

			  internal GraphDatabaseFacade GraphDatabaseFacade;

			  internal GuardingServerBuilder( TransactionGuardIT outerInstance, GraphDatabaseFacade graphDatabaseAPI ) : base( NullLogProvider.Instance )
			  {
				  this._outerInstance = outerInstance;
					this.GraphDatabaseFacade = graphDatabaseAPI;
			  }

			  protected internal override CommunityNeoServer Build( File configFile, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
			  {
					return new GuardTestServer( this, config, newDependencies( dependencies ).userLogProvider( NullLogProvider.Instance ) );
			  }

			  private class GuardTestServer : OpenEnterpriseNeoServer
			  {
				  private readonly TransactionGuardIT.GuardingServerBuilder _outerInstance;

					internal GuardTestServer( TransactionGuardIT.GuardingServerBuilder outerInstance, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( config, new SimpleGraphFactory( outerInstance.GraphDatabaseFacade ), dependencies )
					{
						this._outerInstance = outerInstance;
					}
			  }
		 }

		 private class CustomGuardTestTestGraphDatabaseFactory : TestGraphDatabaseFactory
		 {
			 private readonly TransactionGuardIT _outerInstance;


			  internal GraphDatabaseFacadeFactory CustomFacadeFactory;

			  internal CustomGuardTestTestGraphDatabaseFactory( TransactionGuardIT outerInstance, GraphDatabaseFacadeFactory customFacadeFactory )
			  {
				  this._outerInstance = outerInstance;
					this.CustomFacadeFactory = customFacadeFactory;
			  }

			  protected internal override GraphDatabaseBuilder.DatabaseCreator CreateImpermanentDatabaseCreator( File storeDir, TestGraphDatabaseFactoryState state )
			  {
					return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
			  }

			  private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
			  {
				  private readonly CustomGuardTestTestGraphDatabaseFactory _outerInstance;

				  private File _storeDir;
				  private TestGraphDatabaseFactoryState _state;

				  public DatabaseCreatorAnonymousInnerClass( CustomGuardTestTestGraphDatabaseFactory outerInstance, File storeDir, TestGraphDatabaseFactoryState state )
				  {
					  this.outerInstance = outerInstance;
					  this._storeDir = storeDir;
					  this._state = state;
				  }

				  public GraphDatabaseService newDatabase( Config config )
				  {
						return _outerInstance.customFacadeFactory.newFacade( _storeDir, config, newDependencies( _state.databaseDependencies() ) );
				  }
			  }
		 }

		 private class TransactionGuardTerminationEditionModule : EnterpriseEditionModule
		 {
			 private readonly TransactionGuardIT _outerInstance;

			  internal TransactionGuardTerminationEditionModule( TransactionGuardIT outerInstance, PlatformModule platformModule ) : base( platformModule )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override IdContextFactory CreateIdContextFactory( PlatformModule platformModule, FileSystemAbstraction fileSystem )
			  {
					return IdContextFactoryBuilder.of( new EnterpriseIdTypeConfigurationProvider( platformModule.Config ), platformModule.JobScheduler ).withIdGenerationFactoryProvider( any => new TerminationIdGeneratorFactory( _outerInstance, new DefaultIdGeneratorFactory( platformModule.FileSystem ) ) ).build();
			  }
		 }

		 private class CustomClockEnterpriseFacadeFactory : GraphDatabaseFacadeFactory
		 {
			 private readonly TransactionGuardIT _outerInstance;


			  internal CustomClockEnterpriseFacadeFactory( TransactionGuardIT outerInstance ) : base(DatabaseInfo.ENTERPRISE, new System.Func<PlatformModule, AbstractEditionModule>() // Don't make a lambda
			  {
					// XXX: This has to be a Function, JVM crashes with ClassFormatError if you pass a lambda here
				  this._outerInstance = outerInstance;
				  {
						 public AbstractEditionModule apply( PlatformModule platformModule )
						 {
							  return new TransactionGuardTerminationEditionModule( outerInstance, platformModule );
						 }
				  }
				  );
			  }
			  protected internal override PlatformModule CreatePlatform( File storeDir, Config config, Dependencies dependencies )
			  {
					return new PlatformModuleAnonymousInnerClass( this, storeDir, config, DatabaseInfo, dependencies );
			  }

			  private class PlatformModuleAnonymousInnerClass : PlatformModule
			  {
				  private readonly CustomClockEnterpriseFacadeFactory _outerInstance;

				  public PlatformModuleAnonymousInnerClass( CustomClockEnterpriseFacadeFactory outerInstance, File storeDir, Config config, DatabaseInfo databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override SystemNanoClock createClock()
				  {
						return _fakeClock;
				  }
			  }
		 }

		 private class TerminationIdGeneratorFactory : IdGeneratorFactory
		 {
			 private readonly TransactionGuardIT _outerInstance;

			  internal IdGeneratorFactory Delegate;

			  internal TerminationIdGeneratorFactory( TransactionGuardIT outerInstance, IdGeneratorFactory @delegate )
			  {
				  this._outerInstance = outerInstance;
					this.Delegate = @delegate;
			  }

			  public override IdGenerator Open( File filename, IdType idType, System.Func<long> highIdSupplier, long maxId )
			  {
					return Delegate.open( filename, idType, highIdSupplier, maxId );
			  }

			  public override IdGenerator Open( File filename, int grabSize, IdType idType, System.Func<long> highIdSupplier, long maxId )
			  {
					return new TerminationIdGenerator( _outerInstance, Delegate.open( filename, grabSize, idType, highIdSupplier, maxId ) );
			  }

			  public override void Create( File filename, long highId, bool throwIfFileExists )
			  {
					Delegate.create( filename, highId, throwIfFileExists );
			  }

			  public override IdGenerator Get( IdType idType )
			  {
					return Delegate.get( idType );
			  }
		 }

		 private sealed class TerminationIdGenerator : IdGenerator
		 {
			 private readonly TransactionGuardIT _outerInstance;


			  internal IdGenerator Delegate;

			  internal TerminationIdGenerator( TransactionGuardIT outerInstance, IdGenerator @delegate )
			  {
				  this._outerInstance = outerInstance;
					this.Delegate = @delegate;
			  }

			  public override IdRange NextIdBatch( int size )
			  {
					return Delegate.nextIdBatch( size );
			  }

			  public long HighId
			  {
				  set
				  {
						Delegate.HighId = value;
				  }
				  get
				  {
						return Delegate.HighId;
				  }
			  }


			  public long HighestPossibleIdInUse
			  {
				  get
				  {
						return Delegate.HighestPossibleIdInUse;
				  }
			  }

			  public override void FreeId( long id )
			  {
					Delegate.freeId( id );
			  }

			  public override void Close()
			  {
					Delegate.Dispose();
			  }

			  public long NumberOfIdsInUse
			  {
				  get
				  {
						return Delegate.NumberOfIdsInUse;
				  }
			  }

			  public long DefragCount
			  {
				  get
				  {
						return Delegate.DefragCount;
				  }
			  }

			  public override void Delete()
			  {
					Delegate.delete();
			  }

			  public override long NextId()
			  {
					_getIdInjectionFunction.tickAndCheck();
					return Delegate.nextId();
			  }
		 }
	}

}