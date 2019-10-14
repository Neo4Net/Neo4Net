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
namespace Neo4Net.causalclustering.scenarios
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using InstalledProtocolsProcedure = Neo4Net.causalclustering.discovery.procedures.InstalledProtocolsProcedure;
	using InstalledProtocolsProcedureTest = Neo4Net.causalclustering.discovery.procedures.InstalledProtocolsProcedureTest;
	using Neo4Net.Collections;
	using Kernel = Neo4Net.Internal.Kernel.Api.Kernel;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using ProcedureCallContext = Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.COMPRESSION_SNAPPY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client_Fields.OUTBOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server_Fields.INBOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.procs.ProcedureSignature.procedureName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	/// <seealso cref= InstalledProtocolsProcedureTest </seealso>
	public class InstalledProtocolsProcedureIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.leader_election_timeout, "2s").withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.compression_implementations, "snappy").withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public ClusterRule ClusterRule = new ClusterRule().withSharedCoreParam(CausalClusteringSettings.leader_election_timeout, "2s").withSharedCoreParam(CausalClusteringSettings.compression_implementations, "snappy").withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private CoreClusterMember _leader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartUp()
		 {
			  _cluster = ClusterRule.startCluster();
			  _leader = _cluster.awaitLeader();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeOutboundInstalledProtocolsOnLeader() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeOutboundInstalledProtocolsOnLeader()
		 {
			  string modifiers = ( new StringJoiner( ",", "[", "]" ) ).add( COMPRESSION_SNAPPY.implementation() ).ToString();

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  ProtocolInfo[] expectedProtocolInfos = _cluster.coreMembers().Where(member => !member.Equals(_leader)).Select(member => new ProtocolInfo(OUTBOUND, Localhost(member.raftListenAddress()), RAFT.canonicalName(), 2, modifiers)).ToArray(ProtocolInfo[]::new);

			  assertEventually( "should see outbound installed protocols on core " + _leader.serverId(), () => InstalledProtocols(_leader.database(), OUTBOUND), hasItems(expectedProtocolInfos), 60, SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeInboundInstalledProtocolsOnLeader() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeInboundInstalledProtocolsOnLeader()
		 {
			  assertEventually( "should see inbound installed protocols on core " + _leader.serverId(), () => InstalledProtocols(_leader.database(), INBOUND), hasSize(greaterThanOrEqualTo(_cluster.coreMembers().Count - 1)), 60, SECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<ProtocolInfo> installedProtocols(org.neo4j.kernel.impl.factory.GraphDatabaseFacade db, String wantedOrientation) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private IList<ProtocolInfo> InstalledProtocols( GraphDatabaseFacade db, string wantedOrientation )
		 {
			  IList<ProtocolInfo> infos = new LinkedList<ProtocolInfo>();
			  Kernel kernel = Db.DependencyResolver.resolveDependency( typeof( Kernel ) );
			  using ( Transaction tx = kernel.BeginTransaction( Neo4Net.Internal.Kernel.Api.Transaction_Type.Implicit, AnonymousContext.read() ) )
			  {
					RawIterator<object[], ProcedureException> itr = tx.Procedures().procedureCallRead(procedureName("dbms", "cluster", InstalledProtocolsProcedure.PROCEDURE_NAME), null, ProcedureCallContext.EMPTY);

					while ( itr.HasNext() )
					{
						 object[] row = itr.Next();
						 string orientation = ( string ) row[0];
						 string address = Localhost( ( string ) row[1] );
						 string protocol = ( string ) row[2];
						 long version = ( long ) row[3];
						 string modifiers = ( string ) row[4];
						 if ( orientation.Equals( wantedOrientation ) )
						 {
							  infos.Add( new ProtocolInfo( orientation, address, protocol, version, modifiers ) );
						 }
					}
					return infos;
			  }
		 }

		 private string Localhost( string uri )
		 {
			  return uri.Replace( "127.0.0.1", "localhost" );
		 }

		 private class ProtocolInfo
		 {
			  internal readonly string Orientation;
			  internal readonly string Address;
			  internal readonly string Protocol;
			  internal readonly long Version;
			  internal readonly string Modifiers;

			  internal ProtocolInfo( string orientation, string address, string protocol, long version, string modifiers )
			  {
					this.Orientation = orientation;
					this.Address = address;
					this.Protocol = protocol;
					this.Version = version;
					this.Modifiers = modifiers;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					ProtocolInfo that = ( ProtocolInfo ) o;
					return Version == that.Version && Objects.Equals( Orientation, that.Orientation ) && Objects.Equals( Address, that.Address ) && Objects.Equals( Protocol, that.Protocol ) && Objects.Equals( Modifiers, that.Modifiers );
			  }

			  public override int GetHashCode()
			  {

					return Objects.hash( Orientation, Address, Protocol, Version, Modifiers );
			  }

			  public override string ToString()
			  {
					return "ProtocolInfo{" + "orientation='" + Orientation + '\'' + ", address='" + Address + '\'' + ", protocol='" + Protocol + '\'' + ", version=" + Version + ", modifiers='" + Modifiers + '\'' + '}';
			  }
		 }
	}

}