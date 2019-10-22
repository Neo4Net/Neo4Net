using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Values.@virtual
{

	using Neo4Net.Values;
	using Neo4Net.Values;

	public abstract class PathValue : VirtualValue
	{
		 public abstract NodeValue StartNode();

		 public abstract NodeValue EndNode();

		 public abstract RelationshipValue LastRelationship();

		 public abstract NodeValue[] Nodes();

		 public abstract RelationshipValue[] Relationships();

		 public override bool Equals( VirtualValue other )
		 {
			  if ( other == null || !( other is PathValue ) )
			  {
					return false;
			  }
			  PathValue that = ( PathValue ) other;
			  return Size() == that.Size() && Arrays.Equals(Nodes(), that.Nodes()) && Arrays.Equals(Relationships(), that.Relationships());
		 }

		 public override int ComputeHash()
		 {
			  NodeValue[] nodes = nodes();
			  RelationshipValue[] relationships = relationships();
			  int result = nodes[0].GetHashCode();
			  for ( int i = 1; i < nodes.Length; i++ )
			  {
					result += 31 * ( result + relationships[i - 1].GetHashCode() );
					result += 31 * ( result + nodes[i].GetHashCode() );
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void writeTo(org.Neo4Net.values.AnyValueWriter<E> writer) throws E
		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  writer.WritePath( Nodes(), Relationships() );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapPath( this );
		 }

		 public override VirtualValueGroup ValueGroup()
		 {
			  return VirtualValueGroup.Path;
		 }

		 public override int CompareTo( VirtualValue other, IComparer<AnyValue> comparator )
		 {
			  if ( other == null || !( other is PathValue ) )
			  {
					throw new System.ArgumentException( "Cannot compare different virtual values" );
			  }

			  PathValue otherPath = ( PathValue ) other;
			  NodeValue[] nodes = nodes();
			  RelationshipValue[] relationships = relationships();
			  NodeValue[] otherNodes = otherPath.Nodes();
			  RelationshipValue[] otherRelationships = otherPath.Relationships();

			  int x = nodes[0].CompareTo( otherNodes[0], comparator );
			  if ( x == 0 )
			  {
					int i = 0;
					int length = Math.Min( relationships.Length, otherRelationships.Length );

					while ( x == 0 && i < length )
					{
						 x = relationships[i].CompareTo( otherRelationships[i], comparator );
						 ++i;
					}

					if ( x == 0 )
					{
						 x = Integer.compare( relationships.Length, otherRelationships.Length );
					}
			  }

			  return x;
		 }

		 public override string ToString()
		 {
			  NodeValue[] nodes = nodes();
			  RelationshipValue[] relationships = relationships();
			  StringBuilder sb = new StringBuilder( TypeName + "{" );
			  int i = 0;
			  for ( ; i < relationships.Length; i++ )
			  {
					sb.Append( nodes[i] );
					sb.Append( relationships[i] );
			  }
			  sb.Append( nodes[i] );
			  sb.Append( '}' );
			  return sb.ToString();
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Path";
			 }
		 }

		 public virtual ListValue AsList()
		 {
			  NodeValue[] nodes = nodes();
			  RelationshipValue[] relationships = relationships();
			  int size = nodes.Length + relationships.Length;
			  AnyValue[] anyValues = new AnyValue[size];
			  for ( int i = 0; i < size; i++ )
			  {
					if ( i % 2 == 0 )
					{
						 anyValues[i] = nodes[i / 2];
					}
					else
					{
						 anyValues[i] = relationships[i / 2];
					}
			  }
			  return VirtualValues.List( anyValues );
		 }

		 public virtual int Size()
		 {
			  return Relationships().Length;
		 }

		 public class DirectPathValue : PathValue
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly NodeValue[] NodesConflict;
			  internal readonly RelationshipValue[] Edges;

			  internal DirectPathValue( NodeValue[] nodes, RelationshipValue[] edges )
			  {
					Debug.Assert( nodes != null );
					Debug.Assert( edges != null );
					Debug.Assert( nodes.Length == edges.Length + 1 );

					this.NodesConflict = nodes;
					this.Edges = edges;
			  }

			  public override NodeValue StartNode()
			  {
					return NodesConflict[0];
			  }

			  public override NodeValue EndNode()
			  {
					return NodesConflict[NodesConflict.Length - 1];
			  }

			  public override RelationshipValue LastRelationship()
			  {
					Debug.Assert( Edges.Length > 0 );
					return Edges[Edges.Length - 1];
			  }

			  public override NodeValue[] Nodes()
			  {
					return NodesConflict;
			  }

			  public override RelationshipValue[] Relationships()
			  {
					return Edges;
			  }

		 }
	}

}