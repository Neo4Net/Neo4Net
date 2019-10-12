using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
namespace Org.Neo4j.Kernel.impl.core
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Paths = Org.Neo4j.Graphdb.traversal.Paths;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iteratorsEqual;

	public class PathProxy : Path
	{
		 private readonly EmbeddedProxySPI _proxySPI;
		 private readonly long[] _nodes;
		 private readonly long[] _relationships;
		 private readonly int[] _directedTypes;

		 /// <param name="proxySPI">
		 ///         the API into the kernel. </param>
		 /// <param name="nodes">
		 ///         the ids of the nodes in the path, in order. </param>
		 /// <param name="relationships">
		 ///         the ids of the relationships in the path, in order. </param>
		 /// <param name="directedTypes">
		 ///         an encoding of the types and directions of the relationships.
		 ///         An entry at position {@code i} of this array should be {@code typeId} if the relationship at {@code i}
		 ///         has its start node at {@code i} and its end node at {@code i + 1}, and should be {@code ~typeId} if the
		 ///         relationship at {@code i} has its start node at {@code i + 1} and its end node at {@code i}. </param>
		 public PathProxy( EmbeddedProxySPI proxySPI, long[] nodes, long[] relationships, int[] directedTypes )
		 {
			  Debug.Assert( nodes.Length == relationships.Length + 1 );
			  Debug.Assert( relationships.Length == directedTypes.Length );
			  this._proxySPI = proxySPI;
			  this._nodes = nodes;
			  this._relationships = relationships;
			  this._directedTypes = directedTypes;
		 }

		 public override string ToString()
		 {
			  StringBuilder @string = new StringBuilder();
			  @string.Append( '(' ).Append( _nodes[0] ).Append( ')' );
			  bool inTx = true;
			  for ( int i = 0; i < _relationships.Length; i++ )
			  {
					int type = _directedTypes[i];
					@string.Append( type < 0 ? "<-[" : "-[" );
					@string.Append( _relationships[i] );
					if ( inTx )
					{
						 try
						 {
							  string name = _proxySPI.getRelationshipTypeById( type < 0 ?~type : type ).name();
							  @string.Append( ':' ).Append( name );
						 }
						 catch ( Exception )
						 {
							  inTx = false;
						 }
					}
					@string.Append( type < 0 ? "]-(" : "]->(" ).Append( _nodes[i + 1] ).Append( ')' );
			  }
			  return @string.ToString();
		 }

		 public override int GetHashCode()
		 {
			  if ( _relationships.Length == 0 )
			  {
					return Long.GetHashCode( _nodes[0] );
			  }
			  else
			  {
					return Arrays.GetHashCode( _relationships );
			  }
		 }

		 public override bool Equals( object obj )
		 {
			  if ( this == obj )
			  {
					return true;
			  }
			  if ( obj is PathProxy )
			  {
					PathProxy that = ( PathProxy ) obj;
					return Arrays.Equals( this._nodes, that._nodes ) && Arrays.Equals( this._relationships, that._relationships );
			  }
			  else if ( obj is Path )
			  {
					Path other = ( Path ) obj;
					if ( _nodes[0] != other.StartNode().Id )
					{
						 return false;
					}
					return iteratorsEqual( this.Relationships().GetEnumerator(), other.Relationships().GetEnumerator() );
			  }
			  else
			  {
					return false;
			  }
		 }

		 public override Node StartNode()
		 {
			  return new NodeProxy( _proxySPI, _nodes[0] );
		 }

		 public override Node EndNode()
		 {
			  return new NodeProxy( _proxySPI, _nodes[_nodes.Length - 1] );
		 }

		 public override Relationship LastRelationship()
		 {
			  return _relationships.Length == 0 ? null : Relationship( _relationships.Length - 1 );
		 }

		 private RelationshipProxy Relationship( int offset )
		 {
			  int type = _directedTypes[offset];
			  if ( type >= 0 )
			  {
					return new RelationshipProxy( _proxySPI, _relationships[offset], _nodes[offset], type, _nodes[offset + 1] );
			  }
			  else
			  {
					return new RelationshipProxy( _proxySPI, _relationships[offset], _nodes[offset + 1], ~type, _nodes[offset] );
			  }
		 }

		 public override IEnumerable<Relationship> Relationships()
		 {
			  return () => new IteratorAnonymousInnerClass(this);
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<Relationship>
		 {
			 private readonly PathProxy _outerInstance;

			 public IteratorAnonymousInnerClass( PathProxy outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal int i;

			 public bool hasNext()
			 {
				  return i < _outerInstance.relationships.Length;
			 }

			 public Relationship next()
			 {
				  return outerInstance.relationship( i++ );
			 }
		 }

		 public override IEnumerable<Relationship> ReverseRelationships()
		 {
			  return () => new IteratorAnonymousInnerClass2(this);
		 }

		 private class IteratorAnonymousInnerClass2 : IEnumerator<Relationship>
		 {
			 private readonly PathProxy _outerInstance;

			 public IteratorAnonymousInnerClass2( PathProxy outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 i = outerInstance.relationships.Length;
			 }

			 internal int i;

			 public bool hasNext()
			 {
				  return i > 0;
			 }

			 public Relationship next()
			 {
				  return outerInstance.relationship( --i );
			 }
		 }

		 public override IEnumerable<Node> Nodes()
		 {
			  return () => new IteratorAnonymousInnerClass3(this);
		 }

		 private class IteratorAnonymousInnerClass3 : IEnumerator<Node>
		 {
			 private readonly PathProxy _outerInstance;

			 public IteratorAnonymousInnerClass3( PathProxy outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal int i;

			 public bool hasNext()
			 {
				  return i < _outerInstance.nodes.Length;
			 }

			 public Node next()
			 {
				  return new NodeProxy( _outerInstance.proxySPI, _outerInstance.nodes[i++] );
			 }
		 }

		 public override IEnumerable<Node> ReverseNodes()
		 {
			  return () => new IteratorAnonymousInnerClass4(this);
		 }

		 private class IteratorAnonymousInnerClass4 : IEnumerator<Node>
		 {
			 private readonly PathProxy _outerInstance;

			 public IteratorAnonymousInnerClass4( PathProxy outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 i = outerInstance.nodes.Length;
			 }

			 internal int i;

			 public bool hasNext()
			 {
				  return i > 0;
			 }

			 public Node next()
			 {
				  return new NodeProxy( _outerInstance.proxySPI, _outerInstance.nodes[--i] );
			 }
		 }

		 public override int Length()
		 {
			  return _relationships.Length;
		 }

		 public override IEnumerator<PropertyContainer> Iterator()
		 {
			  return new IteratorAnonymousInnerClass5( this );
		 }

		 private class IteratorAnonymousInnerClass5 : IEnumerator<PropertyContainer>
		 {
			 private readonly PathProxy _outerInstance;

			 public IteratorAnonymousInnerClass5( PathProxy outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal int i;
			 internal bool relationship;

			 public bool hasNext()
			 {
				  return i < _outerInstance.relationships.Length || !relationship;
			 }

			 public PropertyContainer next()
			 {
				  if ( relationship )
				  {
						relationship = false;
						return outerInstance.relationship( i++ );
				  }
				  else
				  {
						relationship = true;
						return new NodeProxy( _outerInstance.proxySPI, _outerInstance.nodes[i] );
				  }
			 }
		 }
	}

}