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
namespace Org.Neo4j.Graphdb.impl
{

	using Org.Neo4j.Helpers.Collection;

	public class ExtendedPath : Path
	{
		 private readonly Path _start;
		 private readonly Relationship _lastRelationship;
		 private readonly Node _endNode;

		 public ExtendedPath( Path start, Relationship lastRelationship )
		 {
			  this._start = start;
			  this._lastRelationship = lastRelationship;
			  this._endNode = lastRelationship.GetOtherNode( start.EndNode() );
		 }

		 public override Node StartNode()
		 {
			  return _start.startNode();
		 }

		 public override Node EndNode()
		 {
			  return _endNode;
		 }

		 public override Relationship LastRelationship()
		 {
			  return _lastRelationship;
		 }

		 public override IEnumerable<Relationship> Relationships()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<Relationship>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startRelationships = outerInstance.start.Relationships().GetEnumerator();
			 }

			 internal readonly IEnumerator<Relationship> startRelationships;
			 internal bool lastReturned;

			 protected internal override Relationship fetchNextOrNull()
			 {
				  if ( startRelationships.hasNext() )
				  {
						return startRelationships.next();
				  }
				  if ( !lastReturned )
				  {
						lastReturned = true;
						return _outerInstance.lastRelationship;
				  }
				  return null;
			 }
		 }

		 public override IEnumerable<Relationship> ReverseRelationships()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass2(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass2 : PrefetchingIterator<Relationship>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass2( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startRelationships = outerInstance.start.ReverseRelationships().GetEnumerator();
			 }

			 internal readonly IEnumerator<Relationship> startRelationships;
			 internal bool endReturned;

			 protected internal override Relationship fetchNextOrNull()
			 {
				  if ( !endReturned )
				  {
						endReturned = true;
						return _outerInstance.lastRelationship;
				  }
				  return startRelationships.hasNext() ? startRelationships.next() : null;
			 }
		 }

		 public override IEnumerable<Node> Nodes()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass3(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass3 : PrefetchingIterator<Node>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass3( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startNodes = outerInstance.start.Nodes().GetEnumerator();
			 }

			 internal readonly IEnumerator<Node> startNodes;
			 internal bool lastReturned;

			 protected internal override Node fetchNextOrNull()
			 {
				  if ( startNodes.hasNext() )
				  {
						return startNodes.next();
				  }
				  if ( !lastReturned )
				  {
						lastReturned = true;
						return _outerInstance.endNode;
				  }
				  return null;
			 }
		 }

		 public override IEnumerable<Node> ReverseNodes()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass4(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass4 : PrefetchingIterator<Node>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass4( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startNodes = outerInstance.start.ReverseNodes().GetEnumerator();
			 }

			 internal readonly IEnumerator<Node> startNodes;
			 internal bool endReturned;

			 protected internal override Node fetchNextOrNull()
			 {
				  if ( !endReturned )
				  {
						endReturned = true;
						return _outerInstance.endNode;
				  }
				  return startNodes.hasNext() ? startNodes.next() : null;
			 }
		 }

		 public override int Length()
		 {
			  return _start.length() + 1;
		 }

		 public override IEnumerator<PropertyContainer> Iterator()
		 {
			  return new PrefetchingIteratorAnonymousInnerClass5( this );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass5 : PrefetchingIterator<PropertyContainer>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass5( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startEntities = outerInstance.start.GetEnumerator();
				 lastReturned = 2;
			 }

			 internal readonly IEnumerator<PropertyContainer> startEntities;
			 internal int lastReturned;

			 protected internal override PropertyContainer fetchNextOrNull()
			 {
				  if ( startEntities.hasNext() )
				  {
						return startEntities.next();
				  }
				  switch ( lastReturned-- )
				  {
				  case 2:
					  return _outerInstance.endNode;
				  case 1:
					  return _outerInstance.lastRelationship;
				  default:
					  return null;
				  }
			 }
		 }

		 /// <summary>
		 /// Appends a <seealso cref="Relationship relationship"/>, {@code withRelationship}, to the specified <seealso cref="Path path"/> </summary>
		 /// <param name="path"> </param>
		 /// <param name="withRelationship"> </param>
		 /// <returns> The path with the relationship and its end node appended. </returns>
		 public static Path Extend( Path path, Relationship withRelationship )
		 {
			  return new ExtendedPath( path, withRelationship );
		 }
	}

}