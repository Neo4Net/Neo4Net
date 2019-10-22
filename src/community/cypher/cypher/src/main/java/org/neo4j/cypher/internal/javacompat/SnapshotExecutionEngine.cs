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
namespace Neo4Net.Cypher.Internal.javacompat
{
	using Result = Neo4Net.GraphDb.Result;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelStatement = Neo4Net.Kernel.Impl.Api.KernelStatement;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	/// <summary>
	/// <seealso cref="ExecutionEngine"/> engine that will try to run cypher query with guarantee that query will never see any data
	/// that coming from transaction that are newer then transaction that was the last closed on a moment when
	/// <seealso cref="VersionContext"/> was initialised. Observed behaviour is the same as executing query on top data snapshot for
	/// that version.
	/// </summary>
	public class SnapshotExecutionEngine : ExecutionEngine
	{
		 private readonly int _maxQueryExecutionAttempts;

		 internal SnapshotExecutionEngine( GraphDatabaseQueryService queryService, Config config, LogProvider logProvider, CompilerFactory compilerFactory ) : base( queryService, logProvider, compilerFactory )
		 {
			  this._maxQueryExecutionAttempts = config.Get( GraphDatabaseSettings.snapshot_query_retries );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result executeQuery(String query, org.Neo4Net.values.virtual.MapValue parameters, org.Neo4Net.kernel.impl.query.TransactionalContext context) throws org.Neo4Net.kernel.impl.query.QueryExecutionKernelException
		 public override Result ExecuteQuery( string query, MapValue parameters, TransactionalContext context )
		 {
			  return ExecuteWithRetries( query, parameters, context, base.executeQuery );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.Result profileQuery(String query, org.Neo4Net.values.virtual.MapValue parameters, org.Neo4Net.kernel.impl.query.TransactionalContext context) throws org.Neo4Net.kernel.impl.query.QueryExecutionKernelException
		 public override Result ProfileQuery( string query, MapValue parameters, TransactionalContext context )
		 {
			  return ExecuteWithRetries( query, parameters, context, base.profileQuery );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected <T> org.Neo4Net.graphdb.Result executeWithRetries(String query, T parameters, org.Neo4Net.kernel.impl.query.TransactionalContext context, ParametrizedQueryExecutor<T> executor) throws org.Neo4Net.kernel.impl.query.QueryExecutionKernelException
		 protected internal virtual Result ExecuteWithRetries<T>( string query, T parameters, TransactionalContext context, ParametrizedQueryExecutor<T> executor )
		 {
			  VersionContext versionContext = GetCursorContext( context );
			  EagerResult eagerResult;
			  int attempt = 0;
			  bool dirtySnapshot;
			  do
			  {
					if ( attempt == _maxQueryExecutionAttempts )
					{
						 return ThrowQueryExecutionException( "Unable to get clean data snapshot for query '%s' after %d attempts.", query, attempt );
					}
					attempt++;
					versionContext.InitRead();
					Result result = executor( query, parameters, context );
					eagerResult = new EagerResult( result, versionContext );
					eagerResult.Consume();
					dirtySnapshot = versionContext.Dirty;
					if ( dirtySnapshot && result.QueryStatistics.containsUpdates() )
					{
						 return ThrowQueryExecutionException( "Unable to get clean data snapshot for query '%s' that perform updates.", query, attempt );
					}
			  } while ( dirtySnapshot );
			  return eagerResult;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.graphdb.Result throwQueryExecutionException(String message, Object... parameters) throws org.Neo4Net.kernel.impl.query.QueryExecutionKernelException
		 private Result ThrowQueryExecutionException( string message, params object[] parameters )
		 {
			  throw new QueryExecutionKernelException( new UnstableSnapshotException( message, parameters ) );
		 }

		 private static VersionContext GetCursorContext( TransactionalContext context )
		 {
			  return ( ( KernelStatement ) context.Statement() ).VersionContext;
		 }

		 protected delegate Result ParametrizedQueryExecutor<T>( string query, T parameters, TransactionalContext context );

	}

}