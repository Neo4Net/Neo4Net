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
namespace Org.Neo4j.Metatest
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Graphdb.index;
	using GraphDescription = Org.Neo4j.Test.GraphDescription;
	using Graph = Org.Neo4j.Test.GraphDescription.Graph;
	using NODE = Org.Neo4j.Test.GraphDescription.NODE;
	using PROP = Org.Neo4j.Test.GraphDescription.PROP;
	using REL = Org.Neo4j.Test.GraphDescription.REL;
	using GraphHolder = Org.Neo4j.Test.GraphHolder;
	using Org.Neo4j.Test;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class TestGraphDescription : GraphHolder
	{
		private bool InstanceFieldsInitialized = false;

		public TestGraphDescription()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Data = TestData.producedThrough( GraphDescription.createGraphFor( this, true ) );
		}

		 private static GraphDatabaseService _graphdb;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.TestData<java.util.Map<String,org.neo4j.graphdb.Node>> data = org.neo4j.test.TestData.producedThrough(org.neo4j.test.GraphDescription.createGraphFor(this, true));
		 public TestData<IDictionary<string, Node>> Data;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void havingNoGraphAnnotationCreatesAnEmptyDataCollection()
		 public virtual void HavingNoGraphAnnotationCreatesAnEmptyDataCollection()
		 {
			  assertTrue( "collection was not empty", Data.get().Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph("I know you") public void canCreateGraphFromSingleString()
		 [Graph("I know you")]
		 public virtual void CanCreateGraphFromSingleString()
		 {
			  VerifyIKnowYou( "know", "I" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph({ "a TO b", "b TO c", "c TO a" }) public void canCreateGraphFromMultipleStrings()
		 [Graph({ "a TO b", "b TO c", "c TO a" })]
		 public virtual void CanCreateGraphFromMultipleStrings()
		 {
			  IDictionary<string, Node> graph = Data.get();
			  ISet<Node> unique = new HashSet<Node>();
			  Node n = graph["a"];
			  while ( unique.Add( n ) )
			  {
					using ( Transaction ignored = _graphdb.beginTx() )
					{
						 n = n.GetSingleRelationship( RelationshipType.withName( "TO" ), Direction.OUTGOING ).EndNode;
					}
			  }
			  assertEquals( graph.Count, unique.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph({ "a:Person EATS b:Banana" }) public void ensurePeopleCanEatBananas()
		 [Graph({ "a:Person EATS b:Banana" })]
		 public virtual void EnsurePeopleCanEatBananas()
		 {
			  IDictionary<string, Node> graph = Data.get();
			  Node a = graph["a"];
			  Node b = graph["b"];

			  using ( Transaction ignored = _graphdb.beginTx() )
			  {
					assertTrue( a.HasLabel( label( "Person" ) ) );
					assertTrue( b.HasLabel( label( "Banana" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph({ "a:Person EATS b:Banana", "a EATS b:Apple" }) public void ensurePeopleCanEatBananasAndApples()
		 [Graph({ "a:Person EATS b:Banana", "a EATS b:Apple" })]
		 public virtual void EnsurePeopleCanEatBananasAndApples()
		 {
			  IDictionary<string, Node> graph = Data.get();
			  Node a = graph["a"];
			  Node b = graph["b"];

			  using ( Transaction ignored = _graphdb.beginTx() )
			  {
					assertTrue( "Person label missing", a.HasLabel( label( "Person" ) ) );
					assertTrue( "Banana label missing", b.HasLabel( label( "Banana" ) ) );
					assertTrue( "Apple label missing", b.HasLabel( label( "Apple" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(value = {"I know you"}, autoIndexNodes = true) public void canAutoIndexNodes()
		 [Graph(value : {"I know you"}, autoIndexNodes : true)]
		 public virtual void CanAutoIndexNodes()
		 {
			  Data.get();

			  using ( Transaction ignored = _graphdb.beginTx() )
			  {
					using ( IndexHits<Node> indexHits = Graphdb().index().NodeAutoIndexer.AutoIndex.get("name", "I") )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( "can't look up node.", indexHits.hasNext() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = {@NODE(name = "I", setNameProperty = true, properties = { @PROP(key = "name", value = "I")})}, autoIndexNodes = true) public void canAutoIndexNodesExplicitProps()
		 [Graph(nodes : {@NODE(name : "I", setNameProperty : true, properties : { @PROP(key : "name", value : "I")})}, autoIndexNodes : true)]
		 public virtual void CanAutoIndexNodesExplicitProps()
		 {
			  Data.get();

			  using ( Transaction ignored = _graphdb.beginTx(), IndexHits<Node> nodes = _graphdb().index().NodeAutoIndexer.AutoIndex.get("name", "I") )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "can't look up node.", nodes.hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Graph(nodes = { @NODE(name = "I", properties = { @PROP(key = "name", value = "me"), @PROP(key = "bool", value = "true", type = org.neo4j.test.GraphDescription.PropType.BOOLEAN) }), @NODE(name = "you", setNameProperty = true) }, relationships = { @REL(start = "I", end = "you", type = "knows", properties = { @PROP(key = "name", value = "relProp"), @PROP(key = "valid", value = "true", type = org.neo4j.test.GraphDescription.PropType.BOOLEAN) }) }, autoIndexRelationships = true) public void canCreateMoreInvolvedGraphWithPropertiesAndAutoIndex()
		 [Graph(nodes : { @NODE(name : "I", properties : { @PROP(key : "name", value : "me"), @PROP(key : "bool", value : "true", type : Org.Neo4j.Test.GraphDescription.PropType.BOOLEAN) }), @NODE(name : "you", setNameProperty : true) }, relationships : { @REL(start : "I", end : "you", type : "knows", properties : { @PROP(key : "name", value : "relProp"), @PROP(key : "valid", value : "true", type : Org.Neo4j.Test.GraphDescription.PropType.BOOLEAN) }) }, autoIndexRelationships : true)]
		 public virtual void CanCreateMoreInvolvedGraphWithPropertiesAndAutoIndex()
		 {
			  Data.get();
			  VerifyIKnowYou( "knows", "me" );
			  using ( Transaction ignored = _graphdb.beginTx() )
			  {
					assertEquals( true, Data.get()["I"].getProperty("bool") );
					assertFalse( "node autoindex enabled.", Graphdb().index().NodeAutoIndexer.Enabled );
					using ( IndexHits<Relationship> relationships = Graphdb().index().RelationshipAutoIndexer.AutoIndex.get("name", "relProp") )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( "can't look up rel.", relationships.hasNext() );
					}
					assertTrue( "relationship autoindex enabled.", Graphdb().index().RelationshipAutoIndexer.Enabled );
			  }
		 }

		 [Graph(value : { "I know you" }, nodes : { @NODE(name : "I", properties : { @PROP(key : "name", value : "me") }) })]
		 private void VerifyIKnowYou( string type, string myName )
		 {
			  using ( Transaction ignored = _graphdb.beginTx() )
			  {
					IDictionary<string, Node> graph = Data.get();
					assertEquals( "Wrong graph size.", 2, graph.Count );
					Node iNode = graph["I"];
					assertNotNull( "The node 'I' was not defined", iNode );
					Node you = graph["you"];
					assertNotNull( "The node 'you' was not defined", you );
					assertEquals( "'I' has wrong 'name'.", myName, iNode.GetProperty( "name" ) );
					assertEquals( "'you' has wrong 'name'.", "you", you.GetProperty( "name" ) );

					IEnumerator<Relationship> rels = iNode.Relationships.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "'I' has too few relationships", rels.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Relationship rel = rels.next();
					assertEquals( "'I' is not related to 'you'", you, rel.GetOtherNode( iNode ) );
					assertEquals( "Wrong relationship type.", type, rel.Type.name() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "'I' has too many relationships", rels.hasNext() );

					rels = you.Relationships.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "'you' has too few relationships", rels.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					rel = rels.next();
					assertEquals( "'you' is not related to 'i'", iNode, rel.GetOtherNode( you ) );
					assertEquals( "Wrong relationship type.", type, rel.Type.name() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "'you' has too many relationships", rels.hasNext() );

					assertEquals( "wrong direction", iNode, rel.StartNode );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void startDatabase()
		 public static void StartDatabase()
		 {
			  _graphdb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopDatabase()
		 public static void StopDatabase()
		 {
			  if ( _graphdb != null )
			  {
					_graphdb.shutdown();
			  }
			  _graphdb = null;
		 }

		 public override GraphDatabaseService Graphdb()
		 {
			  return _graphdb;
		 }
	}

}