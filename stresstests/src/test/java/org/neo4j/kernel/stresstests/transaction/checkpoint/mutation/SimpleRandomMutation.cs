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
namespace Org.Neo4j.Kernel.stresstests.transaction.checkpoint.mutation
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;

	internal class SimpleRandomMutation : RandomMutation
	{
		 private readonly long _nodeCount;
		 private readonly GraphDatabaseService _db;
		 private readonly Mutation _rareMutation;
		 private readonly Mutation _commonMutation;

		 internal SimpleRandomMutation( long nodeCount, GraphDatabaseService db, Mutation rareMutation, Mutation commonMutation )
		 {
			  this._nodeCount = nodeCount;
			  this._db = db;
			  this._rareMutation = rareMutation;
			  this._commonMutation = commonMutation;
		 }

		 private static string[] _names = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s" };

		 public override void Perform()
		 {
			  ThreadLocalRandom rng = ThreadLocalRandom.current();
			  long nodeId = rng.nextLong( _nodeCount );
			  string value = _names[rng.Next( _names.Length )];

			  if ( rng.NextDouble() < 0.01 )
			  {
					_rareMutation.perform( nodeId, value );
			  }
			  else
			  {
					_commonMutation.perform( nodeId, value );
			  }
		 }
	}

}