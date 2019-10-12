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
namespace Org.Neo4j.@internal.Kernel.Api
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ManagedTestCursors : CursorFactory
	{
		 private IList<Cursor> _allCursors = new List<Cursor>();

		 private CursorFactory _cursors;

		 internal ManagedTestCursors( CursorFactory c )
		 {
			  this._cursors = c;
		 }

		 internal virtual void AssertAllClosedAndReset()
		 {
			  foreach ( Cursor n in _allCursors )
			  {
					if ( !n.Closed )
					{
						 fail( "The Cursor " + n.ToString() + " was not closed properly." );
					}
			  }

			  _allCursors.Clear();
		 }

		 public override NodeCursor AllocateNodeCursor()
		 {
			  NodeCursor n = _cursors.allocateNodeCursor();
			  _allCursors.Add( n );
			  return n;
		 }

		 public override RelationshipScanCursor AllocateRelationshipScanCursor()
		 {
			  RelationshipScanCursor n = _cursors.allocateRelationshipScanCursor();
			  _allCursors.Add( n );
			  return n;
		 }

		 public override RelationshipTraversalCursor AllocateRelationshipTraversalCursor()
		 {
			  RelationshipTraversalCursor n = _cursors.allocateRelationshipTraversalCursor();
			  _allCursors.Add( n );
			  return n;
		 }

		 public override PropertyCursor AllocatePropertyCursor()
		 {
			  PropertyCursor n = _cursors.allocatePropertyCursor();
			  _allCursors.Add( n );
			  return n;
		 }

		 public override RelationshipGroupCursor AllocateRelationshipGroupCursor()
		 {
			  RelationshipGroupCursor n = _cursors.allocateRelationshipGroupCursor();
			  _allCursors.Add( n );
			  return n;
		 }

		 public override NodeValueIndexCursor AllocateNodeValueIndexCursor()
		 {
			  NodeValueIndexCursor n = _cursors.allocateNodeValueIndexCursor();
			  _allCursors.Add( n );
			  return n;
		 }

		 public override NodeLabelIndexCursor AllocateNodeLabelIndexCursor()
		 {
			  NodeLabelIndexCursor n = _cursors.allocateNodeLabelIndexCursor();
			  _allCursors.Add( n );
			  return n;
		 }

		 public override NodeExplicitIndexCursor AllocateNodeExplicitIndexCursor()
		 {
			  NodeExplicitIndexCursor n = _cursors.allocateNodeExplicitIndexCursor();
			  _allCursors.Add( n );
			  return n;
		 }

		 public override RelationshipExplicitIndexCursor AllocateRelationshipExplicitIndexCursor()
		 {
			  RelationshipExplicitIndexCursor n = _cursors.allocateRelationshipExplicitIndexCursor();
			  _allCursors.Add( n );
			  return n;
		 }
	}

}