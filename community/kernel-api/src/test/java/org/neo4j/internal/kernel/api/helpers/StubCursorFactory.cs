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
namespace Org.Neo4j.@internal.Kernel.Api.helpers
{


	public class StubCursorFactory : CursorFactory
	{
		 private readonly bool _continueWithLastItem;
		 private LinkedList<NodeCursor> _nodeCursors = new LinkedList<NodeCursor>();
		 private LinkedList<RelationshipScanCursor> _relationshipScanCursors = new LinkedList<RelationshipScanCursor>();
		 private LinkedList<RelationshipTraversalCursor> _relationshiTraversalCursors = new LinkedList<RelationshipTraversalCursor>();
		 private LinkedList<PropertyCursor> _propertyCursors = new LinkedList<PropertyCursor>();
		 private LinkedList<RelationshipGroupCursor> _groupCursors = new LinkedList<RelationshipGroupCursor>();
		 private LinkedList<NodeValueIndexCursor> _nodeValueIndexCursors = new LinkedList<NodeValueIndexCursor>();
		 private LinkedList<NodeLabelIndexCursor> _nodeLabelIndexCursors = new LinkedList<NodeLabelIndexCursor>();
		 private LinkedList<NodeExplicitIndexCursor> _nodeExplicitIndexCursors = new LinkedList<NodeExplicitIndexCursor>();
		 private LinkedList<RelationshipExplicitIndexCursor> _relationshipExplicitIndexCursors = new LinkedList<RelationshipExplicitIndexCursor>();

		 public StubCursorFactory() : this(false)
		 {
		 }

		 public StubCursorFactory( bool continueWithLastItem )
		 {
			  this._continueWithLastItem = continueWithLastItem;
		 }

		 public override NodeCursor AllocateNodeCursor()
		 {
			  return Poll( _nodeCursors );
		 }

		 public override RelationshipScanCursor AllocateRelationshipScanCursor()
		 {
			  return Poll( _relationshipScanCursors );
		 }

		 public override RelationshipTraversalCursor AllocateRelationshipTraversalCursor()
		 {
			  return Poll( _relationshiTraversalCursors );
		 }

		 public override PropertyCursor AllocatePropertyCursor()
		 {
			  return Poll( _propertyCursors );
		 }

		 public override RelationshipGroupCursor AllocateRelationshipGroupCursor()
		 {
			  return Poll( _groupCursors );
		 }

		 public override NodeValueIndexCursor AllocateNodeValueIndexCursor()
		 {
			  return Poll( _nodeValueIndexCursors );
		 }

		 public override NodeLabelIndexCursor AllocateNodeLabelIndexCursor()
		 {
			  return Poll( _nodeLabelIndexCursors );
		 }

		 public override NodeExplicitIndexCursor AllocateNodeExplicitIndexCursor()
		 {
			  return Poll( _nodeExplicitIndexCursors );
		 }

		 public override RelationshipExplicitIndexCursor AllocateRelationshipExplicitIndexCursor()
		 {
			  return Poll( _relationshipExplicitIndexCursors );
		 }

		 public virtual StubCursorFactory WithGroupCursors( params RelationshipGroupCursor[] cursors )
		 {
			  _groupCursors.addAll( Arrays.asList( cursors ) );
			  return this;
		 }

		 public virtual StubCursorFactory WithRelationshipTraversalCursors( params RelationshipTraversalCursor[] cursors )
		 {
			  _relationshiTraversalCursors.addAll( Arrays.asList( cursors ) );
			  return this;
		 }

		 private T Poll<T>( LinkedList<T> queue )
		 {
			  T poll = queue.RemoveFirst();
			  if ( _continueWithLastItem && queue.Count == 0 )
			  {
					queue.AddLast( poll );
			  }
			  return poll;
		 }
	}

}