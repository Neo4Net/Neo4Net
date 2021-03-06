﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.discovery
{

	using Org.Neo4j.causalclustering.core.state.storage;
	using StringMarshal = Org.Neo4j.causalclustering.messaging.marshalling.StringMarshal;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using SocketAddressParser = Org.Neo4j.Helpers.SocketAddressParser;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Encryption = Org.Neo4j.Kernel.configuration.HttpConnector.Encryption;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ClientConnectorAddresses.Scheme.bolt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ClientConnectorAddresses.Scheme.http;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ClientConnectorAddresses.Scheme.https;

	public class ClientConnectorAddresses : IEnumerable<ClientConnectorAddresses.ConnectorUri>
	{
		 private readonly IList<ConnectorUri> _connectorUris;

		 public ClientConnectorAddresses( IList<ConnectorUri> connectorUris )
		 {
			  this._connectorUris = connectorUris;
		 }

		 internal static ClientConnectorAddresses ExtractFromConfig( Config config )
		 {
			  IList<ConnectorUri> connectorUris = new List<ConnectorUri>();

			  IList<BoltConnector> boltConnectors = config.EnabledBoltConnectors();

			  if ( boltConnectors.Count == 0 )
			  {
					throw new System.ArgumentException( "A Bolt connector must be configured to run a cluster" );
			  }

			  boltConnectors.ForEach( c => connectorUris.Add( new ConnectorUri( bolt, config.Get( c.advertised_address ) ) ) );

			  config.EnabledHttpConnectors().ForEach(c => connectorUris.Add(new ConnectorUri(Encryption.NONE.Equals(c.encryptionLevel()) ? http : https, config.Get(c.advertised_address))));

			  return new ClientConnectorAddresses( connectorUris );
		 }

		 public virtual AdvertisedSocketAddress BoltAddress()
		 {
			  return _connectorUris.Where( connectorUri => connectorUri.scheme == bolt ).First().orElseThrow(() => new System.ArgumentException("A Bolt connector must be configured to run a cluster")).socketAddress;
		 }

		 public virtual IList<URI> UriList()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _connectorUris.Select( ConnectorUri::toUri ).ToList();
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
			  ClientConnectorAddresses that = ( ClientConnectorAddresses ) o;
			  return Objects.Equals( _connectorUris, that._connectorUris );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _connectorUris );
		 }

		 public override string ToString()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return _connectorUris.Select( ConnectorUri::toString ).collect( Collectors.joining( "," ) );
		 }

		 internal static ClientConnectorAddresses FromString( string value )
		 {
			  return new ClientConnectorAddresses( Stream.of( value.Split( ",", true ) ).map( ConnectorUri.fromString ).collect( Collectors.toList() ) );
		 }

		 public override IEnumerator<ConnectorUri> Iterator()
		 {
			  return _connectorUris.GetEnumerator();
		 }

		 public enum Scheme
		 {
			  Bolt,
			  Http,
			  Https
		 }

		 public class ConnectorUri
		 {
			  internal readonly Scheme Scheme;
			  internal readonly AdvertisedSocketAddress SocketAddress;

			  public ConnectorUri( Scheme scheme, AdvertisedSocketAddress socketAddress )
			  {
					this.Scheme = scheme;
					this.SocketAddress = socketAddress;
			  }

			  internal virtual URI ToUri()
			  {
					try
					{
						 return new URI( Scheme.name().ToLower(), null, SocketAddress.Hostname, SocketAddress.Port, null, null, null );
					}
					catch ( URISyntaxException e )
					{
						 throw new System.ArgumentException( e );
					}
			  }

			  public override string ToString()
			  {
					return ToUri().ToString();
			  }

			  internal static ConnectorUri FromString( string @string )
			  {
					URI uri = URI.create( @string );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					AdvertisedSocketAddress advertisedSocketAddress = SocketAddressParser.socketAddress( uri.Authority, AdvertisedSocketAddress::new );
					return new ConnectorUri( Enum.Parse( typeof( Scheme ), uri.Scheme ), advertisedSocketAddress );
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
					ConnectorUri that = ( ConnectorUri ) o;
					return Scheme == that.Scheme && Objects.Equals( SocketAddress, that.SocketAddress );
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( Scheme, SocketAddress );
			  }
		 }

		 public class Marshal : SafeChannelMarshal<ClientConnectorAddresses>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected ClientConnectorAddresses unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
			  protected internal override ClientConnectorAddresses Unmarshal0( ReadableChannel channel )
			  {
					int size = channel.Int;
					IList<ConnectorUri> connectorUris = new List<ConnectorUri>( size );
					for ( int i = 0; i < size; i++ )
					{
						 string schemeName = StringMarshal.unmarshal( channel );
						 string hostName = StringMarshal.unmarshal( channel );
						 int port = channel.Int;
						 connectorUris.Add( new ConnectorUri( Enum.Parse( typeof( Scheme ), schemeName ), new AdvertisedSocketAddress( hostName, port ) ) );
					}
					return new ClientConnectorAddresses( connectorUris );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(ClientConnectorAddresses connectorUris, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( ClientConnectorAddresses connectorUris, WritableChannel channel )
			  {
					channel.PutInt( connectorUris.connectorUris.size() );
					foreach ( ConnectorUri uri in connectorUris )
					{
						 StringMarshal.marshal( channel, uri.Scheme.name() );
						 StringMarshal.marshal( channel, uri.SocketAddress.Hostname );
						 channel.PutInt( uri.SocketAddress.Port );
					}
			  }
		 }
	}

}