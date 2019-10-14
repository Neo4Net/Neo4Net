using System;

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
namespace Neo4Net.Values
{

	using BufferValueWriter = Neo4Net.Values.Storable.BufferValueWriter;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;

	public class BufferAnyValueWriter : BufferValueWriter, AnyValueWriter<Exception>
	{

		 internal enum SpecialKind
		 {
			  WriteNode,
			  WriteNodeReference,
			  EndNode,
			  BeginLabels,
			  EndLabels,
			  WriteRelationship,
			  WriteRelationshipReference,
			  EndEdge,
			  WritePath,
			  BeginMap,
			  WriteKeyId,
			  EndMap,
			  BeginList,
			  EndList,
		 }

		 public class Special
		 {
			  internal readonly SpecialKind Kind;
			  internal readonly string Key;

			  public override bool Equals( object o )
			  {
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}

					Special special = ( Special ) o;
					return Kind == special.Kind && Key.Equals( special.Key );
			  }

			  public override int GetHashCode()
			  {
					return 31 * Kind.GetHashCode() + Key.GetHashCode();
			  }

			  internal Special( SpecialKind kind, string key )
			  {
					this.Kind = kind;
					this.Key = key;
			  }

			  internal Special( SpecialKind kind, int key )
			  {
					this.Kind = kind;
					this.Key = Convert.ToString( key );
			  }

			  public override string ToString()
			  {
					return format( "Special(%s)", Key );
			  }
		 }

		 public override void WriteNodeReference( long nodeId )
		 {
			  Buffer.Add( Specials.WriteNodeReference( nodeId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeNode(long nodeId, org.neo4j.values.storable.TextArray labels, org.neo4j.values.virtual.MapValue properties) throws RuntimeException
		 public override void WriteNode( long nodeId, TextArray labels, MapValue properties )
		 {
			  Buffer.Add( Specials.WriteNode( nodeId, labels, properties ) );
		 }

		 public override void WriteRelationshipReference( long relId )
		 {
			  Buffer.Add( Specials.WriteRelationshipReference( relId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeRelationship(long relId, long startNodeId, long endNodeId, org.neo4j.values.storable.TextValue type, org.neo4j.values.virtual.MapValue properties) throws RuntimeException
		 public override void WriteRelationship( long relId, long startNodeId, long endNodeId, TextValue type, MapValue properties )
		 {
			  Buffer.Add( Specials.WriteRelationship( relId, startNodeId, endNodeId, type, properties ) );
		 }

		 public override void BeginMap( int size )
		 {
			  Buffer.Add( Specials.BeginMap( size ) );
		 }

		 public override void EndMap()
		 {
			  Buffer.Add( Specials.EndMap() );
		 }

		 public override void BeginList( int size )
		 {
			  Buffer.Add( Specials.BeginList( size ) );
		 }

		 public override void EndList()
		 {
			  Buffer.Add( Specials.EndList() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writePath(org.neo4j.values.virtual.NodeValue[] nodes, org.neo4j.values.virtual.RelationshipValue[] relationships) throws RuntimeException
		 public override void WritePath( NodeValue[] nodes, RelationshipValue[] relationships )
		 {
			  Buffer.Add( Specials.WritePath( nodes, relationships ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public static class Specials
		 public class Specials
		 {

			  public static Special WriteNode( long nodeId, TextArray labels, MapValue properties )
			  {
					return new Special( SpecialKind.WriteNode, Arrays.GetHashCode( new object[]{ nodeId, properties } ) + 31 * labels.GetHashCode() );
			  }

			  public static Special WriteRelationship( long edgeId, long startNodeId, long endNodeId, TextValue type, MapValue properties )
			  {
					return new Special( SpecialKind.WriteRelationship, Arrays.GetHashCode( new object[]{ edgeId, startNodeId, endNodeId, type, properties } ) );
			  }

			  public static Special WritePath( NodeValue[] nodes, RelationshipValue[] edges )
			  {
					return new Special( SpecialKind.WritePath, Arrays.GetHashCode( nodes ) + 31 * Arrays.GetHashCode( edges ) );
			  }

			  public static Special WriteNodeReference( long nodeId )
			  {
					return new Special( SpecialKind.WriteNodeReference, ( int ) nodeId );
			  }

			  public static Special WriteRelationshipReference( long edgeId )
			  {
					return new Special( SpecialKind.WriteRelationshipReference, ( int ) edgeId );
			  }

			  public static Special BeginMap( int size )
			  {
					return new Special( SpecialKind.BeginMap, size );
			  }

			  public static Special EndMap()
			  {
					return new Special( SpecialKind.EndMap, 0 );
			  }

			  public static Special BeginList( int size )
			  {
					return new Special( SpecialKind.BeginList, size );
			  }

			  public static Special EndList()
			  {
					return new Special( SpecialKind.EndList, 0 );
			  }
		 }
	}

}