﻿/*
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
namespace Org.Neo4j.Kernel.stresstests.transaction.checkpoint.mutation
{
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;

	/// <summary>
	/// Created by davide on 22/06/15.
	/// </summary>
	internal class PropertyMutation : Mutation
	{
		 private readonly GraphDatabaseService _db;

		 internal PropertyMutation( GraphDatabaseService db )
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