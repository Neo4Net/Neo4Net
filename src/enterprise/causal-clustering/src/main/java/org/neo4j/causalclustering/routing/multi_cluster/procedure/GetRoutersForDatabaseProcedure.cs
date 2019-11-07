﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.routing.multi_cluster.procedure
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using Neo4Net.Collections;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using ProcedureSignature = Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature;
	using ResourceTracker = Neo4Net.Kernel.Api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.Api.Procs.CallableProcedure;
	using Context = Neo4Net.Kernel.Api.Procs.Context;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.routing.multi_cluster.procedure.ProcedureNames.GET_ROUTERS_FOR_DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.routing.multi_cluster.procedure.ParameterNames.DATABASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.routing.multi_cluster.procedure.ParameterNames.ROUTERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.routing.multi_cluster.procedure.ParameterNames.TTL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.routing.Util.extractBoltAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureSignature;

	public class GetRoutersForDatabaseProcedure : CallableProcedure
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_procedureSignature = _procedureSignature( GET_ROUTERS_FOR_DATABASE.fullyQualifiedProcedureName() ).@in(DATABASE.parameterName(), Neo4NetTypes.NTString).@out(TTL.parameterName(), Neo4NetTypes.NTInteger).@out(ROUTERS.parameterName(), Neo4NetTypes.NTList(Neo4NetTypes.NTMap)).description(DESCRIPTION).build();
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
//ORIGINAL LINE: public Neo4Net.collection.RawIterator<Object[],Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> apply(Neo4Net.kernel.api.proc.Context ctx, Object[] input, Neo4Net.kernel.api.ResourceTracker resourceTracker) throws Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
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