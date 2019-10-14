/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.database
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using ExecutionEngine = Neo4Net.Cypher.Internal.javacompat.ExecutionEngine;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using QueryRegistryOperations = Neo4Net.Kernel.api.QueryRegistryOperations;
	using Statement = Neo4Net.Kernel.api.Statement;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using TopLevelTransaction = Neo4Net.Kernel.impl.coreapi.TopLevelTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using HttpHeaderUtils = Neo4Net.Server.web.HttpHeaderUtils;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;

	public class CypherExecutorTest
	{
		 private const long CUSTOM_TRANSACTION_TIMEOUT = 1000L;
		 private const string QUERY = "create (n)";

		 private Database _database;
		 private GraphDatabaseFacade _databaseFacade;
		 private DependencyResolver _resolver;
		 private QueryExecutionEngine _executionEngine;
		 private ThreadToStatementContextBridge _statementBridge;
		 private GraphDatabaseQueryService _databaseQueryService;
		 private KernelTransaction _kernelTransaction;
		 private Statement _statement;
		 private HttpServletRequest _request;
		 private AssertableLogProvider _logProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  SetUpMocks();
			  InitLogProvider();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startDefaultTransaction()
		 public virtual void StartDefaultTransaction()
		 {
			  CypherExecutor cypherExecutor = new CypherExecutor( _database, _logProvider );
			  cypherExecutor.Start();

			  cypherExecutor.CreateTransactionContext( QUERY, VirtualValues.emptyMap(), _request );

			  verify( _databaseQueryService ).beginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED );
			  _logProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startTransactionWithCustomTimeout()
		 public virtual void StartTransactionWithCustomTimeout()
		 {
			  when( _request.getHeader( HttpHeaderUtils.MAX_EXECUTION_TIME_HEADER ) ).thenReturn( CUSTOM_TRANSACTION_TIMEOUT.ToString() );

			  CypherExecutor cypherExecutor = new CypherExecutor( _database, _logProvider );
			  cypherExecutor.Start();

			  cypherExecutor.CreateTransactionContext( QUERY, VirtualValues.emptyMap(), _request );

			  verify( _databaseQueryService ).beginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED, CUSTOM_TRANSACTION_TIMEOUT, TimeUnit.MILLISECONDS );
			  _logProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startDefaultTransactionWhenHeaderHasIncorrectValue()
		 public virtual void StartDefaultTransactionWhenHeaderHasIncorrectValue()
		 {
			  when( _request.getHeader( HttpHeaderUtils.MAX_EXECUTION_TIME_HEADER ) ).thenReturn( "not a number" );

			  CypherExecutor cypherExecutor = new CypherExecutor( _database, _logProvider );
			  cypherExecutor.Start();

			  cypherExecutor.CreateTransactionContext( QUERY, VirtualValues.emptyMap(), _request );

			  verify( _databaseQueryService ).beginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED );
			  _logProvider.rawMessageMatcher().assertContains("Fail to parse `max-execution-time` header with value: 'not a number'. Should be a positive number.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startDefaultTransactionIfTimeoutIsNegative()
		 public virtual void StartDefaultTransactionIfTimeoutIsNegative()
		 {
			  when( _request.getHeader( HttpHeaderUtils.MAX_EXECUTION_TIME_HEADER ) ).thenReturn( "-2" );

			  CypherExecutor cypherExecutor = new CypherExecutor( _database, _logProvider );
			  cypherExecutor.Start();

			  cypherExecutor.CreateTransactionContext( QUERY, VirtualValues.emptyMap(), _request );

			  verify( _databaseQueryService ).beginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED );
			  _logProvider.assertNoLoggingOccurred();
		 }

		 private void InitLogProvider()
		 {
			  _logProvider = new AssertableLogProvider( true );
		 }

		 private void SetUpMocks()
		 {
			  _database = mock( typeof( Database ) );
			  _databaseFacade = mock( typeof( GraphDatabaseFacade ) );
			  _resolver = mock( typeof( DependencyResolver ) );
			  _executionEngine = mock( typeof( ExecutionEngine ) );
			  _statementBridge = mock( typeof( ThreadToStatementContextBridge ) );
			  _databaseQueryService = mock( typeof( GraphDatabaseQueryService ) );
			  _kernelTransaction = mock( typeof( KernelTransaction ) );
			  _statement = mock( typeof( Statement ) );
			  _request = mock( typeof( HttpServletRequest ) );

			  InternalTransaction transaction = new TopLevelTransaction( _kernelTransaction );

			  LoginContext loginContext = AUTH_DISABLED;
			  KernelTransaction.Type type = KernelTransaction.Type.@implicit;
			  QueryRegistryOperations registryOperations = mock( typeof( QueryRegistryOperations ) );
			  when( _statement.queryRegistration() ).thenReturn(registryOperations);
			  when( _statementBridge.get() ).thenReturn(_statement);
			  when( _kernelTransaction.securityContext() ).thenReturn(loginContext.Authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME));
			  when( _kernelTransaction.transactionType() ).thenReturn(type);
			  when( _database.Graph ).thenReturn( _databaseFacade );
			  when( _databaseFacade.DependencyResolver ).thenReturn( _resolver );
			  when( _resolver.resolveDependency( typeof( QueryExecutionEngine ) ) ).thenReturn( _executionEngine );
			  when( _resolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ) ).thenReturn( _statementBridge );
			  when( _resolver.resolveDependency( typeof( GraphDatabaseQueryService ) ) ).thenReturn( _databaseQueryService );
			  when( _databaseQueryService.beginTransaction( type, loginContext ) ).thenReturn( transaction );
			  when( _databaseQueryService.beginTransaction( type, loginContext, CUSTOM_TRANSACTION_TIMEOUT, TimeUnit.MILLISECONDS ) ).thenReturn( transaction );
			  when( _databaseQueryService.DependencyResolver ).thenReturn( _resolver );
			  when( _request.Scheme ).thenReturn( "http" );
			  when( _request.RemoteAddr ).thenReturn( "127.0.0.1" );
			  when( _request.RemotePort ).thenReturn( 5678 );
			  when( _request.ServerName ).thenReturn( "127.0.0.1" );
			  when( _request.ServerPort ).thenReturn( 7474 );
			  when( _request.RequestURI ).thenReturn( "/" );
		 }
	}

}