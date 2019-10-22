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
	using Streams = Neo4Net.Stream.Streams;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.handshake.StatusCode.SUCCESS;

	public class HandshakeServer : ServerMessageHandler
	{
		 private readonly Channel _channel;
		 private readonly ApplicationProtocolRepository _applicationProtocolRepository;
		 private readonly ModifierProtocolRepository _modifierProtocolRepository;
		 private readonly SupportedProtocols<int, Protocol_ApplicationProtocol> _supportedApplicationProtocol;
		 private readonly ProtocolStack.Builder _protocolStackBuilder = ProtocolStack.Builder();
		 private readonly CompletableFuture<ProtocolStack> _protocolStackFuture = new CompletableFuture<ProtocolStack>();
		 private bool _magicReceived;
		 private bool _initialised;

		 internal HandshakeServer( ApplicationProtocolRepository applicationProtocolRepository, ModifierProtocolRepository modifierProtocolRepository, Channel channel )
		 {
			  this._channel = channel;
			  this._applicationProtocolRepository = applicationProtocolRepository;
			  this._modifierProtocolRepository = modifierProtocolRepository;
			  this._supportedApplicationProtocol = applicationProtocolRepository.SupportedProtocol();
		 }

		 public virtual void Init()
		 {
			  _channel.writeAndFlush( InitialMagicMessage.Instance() );
			  _initialised = true;
		 }

		 private void EnsureMagic()
		 {
			  if ( !_magicReceived )
			  {
					Decline( "No magic value received" );
					throw new System.InvalidOperationException( "Magic value not received." );
			  }
			  if ( !_initialised )
			  {
					Init();
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

		 public override void Handle( ApplicationProtocolRequest request )
		 {
			  EnsureMagic();

			  ApplicationProtocolResponse response;
			  if ( !request.ProtocolName().Equals(_supportedApplicationProtocol.identifier().canonicalName()) )
			  {
					response = ApplicationProtocolResponse.NoProtocol;
					_channel.writeAndFlush( response );
					Decline( string.Format( "Requested protocol {0} not supported", request.ProtocolName() ) );
			  }
			  else
			  {
					Optional<Protocol_ApplicationProtocol> selected = _applicationProtocolRepository.select( request.ProtocolName(), SupportedVersionsFor(request) );

					if ( selected.Present )
					{
						 Protocol_ApplicationProtocol selectedProtocol = selected.get();
						 _protocolStackBuilder.application( selectedProtocol );
						 response = new ApplicationProtocolResponse( SUCCESS, selectedProtocol.category(), selectedProtocol.implementation() );
						 _channel.writeAndFlush( response );
					}
					else
					{
						 response = ApplicationProtocolResponse.NoProtocol;
						 _channel.writeAndFlush( response );
						 Decline( string.Format( "Do not support requested protocol {0} versions {1}", request.ProtocolName(), request.Versions() ) );
					}
			  }
		 }

		 public override void Handle( ModifierProtocolRequest modifierProtocolRequest )
		 {
			  EnsureMagic();

			  ModifierProtocolResponse response;
			  Optional<Protocol_ModifierProtocol> selected = _modifierProtocolRepository.select( modifierProtocolRequest.ProtocolName(), SupportedVersionsFor(modifierProtocolRequest) );

			  if ( selected.Present )
			  {
					Protocol_ModifierProtocol modifierProtocol = selected.get();
					_protocolStackBuilder.modifier( modifierProtocol );
					response = new ModifierProtocolResponse( SUCCESS, modifierProtocol.category(), modifierProtocol.implementation() );
			  }
			  else
			  {
					response = ModifierProtocolResponse.Failure( modifierProtocolRequest.ProtocolName() );
			  }

			  _channel.writeAndFlush( response );
		 }

		 public override void Handle( SwitchOverRequest switchOverRequest )
		 {
			  EnsureMagic();
			  ProtocolStack protocolStack = _protocolStackBuilder.build();
			  Optional<Protocol_ApplicationProtocol> switchOverProtocol = _applicationProtocolRepository.select( switchOverRequest.ProtocolName(), switchOverRequest.Version() );
			  IList<Protocol_ModifierProtocol> switchOverModifiers = switchOverRequest.ModifierProtocols().Select(pair => _modifierProtocolRepository.select(pair.first(), pair.other())).flatMap(Streams.ofOptional).ToList();

			  if ( !switchOverProtocol.Present )
			  {
					_channel.writeAndFlush( SwitchOverResponse.Failure );
					Decline( string.Format( "Cannot switch to protocol {0} version {1:D}", switchOverRequest.ProtocolName(), switchOverRequest.Version() ) );
			  }
			  else if ( protocolStack.ApplicationProtocol() == null )
			  {
					_channel.writeAndFlush( SwitchOverResponse.Failure );
					Decline( string.Format( "Attempted to switch to protocol {0} version {1:D} before negotiation complete", switchOverRequest.ProtocolName(), switchOverRequest.Version() ) );
			  }
			  else if ( !switchOverProtocol.get().Equals(protocolStack.ApplicationProtocol()) )
			  {
					_channel.writeAndFlush( SwitchOverResponse.Failure );
					Decline( string.Format( "Switch over mismatch: requested {0} version {1} but negotiated {2} version {3}", switchOverRequest.ProtocolName(), switchOverRequest.Version(), protocolStack.ApplicationProtocol().category(), protocolStack.ApplicationProtocol().implementation() ) );
			  }
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: else if (!switchOverModifiers.equals(protocolStack.modifierProtocols()))
			  else if ( !switchOverModifiers.SequenceEqual( protocolStack.ModifierProtocols() ) )
			  {
					_channel.writeAndFlush( SwitchOverResponse.Failure );
					Decline( string.Format( "Switch over mismatch: requested modifiers {0} but negotiated {1}", switchOverRequest.ModifierProtocols(), protocolStack.ModifierProtocols() ) );
			  }
			  else
			  {
					SwitchOverResponse response = new SwitchOverResponse( SUCCESS );
					_channel.writeAndFlush( response );

					_protocolStackFuture.complete( protocolStack );
			  }
		 }

		 private ISet<string> SupportedVersionsFor( ModifierProtocolRequest request )
		 {
			  return _modifierProtocolRepository.supportedProtocolFor( request.ProtocolName() ).map(supported => supported.mutuallySupportedVersionsFor(request.Versions())).orElse(Collections.emptySet());
		 }

		 private ISet<int> SupportedVersionsFor( ApplicationProtocolRequest request )
		 {
			  return _supportedApplicationProtocol.mutuallySupportedVersionsFor( request.Versions() );
		 }

		 private void Decline( string message )
		 {
			  _channel.dispose();
			  _protocolStackFuture.completeExceptionally( new ServerHandshakeException( message, _protocolStackBuilder ) );
		 }

		 internal virtual CompletableFuture<ProtocolStack> ProtocolStackFuture()
		 {
			  return _protocolStackFuture;
		 }
	}

}