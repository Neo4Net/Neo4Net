﻿using System.Collections;
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
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ProcedureNames.GET_ROUTERS_FOR_ALL_DATABASES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ParameterNames.ROUTERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ParameterNames.TTL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.Util.extractBoltAddress;

	public class GetRoutersForAllDatabasesProcedure : CallableProcedure
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_procedureSignature = _procedureSignature( GET_ROUTERS_FOR_ALL_DATABASES.fullyQualifiedProcedureName() ).@out(TTL.parameterName(), Neo4jTypes.NTInteger).@out(ROUTERS.parameterName(), Neo4jTypes.NTList(Neo4jTypes.NTMap)).description(DESCRIPTION).build();
		}


		 private const string DESCRIPTION = "Returns router capable endpoints for each database name in a multi-cluster.";

		 private ProcedureSignature _procedureSignature;

		 private readonly TopologyService _topologyService;
		 private readonly long _timeToLiveMillis;

		 public GetRoutersForAllDatabasesProcedure( TopologyService topologyService, Config config )
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
			  IDictionary<string, IList<Endpoint>> routersPerDb = RouteEndpoints();
			  MultiClusterRoutingResult result = new MultiClusterRoutingResult( routersPerDb, _timeToLiveMillis );
			  return RawIterator.of<object[], ProcedureException>( MultiClusterRoutingResultFormat.Build( result ) );
		 }

		 private IDictionary<string, IList<Endpoint>> RouteEndpoints()
		 {
			  CoreTopology core = _topologyService.allCoreServers();
			  Stream<CoreServerInfo> allCoreMemberInfo = _topologyService.allCoreServers().members().Values.stream();

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IDictionary<string, IList<CoreServerInfo>> coresByDb = allCoreMemberInfo.collect( Collectors.groupingBy( CoreServerInfo::getDatabaseName ) );

			  System.Func<KeyValuePair<string, IList<CoreServerInfo>>, IList<Endpoint>> extractQualifiedBoltAddresses = entry =>
			  {
				IList<CoreServerInfo> cores = entry.Value;
				return cores.Select( extractBoltAddress() ).Select(Endpoint.route).ToList();
			  };

			  return coresByDb.SetOfKeyValuePairs().ToDictionary(DictionaryEntry.getKey, extractQualifiedBoltAddresses);
		 }
	}

}