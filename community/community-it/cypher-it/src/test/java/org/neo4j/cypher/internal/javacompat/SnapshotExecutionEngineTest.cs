﻿/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Cypher.@internal.javacompat
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using QueryStatistics = Org.Neo4j.Graphdb.QueryStatistics;
	using Result = Org.Neo4j.Graphdb.Result;
	using VersionContext = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContext;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using KernelStatement = Org.Neo4j.Kernel.Impl.Api.KernelStatement;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SnapshotExecutionEngineTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule database = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Database = new ImpermanentDatabaseRule();

		 private CompilerFactory _compilerFactory;
		 private TestSnapshotExecutionEngine _executionEngine;
		 private VersionContext _versionContext;
		 private SnapshotExecutionEngine.ParametrizedQueryExecutor _executor;
		 private TransactionalContext _transactionalContext;
		 private readonly Config _config = Config.defaults();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  GraphDatabaseQueryService cypherService = new GraphDatabaseCypherService( this.Database.GraphDatabaseAPI );

			  _compilerFactory = mock( typeof( CompilerFactory ) );
			  _transactionalContext = mock( typeof( TransactionalContext ) );
			  KernelStatement kernelStatement = mock( typeof( KernelStatement ) );
			  _executor = mock( typeof( SnapshotExecutionEngine.ParametrizedQueryExecutor ) );
			  _versionContext = mock( typeof( VersionContext ) );

			  _executionEngine = CreateExecutionEngine( cypherService );
			  when( kernelStatement.VersionContext ).thenReturn( _versionContext );
			  when( _transactionalContext.statement() ).thenReturn(kernelStatement);
			  Result result = mock( typeof( Result ) );
			  QueryStatistics statistics = mock( typeof( QueryStatistics ) );
			  when( result.QueryStatistics ).thenReturn( statistics );
			  when( _executor.execute( any(), anyMap(), any() ) ).thenReturn(result);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryWithoutRetries() throws org.neo4j.kernel.impl.query.QueryExecutionKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExecuteQueryWithoutRetries()
		 {
			  _executionEngine.executeWithRetries( "query", Collections.emptyMap(), _transactionalContext, _executor );

			  verify( _executor, times( 1 ) ).execute( any(), anyMap(), any() );
			  verify( _versionContext, times( 1 ) ).initRead();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryAfterSeveralRetries() throws org.neo4j.kernel.impl.query.QueryExecutionKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExecuteQueryAfterSeveralRetries()
		 {
			  when( _versionContext.Dirty ).thenReturn( true, true, false );

			  _executionEngine.executeWithRetries( "query", Collections.emptyMap(), _transactionalContext, _executor );

			  verify( _executor, times( 3 ) ).execute( any(), anyMap(), any() );
			  verify( _versionContext, times( 3 ) ).initRead();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failQueryAfterMaxRetriesReached() throws org.neo4j.kernel.impl.query.QueryExecutionKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailQueryAfterMaxRetriesReached()
		 {
			  when( _versionContext.Dirty ).thenReturn( true );

			  try
			  {
					_executionEngine.executeWithRetries( "query", Collections.emptyMap(), _transactionalContext, _executor );
			  }
			  catch ( QueryExecutionKernelException e )
			  {
					assertEquals( "Unable to get clean data snapshot for query 'query' after 5 attempts.", e.Message );
			  }

			  verify( _executor, times( 5 ) ).execute( any(), anyMap(), any() );
			  verify( _versionContext, times( 5 ) ).initRead();
		 }

		 private class TestSnapshotExecutionEngine : SnapshotExecutionEngine
		 {
			 private readonly SnapshotExecutionEngineTest _outerInstance;


			  internal TestSnapshotExecutionEngine( SnapshotExecutionEngineTest outerInstance, GraphDatabaseQueryService queryService, Config config, LogProvider logProvider, CompilerFactory compatibilityFactory ) : base( queryService, config, logProvider, compatibilityFactory )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> org.neo4j.graphdb.Result executeWithRetries(String query, T parameters, org.neo4j.kernel.impl.query.TransactionalContext context, ParametrizedQueryExecutor<T> executor) throws org.neo4j.kernel.impl.query.QueryExecutionKernelException
			  public override Result ExecuteWithRetries<T>( string query, T parameters, TransactionalContext context, ParametrizedQueryExecutor<T> executor )
			  {
					return base.ExecuteWithRetries( query, parameters, context, executor );
			  }
		 }

		 private TestSnapshotExecutionEngine CreateExecutionEngine( GraphDatabaseQueryService cypherService )
		 {
			  return new TestSnapshotExecutionEngine( this, cypherService, _config, NullLogProvider.Instance, _compilerFactory );
		 }
	}

}