using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.rest.discovery
{

	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Neo4Net.Graphdb.config;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.DiscoverableURIs.Precedence.HIGH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.DiscoverableURIs.Precedence.HIGHEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.DiscoverableURIs.Precedence.LOW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.DiscoverableURIs.Precedence.LOWEST;

	/// <summary>
	/// Repository of URIs that the REST API publicly advertises at the root endpoint.
	/// </summary>
	public class DiscoverableURIs
	{
		 public enum Precedence
		 {
			  Lowest,
			  Low,
			  Normal,
			  High,
			  Highest
		 }

		 private readonly ICollection<URIEntry> _entries;

		 private DiscoverableURIs( ICollection<URIEntry> entries )
		 {
			  this._entries = entries;
		 }

		 public virtual void ForEach( System.Action<string, URI> consumer )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  _entries.collect( Collectors.groupingBy( e => e.key ) ).ForEach( ( key, list ) => list.Max( System.Collections.IComparer.comparing( e => e.precedence ) ).ifPresent( e => consumer( key, e.uri ) ) );
		 }

		 private class URIEntry
		 {
			  internal string Key;
			  internal Precedence Precedence;
			  internal URI Uri;

			  internal URIEntry( string key, URI uri, Precedence precedence )
			  {
					this.Key = key;
					this.Uri = uri;
					this.Precedence = precedence;
			  }
		 }

		 public class Builder
		 {
			  internal readonly ICollection<URIEntry> Entries;

			  public Builder()
			  {
					Entries = new List<URIEntry>();
			  }

			  public Builder( DiscoverableURIs copy )
			  {
					Entries = new List<URIEntry>( copy.entries );
			  }

			  public virtual Builder Add( string key, URI uri, Precedence precedence )
			  {
					if ( Entries.Any( e => e.key.Equals( key ) && e.precedence == precedence ) )
					{
						 throw new InvalidSettingException( string.Format( "Unable to add two entries with the same precedence using key '{0}' and precedence '{1}'", key, precedence ) );
					}

					Entries.Add( new URIEntry( key, uri, precedence ) );
					return this;
			  }

			  public virtual Builder Add( string key, string uri, Precedence precedence )
			  {
					try
					{
						 return Add( key, new URI( uri ), precedence );
					}
					catch ( URISyntaxException e )
					{
						 throw new InvalidSettingException( string.Format( "Unable to construct bolt discoverable URI using '{0}' as uri: " + "{1}", uri, e.Message ), e );
					}
			  }

			  public virtual Builder Add( string key, string scheme, string hostname, int port, Precedence precedence )
			  {
					try
					{
						 return Add( key, new URI( scheme, null, hostname, port, null, null, null ), precedence );
					}
					catch ( URISyntaxException e )
					{
						 throw new InvalidSettingException( string.Format( "Unable to construct bolt discoverable URI using '{0}' as hostname: " + "{1}", hostname, e.Message ), e );
					}
			  }

			  public virtual Builder AddBoltConnectorFromConfig( string key, string scheme, Config config, Setting<URI> @override, ConnectorPortRegister portRegister )
			  {
					// If an override is configured, add it with the HIGHEST precedence
					if ( config.IsConfigured( @override ) )
					{
						 add( key, config.Get( @override ), HIGHEST );
					}

					config.EnabledBoltConnectors().First().ifPresent(c =>
					{
					 AdvertisedSocketAddress address = config.Get( c.advertised_address );
					 int port = address.Port;
					 if ( port == 0 )
					 {
						  port = portRegister.GetLocalAddress( c.key() ).Port;
					 }

					 // If advertised address is explicitly set, set the precedence to HIGH - eitherwise set it as LOWEST (default)
					 Add( key, scheme, address.Hostname, port, config.IsConfigured( c.advertised_address ) ? HIGH : LOWEST );
					});

					return this;
			  }

			  public virtual Builder OverrideAbsolutesFromRequest( URI requestUri )
			  {
					// Find all default entries with absolute URIs and replace the corresponding host name entries with the one from the request uri.
					IList<URIEntry> defaultEntries = Entries.Where( e => e.uri.Absolute && e.precedence == LOWEST ).ToList();

					foreach ( URIEntry entry in defaultEntries )
					{
						 Add( entry.Key, entry.Uri.Scheme, requestUri.Host, entry.Uri.Port, LOW );
					}

					return this;
			  }

			  public virtual DiscoverableURIs Build()
			  {
					return new DiscoverableURIs( Entries );
			  }
		 }

	}

}