using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.factory
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using Mockito = org.mockito.Mockito;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using TopLevelTransaction = Neo4Net.Kernel.impl.coreapi.TopLevelTransaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_DEEP_STUBS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.MockedNeoStores.mockedTokenHolders;

	internal class GraphDatabaseFacadeTest
	{
		 private readonly GraphDatabaseFacade.SPI _spi = Mockito.mock( typeof( GraphDatabaseFacade.SPI ), RETURNS_DEEP_STUBS );
		 private readonly GraphDatabaseFacade _graphDatabaseFacade = new GraphDatabaseFacade();
		 private GraphDatabaseQueryService _queryService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _queryService = mock( typeof( GraphDatabaseQueryService ) );
			  DependencyResolver resolver = mock( typeof( DependencyResolver ) );
			  Statement statement = mock( typeof( Statement ), RETURNS_DEEP_STUBS );
			  ThreadToStatementContextBridge contextBridge = mock( typeof( ThreadToStatementContextBridge ) );

			  when( _spi.queryService() ).thenReturn(_queryService);
			  when( _spi.resolver() ).thenReturn(resolver);
			  when( resolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) ) ).thenReturn( contextBridge );
			  when( contextBridge.Get() ).thenReturn(statement);
			  Config config = Config.defaults();
			  when( resolver.ResolveDependency( typeof( Config ) ) ).thenReturn( config );

			  _graphDatabaseFacade.init( _spi, contextBridge, config, mockedTokenHolders() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void BeginTransactionWithCustomTimeout()
		 internal virtual void BeginTransactionWithCustomTimeout()
		 {
			  _graphDatabaseFacade.beginTx( 10, TimeUnit.MILLISECONDS );

			  verify( _spi ).BeginTransaction( KernelTransaction.Type.@explicit, AUTH_DISABLED, 10L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void BeginTransaction()
		 internal virtual void BeginTransaction()
		 {
			  _graphDatabaseFacade.beginTx();

			  long timeout = Config.defaults().get(GraphDatabaseSettings.transaction_timeout).toMillis();
			  verify( _spi ).BeginTransaction( KernelTransaction.Type.@explicit, AUTH_DISABLED, timeout );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void executeQueryWithCustomTimeoutShouldStartTransactionWithRequestedTimeout()
		 internal virtual void ExecuteQueryWithCustomTimeoutShouldStartTransactionWithRequestedTimeout()
		 {
			  _graphDatabaseFacade.execute( "create (n)", 157L, TimeUnit.SECONDS );
			  verify( _spi ).BeginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED, TimeUnit.SECONDS.toMillis( 157L ) );

			  _graphDatabaseFacade.execute( "create (n)", new Dictionary<string, object>(), 247L, TimeUnit.MINUTES );
			  verify( _spi ).BeginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED, TimeUnit.MINUTES.toMillis( 247L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void executeQueryStartDefaultTransaction()
		 internal virtual void ExecuteQueryStartDefaultTransaction()
		 {
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ) );
			  InternalTransaction transaction = new TopLevelTransaction( kernelTransaction );

			  when( _queryService.BeginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED ) ).thenReturn( transaction );

			  _graphDatabaseFacade.execute( "create (n)" );
			  _graphDatabaseFacade.execute( "create (n)", new Dictionary<string, object>() );

			  long timeout = Config.defaults().get(GraphDatabaseSettings.transaction_timeout).toMillis();
			  verify( _spi, times( 2 ) ).BeginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED, timeout );
		 }
	}

}