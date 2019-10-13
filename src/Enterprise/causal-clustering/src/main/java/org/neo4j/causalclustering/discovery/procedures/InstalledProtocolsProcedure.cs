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
namespace Neo4Net.causalclustering.discovery.procedures
{

	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ProtocolStack = Neo4Net.causalclustering.protocol.handshake.ProtocolStack;
	using Neo4Net.Collections;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using SocketAddress = Neo4Net.Helpers.SocketAddress;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;
	using ProcedureSignature = Neo4Net.@internal.Kernel.Api.procs.ProcedureSignature;
	using QualifiedName = Neo4Net.@internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;

	public class InstalledProtocolsProcedure : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
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
			  Stream<object[]> outbound = ToOutputRows( _clientInstalledProtocols, Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client_Fields.OUTBOUND );

			  Stream<object[]> inbound = ToOutputRows( _serverInstalledProtocols, Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server_Fields.INBOUND );

			  return Iterators.asRawIterator( Stream.concat( outbound, inbound ) );
		 }

		 private Stream<object[]> ToOutputRows<T>( System.Func<Stream<Pair<T, ProtocolStack>>> installedProtocols, string orientation ) where T : Neo4Net.Helpers.SocketAddress
		 {
			  IComparer<Pair<T, ProtocolStack>> connectionInfoComparator = System.Collections.IComparer.comparing( ( Pair<T, ProtocolStack> entry ) => entry.First().Hostname ).thenComparing(entry => entry.First().Port);

			  return installedProtocols().sorted(connectionInfoComparator).map(entry => BuildRow(entry, orientation));
		 }

		 private object[] BuildRow<T>( Pair<T, ProtocolStack> connectionInfo, string orientation ) where T : Neo4Net.Helpers.SocketAddress
		 {
			  T socketAddress = connectionInfo.First();
			  ProtocolStack protocolStack = connectionInfo.Other();
			  return new object[] { orientation, socketAddress.ToString(), protocolStack.ApplicationProtocol().category(), (long) protocolStack.ApplicationProtocol().implementation(), ModifierString(protocolStack) };
		 }

		 private string ModifierString( ProtocolStack protocolStack )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return protocolStack.ModifierProtocols().Select(Neo4Net.causalclustering.protocol.Protocol_ModifierProtocol.implementation).collect(Collectors.joining(",", "[", "]"));
		 }
	}

}