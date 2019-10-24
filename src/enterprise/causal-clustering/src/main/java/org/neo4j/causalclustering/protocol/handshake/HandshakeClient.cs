using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.protocol.handshake
{

	using Channel = Neo4Net.causalclustering.messaging.Channel;
	using Neo4Net.Collections.Helpers;
	using Streams = Neo4Net.Stream.Streams;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.handshake.StatusCode.SUCCESS;

	public class HandshakeClient : ClientMessageHandler
	{
		 private Channel _channel;
		 private ApplicationProtocolRepository _applicationProtocolRepository;
		 private ApplicationSupportedProtocols _supportedApplicationProtocol;
		 private ModifierProtocolRepository _modifierProtocolRepository;
		 private ICollection<ModifierSupportedProtocols> _supportedModifierProtocols;
		 private Protocol_ApplicationProtocol _negotiatedApplicationProtocol;
		 private IList<Pair<string, Optional<Protocol_ModifierProtocol>>> _negotiatedModifierProtocols;
		 private ProtocolStack _protocolStack;
		 private CompletableFuture<ProtocolStack> _future = new CompletableFuture<ProtocolStack>();
		 private bool _magicReceived;

		 public virtual CompletableFuture<ProtocolStack> Initiate( Channel channel, ApplicationProtocolRepository applicationProtocolRepository, ModifierProtocolRepository modifierProtocolRepository )
		 {
			  this._channel = channel;

			  this._applicationProtocolRepository = applicationProtocolRepository;
			  this._supportedApplicationProtocol = applicationProtocolRepository.SupportedProtocol();

			  this._modifierProtocolRepository = modifierProtocolRepository;
			  this._supportedModifierProtocols = modifierProtocolRepository.SupportedProtocols();

			  _negotiatedModifierProtocols = new List<Pair<string, Optional<Protocol_ModifierProtocol>>>( _supportedModifierProtocols.Count );

			  channel.Write( InitialMagicMessage.Instance() );

			  SendProtocolRequests( channel, _supportedApplicationProtocol, _supportedModifierProtocols );

			  return _future;
		 }

		 private void SendProtocolRequests( Channel channel, ApplicationSupportedProtocols applicationProtocols, ICollection<ModifierSupportedProtocols> supportedModifierProtocols )
		 {
			  supportedModifierProtocols.forEach(modifierProtocol =>
			  {
						  ProtocolSelection<string, ModifierProtocol> protocolSelection = _modifierProtocolRepository.getAll( modifierProtocol.identifier(), modifierProtocol.versions() );
						  channel.Write( new ModifierProtocolRequest( protocolSelection.Identifier(), protocolSelection.Versions() ) );
			  });

			  ProtocolSelection<int, Protocol_ApplicationProtocol> applicationProtocolSelection = _applicationProtocolRepository.getAll( applicationProtocols.Identifier(), applicationProtocols.Versions() );
			  channel.WriteAndFlush( new ApplicationProtocolRequest( applicationProtocolSelection.Identifier(), applicationProtocolSelection.Versions() ) );
		 }

		 private void EnsureMagic()
		 {
			  if ( !_magicReceived )
			  {
					Decline( "Magic value not received." );
					throw new System.InvalidOperationException( "Magic value not received." );
			  }
		 }

		 public override void Handle( InitialMagicMessage magicMessage )
		 {
			  if ( !magicMessage.CorrectMagic )
			  {
					Decline( "Incorrect magic value received" );
			  }
			  // TODO: check clusterId as well

			  _magicReceived = true;
		 }

		 public override void Handle( ApplicationProtocolResponse applicationProtocolResponse )
		 {
			  EnsureMagic();
			  if ( applicationProtocolResponse.StatusCode() != SUCCESS )
			  {
					Decline( "Unsuccessful application protocol response" );
					return;
			  }

			  Optional<Protocol_ApplicationProtocol> protocol = _applicationProtocolRepository.select( applicationProtocolResponse.ProtocolName(), applicationProtocolResponse.Version() );

			  if ( !protocol.Present )
			  {
					ProtocolSelection<int, Protocol_ApplicationProtocol> knownApplicationProtocolVersions = _applicationProtocolRepository.getAll( _supportedApplicationProtocol.identifier(), _supportedApplicationProtocol.versions() );
					Decline( string.Format( "Mismatch of application protocols between client and server: Server protocol {0} version {1:D}: Client protocol {2} versions {3}", applicationProtocolResponse.ProtocolName(), applicationProtocolResponse.Version(), knownApplicationProtocolVersions.Identifier(), knownApplicationProtocolVersions.Versions() ) );
			  }
			  else
			  {
					_negotiatedApplicationProtocol = protocol.get();

					SendSwitchOverRequestIfReady();
			  }
		 }

		 public override void Handle( ModifierProtocolResponse modifierProtocolResponse )
		 {
			  EnsureMagic();
			  if ( modifierProtocolResponse.StatusCode() == StatusCode.Success )
			  {
					Optional<Protocol_ModifierProtocol> selectedModifierProtocol = _modifierProtocolRepository.select( modifierProtocolResponse.ProtocolName(), modifierProtocolResponse.Version() );
					_negotiatedModifierProtocols.Add( Pair.of( modifierProtocolResponse.ProtocolName(), selectedModifierProtocol ) );
			  }
			  else
			  {
					_negotiatedModifierProtocols.Add( Pair.of( modifierProtocolResponse.ProtocolName(), null ) );
			  }

			  SendSwitchOverRequestIfReady();
		 }

		 private void SendSwitchOverRequestIfReady()
		 {
			  if ( _negotiatedApplicationProtocol != null && _negotiatedModifierProtocols.Count == _supportedModifierProtocols.Count )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					IList<Protocol_ModifierProtocol> agreedModifierProtocols = _negotiatedModifierProtocols.Select( Pair::other ).flatMap( Streams.ofOptional ).ToList();

					_protocolStack = new ProtocolStack( _negotiatedApplicationProtocol, agreedModifierProtocols );
					IList<Pair<string, string>> switchOverModifierProtocols = agreedModifierProtocols.Select( protocol => Pair.of( protocol.category(), protocol.implementation() ) ).ToList();

					_channel.writeAndFlush( new SwitchOverRequest( _negotiatedApplicationProtocol.category(), _negotiatedApplicationProtocol.implementation(), switchOverModifierProtocols ) );
			  }
		 }

		 public override void Handle( SwitchOverResponse response )
		 {
			  EnsureMagic();
			  if ( _protocolStack == null )
			  {
					Decline( "Attempted to switch over when protocol stack not established" );
			  }
			  else if ( response.Status() != StatusCode.Success )
			  {
					Decline( "Server failed to switch over" );
			  }
			  else
			  {
					_future.complete( _protocolStack );
			  }
		 }

		 internal virtual bool FailIfNotDone( string message )
		 {
			  if ( !_future.Done )
			  {
					Decline( message );
					return true;
			  }
			  return false;
		 }

		 private void Decline( string message )
		 {
			  _future.completeExceptionally( new ClientHandshakeException( message, _negotiatedApplicationProtocol, _negotiatedModifierProtocols ) );
		 }
	}

}