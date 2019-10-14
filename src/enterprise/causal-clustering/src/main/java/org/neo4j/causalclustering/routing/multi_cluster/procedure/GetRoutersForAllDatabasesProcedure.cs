using System.Collections;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.routing.multi_cluster.procedure
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using Neo4Net.Collections;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Neo4Net.Internal.Kernel.Api.procs.Neo4jTypes;
	using ProcedureSignature = Neo4Net.Internal.Kernel.Api.procs.ProcedureSignature;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ProcedureNames.GET_ROUTERS_FOR_ALL_DATABASES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ParameterNames.ROUTERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.routing.multi_cluster.procedure.ParameterNames.TTL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.procs.ProcedureSignature.procedureSignature;
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