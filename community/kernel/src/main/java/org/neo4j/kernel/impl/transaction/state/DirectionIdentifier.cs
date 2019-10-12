using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using DirectionWrapper = Org.Neo4j.Kernel.impl.util.DirectionWrapper;

	public class DirectionIdentifier
	{
		 private DirectionIdentifier()
		 {
		 }

		 public static DirectionWrapper WrapDirection( RelationshipRecord rel, NodeRecord startNode )
		 {
			  bool isOut = rel.FirstNode == startNode.Id;
			  bool isIn = rel.SecondNode == startNode.Id;
			  Debug.Assert( isOut | isIn );
			  if ( isOut & isIn )
			  {
					return DirectionWrapper.BOTH;
			  }
			  return isOut ? DirectionWrapper.OUTGOING : DirectionWrapper.INCOMING;
		 }
	}

}