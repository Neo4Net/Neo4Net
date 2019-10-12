/*
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

	using CompilationTracer = Org.Neo4j.Cypher.@internal.tracing.CompilationTracer;
	using TimingCompilationTracer = Org.Neo4j.Cypher.@internal.tracing.TimingCompilationTracer;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Result = Org.Neo4j.Graphdb.Result;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using QueryExecution = Org.Neo4j.Kernel.impl.query.QueryExecution;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using QueryExecutionKernelException = Org.Neo4j.Kernel.impl.query.QueryExecutionKernelException;
	using ResultBuffer = Org.Neo4j.Kernel.impl.query.ResultBuffer;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

	/// <summary>
	/// To run a Cypher query, use this class.
	/// 
	/// This class construct and initialize both the cypher compiler and the cypher runtime, which is a very expensive
	/// operation so please make sure this will be constructed only once and properly reused.
	/// 
	/// </summary>
	public class ExecutionEngine : QueryExecutionEngine
	{
		 private Org.Neo4j.Cypher.@internal.ExecutionEngine _inner;

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
			  _inner = new Org.Neo4j.Cypher.@internal.ExecutionEngine( queryService, monitors, tracer, cacheTracer, cypherConfiguration, compilerFactory, logProvider, Clock.systemUTC() );
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