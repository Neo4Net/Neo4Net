﻿/*
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
	/// <summary>
	/// Helper cursor for traversing specific types and directions of a dense node.
	/// </summary>
	public sealed class RelationshipSparseSelectionCursor : RelationshipSparseSelection, RelationshipSelectionCursor
	{
		 public override bool Next()
		 {
			  if ( !FetchNext() )
			  {
					Close();
					return false;
			  }
			  return true;
		 }

		 public override long RelationshipReference()
		 {
			  return Cursor.relationshipReference();
		 }

		 public override int Type()
		 {
			  return Cursor.type();
		 }

		 public override long OtherNodeReference()
		 {
			  return Cursor.originNodeReference() == Cursor.sourceNodeReference() ? Cursor.targetNodeReference() : Cursor.sourceNodeReference();
		 }

		 public override long SourceNodeReference()
		 {
			  return Cursor.sourceNodeReference();
		 }

		 public override long TargetNodeReference()
		 {
			  return Cursor.targetNodeReference();
		 }

		 public override long PropertiesReference()
		 {
			  return Cursor.propertiesReference();
		 }
	}

}