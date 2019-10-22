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
namespace Neo4Net.causalclustering.discovery
{

	using Neo4Net.causalclustering.core.state.storage;
	using StringMarshal = Neo4Net.causalclustering.messaging.marshalling.StringMarshal;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using SocketAddressParser = Neo4Net.Helpers.SocketAddressParser;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.discovery.ClientConnectorAddresses.Scheme.bolt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.discovery.ClientConnectorAddresses.Scheme.http;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.discovery.ClientConnectorAddresses.Scheme.https;

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
//ORIGINAL LINE: protected ClientConnectorAddresses unmarshal0(org.Neo4Net.storageengine.api.ReadableChannel channel) throws java.io.IOException
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
//ORIGINAL LINE: public void marshal(ClientConnectorAddresses connectorUris, org.Neo4Net.storageengine.api.WritableChannel channel) throws java.io.IOException
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