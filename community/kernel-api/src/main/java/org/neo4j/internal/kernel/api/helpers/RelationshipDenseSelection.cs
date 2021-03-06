﻿using System;

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
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;


	/// <summary>
	/// Helper for traversing specific types and directions of a dense node.
	/// </summary>
	internal abstract class RelationshipDenseSelection
	{
		 private enum Dir
		 {
			  Out,
			  In,
			  Loop
		 }

		 private RelationshipGroupCursor _groupCursor;
		 protected internal RelationshipTraversalCursor RelationshipCursor;
		 private int[] _types;
		 private Dir[] _directions;
		 private int _currentDirection;
		 private int _nDirections;
		 private bool _onRelationship;
		 private bool _onGroup;
		 private int _foundTypes;

		 internal RelationshipDenseSelection()
		 {
			  this._directions = new Dir[3];
		 }

		 /// <summary>
		 /// Traverse all outgoing relationships including loops of the provided relationship types.
		 /// </summary>
		 /// <param name="groupCursor"> Group cursor to use. Pre-initialized on node. </param>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. </param>
		 public void Outgoing( RelationshipGroupCursor groupCursor, RelationshipTraversalCursor relationshipCursor )
		 {
			  Outgoing( groupCursor, relationshipCursor, null );
		 }

		 /// <summary>
		 /// Traverse all outgoing relationships including loops of the provided relationship types.
		 /// </summary>
		 /// <param name="groupCursor"> Group cursor to use. Pre-initialized on node. </param>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. </param>
		 /// <param name="types"> Relationship types to traverse </param>
		 public void Outgoing( RelationshipGroupCursor groupCursor, RelationshipTraversalCursor relationshipCursor, int[] types )
		 {
			  this._groupCursor = groupCursor;
			  this.RelationshipCursor = relationshipCursor;
			  this._types = types;
			  this._directions[0] = Dir.Out;
			  this._directions[1] = Dir.Loop;
			  this._nDirections = 2;
			  this._currentDirection = _directions.Length;
			  this._onRelationship = false;
			  this._onGroup = false;
			  this._foundTypes = 0;
		 }

		 /// <summary>
		 /// Traverse all incoming relationships including loops of the provided relationship types.
		 /// </summary>
		 /// <param name="groupCursor"> Group cursor to use. Pre-initialized on node. </param>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. </param>
		 public void Incoming( RelationshipGroupCursor groupCursor, RelationshipTraversalCursor relationshipCursor )
		 {
			  Incoming( groupCursor, relationshipCursor, null );
		 }

		 /// <summary>
		 /// Traverse all incoming relationships including loops of the provided relationship types.
		 /// </summary>
		 /// <param name="groupCursor"> Group cursor to use. Pre-initialized on node. </param>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. </param>
		 /// <param name="types"> Relationship types to traverse </param>
		 public void Incoming( RelationshipGroupCursor groupCursor, RelationshipTraversalCursor relationshipCursor, int[] types )
		 {
			  this._groupCursor = groupCursor;
			  this.RelationshipCursor = relationshipCursor;
			  this._types = types;
			  this._directions[0] = Dir.In;
			  this._directions[1] = Dir.Loop;
			  this._nDirections = 2;
			  this._currentDirection = _directions.Length;
			  this._onRelationship = false;
			  this._onGroup = false;
			  this._foundTypes = 0;
		 }

		 /// <summary>
		 /// Traverse all relationships of the provided relationship types.
		 /// </summary>
		 /// <param name="groupCursor"> Group cursor to use. Pre-initialized on node. </param>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. </param>
		 public void All( RelationshipGroupCursor groupCursor, RelationshipTraversalCursor relationshipCursor )
		 {
			  All( groupCursor, relationshipCursor, null );
		 }

		 /// <summary>
		 /// Traverse all relationships of the provided relationship types.
		 /// </summary>
		 /// <param name="groupCursor"> Group cursor to use. Pre-initialized on node. </param>
		 /// <param name="relationshipCursor"> Relationship traversal cursor to use. </param>
		 /// <param name="types"> Relationship types to traverse </param>
		 public void All( RelationshipGroupCursor groupCursor, RelationshipTraversalCursor relationshipCursor, int[] types )
		 {
			  this._groupCursor = groupCursor;
			  this.RelationshipCursor = relationshipCursor;
			  this._types = types;
			  this._directions[0] = Dir.Out;
			  this._directions[1] = Dir.In;
			  this._directions[2] = Dir.Loop;
			  this._nDirections = 3;
			  this._currentDirection = _directions.Length;
			  this._onRelationship = false;
			  this._onGroup = false;
			  this._foundTypes = 0;
		 }

		 /// <summary>
		 /// Fetch the next valid relationship.
		 /// </summary>
		 /// <returns> True is a valid relationship was found </returns>
		 protected internal virtual bool FetchNext()
		 {
			  if ( _onRelationship )
			  {
					_onRelationship = RelationshipCursor.next();
			  }

			  while ( !_onRelationship )
			  {
					_currentDirection++;
					if ( _currentDirection >= _nDirections )
					{
						 if ( _types != null && _foundTypes >= _types.Length )
						 {
							  _onGroup = false;
							  return false;
						 }

						 LoopOnRelationship();
					}

					if ( !_onGroup )
					{
						 return false;
					}

					SetupCursors();
			  }

			  return true;
		 }

		 private void LoopOnRelationship()
		 {
			  do
			  {
					_onGroup = _groupCursor.next();
			  } while ( _onGroup && !CorrectRelationshipType() );

			  if ( _onGroup )
			  {
					_foundTypes++;
					_currentDirection = 0;
			  }
		 }

		 private void SetupCursors()
		 {
			  Dir d = _directions[_currentDirection];

			  switch ( d )
			  {
			  case Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipDenseSelection.Dir.Out:
					_groupCursor.outgoing( RelationshipCursor );
					_onRelationship = RelationshipCursor.next();
					break;

			  case Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipDenseSelection.Dir.In:
					_groupCursor.incoming( RelationshipCursor );
					_onRelationship = RelationshipCursor.next();
					break;

			  case Org.Neo4j.@internal.Kernel.Api.helpers.RelationshipDenseSelection.Dir.Loop:
					_groupCursor.loops( RelationshipCursor );
					_onRelationship = RelationshipCursor.next();
					break;

			  default:
					throw new System.InvalidOperationException( "Lorem ipsus, Brutus. (could not setup cursor for Dir='" + d + "')" );
			  }
		 }

		 private bool CorrectRelationshipType()
		 {
			  return _types == null || ArrayUtils.contains( _types, _groupCursor.type() );
		 }

		 public virtual void Close()
		 {
			  Exception closeGroupError = null;
			  try
			  {
					if ( _groupCursor != null )
					{
						 _groupCursor.close();
					}
			  }
			  catch ( Exception t )
			  {
					closeGroupError = t;
			  }

			  try
			  {
					if ( RelationshipCursor != null )
					{
						 RelationshipCursor.close();
					}
			  }
			  catch ( Exception t )
			  {
					if ( closeGroupError != null )
					{
						 t.addSuppressed( closeGroupError );
					}
					throw t;
			  }
			  finally
			  {
					RelationshipCursor = null;
					_groupCursor = null;
			  }
		 }
	}

}