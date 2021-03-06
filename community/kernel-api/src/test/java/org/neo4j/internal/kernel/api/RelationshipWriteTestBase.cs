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
namespace Org.Neo4j.@internal.Kernel.Api
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") public abstract class RelationshipWriteTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class RelationshipWriteTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateRelationship()
		 {
			  long n1, n2;
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					n1 = graphDb.createNode().Id;
					n2 = graphDb.createNode().Id;
					tx.Success();
			  }

			  long r;
			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.token().relationshipTypeGetOrCreateForName("R");
					r = tx.dataWrite().relationshipCreate(n1, label, n2);
					tx.Success();
			  }

			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					IList<Relationship> relationships = Iterables.asList( graphDb.getNodeById( n1 ).Relationships );
					assertEquals( 1, relationships.Count );
					assertEquals( relationships[0].Id, r );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateRelationshipBetweenInTransactionNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateRelationshipBetweenInTransactionNodes()
		 {
			  long n1, n2, r;
			  using ( Transaction tx = beginTransaction() )
			  {
					n1 = tx.DataWrite().nodeCreate();
					n2 = tx.DataWrite().nodeCreate();
					int label = tx.Token().relationshipTypeGetOrCreateForName("R");
					r = tx.DataWrite().relationshipCreate(n1, label, n2);
					tx.Success();
			  }

			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					IList<Relationship> relationships = Iterables.asList( graphDb.getNodeById( n1 ).Relationships );
					assertEquals( 1, relationships.Count );
					assertEquals( relationships[0].Id, r );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackRelationshipOnFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackRelationshipOnFailure()
		 {
			  long n1, n2;
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					n1 = graphDb.createNode().Id;
					n2 = graphDb.createNode().Id;
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.token().relationshipTypeGetOrCreateForName("R");
					tx.dataWrite().relationshipCreate(n1, label, n2);
					tx.Failure();
			  }

			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertEquals( 0, graphDb.getNodeById( n1 ).Degree );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteRelationship()
		 {
			  long n1, r;
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node node1 = graphDb.createNode();
					Node node2 = graphDb.createNode();

					n1 = node1.Id;
					r = node1.CreateRelationshipTo( node2, RelationshipType.withName( "R" ) ).Id;

					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					assertTrue( "should delete relationship", tx.dataWrite().relationshipDelete(r) );
					tx.Success();
			  }

			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertEquals( 0, graphDb.getNodeById( n1 ).Degree );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeleteRelationshipThatDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDeleteRelationshipThatDoesNotExist()
		 {
			  long relationship = 0;

			  using ( Transaction tx = beginTransaction() )
			  {
					assertFalse( tx.DataWrite().relationshipDelete(relationship) );
					tx.Failure();
			  }
			  using ( Transaction tx = beginTransaction() )
			  {
					assertFalse( tx.DataWrite().relationshipDelete(relationship) );
					tx.Success();
			  }
			  // should not crash
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteRelationshipAddedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteRelationshipAddedInTransaction()
		 {
			  long n1, n2;
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					n1 = graphDb.createNode().Id;
					n2 = graphDb.createNode().Id;
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.token().relationshipTypeGetOrCreateForName("R");
					long r = tx.dataWrite().relationshipCreate(n1, label, n2);

					assertTrue( tx.dataWrite().relationshipDelete(r) );
					tx.Success();
			  }

			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertEquals( 0, graphDb.getNodeById( n1 ).Degree );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddPropertyToRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddPropertyToRelationship()
		 {
			  // Given
			  long relationshipId;
			  string propertyKey = "prop";
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node node1 = graphDb.createNode();
					Node node2 = graphDb.createNode();

					relationshipId = node1.CreateRelationshipTo( node2, RelationshipType.withName( "R" ) ).Id;

					tx.Success();
			  }

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					int token = tx.token().propertyKeyGetOrCreateForName(propertyKey);
					assertThat( tx.dataWrite().relationshipSetProperty(relationshipId, token, stringValue("hello")), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  // Then
			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( graphDb.getRelationshipById( relationshipId ).getProperty( "prop" ), equalTo( "hello" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdatePropertyToRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdatePropertyToRelationship()
		 {
			  // Given
			  long relationshipId;
			  string propertyKey = "prop";
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node node1 = graphDb.createNode();
					Node node2 = graphDb.createNode();

					Relationship r = node1.CreateRelationshipTo( node2, RelationshipType.withName( "R" ) );
					r.SetProperty( propertyKey, 42 );
					relationshipId = r.Id;

					tx.Success();
			  }

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					int token = tx.token().propertyKeyGetOrCreateForName(propertyKey);
					assertThat( tx.dataWrite().relationshipSetProperty(relationshipId, token, stringValue("hello")), equalTo(intValue(42)) );
					tx.Success();
			  }

			  // Then
			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( graphDb.getRelationshipById( relationshipId ).getProperty( "prop" ), equalTo( "hello" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemovePropertyFromRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemovePropertyFromRelationship()
		 {
			  // Given
			  long relationshipId;
			  string propertyKey = "prop";
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node node1 = graphDb.createNode();
					Node node2 = graphDb.createNode();

					Relationship proxy = node1.CreateRelationshipTo( node2, RelationshipType.withName( "R" ) );
					relationshipId = proxy.Id;
					proxy.SetProperty( propertyKey, 42 );
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					int token = tx.token().propertyKeyGetOrCreateForName(propertyKey);
					assertThat( tx.dataWrite().relationshipRemoveProperty(relationshipId, token), equalTo(intValue(42)) );
					tx.Success();
			  }

			  // Then
			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertFalse( graphDb.getRelationshipById( relationshipId ).hasProperty( "prop" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveNonExistingPropertyFromRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveNonExistingPropertyFromRelationship()
		 {
			  // Given
			  long relationshipId;
			  string propertyKey = "prop";
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node node1 = graphDb.createNode();
					Node node2 = graphDb.createNode();

					Relationship proxy = node1.CreateRelationshipTo( node2, RelationshipType.withName( "R" ) );
					relationshipId = proxy.Id;
					tx.Success();
			  }
			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					int token = tx.token().propertyKeyGetOrCreateForName(propertyKey);
					assertThat( tx.dataWrite().relationshipRemoveProperty(relationshipId, token), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  // Then
			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertFalse( graphDb.getRelationshipById( relationshipId ).hasProperty( "prop" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemovePropertyFromRelationshipTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemovePropertyFromRelationshipTwice()
		 {
			  // Given
			  long relationshipId;
			  string propertyKey = "prop";
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node node1 = graphDb.createNode();
					Node node2 = graphDb.createNode();

					Relationship proxy = node1.CreateRelationshipTo( node2, RelationshipType.withName( "R" ) );
					relationshipId = proxy.Id;
					proxy.SetProperty( propertyKey, 42 );
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					int token = tx.token().propertyKeyGetOrCreateForName(propertyKey);
					assertThat( tx.dataWrite().relationshipRemoveProperty(relationshipId, token), equalTo(intValue(42)) );
					assertThat( tx.dataWrite().relationshipRemoveProperty(relationshipId, token), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  // Then
			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertFalse( graphDb.getRelationshipById( relationshipId ).hasProperty( "prop" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdatePropertyToRelationshipInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdatePropertyToRelationshipInTransaction()
		 {
			  // Given
			  long relationshipId;
			  string propertyKey = "prop";
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node node1 = graphDb.createNode();
					Node node2 = graphDb.createNode();

					relationshipId = node1.CreateRelationshipTo( node2, RelationshipType.withName( "R" ) ).Id;

					tx.Success();
			  }

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					int token = tx.token().propertyKeyGetOrCreateForName(propertyKey);
					assertThat( tx.dataWrite().relationshipSetProperty(relationshipId, token, stringValue("hello")), equalTo(NO_VALUE) );
					assertThat( tx.dataWrite().relationshipSetProperty(relationshipId, token, stringValue("world")), equalTo(stringValue("hello")) );
					assertThat( tx.dataWrite().relationshipSetProperty(relationshipId, token, intValue(1337)), equalTo(stringValue("world")) );
					tx.Success();
			  }

			  // Then
			  using ( Org.Neo4j.Graphdb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( graphDb.getRelationshipById( relationshipId ).getProperty( "prop" ), equalTo( 1337 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWriteWhenSettingPropertyToSameValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotWriteWhenSettingPropertyToSameValue()
		 {
			  // Given
			  long relationshipId;
			  string propertyKey = "prop";
			  Value theValue = stringValue( "The Value" );

			  using ( Org.Neo4j.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					Node node1 = graphDb.createNode();
					Node node2 = graphDb.createNode();

					Relationship r = node1.CreateRelationshipTo( node2, RelationshipType.withName( "R" ) );

					r.SetProperty( propertyKey, theValue.AsObject() );
					relationshipId = r.Id;
					ctx.Success();
			  }

			  // When
			  Transaction tx = beginTransaction();
			  int property = tx.Token().propertyKeyGetOrCreateForName(propertyKey);
			  assertThat( tx.DataWrite().relationshipSetProperty(relationshipId, property, theValue), equalTo(theValue) );
			  tx.Success();

			  assertThat( tx.CloseTransaction(), equalTo(Transaction_Fields.READ_ONLY) );
		 }
	}

}