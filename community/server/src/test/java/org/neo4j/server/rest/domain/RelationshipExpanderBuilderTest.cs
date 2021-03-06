﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.domain
{
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using MyRelTypes = Org.Neo4j.Kernel.impl.MyRelTypes;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.BranchState.NO_STATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Paths.singleNodePath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class RelationshipExpanderBuilderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static readonly DatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterpretNoSpecifiedRelationshipsAsAll()
		 public virtual void ShouldInterpretNoSpecifiedRelationshipsAsAll()
		 {
			  // GIVEN
			  Node node = CreateSomeData();
			  PathExpander expander = RelationshipExpanderBuilder.DescribeRelationships( map() );

			  // WHEN
			  ISet<Relationship> expanded;
			  using ( Transaction tx = Db.beginTx() )
			  {
					expanded = asSet( expander.expand( singleNodePath( node ), NO_STATE ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					// THEN
					assertEquals( asSet( node.Relationships ), expanded );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterpretSomeSpecifiedRelationships()
		 public virtual void ShouldInterpretSomeSpecifiedRelationships()
		 {
			  // GIVEN
			  Node node = CreateSomeData();
			  PathExpander expander = RelationshipExpanderBuilder.DescribeRelationships( map( "relationships", map( "type", MyRelTypes.TEST.name(), "direction", RelationshipDirection.Out.name() ) ) );

			  // WHEN
			  ISet<Relationship> expanded;
			  using ( Transaction tx = Db.beginTx() )
			  {
					expanded = asSet( expander.expand( singleNodePath( node ), NO_STATE ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					// THEN
					assertEquals( asSet( node.GetRelationships( MyRelTypes.TEST ) ), expanded );
					tx.Success();
			  }
		 }

		 private Node CreateSomeData()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.CreateRelationshipTo( Db.createNode(), MyRelTypes.TEST );
					node.CreateRelationshipTo( Db.createNode(), MyRelTypes.TEST2 );
					tx.Success();
					return node;
			  }
		 }
	}

}