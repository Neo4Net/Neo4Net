using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.Values.@virtual
{

	using ArrayValue = Org.Neo4j.Values.Storable.ArrayValue;
	using TextArray = Org.Neo4j.Values.Storable.TextArray;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using DirectPathValue = Org.Neo4j.Values.@virtual.PathValue.DirectPathValue;

	/// <summary>
	/// Entry point to the virtual values library.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public final class VirtualValues
	public sealed class VirtualValues
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public static readonly MapValue EmptyMapConflict = MapValue.EMPTY;
		 public static readonly ListValue EmptyList = new ListValue.ArrayListValue( new AnyValue[0] );

		 private VirtualValues()
		 {
		 }

		 // DIRECT FACTORY METHODS

		 public static ListValue List( params AnyValue[] values )
		 {
			  return new ListValue.ArrayListValue( values );
		 }

		 public static ListValue FromList( IList<AnyValue> values )
		 {
			  return new ListValue.JavaListListValue( values );
		 }

		 public static ListValue Range( long start, long end, long step )
		 {
			  return new ListValue.IntegralRangeListValue( start, end, step );
		 }

		 public static ListValue FromArray( ArrayValue arrayValue )
		 {
			  return new ListValue.ArrayValueListValue( arrayValue );
		 }

		 /*
		 TOMBSTONE: TransformedListValue & FilteredListValue
	
		 This list value variant would lazily apply a transform/filter on a inner list. The lazy behavior made it hard
		 to guarantee that the transform/filter was still evaluable and correct on reading the transformed list, so
		 this was removed. If we want lazy values again, remember the problems of
	
		    - returning results out of Cypher combined with auto-closing iterators
		    - reading modified tx-state which was not visible at TransformedListValue creation
	
		 */

		 public static ListValue Concat( params ListValue[] lists )
		 {
			  return new ListValue.ConcatList( lists );
		 }

		 public static MapValue EmptyMap()
		 {
			  return EmptyMapConflict;
		 }

		 public static MapValue Map( string[] keys, AnyValue[] values )
		 {
			  Debug.Assert( keys.Length == values.Length );
			  Dictionary<string, AnyValue> map = new Dictionary<string, AnyValue>( keys.Length );
			  for ( int i = 0; i < keys.Length; i++ )
			  {
					map[keys[i]] = values[i];
			  }
			  return new MapValue.MapWrappingMapValue( map );
		 }

		 public static ErrorValue Error( Exception e )
		 {
			  return new ErrorValue( e );
		 }

		 public static NodeReference Node( long id )
		 {
			  return new NodeReference( id );
		 }

		 public static RelationshipReference Relationship( long id )
		 {
			  return new RelationshipReference( id );
		 }

		 public static PathValue Path( NodeValue[] nodes, RelationshipValue[] relationships )
		 {
			  Debug.Assert( nodes != null );
			  Debug.Assert( relationships != null );
			  if ( ( nodes.Length + relationships.Length ) % 2 == 0 )
			  {
					throw new System.ArgumentException( "Tried to construct a path that is not built like a path: even number of elements" );
			  }
			  return new DirectPathValue( nodes, relationships );
		 }

		 public static NodeValue NodeValue( long id, TextArray labels, MapValue properties )
		 {
			  return new NodeValue.DirectNodeValue( id, labels, properties );
		 }

		 public static RelationshipValue RelationshipValue( long id, NodeValue startNode, NodeValue endNode, TextValue type, MapValue properties )
		 {
			  return new RelationshipValue.DirectRelationshipValue( id, startNode, endNode, type, properties );
		 }
	}

}