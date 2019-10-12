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
namespace Org.Neo4j.causalclustering.discovery.procedures
{
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.Collection;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.TestTopology.addressesForCore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.TestTopology.addressesForReadReplica;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class ClusterOverviewProcedureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideOverviewOfCoreServersAndReadReplicas() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideOverviewOfCoreServersAndReadReplicas()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService topologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  MemberId theLeader = new MemberId( System.Guid.randomUUID() );
			  MemberId follower1 = new MemberId( System.Guid.randomUUID() );
			  MemberId follower2 = new MemberId( System.Guid.randomUUID() );

			  coreMembers[theLeader] = addressesForCore( 0, false );
			  coreMembers[follower1] = addressesForCore( 1, false );
			  coreMembers[follower2] = addressesForCore( 2, false );

			  IDictionary<MemberId, ReadReplicaInfo> replicaMembers = new Dictionary<MemberId, ReadReplicaInfo>();
			  MemberId replica4 = new MemberId( System.Guid.randomUUID() );
			  MemberId replica5 = new MemberId( System.Guid.randomUUID() );

			  replicaMembers[replica4] = addressesForReadReplica( 4 );
			  replicaMembers[replica5] = addressesForReadReplica( 5 );

			  IDictionary<MemberId, RoleInfo> roleMap = new Dictionary<MemberId, RoleInfo>();
			  roleMap[theLeader] = RoleInfo.LEADER;
			  roleMap[follower1] = RoleInfo.FOLLOWER;
			  roleMap[follower2] = RoleInfo.FOLLOWER;

			  when( topologyService.AllCoreServers() ).thenReturn(new CoreTopology(null, false, coreMembers));
			  when( topologyService.AllReadReplicas() ).thenReturn(new ReadReplicaTopology(replicaMembers));
			  when( topologyService.AllCoreRoles() ).thenReturn(roleMap);

			  ClusterOverviewProcedure procedure = new ClusterOverviewProcedure( topologyService, NullLogProvider.Instance );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> members = procedure.apply(null, new Object[0], null);
			  RawIterator<object[], ProcedureException> members = procedure.Apply( null, new object[0], null );

			  assertThat( members.Next(), new IsRecord(this, theLeader.Uuid, 5000, RoleInfo.LEADER, asSet("core", "core0")) );
			  assertThat( members.Next(), new IsRecord(this, follower1.Uuid, 5001, RoleInfo.FOLLOWER, asSet("core", "core1")) );
			  assertThat( members.Next(), new IsRecord(this, follower2.Uuid, 5002, RoleInfo.FOLLOWER, asSet("core", "core2")) );

			  assertThat( members.Next(), new IsRecord(this, replica4.Uuid, 6004, RoleInfo.READ_REPLICA, asSet("replica", "replica4")) );
			  assertThat( members.Next(), new IsRecord(this, replica5.Uuid, 6005, RoleInfo.READ_REPLICA, asSet("replica", "replica5")) );

			  assertFalse( members.HasNext() );
		 }

		 internal class IsRecord : TypeSafeMatcher<object[]>
		 {
			 private readonly ClusterOverviewProcedureTest _outerInstance;

			  internal readonly System.Guid MemberId;
			  internal readonly int BoltPort;
			  internal readonly RoleInfo Role;
			  internal readonly ISet<string> Groups;
			  internal readonly string DbName;

			  internal IsRecord( ClusterOverviewProcedureTest outerInstance, System.Guid memberId, int boltPort, RoleInfo role, ISet<string> groups, string dbName )
			  {
				  this._outerInstance = outerInstance;
					this.MemberId = memberId;
					this.BoltPort = boltPort;
					this.Role = role;
					this.Groups = groups;
					this.DbName = dbName;
			  }

			  internal IsRecord( ClusterOverviewProcedureTest outerInstance, System.Guid memberId, int boltPort, RoleInfo role, ISet<string> groups )
			  {
				  this._outerInstance = outerInstance;
					this.MemberId = memberId;
					this.BoltPort = boltPort;
					this.Role = role;
					this.Groups = groups;
					this.DbName = CausalClusteringSettings.database.DefaultValue;
			  }

			  protected internal override bool MatchesSafely( object[] record )
			  {
					if ( record.Length != 5 )
					{
						 return false;
					}

					if ( !MemberId.ToString().Equals(record[0]) )
					{
						 return false;
					}

					IList<string> boltAddresses = Collections.singletonList( "bolt://localhost:" + BoltPort );

//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if (!boltAddresses.equals(record[1]))
					if ( !boltAddresses.SequenceEqual( record[1] ) )
					{
						 return false;
					}

					if ( !Role.name().Equals(record[2]) )
					{
						 return false;
					}

					ISet<string> recordGroups = Iterables.asSet( ( IList<string> ) record[3] );
					if ( !Groups.SetEquals( recordGroups ) )
					{
						 return false;
					}

					return DbName.Equals( record[4] );
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( "memberId=" + MemberId + ", boltPort=" + BoltPort + ", role=" + Role + ", groups=" + Groups + ", database=" + DbName + '}' );
			  }
		 }
	}

}