/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.stresstests.transaction.checkpoint.mutation
{
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;

	/// <summary>
	/// Created by davide on 22/06/15.
	/// </summary>
	internal class PropertyMutation : Mutation
	{
		 private readonly IGraphDatabaseService _db;

		 internal PropertyMutation( IGraphDatabaseService db )
		 {
			  this._db = db;
		 }

		 public override void Perform( long nodeId, string value )
		 {
			  Node node = _db.getNodeById( nodeId );
			  if ( node.HasProperty( value ) )
			  {
					node.RemoveProperty( value );
			  }
			  else
			  {
					node.SetProperty( value, 10 );
			  }
		 }
	}

}