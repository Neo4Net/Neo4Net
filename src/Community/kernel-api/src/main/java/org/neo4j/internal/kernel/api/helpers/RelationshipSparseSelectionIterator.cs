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
namespace Neo4Net.@internal.Kernel.Api.helpers
{

	using Neo4Net.Graphdb;

	/// <summary>
	/// Helper iterator for traversing specific types and directions of a sparse node.
	/// </summary>
	public sealed class RelationshipSparseSelectionIterator<R> : RelationshipSparseSelection, ResourceIterator<R>
	{

		 private readonly RelationshipFactory<R> _factory;
		 private long _next;

		 internal RelationshipSparseSelectionIterator( RelationshipFactory<R> factory )
		 {
			  this._factory = factory;
			  this._next = RelationshipSelections.UNINITIALIZED;
		 }

		 public override bool HasNext()
		 {
			  if ( _next == RelationshipSelections.UNINITIALIZED )
			  {
					FetchNext();
					_next = Cursor.relationshipReference();
			  }

			  if ( _next == RelationshipSelections.NO_ID )
			  {
					Close();
					return false;
			  }
			  return true;
		 }

		 public override R Next()
		 {
			  if ( !HasNext() )
			  {
					throw new NoSuchElementException();
			  }
			  R current = _factory.relationship( _next, Cursor.sourceNodeReference(), Cursor.type(), Cursor.targetNodeReference() );

			  if ( !FetchNext() )
			  {
					Close();
					_next = RelationshipSelections.NO_ID;
			  }
			  else
			  {
					_next = Cursor.relationshipReference();
			  }

			  return current;
		 }
	}

}