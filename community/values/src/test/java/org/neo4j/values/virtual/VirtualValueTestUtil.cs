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

	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.relationshipValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.emptyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.nodeValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class VirtualValueTestUtil
	public class VirtualValueTestUtil
	{
		 public static AnyValue ToAnyValue( object o )
		 {
			  if ( o is AnyValue )
			  {
					return ( AnyValue ) o;
			  }
			  else
			  {
					return Values.of( o );
			  }
		 }

		 public static NodeValue Node( long id, params string[] labels )
		 {
			  TextValue[] labelValues = new TextValue[labels.Length];
			  for ( int i = 0; i < labels.Length; i++ )
			  {
					labelValues[i] = stringValue( labels[i] );
			  }
			  return nodeValue( id, stringArray( labels ), emptyMap() );
		 }

		 public static VirtualValue Path( params VirtualValue[] pathElements )
		 {
			  Debug.Assert( pathElements.Length % 2 == 1 );
			  NodeValue[] nodes = new NodeValue[pathElements.Length / 2 + 1];
			  RelationshipValue[] rels = new RelationshipValue[pathElements.Length / 2];
			  nodes[0] = ( NodeValue ) pathElements[0];
			  for ( int i = 1; i < pathElements.Length; i += 2 )
			  {
					rels[i / 2] = ( RelationshipValue ) pathElements[i];
					nodes[i / 2 + 1] = ( NodeValue ) pathElements[i + 1];
			  }
			  return VirtualValues.Path( nodes, rels );
		 }

		 public static RelationshipValue Rel( long id, long start, long end )
		 {
			  return relationshipValue( id, Node( start ), Node( end ), stringValue( "T" ), emptyMap() );
		 }

		 public static ListValue List( params object[] objects )
		 {
			  AnyValue[] values = new AnyValue[objects.Length];
			  for ( int i = 0; i < objects.Length; i++ )
			  {
					values[i] = ToAnyValue( objects[i] );
			  }
			  return VirtualValues.List( values );
		 }

		 public static VirtualValue Map( params object[] keyOrVal )
		 {
			  Debug.Assert( keyOrVal.Length % 2 == 0 );
			  string[] keys = new string[keyOrVal.Length / 2];
			  AnyValue[] values = new AnyValue[keyOrVal.Length / 2];
			  for ( int i = 0; i < keyOrVal.Length; i += 2 )
			  {
					keys[i / 2] = ( string ) keyOrVal[i];
					values[i / 2] = ToAnyValue( keyOrVal[i + 1] );
			  }
			  return VirtualValues.Map( keys, values );
		 }

		 public static NodeValue[] Nodes( params long[] ids )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Arrays.stream( ids ).mapToObj( id => nodeValue( id, stringArray( "L" ), emptyMap() ) ).toArray(NodeValue[]::new);
		 }

		 public static RelationshipValue[] Relationships( params long[] ids )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Arrays.stream( ids ).mapToObj( id => relationshipValue( id, Node( 0L ), Node( 1L ), stringValue( "T" ), emptyMap() ) ).toArray(RelationshipValue[]::new);
		 }
	}

}