using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.GraphAlgo.Utils
{

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Paths = Neo4Net.GraphDb.Traversal.Paths;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Collections.Helpers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iteratorsEqual;

	public sealed class PathImpl : Path
	{
		 public sealed class Builder
		 {
			  internal readonly Builder Previous;
			  internal readonly Node Start;
			  internal readonly Relationship Relationship;
			  internal readonly int Size;

			  public Builder( Node start )
			  {
					if ( start == null )
					{
						 throw new System.NullReferenceException();
					}
					this.Start = start;
					this.Previous = null;
					this.Relationship = null;
					this.Size = 0;
			  }

			  internal Builder( Builder prev, Relationship rel )
			  {
					this.Start = prev.Start;
					this.Previous = prev;
					this.Relationship = rel;
					this.Size = prev.Size + 1;
			  }

			  public Node StartNode
			  {
				  get
				  {
						return Start;
				  }
			  }

			  public Path Build()
			  {
					return new PathImpl( this, null );
			  }

			  public Builder Push( Relationship relationship )
			  {
					if ( relationship == null )
					{
						 throw new System.NullReferenceException();
					}
					return new Builder( this, relationship );
			  }

			  public Path Build( Builder other )
			  {
					return new PathImpl( this, other );
			  }

			  public override string ToString()
			  {
					if ( Previous == null )
					{
						 return Start.ToString();
					}
					else
					{
						 return RelToString( Relationship ) + ":" + Previous.ToString();
					}
			  }
		 }

		 private static string RelToString( Relationship rel )
		 {
			  return rel.StartNode + "--" + rel.Type + "-->"
						+ rel.EndNode;
		 }

		 private readonly Node _start;
		 private readonly Relationship[] _path;
		 private readonly Node _end;

		 private PathImpl( Builder left, Builder right )
		 {
			  Node endNode = null;
			  _path = new Relationship[left.Size + ( right == null ? 0 : right.Size )];
			  if ( right != null )
			  {
					for ( int i = left.Size, total = i + right.Size; i < total; i++ )
					{
						 _path[i] = right.Relationship;
						 right = right.Previous;
					}
					Debug.Assert( right.Relationship == null, "right Path.Builder size error" );
					endNode = right.Start;
			  }

			  for ( int i = left.Size - 1; i >= 0; i-- )
			  {
					_path[i] = left.Relationship;
					left = left.Previous;
			  }
			  Debug.Assert( left.Relationship == null, "left Path.Builder size error" );
			  _start = left.Start;
			  _end = endNode;
		 }

		 public static Path Singular( Node start )
		 {
			  return ( new Builder( start ) ).Build();
		 }

		 public override Node StartNode()
		 {
			  return _start;
		 }

		 public override Node EndNode()
		 {
			  if ( _end != null )
			  {
					return _end;
			  }

			  // TODO We could really figure this out in the constructor
			  Node stepNode = null;
			  foreach ( Node node in Nodes() )
			  {
					stepNode = node;
			  }
			  return stepNode;
		 }

		 public override Relationship LastRelationship()
		 {
			  return _path != null && _path.Length > 0 ? _path[_path.Length - 1] : null;
		 }

		 public override IEnumerable<Node> Nodes()
		 {
			  return NodeIterator( _start, Relationships() );
		 }

		 public override IEnumerable<Node> ReverseNodes()
		 {
			  return NodeIterator( EndNode(), ReverseRelationships() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Iterable<org.Neo4Net.graphdb.Node> nodeIterator(final org.Neo4Net.graphdb.Node start, final Iterable<org.Neo4Net.graphdb.Relationship> relationships)
		 private IEnumerable<Node> NodeIterator( Node start, IEnumerable<Relationship> relationships )
		 {
			  return () => new IteratorAnonymousInnerClass(this, start, relationships);
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<Node>
		 {
			 private readonly PathImpl _outerInstance;

			 private Node _start;
			 private IEnumerable<Relationship> _relationships;

			 public IteratorAnonymousInnerClass( PathImpl outerInstance, Node start, IEnumerable<Relationship> relationships )
			 {
				 this.outerInstance = outerInstance;
				 this._start = start;
				 this._relationships = relationships;
				 current = start;
				 relationshipIterator = relationships.GetEnumerator();
			 }

			 internal Node current;
			 internal int index;
			 internal IEnumerator<Relationship> relationshipIterator;

			 public bool hasNext()
			 {
				  return index <= _outerInstance.path.Length;
			 }

			 public Node next()
			 {
				  if ( current == null )
				  {
						throw new NoSuchElementException();
				  }
				  Node next = null;
				  if ( index < _outerInstance.path.Length )
				  {
						if ( !relationshipIterator.hasNext() )
						{
							 throw new System.InvalidOperationException( string.Format( "Number of relationships: {0:D} does not" + " match with path length: {1:D}.", index, _outerInstance.path.Length ) );
						}
						next = relationshipIterator.next().getOtherNode(current);
				  }
				  index += 1;
				  try
				  {
						return current;
				  }
				  finally
				  {
						current = next;
				  }
			 }

			 public void remove()
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override IEnumerable<Relationship> Relationships()
		 {
			  return () => new ArrayIterator<Relationship>(_path);
		 }

		 public override IEnumerable<Relationship> ReverseRelationships()
		 {
			  return () => new ReverseArrayIterator<Relationship>(_path);
		 }

		 public override IEnumerator<PropertyContainer> Iterator()
		 {
			  return new IteratorAnonymousInnerClass2( this );
		 }

		 private class IteratorAnonymousInnerClass2 : IEnumerator<PropertyContainer>
		 {
			 private readonly PathImpl _outerInstance;

			 public IteratorAnonymousInnerClass2( PathImpl outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 current = outerInstance.Nodes().GetEnumerator();
				 next = outerInstance.Relationships().GetEnumerator();
			 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.graphdb.PropertyContainer> current;
			 internal IEnumerator<PropertyContainer> current;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.graphdb.PropertyContainer> next;
			 internal IEnumerator<PropertyContainer> next;

			 public bool hasNext()
			 {
				  return current.hasNext();
			 }

			 public IPropertyContainer next()
			 {
				  try
				  {
						return current.next();
				  }
				  finally
				  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends org.Neo4Net.graphdb.PropertyContainer> temp = current;
						IEnumerator<PropertyContainer> temp = current;
						current = next;
						next = temp;
				  }
			 }

			 public void remove()
			 {
				  next.remove();
			 }
		 }

		 public override int Length()
		 {
			  return _path.Length;
		 }

		 public override int GetHashCode()
		 {
			  if ( _path.Length == 0 )
			  {
					return _start.GetHashCode();
			  }
			  else
			  {
					return Arrays.GetHashCode( _path );
			  }
		 }

		 public override bool Equals( object obj )
		 {
			  if ( this == obj )
			  {
					return true;
			  }
			  else if ( obj is Path )
			  {
					Path other = ( Path ) obj;
					return _start.Equals( other.StartNode() ) && iteratorsEqual(this.Relationships().GetEnumerator(), other.Relationships().GetEnumerator());
			  }
			  else
			  {
					return false;
			  }
		 }

		 public override string ToString()
		 {
			  return Paths.defaultPathToString( this );
		 }
	}

}