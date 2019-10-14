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
namespace Neo4Net.Server.rest.repr
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using ConstraintDefinition = Neo4Net.Graphdb.schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Point = Neo4Net.Graphdb.spatial.Point;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;

	public sealed class RepresentationType
	{
		 private static readonly IDictionary<Type, Type> _boxed = new Dictionary<Type, Type>();

		 static RepresentationType()
		 {
			  _boxed[typeof( sbyte )] = typeof( Byte );
			  _boxed[typeof( char )] = typeof( Character );
			  _boxed[typeof( short )] = typeof( Short );
			  _boxed[typeof( int )] = typeof( Integer );
			  _boxed[typeof( long )] = typeof( Long );
			  _boxed[typeof( float )] = typeof( Float );
			  _boxed[typeof( double )] = typeof( Double );
			  _boxed[typeof( bool )] = typeof( Boolean );
		 }

		 private static readonly IDictionary<string, RepresentationType> _types = new Dictionary<string, RepresentationType>();
		 private static readonly IDictionary<Type, RepresentationType> _extended = new Dictionary<Type, RepresentationType>();
		 // Graph database types
		 public static readonly RepresentationType Graphdb = new RepresentationType( "graphdb", null, typeof( GraphDatabaseService ) );
		 public static readonly RepresentationType Node = new RepresentationType( "node", "nodes", typeof( Node ) );
		 public static readonly RepresentationType Relationship = new RepresentationType( "relationship", "relationships", typeof( Relationship ) );
		 public static readonly RepresentationType Path = new RepresentationType( "path", "paths", typeof( Path ) );
		 public static readonly RepresentationType FullPath = new RepresentationType( "full-path", "full-paths", typeof( FullPath ) );
		 public static readonly RepresentationType RelationshipType = new RepresentationType( "relationship-type", "relationship-types", typeof( RelationshipType ) );
		 public static readonly RepresentationType Properties = new RepresentationType( "properties" );
		 public static readonly RepresentationType Index = new RepresentationType( "index" );
		 public static readonly RepresentationType NodeIndexRoot = new RepresentationType( "node-index" );
		 public static readonly RepresentationType RelationshipIndexRoot = new RepresentationType( "relationship-index" );
		 public static readonly RepresentationType IndexDefinition = new RepresentationType( "index-definition", "index-definitions", typeof( IndexDefinition ) );
		 public static readonly RepresentationType ConstraintDefinition = new RepresentationType( "constraint-definition", "constraint-definitions", typeof( ConstraintDefinition ) );
		 public static readonly RepresentationType Plugins = new RepresentationType( "plugins" );
		 public static readonly RepresentationType Plugin = new RepresentationType( "plugin" );
		 public static readonly RepresentationType PluginDescription = new RepresentationType( "plugin-point" );
		 public static readonly RepresentationType ServerPluginDescription = new RepresentationType( "server-plugin", null );
		 public static readonly RepresentationType PluginParameter = new RepresentationType( "plugin-parameter", "plugin-parameter-list" );
		 public static readonly RepresentationType Uri = new RepresentationType( "uri", null );
		 public static readonly RepresentationType Template = new RepresentationType( "uri-template" );
		 public static readonly RepresentationType String = new RepresentationType( "string", "strings", typeof( string ) );
		 public static readonly RepresentationType Point = new RepresentationType( "point", "points", typeof( Point ) );
		 public static readonly RepresentationType Temporal = new RepresentationType( "temporal", "temporals", typeof( Temporal ) );
		 public static readonly RepresentationType TemporalAmount = new RepresentationType( "temporal-amount", "temporal-amounts", typeof( TemporalAmount ) );
		 public static readonly RepresentationType Byte = new RepresentationType( "byte", "bytes", typeof( sbyte ) );
		 public static readonly RepresentationType Char = new RepresentationType( "character", "characters", typeof( char ) );
		 public static readonly RepresentationType Short = new RepresentationType( "short", "shorts", typeof( short ) );
		 public static readonly RepresentationType Integer = new RepresentationType( "integer", "integers", typeof( int ) );
		 public static readonly RepresentationType Long = new RepresentationType( "long", "longs", typeof( long ) );
		 public static readonly RepresentationType Float = new RepresentationType( "float", "floats", typeof( float ) );
		 public static readonly RepresentationType Double = new RepresentationType( "double", "doubles", typeof( double ) );
		 public static readonly RepresentationType Boolean = new RepresentationType( "boolean", "booleans", typeof( bool ) );
		 public static readonly RepresentationType Nothing = new RepresentationType( "void", null );
		 public static readonly RepresentationType Exception = new RepresentationType( "exception" );
		 public static readonly RepresentationType Authorization = new RepresentationType( "authorization" );
		 public static readonly RepresentationType Map = new RepresentationType( "map", "maps", typeof( System.Collections.IDictionary ) );
		 public static readonly RepresentationType Null = new RepresentationType( "null", "nulls", typeof( object ) );

		 internal readonly string ValueName;
		 internal readonly string ListName;
		 internal readonly Type Extend;

		 private RepresentationType( string valueName, string listName ) : this( valueName, listName, null )
		 {
		 }

		 private RepresentationType( string valueName, string listName, Type extend )
		 {
			  this.ValueName = valueName;
			  this.ListName = listName;
			  this.Extend = extend;
			  if ( !string.ReferenceEquals( valueName, null ) )
			  {
					_types[valueName.Replace( "-", "" )] = this;
			  }
			  if ( extend != null )
			  {
					_extended[extend] = this;
					if ( extend.IsPrimitive )
					{
						 _extended[_boxed[extend]] = this;
					}
			  }
		 }

		 internal RepresentationType( string type )
		 {
			  if ( string.ReferenceEquals( type, null ) )
			  {
					throw new System.ArgumentException( "type may not be null" );
			  }
			  this.ValueName = type;
			  this.ListName = type + "s";
			  this.Extend = null;
		 }

		 public override string ToString()
		 {
			  return ValueName;
		 }

		 internal static RepresentationType ValueOf( Type type )
		 {
			  return _types[type.Name.ToLower()];
		 }

		 public override int GetHashCode()
		 {
			  if ( string.ReferenceEquals( ValueName, null ) )
			  {
					return ListName.GetHashCode();
			  }
			  return ValueName.GetHashCode();
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj is RepresentationType )
			  {
					RepresentationType that = ( RepresentationType ) obj;
					if ( !string.ReferenceEquals( this.ValueName, null ) )
					{
						 if ( ValueName.Equals( that.ValueName ) )
						 {
							  if ( !string.ReferenceEquals( this.ListName, null ) )
							  {
									return ListName.Equals( that.ListName );
							  }
							  else
							  {
									return string.ReferenceEquals( that.ListName, null );
							  }
						 }
					}
					else if ( !string.ReferenceEquals( this.ListName, null ) )
					{
						 return string.ReferenceEquals( that.ValueName, null ) && ListName.Equals( that.ListName );
					}
			  }
			  return false;
		 }

		 internal static RepresentationType Extended( Type extend )
		 {
			  return _extended[extend];
		 }
	}

}