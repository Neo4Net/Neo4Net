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
namespace Neo4Net.@internal.Kernel.Api.helpers
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;

	/// <summary>
	/// Helper for traversing specific types and directions of a sparse node.
	/// </summary>
	internal abstract class RelationshipSparseSelection
	{
		 private enum Dir
		 {
			  Out,
			  In,
			  Both
		 }

		 protected internal RelationshipTraversalCursor Cursor;
		 private int[] _types;
		 private Dir _targetDirection;
		 private bool _onRelationship;
		 private bool _firstNext;

		 /// <summary>
		 /// Traverse all outgoing relationships including loops of the provided relationship types.
		 /// </summary>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. Pre-initialized on node. </param>
		 public void Outgoing( RelationshipTraversalCursor relationshipCursor )
		 {
			  Init( relationshipCursor, null, Dir.Out );
		 }

		 /// <summary>
		 /// Traverse all outgoing relationships including loops of the provided relationship types.
		 /// </summary>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. Pre-initialized on node. </param>
		 /// <param name="types"> Relationship types to traverse </param>
		 public void Outgoing( RelationshipTraversalCursor relationshipCursor, int[] types )
		 {
			  Init( relationshipCursor, types, Dir.Out );
		 }

		 /// <summary>
		 /// Traverse all incoming relationships including loops of the provided relationship types.
		 /// </summary>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. Pre-initialized on node. </param>
		 public void Incoming( RelationshipTraversalCursor relationshipCursor )
		 {
			  Init( relationshipCursor, null, Dir.In );
		 }

		 /// <summary>
		 /// Traverse all incoming relationships including loops of the provided relationship types.
		 /// </summary>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. Pre-initialized on node. </param>
		 /// <param name="types"> Relationship types to traverse </param>
		 public void Incoming( RelationshipTraversalCursor relationshipCursor, int[] types )
		 {
			  Init( relationshipCursor, types, Dir.In );
		 }

		 /// <summary>
		 /// Traverse all relationships of the provided relationship types.
		 /// </summary>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. Pre-initialized on node. </param>
		 public void All( RelationshipTraversalCursor relationshipCursor )
		 {
			  Init( relationshipCursor, null, Dir.Both );
		 }

		 /// <summary>
		 /// Traverse all relationships of the provided relationship types.
		 /// </summary>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. Pre-initialized on node. </param>
		 /// <param name="types"> Relationship types to traverse </param>
		 public void All( RelationshipTraversalCursor relationshipCursor, int[] types )
		 {
			  Init( relationshipCursor, types, Dir.Both );
		 }

		 private void Init( RelationshipTraversalCursor relationshipCursor, int[] types, Dir targetDirection )
		 {
			  this.Cursor = relationshipCursor;
			  this._types = types;
			  this._targetDirection = targetDirection;
			  this._onRelationship = false;
			  this._firstNext = true;
		 }

		 /// <summary>
		 /// Fetch the next valid relationship.
		 /// </summary>
		 /// <returns> True is a valid relationship was found </returns>
		 protected internal virtual bool FetchNext()
		 {
			  if ( _onRelationship || _firstNext )
			  {
					_firstNext = false;
					do
					{
						 _onRelationship = Cursor.next();
					} while ( _onRelationship && ( !CorrectDirection() || !CorrectType() ) );
			  }

			  return _onRelationship;
		 }

		 private bool CorrectDirection()
		 {
			  return _targetDirection == Dir.Both || ( _targetDirection == Dir.Out && Cursor.originNodeReference() == Cursor.sourceNodeReference() ) || (_targetDirection == Dir.In && Cursor.originNodeReference() == Cursor.targetNodeReference());
		 }

		 private bool CorrectType()
		 {
			  return _types == null || ArrayUtils.contains( _types, Cursor.type() );
		 }

		 public virtual void Close()
		 {
			  try
			  {
					if ( Cursor != null )
					{
						 Cursor.close();
					}
			  }
			  finally
			  {
					Cursor = null;
			  }
		 }
	}

}