using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.plugins
{

	using Neo4Net.Helpers.Collections;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using ExtensionPointRepresentation = Neo4Net.Server.rest.repr.ExtensionPointRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;

	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public sealed class DefaultPluginManager : PluginManager
	{
		 private readonly IDictionary<string, ServerExtender> _extensions = new Dictionary<string, ServerExtender>();

		 [Obsolete]
		 public DefaultPluginManager( LogProvider logProvider )
		 {
			  IDictionary<string, Pair<ServerPlugin, ServerExtender>> extensions = new Dictionary<string, Pair<ServerPlugin, ServerExtender>>();
			  Log log = logProvider.getLog( this.GetType() );
			  IEnumerable<ServerPlugin> loadedPlugins = ServerPlugin.Load();
			  foreach ( ServerPlugin plugin in loadedPlugins )
			  {
					PluginPointFactory factory = new PluginPointFactoryImpl();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ServerExtender extender = new ServerExtender(factory);
					ServerExtender extender = new ServerExtender( factory );
					try
					{
						 plugin.LoadServerExtender( extender );
					}
					catch ( Exception ex ) when ( ex is Exception || ex is LinkageError )
					{
						 log.Warn( "Failed to load plugin [%s]: %s", plugin.ToString(), ex.Message );
						 continue;
					}
					Pair<ServerPlugin, ServerExtender> old = extensions[plugin.Name] = Pair.of( plugin, extender );
					if ( old != null )
					{
						 log.Warn( string.Format( "Extension naming conflict \"{0}\" between \"{1}\" and \"{2}\"", plugin.Name, old.First().GetType(), plugin.GetType() ) );
					}
			  }
			  foreach ( Pair<ServerPlugin, ServerExtender> extension in extensions.Values )
			  {
					log.Info( string.Format( "Loaded server plugin \"{0}\"", extension.First().Name ) );
					foreach ( PluginPoint point in extension.Other().all() )
					{
						 log.Info( string.Format( "  {0}.{1}: {2}", point.ForType().Name, point.Name(), point.Description ) );
					}
					this._extensions[extension.First().Name] = extension.Other();
			  }
		 }

		 [Obsolete]
		 public override IDictionary<string, IList<string>> GetExensionsFor( Type type )
		 {
			  IDictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			  foreach ( KeyValuePair<string, ServerExtender> extension in _extensions.SetOfKeyValuePairs() )
			  {
					IList<string> methods = new List<string>();
					foreach ( PluginPoint method in extension.Value.getExtensionsFor( type ) )
					{
						 methods.Add( method.Name() );
					}
					if ( methods.Count > 0 )
					{
						 result[extension.Key] = methods;
					}
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private PluginPoint extension(String name, Class type, String method) throws PluginLookupException
		 private PluginPoint Extension( string name, Type type, string method )
		 {
			  ServerExtender extender = _extensions[name];
			  if ( extender == null )
			  {
					throw new PluginLookupException( "No such ServerPlugin: \"" + name + "\"" );
			  }
			  return extender.GetExtensionPoint( type, method );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.ExtensionPointRepresentation describe(String name, Class type, String method) throws PluginLookupException
		 [Obsolete]
		 public override ExtensionPointRepresentation Describe( string name, Type type, string method )
		 {
			  return Describe( Extension( name, type, method ) );
		 }

		 private ExtensionPointRepresentation Describe( PluginPoint extension )
		 {
			  ExtensionPointRepresentation representation = new ExtensionPointRepresentation( extension.Name(), extension.ForType(), extension.Description );
			  extension.DescribeParameters( representation );
			  return representation;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<org.Neo4Net.server.rest.repr.ExtensionPointRepresentation> describeAll(String name) throws PluginLookupException
		 [Obsolete]
		 public override IList<ExtensionPointRepresentation> DescribeAll( string name )
		 {
			  ServerExtender extender = _extensions[name];
			  if ( extender == null )
			  {
					throw new PluginLookupException( "No such ServerPlugin: \"" + name + "\"" );
			  }
			  IList<ExtensionPointRepresentation> result = new List<ExtensionPointRepresentation>();
			  foreach ( PluginPoint plugin in extender.All() )
			  {
					result.Add( Describe( plugin ) );
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T> org.Neo4Net.server.rest.repr.Representation invoke(org.Neo4Net.kernel.internal.GraphDatabaseAPI graphDb, String name, Class<T> type, String method, T context, ParameterList params) throws PluginLookupException, org.Neo4Net.server.rest.repr.BadInputException, PluginInvocationFailureException, BadPluginInvocationException
		 [Obsolete]
		 public override Representation Invoke<T>( GraphDatabaseAPI graphDb, string name, Type type, string method, T context, ParameterList @params )
		 {
				 type = typeof( T );
			  PluginPoint plugin = Extension( name, type, method );
			  try
			  {
					return plugin.Invoke( graphDb, context, @params );
			  }
			  catch ( Exception e ) when ( e is BadInputException || e is PluginInvocationFailureException || e is BadPluginInvocationException )
			  {
					throw e;
			  }
			  catch ( Exception e )
			  {
					throw new PluginInvocationFailureException( e );
			  }
		 }

		 [Obsolete]
		 public override ISet<string> ExtensionNames()
		 {
			  return Collections.unmodifiableSet( _extensions.Keys );
		 }
	}

}