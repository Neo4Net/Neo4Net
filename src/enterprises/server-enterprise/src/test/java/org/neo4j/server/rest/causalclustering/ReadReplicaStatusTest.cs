using System;
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
namespace Neo4Net.Server.rest.causalclustering
{
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;
	using TypeReference = org.codehaus.jackson.type.TypeReference;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using CommandIndexTracker = Neo4Net.causalclustering.core.state.machines.id.CommandIndexTracker;
	using RoleInfo = Neo4Net.causalclustering.discovery.RoleInfo;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;
	using JsonFormat = Neo4Net.Server.rest.repr.formats.JsonFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ReadReplicaStatusTest
	{
		 private CausalClusteringStatus _status;

		 private FakeTopologyService _topologyService;
		 private Dependencies _dependencyResolver = new Dependencies();
		 private DatabaseHealth _databaseHealth;
		 private CommandIndexTracker _commandIndexTracker;

		 private readonly MemberId _myself = new MemberId( System.Guid.randomUUID() );
		 private readonly LogProvider _logProvider = NullLogProvider.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  OutputFormat output = new OutputFormat( new JsonFormat(), new URI("http://base.local:1234/"), null );
			  ReadReplicaGraphDatabase db = mock( typeof( ReadReplicaGraphDatabase ) );
			  _topologyService = new FakeTopologyService( RandomMembers( 3 ), RandomMembers( 2 ), _myself, RoleInfo.READ_REPLICA );
			  _dependencyResolver.satisfyDependencies( _topologyService );

			  when( Db.DependencyResolver ).thenReturn( _dependencyResolver );
			  _databaseHealth = _dependencyResolver.satisfyDependency( new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), _logProvider.getLog( typeof( DatabaseHealth ) ) ) );
			  _commandIndexTracker = _dependencyResolver.satisfyDependency( new CommandIndexTracker() );

			  _status = CausalClusteringStatusFactory.Build( output, db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAnswers()
		 public virtual void TestAnswers()
		 {
			  // when
			  Response available = _status.available();
			  Response @readonly = _status.@readonly();
			  Response writable = _status.writable();

			  // then
			  assertEquals( OK.StatusCode, available.Status );
			  assertEquals( "true", available.Entity );

			  assertEquals( OK.StatusCode, @readonly.Status );
			  assertEquals( "true", @readonly.Entity );

			  assertEquals( NOT_FOUND.StatusCode, writable.Status );
			  assertEquals( "false", writable.Entity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void statusIncludesAppliedRaftLogIndex() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StatusIncludesAppliedRaftLogIndex()
		 {
			  // given
			  _commandIndexTracker.AppliedCommandIndex = 321;

			  // when
			  Response description = _status.description();

			  // then
			  IDictionary<string, object> responseJson = ResponseAsMap( description );
			  assertEquals( 321, responseJson["lastAppliedRaftIndex"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void responseIncludesAllCoresAndReplicas() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ResponseIncludesAllCoresAndReplicas()
		 {
			  Response description = _status.description();

			  assertEquals( Response.Status.OK.StatusCode, description.Status );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  List<string> expectedVotingMembers = _topologyService.allCoreServers().members().Keys.Select(memberId => memberId.Uuid.ToString()).collect(Collectors.toCollection(List<object>::new));
			  IDictionary<string, object> responseJson = ResponseAsMap( description );
			  IList<string> actualVotingMembers = ( IList<string> ) responseJson["votingMembers"];
			  expectedVotingMembers.Sort();
			  actualVotingMembers.Sort();
			  assertEquals( expectedVotingMembers, actualVotingMembers );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dbHealthIsIncludedInResponse() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DbHealthIsIncludedInResponse()
		 {
			  Response description = _status.description();
			  assertEquals( true, ResponseAsMap( description )["healthy"] );

			  _databaseHealth.panic( new Exception() );
			  description = _status.description();
			  assertEquals( false, ResponseAsMap( description )["healthy"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void includesMemberId() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncludesMemberId()
		 {
			  Response description = _status.description();
			  assertEquals( _myself.Uuid.ToString(), ResponseAsMap(description)["memberId"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderIsOptional() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderIsOptional()
		 {
			  Response description = _status.description();
			  assertFalse( ResponseAsMap( description ).ContainsKey( "leader" ) );

			  MemberId selectedLead = _topologyService.allCoreServers().members().Keys.First().orElseThrow(() => new System.InvalidOperationException("No cores in topology"));
			  _topologyService.replaceWithRole( selectedLead, RoleInfo.LEADER );
			  description = _status.description();
			  assertEquals( selectedLead.Uuid.ToString(), ResponseAsMap(description)["leader"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isNotCore() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void isNotCore()
		 {
			  Response description = _status.description();
			  assertTrue( ResponseAsMap( description ).ContainsKey( "core" ) );
			  assertEquals( false, ResponseAsMap( _status.description() )["core"] );
		 }

		 internal static ICollection<MemberId> RandomMembers( int size )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return IntStream.range( 0, size ).mapToObj( i => System.Guid.randomUUID() ).map(MemberId::new).collect(Collectors.toList());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.util.Map<String,Object> responseAsMap(javax.ws.rs.core.Response response) throws java.io.IOException
		 internal static IDictionary<string, object> ResponseAsMap( Response response )
		 {
			  ObjectMapper objectMapper = new ObjectMapper();
			  IDictionary<string, object> responseJson = objectMapper.readValue( response.Entity.ToString(), new TypeReferenceAnonymousInnerClass() );
			  return responseJson;
		 }

		 private class TypeReferenceAnonymousInnerClass : TypeReference<IDictionary<string, object>>
		 {
		 }
	}

}