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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;

	public class CommandState
	{
		 internal readonly Index<Node> Index;
		 internal readonly GraphDatabaseService GraphDb;
		 public volatile Transaction Tx;
		 public volatile bool Alive = true;
		 public volatile Node Node;

		 public CommandState( Index<Node> index, GraphDatabaseService graphDb, Node node )
		 {
			  this.Index = index;
			  this.GraphDb = graphDb;
			  this.Node = node;
		 }
	}

}