using System;
using System.Collections;
using System.Collections.Concurrent;
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
namespace Neo4Net.Server.plugins
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Neo4Net.Helpers.Collection;

	/// @deprecated Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead. 
	[Obsolete("Server plugins are deprecated for removal in the next major release. Please use unmanaged extensions instead.")]
	public sealed class ServerExtender
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final java.util.Map<Class, java.util.Map<String, PluginPoint>> targetToPluginMap = new java.util.HashMap();
		 private readonly IDictionary<Type, IDictionary<string, PluginPoint>> _targetToPluginMap = new Hashtable();
		 private PluginPointFactory _pluginPointFactory;

		 internal ServerExtender( PluginPointFactory pluginPointFactory )
		 {
			  this._pluginPointFactory = pluginPointFactory;
			  _targetToPluginMap[typeof( Node )] = new ConcurrentDictionary<string, PluginPoint>();
			  _targetToPluginMap[typeof( Relationship )] = new ConcurrentDictionary<string, PluginPoint>();
			  _targetToPluginMap[typeof( GraphDatabaseService )] = new ConcurrentDictionary<string, PluginPoint>();
		 }

		 internal IEnumerable<PluginPoint> GetExtensionsFor( Type type )
		 {
			  IDictionary<string, PluginPoint> ext = _targetToPluginMap[type];
			  if ( ext == null )
			  {
					return Collections.emptyList();
			  }
			  return ext.Values;
		 }

		 internal IEnumerable<PluginPoint> All()
		 {
			  return new NestingIterableAnonymousInnerClass( this, _targetToPluginMap.Values );
		 }

		 private class NestingIterableAnonymousInnerClass : NestingIterable<PluginPoint, IDictionary<string, PluginPoint>>
		 {
			 private readonly ServerExtender _outerInstance;

			 public NestingIterableAnonymousInnerClass( ServerExtender outerInstance, UnknownType values ) : base( values )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override IEnumerator<PluginPoint> createNestedIterator( IDictionary<string, PluginPoint> item )
			 {
				  return item.Values.GetEnumerator();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: PluginPoint getExtensionPoint(Class type, String method) throws PluginLookupException
		 internal PluginPoint GetExtensionPoint( Type type, string method )
		 {
			  IDictionary<string, PluginPoint> ext = _targetToPluginMap[type];
			  PluginPoint plugin = null;
			  if ( ext != null )
			  {
					plugin = ext[method];
			  }
			  if ( plugin == null )
			  {
					throw new PluginLookupException( "No plugin \"" + method + "\" for " + type );
			  }
			  return plugin;
		 }

		 internal void AddExtension( Type type, PluginPoint plugin )
		 {
			  IDictionary<string, PluginPoint> ext = _targetToPluginMap[type];
			  if ( ext == null )
			  {
					throw new System.InvalidOperationException( "Cannot extend " + type );
			  }
			  Add( ext, plugin );
		 }

		 [Obsolete]
		 public void AddGraphDatabaseExtensions( PluginPoint plugin )
		 {
			  Add( _targetToPluginMap[typeof( GraphDatabaseService )], plugin );
		 }

		 [Obsolete]
		 public void AddNodeExtensions( PluginPoint plugin )
		 {
			  Add( _targetToPluginMap[typeof( Node )], plugin );
		 }

		 [Obsolete]
		 public void AddRelationshipExtensions( PluginPoint plugin )
		 {
			  Add( _targetToPluginMap[typeof( Relationship )], plugin );
		 }

		 private static void Add( IDictionary<string, PluginPoint> extensions, PluginPoint plugin )
		 {
			  if ( extensions[plugin.Name()] != null )
			  {
					throw new System.ArgumentException( "This plugin already has an plugin point with the name \"" + plugin.Name() + "\"" );
			  }
			  extensions[plugin.Name()] = plugin;
		 }

		 [Obsolete]
		 public PluginPointFactory PluginPointFactory
		 {
			 get
			 {
				  return _pluginPointFactory;
			 }
		 }
	}

}