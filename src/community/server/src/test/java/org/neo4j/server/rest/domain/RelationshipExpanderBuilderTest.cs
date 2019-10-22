using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.domain
{
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;

	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.BranchState.NO_STATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Paths.singleNodePath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;

	public class RelationshipExpanderBuilderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
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