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

	using CompilationTracer = Neo4Net.Cypher.Internal.Tracing.CompilationTracer;
	using TimingCompilationTracer = Neo4Net.Cypher.Internal.Tracing.TimingCompilationTracer;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using Result = Neo4Net.Graphdb.Result;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using QueryExecution = Neo4Net.Kernel.impl.query.QueryExecution;
	using Config = Neo4Net.Kernel.configuration.Config;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Neo4Net.Kernel.impl.query.QueryExecutionKernelException;
	using ResultBuffer = Neo4Net.Kernel.impl.query.ResultBuffer;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	/// <summary>
	/// To run a Cypher query, use this class.
	/// 
	/// This class construct and initialize both the cypher compiler and the cypher runtime, which is a very expensive
	/// operation so please make sure this will be constructed only once and properly reused.
	/// 
	/// </summary>
	public class ExecutionEngine : QueryExecutionEngine
	{
		 private Neo4Net.Cypher.Internal.ExecutionEngine _inner;

		 /// <summary>
		 /// Creates an execution engine around the give graph database </summary>
		 /// <param name="queryService"> The database to wrap </param>
		 /// <param name="logProvider"> A <seealso cref="LogProvider"/> for cypher-statements </param>
		 public ExecutionEngine( GraphDatabaseQueryService queryService, LogProvider logProvider, CompilerFactory compilerFactory )
		 {
			  DependencyResolver resolver = queryService.DependencyResolver;
			  Monitors monitors = resolver.ResolveDependency( typeof( Monitors ) );
			  CacheTracer cacheTracer = new MonitoringCacheTracer( monitors.NewMonitor( typeof( StringCacheMonitor ) ) );
			  Config config = resolver.ResolveDependency( typeof( Config ) );
			  CypherConfiguration cypherConfiguration = CypherConfiguration.fromConfig( config );
			  CompilationTracer tracer = new TimingCompilationTracer( monitors.NewMonitor( typeof( TimingCompilationTracer.EventListener ) ) );
			  _inner = new Neo4Net.Cypher.Internal.ExecutionEngine( queryService, monitors, tracer, cacheTracer, cypherConfiguration, compilerFactory, logProvider, Clock.systemUTC() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.Result executeQuery(String query, org.neo4j.values.virtual.MapValue parameters, org.neo4j.kernel.impl.query.TransactionalContext context) throws org.neo4j.kernel.impl.query.QueryExecutionKernelException
		 public override Result ExecuteQuery( string query, MapValue parameters, TransactionalContext context )
		 {
			  try
			  {
					return _inner.execute( query, parameters, context, false );
			  }
			  catch ( CypherException e )
			  {
					throw new QueryExecutionKernelException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.Result profileQuery(String query, org.neo4j.values.virtual.MapValue parameters, org.neo4j.kernel.impl.query.TransactionalContext context) throws org.neo4j.kernel.impl.query.QueryExecutionKernelException
		 public override Result ProfileQuery( string query, MapValue parameters, TransactionalContext context )
		 {
			  try
			  {
					return _inner.execute( query, parameters, context, true );
			  }
			  catch ( CypherException e )
			  {
					throw new QueryExecutionKernelException( e );
			  }
		 }

		 public override bool IsPeriodicCommit( string query )
		 {
			  return _inner.isPeriodicCommit( query );
		 }

		 public override long ClearQueryCaches()
		 {
			  return _inner.clearQueryCaches();
		 }
	}

}