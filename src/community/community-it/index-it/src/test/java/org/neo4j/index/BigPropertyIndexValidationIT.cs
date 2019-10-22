﻿/*
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
namespace Neo4Net.Index
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using TestName = org.junit.rules.TestName;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using IndexDefinition = Neo4Net.GraphDb.schema.IndexDefinition;
	using Neo4NetMatchers = Neo4Net.Test.mockito.matcher.Neo4NetMatchers;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class BigPropertyIndexValidationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.ImpermanentDatabaseRule dbRule = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
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
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  IndexDefinition index = Neo4NetMatchers.createIndex( db, _label, _propertyKey );

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
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  IndexDefinition index = Neo4NetMatchers.createIndex( db, _label, _propertyKey );

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
			  IGraphDatabaseService db = DbRule.GraphDatabaseAPI;
			  IndexDefinition index = Neo4NetMatchers.createIndex( db, _label, _propertyKey );

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