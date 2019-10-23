﻿using System.Collections.Generic;

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
namespace Neo4Net.GraphDb.impl
{

	using Neo4Net.Helpers.Collections;

	public class ExtendedPath : IPath
	{
		 private readonly IPath _start;
		 private readonly IRelationship _lastRelationship;
		 private readonly INode _endNode;

		 public ExtendedPath( IPath start, IRelationship lastRelationship )
		 {
			  this._start = start;
			  this._lastRelationship = lastRelationship;
			  this._endNode = lastRelationship.GetOtherNode( start.EndNode() );
		 }

		 public override INode StartNode()
		 {
			  return _start.startNode();
		 }

		 public override INode EndNode()
		 {
			  return _endNode;
		 }

		 public override IRelationship LastRelationship()
		 {
			  return _lastRelationship;
		 }

		 public override IEnumerable<IRelationship> Relationships()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<IRelationship>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startRelationships = outerInstance.start.Relationships().GetEnumerator();
			 }

			 internal readonly IEnumerator<IRelationship> startRelationships;
			 internal bool lastReturned;

			 protected internal override IRelationship fetchNextOrNull()
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

		 public override IEnumerable<IRelationship> ReverseRelationships()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass2(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass2 : PrefetchingIterator<IRelationship>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass2( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startRelationships = outerInstance.start.ReverseRelationships().GetEnumerator();
			 }

			 internal readonly IEnumerator<IRelationship> startRelationships;
			 internal bool endReturned;

			 protected internal override IRelationship fetchNextOrNull()
			 {
				  if ( !endReturned )
				  {
						endReturned = true;
						return _outerInstance.lastRelationship;
				  }
				  return startRelationships.hasNext() ? startRelationships.next() : null;
			 }
		 }

		 public override IEnumerable<INode> Nodes()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass3(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass3 : PrefetchingIterator<INode>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass3( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startNodes = outerInstance.start.Nodes().GetEnumerator();
			 }

			 internal readonly IEnumerator<INode> startNodes;
			 internal bool lastReturned;

			 protected internal override INode fetchNextOrNull()
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

		 public override IEnumerable<INode> ReverseNodes()
		 {
			  return () => new PrefetchingIteratorAnonymousInnerClass4(this);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass4 : PrefetchingIterator<INode>
		 {
			 private readonly ExtendedPath _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass4( ExtendedPath outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 startNodes = outerInstance.start.ReverseNodes().GetEnumerator();
			 }

			 internal readonly IEnumerator<INode> startNodes;
			 internal bool endReturned;

			 protected internal override INode fetchNextOrNull()
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

			 protected internal override IPropertyContainer fetchNextOrNull()
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
		 /// Appends a <seealso cref="IRelationship relationship"/>, {@code withRelationship}, to the specified <seealso cref="IPath path"/> </summary>
		 /// <param name="path"> </param>
		 /// <param name="withRelationship"> </param>
		 /// <returns> The path with the relationship and its end node appended. </returns>
		 public static IPath Extend( IPath path, IRelationship withRelationship )
		 {
			  return new ExtendedPath( path, withRelationship );
		 }
	}

}