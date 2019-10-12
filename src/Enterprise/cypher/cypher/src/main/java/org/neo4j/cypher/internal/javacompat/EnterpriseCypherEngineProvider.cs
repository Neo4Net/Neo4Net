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
namespace Neo4Net.Cypher.@internal.javacompat
{
	using CypherRuntimeConfiguration = Neo4Net.Cypher.@internal.compatibility.CypherRuntimeConfiguration;
	using CypherPlannerConfiguration = Neo4Net.Cypher.@internal.compiler.v3_5.CypherPlannerConfiguration;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Service = Neo4Net.Helpers.Service;
	using Config = Neo4Net.Kernel.configuration.Config;
	using QueryEngineProvider = Neo4Net.Kernel.impl.query.QueryEngineProvider;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.@internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(QueryEngineProvider.class) public class EnterpriseCypherEngineProvider extends org.neo4j.kernel.impl.query.QueryEngineProvider
	public class EnterpriseCypherEngineProvider : QueryEngineProvider
	{
		 public EnterpriseCypherEngineProvider() : base("enterprise-cypher")
		 {
		 }

		 protected internal override int EnginePriority()
		 {
			  return 1; // Lower means better. The enterprise version will have a lower number
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
			  CommunityCompilerFactory communityCompilerFactory = new CommunityCompilerFactory( queryService, monitors, logProvider, plannerConfig, runtimeConfig );

			  EnterpriseCompilerFactory compilerFactory = new EnterpriseCompilerFactory( communityCompilerFactory, queryService, monitors, logProvider, plannerConfig, runtimeConfig );

			  deps.SatisfyDependency( compilerFactory );
			  return CreateEngine( queryService, config, logProvider, compilerFactory );
		 }

		 private QueryExecutionEngine CreateEngine( GraphDatabaseCypherService queryService, Config config, LogProvider logProvider, EnterpriseCompilerFactory compilerFactory )
		 {
			  return config.Get( GraphDatabaseSettings.snapshot_query ) ? SnapshotEngine( queryService, config, logProvider, compilerFactory ) : StandardEngine( queryService, logProvider, compilerFactory );
		 }

		 private SnapshotExecutionEngine SnapshotEngine( GraphDatabaseCypherService queryService, Config config, LogProvider logProvider, EnterpriseCompilerFactory compilerFactory )
		 {
			  return new SnapshotExecutionEngine( queryService, config, logProvider, compilerFactory );
		 }

		 private ExecutionEngine StandardEngine( GraphDatabaseCypherService queryService, LogProvider logProvider, EnterpriseCompilerFactory compilerFactory )
		 {
			  return new ExecutionEngine( queryService, logProvider, compilerFactory );
		 }
	}

}