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
namespace Org.Neo4j.causalclustering.routing.multi_cluster.procedure
{

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using CoreServerInfo = Org.Neo4j.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Org.Neo4j.causalclustering.discovery.CoreTopology;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using Org.Neo4j.Collection;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Org.Neo4j.@internal.Kernel.Api.procs.Neo4jTypes;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using ResourceTracker = Org.Neo4j.Kernel.api.ResourceTracker;
	using CallableProcedure = Org.Neo4j.Kernel.api.proc.CallableProcedure;
	using Context = Org.Neo4j.Kernel.api.proc.Context;
	using Config = Org.Neo4j.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ProcedureNames.GET_ROUTERS_FOR_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ParameterNames.DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ParameterNames.ROUTERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ParameterNames.TTL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.Util.extractBoltAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	public class GetRoutersForDatabaseProcedure : CallableProcedure
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_procedureSignature = _procedureSignature( GET_ROUTERS_FOR_DATABASE.fullyQualifiedProcedureName() ).@in(DATABASE.parameterName(), Neo4jTypes.NTString).@out(TTL.parameterName(), Neo4jTypes.NTInteger).@out(ROUTERS.parameterName(), Neo4jTypes.NTList(Neo4jTypes.NTMap)).description(DESCRIPTION).build();
		}

		 private const string DESCRIPTION = "Returns router capable endpoints for a specific database in a multi-cluster.";

		 private ProcedureSignature _procedureSignature;

		 private readonly TopologyService _topologyService;
		 private readonly long _timeToLiveMillis;

		 public GetRoutersForDatabaseProcedure( TopologyService topologyService, Config config )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._topologyService = topologyService;
			  this._timeToLiveMillis = config.Get( CausalClusteringSettings.cluster_routing_ttl ).toMillis();
		 }

		 public override ProcedureSignature Signature()
		 {
			  return _procedureSignature;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(org.neo4j.kernel.api.proc.Context ctx, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") String dbName = (String) input[0];
			  string dbName = ( string ) input[0];
			  IList<Endpoint> routers = RouteEndpoints( dbName );

			  Dictionary<string, IList<Endpoint>> routerMap = new Dictionary<string, IList<Endpoint>>();
			  routerMap[dbName] = routers;

			  MultiClusterRoutingResult result = new MultiClusterRoutingResult( routerMap, _timeToLiveMillis );
			  return RawIterator.of<object[], ProcedureException>( MultiClusterRoutingResultFormat.Build( result ) );
		 }

		 private IList<Endpoint> RouteEndpoints( string dbName )
		 {
			  CoreTopology filtered = _topologyService.allCoreServers().filterTopologyByDb(dbName);
			  Stream<CoreServerInfo> filteredCoreMemberInfo = filtered.Members().Values.stream();

			  return filteredCoreMemberInfo.map( extractBoltAddress() ).map(Endpoint.route).collect(Collectors.toList());
		 }
	}

}