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
	using CypherRuntimeConfiguration = Neo4Net.Cypher.Internal.compatibility.CypherRuntimeConfiguration;
	using CypherPlannerConfiguration = Neo4Net.Cypher.Internal.compiler.v3_5.CypherPlannerConfiguration;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using QueryEngineProvider = Neo4Net.Kernel.impl.query.QueryEngineProvider;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(QueryEngineProvider.class) public class CommunityCypherEngineProvider extends Neo4Net.kernel.impl.query.QueryEngineProvider
	public class CommunityCypherEngineProvider : QueryEngineProvider
	{
		 public CommunityCypherEngineProvider() : base("cypher")
		 {
		 }

		 protected internal override int EnginePriority()
		 {
			  return 42; // Lower means better. The enterprise version will have a lower number
		 }

		 protected internal override QueryExecutionEngine CreateEngine( Dependencies deps, GraphDatabaseAPI graphAPI )
		 {
			  GraphDatabaseCypherService queryService = new GraphDatabaseCypherService( graphAPI );
			  deps.SatisfyDependency( queryService );

			  DependencyResolver resolver = graphAPI.DependencyResolver;
			  LogService logService = resolver.ResolveDependency( typeof( LogService ) );
			  Monitors monitors = resolver.ResolveDependency( typeof( Monitors ) );
			  Config config = resolver.ResolveDependency( typeof( Config ) );
			  CypherConfiguration cypherConfig = CypherConfiguration.fromConfig( config );
			  CypherPlannerConfiguration plannerConfig = cypherConfig.toCypherPlannerConfiguration( config );
			  CypherRuntimeConfiguration runtimeConfig = cypherConfig.toCypherRuntimeConfiguration();
			  LogProvider logProvider = logService.InternalLogProvider;
			  CommunityCompilerFactory compilerFactory = new CommunityCompilerFactory( queryService, monitors, logProvider, plannerConfig, runtimeConfig );
			  deps.SatisfyDependencies( compilerFactory );
			  return CreateEngine( queryService, config, logProvider, compilerFactory );
		 }

		 private QueryExecutionEngine CreateEngine( GraphDatabaseCypherService queryService, Config config, LogProvider logProvider, CommunityCompilerFactory compilerFactory )
		 {
			  return config.Get( GraphDatabaseSettings.snapshot_query ) ? SnapshotEngine( queryService, config, logProvider, compilerFactory ) : StandardEngine( queryService, logProvider, compilerFactory );
		 }

		 private SnapshotExecutionEngine SnapshotEngine( GraphDatabaseCypherService queryService, Config config, LogProvider logProvider, CommunityCompilerFactory compilerFactory )
		 {
			  return new SnapshotExecutionEngine( queryService, config, logProvider, compilerFactory );
		 }

		 private ExecutionEngine StandardEngine( GraphDatabaseCypherService queryService, LogProvider logProvider, CommunityCompilerFactory compilerFactory )
		 {
			  return new ExecutionEngine( queryService, logProvider, compilerFactory );
		 }
	}

}