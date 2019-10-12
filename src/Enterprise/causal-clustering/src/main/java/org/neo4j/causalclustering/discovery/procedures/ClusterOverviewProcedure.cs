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
namespace Neo4Net.causalclustering.discovery.procedures
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.Collection;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using QualifiedName = Neo4Net.@internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asRawIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	/// <summary>
	/// Overview procedure with added support for server groups.
	/// </summary>
	public class ClusterOverviewProcedure : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
	{
		 private static readonly string[] _procedureNamespace = new string[] { "dbms", "cluster" };
		 public const string PROCEDURE_NAME = "overview";
		 private readonly TopologyService _topologyService;
		 private readonly Log _log;

		 public ClusterOverviewProcedure( TopologyService topologyService, LogProvider logProvider ) : base( procedureSignature( new QualifiedName( _procedureNamespace, PROCEDURE_NAME ) ).@out( "id", Neo4jTypes.NTString ).@out( "addresses", Neo4jTypes.NTList( Neo4jTypes.NTString ) ).@out( "role", Neo4jTypes.NTString ).@out( "groups", Neo4jTypes.NTList( Neo4jTypes.NTString ) ).@out( "database", Neo4jTypes.NTString ).description( "Overview of all currently accessible cluster members and their roles." ).build() )
		 {
			  this._topologyService = topologyService;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
			  IDictionary<MemberId, RoleInfo> roleMap = _topologyService.allCoreRoles();
			  IList<ReadWriteEndPoint> endpoints = new List<ReadWriteEndPoint>();

			  CoreTopology coreTopology = _topologyService.allCoreServers();
			  ISet<MemberId> coreMembers = coreTopology.Members().Keys;

			  foreach ( MemberId memberId in coreMembers )
			  {
					Optional<CoreServerInfo> coreServerInfo = coreTopology.find( memberId );
					if ( coreServerInfo.Present )
					{
						 CoreServerInfo info = coreServerInfo.get();
						 RoleInfo role = roleMap.getOrDefault( memberId, RoleInfo.UNKNOWN );
						 endpoints.Add( new ReadWriteEndPoint( info.Connectors(), role, memberId.Uuid, new IList<string> { info.Groups() }, info.DatabaseName ) );
					}
					else
					{
						 _log.debug( "No Address found for " + memberId );
					}
			  }

			  foreach ( KeyValuePair<MemberId, ReadReplicaInfo> readReplica in _topologyService.allReadReplicas().members().SetOfKeyValuePairs() )
			  {
					ReadReplicaInfo readReplicaInfo = readReplica.Value;
					endpoints.Add( new ReadWriteEndPoint( readReplicaInfo.Connectors(), RoleInfo.READ_REPLICA, readReplica.Key.Uuid, new IList<string> { readReplicaInfo.Groups() }, readReplicaInfo.DatabaseName ) );
			  }

			  endpoints.sort( comparing( o => o.addresses().ToString() ) );

			  return map( endpoint => new object[] { endpoint.memberId().ToString(), endpoint.addresses().uriList().Select(URI.toString).ToList(), endpoint.role().name(), endpoint.groups(), endpoint.dbName() }, asRawIterator(endpoints.GetEnumerator()) );
		 }

		 internal class ReadWriteEndPoint
		 {
			  internal readonly ClientConnectorAddresses ClientConnectorAddresses;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RoleInfo RoleConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly System.Guid MemberIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<string> GroupsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string DbNameConflict;

			  public virtual ClientConnectorAddresses Addresses()
			  {
					return ClientConnectorAddresses;
			  }

			  public virtual RoleInfo Role()
			  {
					return RoleConflict;
			  }

			  internal virtual System.Guid MemberId()
			  {
					return MemberIdConflict;
			  }

			  internal virtual IList<string> Groups()
			  {
					return GroupsConflict;
			  }

			  internal virtual string DbName()
			  {
					return DbNameConflict;
			  }

			  internal ReadWriteEndPoint( ClientConnectorAddresses clientConnectorAddresses, RoleInfo role, System.Guid memberId, IList<string> groups, string dbName )
			  {
					this.ClientConnectorAddresses = clientConnectorAddresses;
					this.RoleConflict = role;
					this.MemberIdConflict = memberId;
					this.GroupsConflict = groups;
					this.DbNameConflict = dbName;
			  }
		 }
	}

}