using System;

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
namespace Neo4Net.Kernel.Impl.Api
{
	using Neo4Net.Storageengine.Api;

	public class RelationshipDataExtractor : RelationshipVisitor<Exception>
	{
		 private int _type;
		 private long _startNode;
		 private long _endNode;
		 private long _relId;

		 public override void Visit( long relId, int type, long startNode, long endNode )
		 {
			  this._relId = relId;
			  this._type = type;
			  this._startNode = startNode;
			  this._endNode = endNode;
		 }

		 public virtual int Type()
		 {
			  return _type;
		 }

		 public virtual long StartNode()
		 {
			  return _startNode;
		 }

		 public virtual long EndNode()
		 {
			  return _endNode;
		 }

		 public virtual long OtherNode( long node )
		 {
			  if ( node == _startNode )
			  {
					return _endNode;
			  }
			  else if ( node == _endNode )
			  {
					return _startNode;
			  }
			  else
			  {
					throw new System.ArgumentException( "Node[" + node + "] is neither start nor end node of relationship[" + _relId + "]" );
			  }
		 }

		 public virtual long Relationship()
		 {
			  return _relId;
		 }
	}

}