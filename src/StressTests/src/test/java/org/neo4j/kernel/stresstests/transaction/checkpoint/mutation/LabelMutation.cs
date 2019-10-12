/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.stresstests.transaction.checkpoint.mutation
{
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;

	internal class LabelMutation : Mutation
	{
		 private readonly GraphDatabaseService _db;

		 internal LabelMutation( GraphDatabaseService db )
		 {
			  this._db = db;
		 }

		 public override void Perform( long nodeId, string value )
		 {
			  Node node = _db.getNodeById( nodeId );
			  Label label = Label.label( value );
			  if ( node.HasLabel( label ) )
			  {
					node.RemoveLabel( label );
			  }
			  else
			  {
					node.AddLabel( label );
			  }
		 }
	}

}