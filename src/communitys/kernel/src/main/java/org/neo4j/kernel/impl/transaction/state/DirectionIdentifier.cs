using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using DirectionWrapper = Neo4Net.Kernel.impl.util.DirectionWrapper;

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