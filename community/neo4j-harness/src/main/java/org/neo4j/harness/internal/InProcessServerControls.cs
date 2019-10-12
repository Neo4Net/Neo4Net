using System;
using System.Collections.Generic;
using System.IO;

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
namespace Org.Neo4j.Harness.@internal
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Configuration = Org.Neo4j.Graphdb.config.Configuration;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Connector = Org.Neo4j.Kernel.configuration.Connector;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using NeoServer = Org.Neo4j.Server.NeoServer;

	using static Org.Neo4j.Kernel.configuration.HttpConnector.Encryption;

	public class InProcessServerControls : ServerControls
	{
		 private const string DEFAULT_BOLT_CONNECTOR_KEY = "bolt";

		 private readonly File _serverFolder;
		 private readonly File _userLogFile;
		 private readonly File _internalLogFile;
		 private readonly NeoServer _server;
		 private readonly System.IDisposable _additionalClosable;
		 private ConnectorPortRegister _connectorPortRegister;

		 public InProcessServerControls( File serverFolder, File userLogFile, File internalLogFile, NeoServer server, System.IDisposable additionalClosable )
		 {
			  this._serverFolder = serverFolder;
			  this._userLogFile = userLogFile;
			  this._internalLogFile = internalLogFile;
			  this._server = server;
			  this._additionalClosable = additionalClosable;
		 }

		 public override URI BoltURI()
		 {
			  IList<BoltConnector> connectors = _server.Config.enabledBoltConnectors();

			  BoltConnector defaultConnector = null;
			  BoltConnector firstConnector = null;

			  foreach ( BoltConnector connector in connectors )
			  {
					if ( DEFAULT_BOLT_CONNECTOR_KEY.Equals( connector.Key() ) )
					{
						 defaultConnector = connector;
					}
					if ( firstConnector == null )
					{
						 firstConnector = connector;
					}
			  }

			  if ( defaultConnector != null )
			  {
					// bolt connector with default key is configured, return its address
					return ConnectorUri( "bolt", defaultConnector );
			  }
			  if ( firstConnector != null )
			  {
					// some bolt connector is configured, return its address
					return ConnectorUri( "bolt", firstConnector );
			  }

			  throw new System.InvalidOperationException( "Bolt connector is not configured" );
		 }

		 public override URI HttpURI()
		 {
			  return HttpConnectorUri( "http", Encryption.NONE ).orElseThrow( () => new System.InvalidOperationException("HTTP connector is not configured") );
		 }

		 public override Optional<URI> HttpsURI()
		 {
			  return HttpConnectorUri( "https", Encryption.TLS );
		 }

		 public virtual void Start()
		 {
			  this._server.start();
			  this._connectorPortRegister = ConnectorPortRegister( _server );
		 }

		 public override void Close()
		 {
			  _server.stop();
			  this._connectorPortRegister = null;
			  try
			  {
					_additionalClosable.Dispose();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  try
			  {
					if ( LooksLikeMd5Hash( _serverFolder.Name ) )
					{
						 FileUtils.deleteRecursively( _serverFolder );
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Failed to clean up test server directory.", e );
			  }
		 }

		 public override void PrintLogs( PrintStream @out )
		 {
			  PrintLog( "User Log File", _userLogFile, @out );
			  PrintLog( "Internal Log File", _internalLogFile, @out );
		 }

		 private static void PrintLog( string description, File file, PrintStream @out )
		 {
			  if ( file != null && file.exists() )
			  {
					@out.println( string.Format( "---------- BEGIN {0} ----------", description ) );

					try
					{
							using ( StreamReader reader = new StreamReader( file ) )
							{
							 reader.lines().forEach(@out.println);
							}
					}
					catch ( IOException ex )
					{
						 @out.println( "Unable to collect log files: " + ex.Message );
						 ex.printStackTrace( @out );
					}
					finally
					{
						 @out.println( string.Format( "---------- END {0} ----------", description ) );
					}
			  }
		 }

		 private bool LooksLikeMd5Hash( string name )
		 {
			  // Pure paranoia, and a silly check - but this decreases the likelihood that we delete something that isn't
			  // our randomly generated folder significantly.
			  return name.Length == 32;
		 }

		 public override GraphDatabaseService Graph()
		 {
			  return _server.Database.Graph;
		 }

		 public override Configuration Config()
		 {
			  return _server.Config;
		 }

		 private Optional<URI> HttpConnectorUri( string scheme, Encryption encryption )
		 {
			  return _server.Config.enabledHttpConnectors().Where(connector => connector.encryptionLevel() == encryption).First().Select(connector => ConnectorUri(scheme, connector));
		 }

		 private URI ConnectorUri( string scheme, Connector connector )
		 {
			  HostnamePort hostPort = _connectorPortRegister.getLocalAddress( connector.Key() );
			  return URI.create( scheme + "://" + hostPort + "/" );
		 }

		 private static ConnectorPortRegister ConnectorPortRegister( NeoServer server )
		 {
			  return ( ( GraphDatabaseAPI ) server.Database.Graph ).DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
		 }
	}

}