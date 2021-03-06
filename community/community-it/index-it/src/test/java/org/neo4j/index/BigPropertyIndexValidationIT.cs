﻿/*
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
namespace Org.Neo4j.Index
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using TestName = org.junit.rules.TestName;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TransactionFailureException = Org.Neo4j.Graphdb.TransactionFailureException;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Neo4jMatchers = Org.Neo4j.Test.mockito.matcher.Neo4jMatchers;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class BigPropertyIndexValidationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public readonly TestName TestName = new TestName();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

		 private Label _label;
		 private string _longString;
		 private string _propertyKey;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _label = Label.label( "LABEL" );
			  char[] chars = new char[1 << 15];
			  Arrays.fill( chars, 'c' );
			  _longString = new string( chars );
			  _propertyKey = "name";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailTransactionThatIndexesLargePropertyDuringNodeCreation()
		 public virtual void ShouldFailTransactionThatIndexesLargePropertyDuringNodeCreation()
		 {
			  // GIVEN
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  IndexDefinition index = Neo4jMatchers.createIndex( db, _label, _propertyKey );

			  //We expect this transaction to fail due to the huge property
			  ExpectedException.expect( typeof( TransactionFailureException ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					try
					{
						 Db.execute( "CREATE (n:" + _label + " {name: \"" + _longString + "\"})" );
						 fail( "Argument was illegal" );
					}
					catch ( System.ArgumentException )
					{
						 //this is expected.
					}
					tx.Success();
			  }
			  //Check that the database is empty.
			  using ( Transaction tx = Db.beginTx() )
			  {
					ResourceIterator<Node> nodes = Db.AllNodes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( nodes.hasNext() );
			  }
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailTransactionThatIndexesLargePropertyAfterNodeCreation()
		 public virtual void ShouldFailTransactionThatIndexesLargePropertyAfterNodeCreation()
		 {
			  // GIVEN
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  IndexDefinition index = Neo4jMatchers.createIndex( db, _label, _propertyKey );

			  //We expect this transaction to fail due to the huge property
			  ExpectedException.expect( typeof( TransactionFailureException ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.execute( "CREATE (n:" + _label + ")" );
					try
					{
						 Db.execute( "match (n:" + _label + ")set n.name= \"" + _longString + "\"" );
						 fail( "Argument was illegal" );
					}
					catch ( System.ArgumentException )
					{
						 //this is expected.
					}
					tx.Success();
			  }
			  //Check that the database is empty.
			  using ( Transaction tx = Db.beginTx() )
			  {
					ResourceIterator<Node> nodes = Db.AllNodes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( nodes.hasNext() );
			  }
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailTransactionThatIndexesLargePropertyOnLabelAdd()
		 public virtual void ShouldFailTransactionThatIndexesLargePropertyOnLabelAdd()
		 {
			  // GIVEN
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  IndexDefinition index = Neo4jMatchers.createIndex( db, _label, _propertyKey );

			  //We expect this transaction to fail due to the huge property
			  ExpectedException.expect( typeof( TransactionFailureException ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					string otherLabel = "SomethingElse";
					Db.execute( "CREATE (n:" + otherLabel + " {name: \"" + _longString + "\"})" );
					try
					{
						 Db.execute( "match (n:" + otherLabel + ")set n:" + _label );
						 fail( "Argument was illegal" );
					}
					catch ( System.ArgumentException )
					{
						 //this is expected.
					}
					tx.Success();
			  }
			  //Check that the database is empty.
			  using ( Transaction tx = Db.beginTx() )
			  {
					ResourceIterator<Node> nodes = Db.AllNodes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( nodes.hasNext() );
			  }
			  Db.shutdown();
		 }
	}

}