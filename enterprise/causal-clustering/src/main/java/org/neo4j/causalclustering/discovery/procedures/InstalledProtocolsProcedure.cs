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

	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using ProtocolStack = Org.Neo4j.causalclustering.protocol.handshake.ProtocolStack;
	using Org.Neo4j.Collection;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using SocketAddress = Org.Neo4j.Helpers.SocketAddress;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using Org.Neo4j.Helpers.Collection;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Org.Neo4j.@internal.Kernel.Api.procs.Neo4jTypes;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using QualifiedName = Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Org.Neo4j.Kernel.api.ResourceTracker;
	using CallableProcedure = Org.Neo4j.Kernel.api.proc.CallableProcedure;
	using Context = Org.Neo4j.Kernel.api.proc.Context;

	public class InstalledProtocolsProcedure : Org.Neo4j.Kernel.api.proc.CallableProcedure_BasicProcedure
	{
		 private static readonly string[] _procedureNamespace = new string[] { "dbms", "cluster" };

		 public const string PROCEDURE_NAME = "protocols";

		 private readonly System.Func<Stream<Pair<AdvertisedSocketAddress, ProtocolStack>>> _clientInstalledProtocols;
		 private readonly System.Func<Stream<Pair<SocketAddress, ProtocolStack>>> _serverInstalledProtocols;

		 public InstalledProtocolsProcedure( System.Func<Stream<Pair<AdvertisedSocketAddress, ProtocolStack>>> clientInstalledProtocols, System.Func<Stream<Pair<SocketAddress, ProtocolStack>>> serverInstalledProtocols ) : base( ProcedureSignature.procedureSignature( new QualifiedName( _procedureNamespace, PROCEDURE_NAME ) ).@out( "orientation", Neo4jTypes.NTString ).@out( "remoteAddress", Neo4jTypes.NTString ).@out( "applicationProtocol", Neo4jTypes.NTString ).@out( "applicationProtocolVersion", Neo4jTypes.NTInteger ).@out( "modifierProtocols", Neo4jTypes.NTString ).description( "Overview of installed protocols" ).build() )
		 {
			  this._clientInstalledProtocols = clientInstalledProtocols;
			  this._serverInstalledProtocols = serverInstalledProtocols;
		 }

		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
			  Stream<object[]> outbound = ToOutputRows( _clientInstalledProtocols, Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client_Fields.OUTBOUND );

			  Stream<object[]> inbound = ToOutputRows( _serverInstalledProtocols, Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server_Fields.INBOUND );

			  return Iterators.asRawIterator( Stream.concat( outbound, inbound ) );
		 }

		 private Stream<object[]> ToOutputRows<T>( System.Func<Stream<Pair<T, ProtocolStack>>> installedProtocols, string orientation ) where T : Org.Neo4j.Helpers.SocketAddress
		 {
			  IComparer<Pair<T, ProtocolStack>> connectionInfoComparator = System.Collections.IComparer.comparing( ( Pair<T, ProtocolStack> entry ) => entry.First().Hostname ).thenComparing(entry => entry.First().Port);

			  return installedProtocols().sorted(connectionInfoComparator).map(entry => BuildRow(entry, orientation));
		 }

		 private object[] BuildRow<T>( Pair<T, ProtocolStack> connectionInfo, string orientation ) where T : Org.Neo4j.Helpers.SocketAddress
		 {
			  T socketAddress = connectionInfo.First();
			  ProtocolStack protocolStack = connectionInfo.Other();
			  return new object[] { orientation, socketAddress.ToString(), protocolStack.ApplicationProtocol().category(), (long) protocolStack.ApplicationProtocol().implementation(), ModifierString(protocolStack) };
		 }

		 private string ModifierString( ProtocolStack protocolStack )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return protocolStack.ModifierProtocols().Select(Org.Neo4j.causalclustering.protocol.Protocol_ModifierProtocol.implementation).collect(Collectors.joining(",", "[", "]"));
		 }
	}

}