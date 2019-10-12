using System.Collections.Generic;

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
namespace Org.Neo4j.Cypher.export
{
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;

	/// <summary>
	/// @author mh
	/// @since 18.02.13
	/// </summary>
	public class DatabaseSubGraph : SubGraph
	{
		 private readonly GraphDatabaseService _gdb;

		 public DatabaseSubGraph( GraphDatabaseService gdb )
		 {
			  this._gdb = gdb;
		 }

		 public static SubGraph From( GraphDatabaseService gdb )
		 {
			  return new DatabaseSubGraph( gdb );
		 }

		 public virtual IEnumerable<Node> Nodes
		 {
			 get
			 {
				  return _gdb.AllNodes;
			 }
		 }

		 public virtual IEnumerable<Relationship> Relationships
		 {
			 get
			 {
				  return _gdb.AllRelationships;
			 }
		 }

		 public override bool Contains( Relationship relationship )
		 {
			  return relationship.GraphDatabase.Equals( _gdb );
		 }

		 public virtual IEnumerable<IndexDefinition> Indexes
		 {
			 get
			 {
				  return _gdb.schema().Indexes;
			 }
		 }

		 public virtual IEnumerable<ConstraintDefinition> Constraints
		 {
			 get
			 {
				  return _gdb.schema().Constraints;
			 }
		 }
	}

}