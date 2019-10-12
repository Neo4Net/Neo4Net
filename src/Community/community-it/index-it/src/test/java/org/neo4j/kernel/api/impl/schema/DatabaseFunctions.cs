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
namespace Neo4Net.Kernel.Api.Impl.Schema
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;

	public sealed class DatabaseFunctions
	{
		 private DatabaseFunctions()
		 {
			  throw new AssertionError( "Not for instantiation!" );
		 }

		 public static System.Func<GraphDatabaseService, Node> CreateNode()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return GraphDatabaseService::createNode;
		 }

		 public static System.Func<Node, Node> AddLabel( Label label )
		 {
			  return node =>
			  {
				node.addLabel( label );
				return node;
			  };
		 }

		 public static System.Func<Node, Node> SetProperty( string propertyKey, object value )
		 {
			  return node =>
			  {
				node.setProperty( propertyKey, value );
				return node;
			  };
		 }

		 public static System.Func<GraphDatabaseService, Void> Index( Label label, string propertyKey )
		 {
			  return graphDb =>
			  {
				graphDb.schema().indexFor(label).on(propertyKey).create();
				return null;
			  };
		 }

		 public static System.Func<GraphDatabaseService, Void> UniquenessConstraint( Label label, string propertyKey )
		 {
			  return graphDb =>
			  {
				graphDb.schema().constraintFor(label).assertPropertyIsUnique(propertyKey).create();
				return null;
			  };
		 }

		 public static System.Func<GraphDatabaseService, Void> AwaitIndexesOnline( long timeout, TimeUnit unit )
		 {
			  return graphDb =>
			  {
				graphDb.schema().awaitIndexesOnline(timeout, unit);
				return null;
			  };
		 }
	}

}