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
	using CypherRuntimeConfiguration = Org.Neo4j.Cypher.@internal.compatibility.CypherRuntimeConfiguration;
	using CypherPlannerConfiguration = Org.Neo4j.Cypher.@internal.compiler.v3_5.CypherPlannerConfiguration;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Service = Org.Neo4j.Helpers.Service;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using QueryEngineProvider = Org.Neo4j.Kernel.impl.query.QueryEngineProvider;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(QueryEngineProvider.class) public class CommunityCypherEngineProvider extends org.neo4j.kernel.impl.query.QueryEngineProvider
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