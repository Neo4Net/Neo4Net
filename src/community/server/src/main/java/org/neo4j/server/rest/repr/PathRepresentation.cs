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

	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Neo4Net.Helpers.Collections;

	public class PathRepresentation<P> : ObjectRepresentation where P : Neo4Net.Graphdb.Path // implements
	{
																										  // ExtensibleRepresentation
		 private readonly P _path;

		 public PathRepresentation( P path ) : base( RepresentationType.Path )
		 {
			  this._path = path;
		 }

		 protected internal virtual P Path
		 {
			 get
			 {
				  return _path;
			 }
		 }

		 [Mapping("start")]
		 public virtual ValueRepresentation StartNode()
		 {
			  return ValueRepresentation.Uri( NodeRepresentation.Path( _path.startNode() ) );
		 }

		 [Mapping("end")]
		 public virtual ValueRepresentation EndNode()
		 {
			  return ValueRepresentation.Uri( NodeRepresentation.Path( _path.endNode() ) );
		 }

		 [Mapping("length")]
		 public virtual ValueRepresentation Length()
		 {
			  return ValueRepresentation.Number( _path.length() );
		 }

		 [Mapping("nodes")]
		 public virtual ListRepresentation Nodes()
		 {
			  return new ListRepresentation( RepresentationType.Node, new IterableWrapperAnonymousInnerClass( this, _path.nodes() ) );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, Node>
		 {
			 private readonly PathRepresentation<P> _outerInstance;

			 public IterableWrapperAnonymousInnerClass( PathRepresentation<P> outerInstance, IEnumerable<Node> nodes ) : base( nodes )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Representation underlyingObjectToObject( Node node )
			 {
				  return ValueRepresentation.Uri( NodeRepresentation.Path( node ) );
			 }
		 }

		 [Mapping("relationships")]
		 public virtual ListRepresentation Relationships()
		 {
			  return new ListRepresentation( RepresentationType.Relationship, new IterableWrapperAnonymousInnerClass2( this, _path.relationships() ) );
		 }

		 private class IterableWrapperAnonymousInnerClass2 : IterableWrapper<Representation, Relationship>
		 {
			 private readonly PathRepresentation<P> _outerInstance;

			 public IterableWrapperAnonymousInnerClass2( PathRepresentation<P> outerInstance, IEnumerable<Relationship> relationships ) : base( relationships )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Representation underlyingObjectToObject( Relationship node )
			 {
				  return ValueRepresentation.Uri( RelationshipRepresentation.Path( node ) );
			 }
		 }

		 [Mapping("directions")]
		 public virtual ListRepresentation Directions()
		 {
			  List<string> directionStrings = new List<string>();

			  IEnumerator<Node> nodeIterator = _path.nodes().GetEnumerator();
			  IEnumerator<Relationship> relationshipIterator = _path.relationships().GetEnumerator();

			  Relationship rel;
			  Node startNode;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Node endNode = nodeIterator.next();

			  while ( relationshipIterator.MoveNext() )
			  {
					rel = relationshipIterator.Current;
					startNode = endNode;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					endNode = nodeIterator.next();
					if ( rel.StartNode.Equals( startNode ) && rel.EndNode.Equals( endNode ) )
					{
						 directionStrings.Add( "->" );
					}
					else
					{
						 directionStrings.Add( "<-" );
					}
			  }

			  return new ListRepresentation( RepresentationType.String, new IterableWrapperAnonymousInnerClass3( this, directionStrings ) );
		 }

		 private class IterableWrapperAnonymousInnerClass3 : IterableWrapper<Representation, string>
		 {
			 private readonly PathRepresentation<P> _outerInstance;

			 public IterableWrapperAnonymousInnerClass3( PathRepresentation<P> outerInstance, List<string> directionStrings ) : base( directionStrings )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Representation underlyingObjectToObject( string directionString )
			 {
				  return ValueRepresentation.String( directionString );
			 }
		 }
	}

}