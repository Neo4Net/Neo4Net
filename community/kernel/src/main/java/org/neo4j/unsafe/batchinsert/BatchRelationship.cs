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
namespace Org.Neo4j.@unsafe.Batchinsert
{
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

	/// <summary>
	/// Simple relationship wrapping start node id, end node id and relationship
	/// type.
	/// </summary>
	public sealed class BatchRelationship
	{
		 private readonly long _id;
		 private readonly long _startNodeId;
		 private readonly long _endNodeId;
		 private readonly RelationshipType _type;

		 public BatchRelationship( long id, long startNodeId, long endNodeId, RelationshipType type )
		 {
			  this._id = id;
			  this._startNodeId = startNodeId;
			  this._endNodeId = endNodeId;
			  this._type = type;
		 }

		 public long Id
		 {
			 get
			 {
				  return _id; // & 0xFFFFFFFFL;
			 }
		 }

		 public long StartNode
		 {
			 get
			 {
				  return _startNodeId; // & 0xFFFFFFFFL;
			 }
		 }

		 public long EndNode
		 {
			 get
			 {
				  return _endNodeId; // & 0xFFFFFFFFL;
			 }
		 }

		 public RelationshipType Type
		 {
			 get
			 {
				  return _type;
			 }
		 }
	}

}