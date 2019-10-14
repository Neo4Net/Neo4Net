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
namespace Neo4Net.Internal.Kernel.Api
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.Test;


	using Relationship = Neo4Net.Graphdb.Relationship;
	using EntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

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
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.BOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTestSupport.assertCounts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTestSupport.computeKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTestSupport.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTestSupport.sparse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTransactionStateTestBase.RelationshipDirection.IN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTransactionStateTestBase.RelationshipDirection.LOOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.RelationshipTransactionStateTestBase.RelationshipDirection.OUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") public abstract class RelationshipTransactionStateTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class RelationshipTransactionStateTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeSingleRelationshipInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeSingleRelationshipInTransaction()
		 {
			  int label;
			  long n1, n2;
			  using ( Transaction tx = beginTransaction() )
			  {
					n1 = tx.DataWrite().nodeCreate();
					n2 = tx.DataWrite().nodeCreate();

					// setup extra relationship to challenge the implementation
					long decoyNode = tx.DataWrite().nodeCreate();
					label = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					tx.DataWrite().relationshipCreate(n2, label, decoyNode);
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					long r = tx.DataWrite().relationshipCreate(n1, label, n2);
					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor() )
					{
						 tx.DataRead().singleRelationship(r, relationship);
						 assertTrue( "should find relationship", relationship.Next() );

						 assertEquals( label, relationship.Type() );
						 assertEquals( n1, relationship.SourceNodeReference() );
						 assertEquals( n2, relationship.TargetNodeReference() );
						 assertEquals( r, relationship.RelationshipReference() );

						 assertFalse( "should only find one relationship", relationship.Next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeSingleRelationshipWhichWasDeletedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeSingleRelationshipWhichWasDeletedInTransaction()
		 {
			  int label;
			  long n1, n2, r;
			  using ( Transaction tx = beginTransaction() )
			  {
					n1 = tx.DataWrite().nodeCreate();
					n2 = tx.DataWrite().nodeCreate();
					label = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");

					long decoyNode = tx.DataWrite().nodeCreate();
					tx.DataWrite().relationshipCreate(n2, label, decoyNode); // to have >1 relationship in the db

					r = tx.DataWrite().relationshipCreate(n1, label, n2);
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					assertTrue( "should delete relationship", tx.DataWrite().relationshipDelete(r) );
					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor() )
					{
						 tx.DataRead().singleRelationship(r, relationship);
						 assertFalse( "should not find relationship", relationship.Next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScanRelationshipInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldScanRelationshipInTransaction()
		 {
			  const int nRelationshipsInStore = 10;

			  int type;
			  long n1, n2;

			  using ( Transaction tx = beginTransaction() )
			  {
					n1 = tx.DataWrite().nodeCreate();
					n2 = tx.DataWrite().nodeCreate();

					// setup some in store relationships
					type = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					RelateNTimes( nRelationshipsInStore, type, n1, n2, tx );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					long r = tx.DataWrite().relationshipCreate(n1, type, n2);
					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor() )
					{
						 tx.DataRead().allRelationshipsScan(relationship);
						 AssertCountRelationships( relationship, nRelationshipsInStore + 1, n1, type, n2 );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotScanRelationshipWhichWasDeletedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotScanRelationshipWhichWasDeletedInTransaction()
		 {
			  const int nRelationshipsInStore = 5 + 1 + 5;

			  int type;
			  long n1, n2, r;
			  using ( Transaction tx = beginTransaction() )
			  {
					n1 = tx.DataWrite().nodeCreate();
					n2 = tx.DataWrite().nodeCreate();
					type = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");

					RelateNTimes( 5, type, n1, n2, tx );
					r = tx.DataWrite().relationshipCreate(n1, type, n2);
					RelateNTimes( 5, type, n1, n2, tx );

					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					assertTrue( "should delete relationship", tx.DataWrite().relationshipDelete(r) );
					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor() )
					{
						 tx.DataRead().allRelationshipsScan(relationship);
						 AssertCountRelationships( relationship, nRelationshipsInStore - 1, n1, type, n2 );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeRelationshipInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeRelationshipInTransaction()
		 {
			  long n1, n2;
			  using ( Transaction tx = beginTransaction() )
			  {
					n1 = tx.DataWrite().nodeCreate();
					n2 = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					long r = tx.DataWrite().relationshipCreate(n1, label, n2);
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor relationship = tx.Cursors().allocateRelationshipTraversalCursor() )
					{
						 tx.DataRead().singleNode(n1, node);
						 assertTrue( "should access node", node.Next() );

						 node.AllRelationships( relationship );
						 assertTrue( "should find relationship", relationship.next() );
						 assertEquals( r, relationship.RelationshipReference() );

						 assertFalse( "should only find one relationship", relationship.next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeRelationshipDeletedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeRelationshipDeletedInTransaction()
		 {
			  long n1, n2, r;
			  using ( Transaction tx = beginTransaction() )
			  {
					n1 = tx.DataWrite().nodeCreate();
					n2 = tx.DataWrite().nodeCreate();

					int label = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					r = tx.DataWrite().relationshipCreate(n1, label, n2);

					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					tx.DataWrite().relationshipDelete(r);
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor relationship = tx.Cursors().allocateRelationshipTraversalCursor() )
					{
						 tx.DataRead().singleNode(n1, node);
						 assertTrue( "should access node", node.Next() );

						 node.AllRelationships( relationship );
						 assertFalse( "should not find relationship", relationship.next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeRelationshipInTransactionBeforeCursorInitialization() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeRelationshipInTransactionBeforeCursorInitialization()
		 {
			  long n1, n2;
			  using ( Transaction tx = beginTransaction() )
			  {
					n1 = tx.DataWrite().nodeCreate();
					n2 = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					long r = tx.DataWrite().relationshipCreate(n1, label, n2);
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor relationship = tx.Cursors().allocateRelationshipTraversalCursor() )
					{
						 tx.DataRead().singleNode(n1, node);
						 assertTrue( "should access node", node.Next() );

						 node.AllRelationships( relationship );
						 assertTrue( "should find relationship", relationship.next() );
						 assertEquals( r, relationship.RelationshipReference() );

						 tx.DataWrite().relationshipCreate(n1, label, n2); // should not be seen
						 assertFalse( "should not find relationship added after cursor init", relationship.next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseSparseNodeWithoutGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseSparseNodeWithoutGroups()
		 {
			  TraverseWithoutGroups( sparse( graphDb ), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseDenseNodeWithoutGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseDenseNodeWithoutGroups()
		 {
			  TraverseWithoutGroups( RelationshipTestSupport.Dense( graphDb ), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseSparseNodeWithoutGroupsWithDetachedReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseSparseNodeWithoutGroupsWithDetachedReferences()
		 {
			  TraverseWithoutGroups( sparse( graphDb ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseDenseNodeWithoutGroupsWithDetachedReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseDenseNodeWithoutGroupsWithDetachedReferences()
		 {
			  TraverseWithoutGroups( RelationshipTestSupport.Dense( graphDb ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseSparseNodeViaGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseSparseNodeViaGroups()
		 {
			  TraverseViaGroups( sparse( graphDb ), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseDenseNodeViaGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseDenseNodeViaGroups()
		 {
			  TraverseViaGroups( RelationshipTestSupport.Dense( graphDb ), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseSparseNodeViaGroupsWithDetachedReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseSparseNodeViaGroupsWithDetachedReferences()
		 {
			  TraverseViaGroups( sparse( graphDb ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTraverseDenseNodeViaGroupsWithDetachedReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTraverseDenseNodeViaGroupsWithDetachedReferences()
		 {
			  TraverseViaGroups( RelationshipTestSupport.Dense( graphDb ), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNewRelationshipPropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNewRelationshipPropertyInTransaction()
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					string propKey1 = "prop1";
					string propKey2 = "prop2";
					long n1 = tx.DataWrite().nodeCreate();
					long n2 = tx.DataWrite().nodeCreate();
					int label = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					long r = tx.DataWrite().relationshipCreate(n1, label, n2);
					int prop1 = tx.Token().propertyKeyGetOrCreateForName(propKey1);
					int prop2 = tx.Token().propertyKeyGetOrCreateForName(propKey2);
					assertEquals( tx.DataWrite().relationshipSetProperty(r, prop1, stringValue("hello")), NO_VALUE );
					assertEquals( tx.DataWrite().relationshipSetProperty(r, prop2, stringValue("world")), NO_VALUE );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor relationship = tx.Cursors().allocateRelationshipTraversalCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleNode(n1, node);
						 assertTrue( "should access node", node.Next() );
						 node.AllRelationships( relationship );

						 assertTrue( "should access relationship", relationship.next() );

						 relationship.Properties( property );

						 while ( property.Next() )
						 {
							  if ( property.PropertyKey() == prop1 )
							  {
									assertEquals( property.PropertyValue(), stringValue("hello") );
							  }
							  else if ( property.PropertyKey() == prop2 )
							  {
									assertEquals( property.PropertyValue(), stringValue("world") );
							  }
							  else
							  {
									fail( property.PropertyKey() + " was not the property key you were looking for" );
							  }
						 }

						 assertFalse( "should only find one relationship", relationship.next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeAddedPropertyFromExistingRelationshipWithoutPropertiesInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeAddedPropertyFromExistingRelationshipWithoutPropertiesInTransaction()
		 {
			  // Given
			  long relationshipId;
			  string propKey = "prop1";
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					relationshipId = write.RelationshipCreate( write.NodeCreate(), tx.TokenWrite().relationshipTypeGetOrCreateForName("R"), write.NodeCreate() );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					int propToken = tx.Token().propertyKeyGetOrCreateForName(propKey);
					assertEquals( tx.DataWrite().relationshipSetProperty(relationshipId, propToken, stringValue("hello")), NO_VALUE );

					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleRelationship(relationshipId, relationship);
						 assertTrue( "should access relationship", relationship.Next() );

						 relationship.Properties( property );
						 assertTrue( property.Next() );
						 assertEquals( propToken, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("hello") );

						 assertFalse( "should only find one properties", property.Next() );
						 assertFalse( "should only find one relationship", relationship.Next() );
					}

					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertThat( graphDb.getRelationshipById( relationshipId ).getProperty( propKey ), equalTo( "hello" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeAddedPropertyFromExistingRelationshipWithPropertiesInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeAddedPropertyFromExistingRelationshipWithPropertiesInTransaction()
		 {
			  // Given
			  long relationshipId;
			  string propKey1 = "prop1";
			  string propKey2 = "prop2";
			  int propToken1;
			  int propToken2;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					relationshipId = write.RelationshipCreate( write.NodeCreate(), tx.TokenWrite().relationshipTypeGetOrCreateForName("R"), write.NodeCreate() );
					propToken1 = tx.Token().propertyKeyGetOrCreateForName(propKey1);
					assertEquals( write.RelationshipSetProperty( relationshipId, propToken1, stringValue( "hello" ) ), NO_VALUE );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					propToken2 = tx.Token().propertyKeyGetOrCreateForName(propKey2);
					assertEquals( tx.DataWrite().relationshipSetProperty(relationshipId, propToken2, stringValue("world")), NO_VALUE );

					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleRelationship(relationshipId, relationship);
						 assertTrue( "should access relationship", relationship.Next() );

						 relationship.Properties( property );

						 while ( property.Next() )
						 {
							  if ( property.PropertyKey() == propToken1 ) //from disk
							  {
									assertEquals( property.PropertyValue(), stringValue("hello") );

							  }
							  else if ( property.PropertyKey() == propToken2 ) //from tx state
							  {
									assertEquals( property.PropertyValue(), stringValue("world") );
							  }
							  else
							  {
									fail( property.PropertyKey() + " was not the property you were looking for" );
							  }
						 }

						 assertFalse( "should only find one relationship", relationship.Next() );
					}
					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					Relationship relationship = graphDb.getRelationshipById( relationshipId );
					assertThat( relationship.getProperty( propKey1 ), equalTo( "hello" ) );
					assertThat( relationship.getProperty( propKey2 ), equalTo( "world" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeUpdatedPropertyFromExistingRelationshipWithPropertiesInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeUpdatedPropertyFromExistingRelationshipWithPropertiesInTransaction()
		 {
			  // Given
			  long relationshipId;
			  string propKey = "prop1";
			  int propToken;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					relationshipId = write.RelationshipCreate( write.NodeCreate(), tx.TokenWrite().relationshipTypeGetOrCreateForName("R"), write.NodeCreate() );
					propToken = tx.Token().propertyKeyGetOrCreateForName(propKey);
					assertEquals( write.RelationshipSetProperty( relationshipId, propToken, stringValue( "hello" ) ), NO_VALUE );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					assertEquals( tx.DataWrite().relationshipSetProperty(relationshipId, propToken, stringValue("world")), stringValue("hello") );
					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleRelationship(relationshipId, relationship);
						 assertTrue( "should access relationship", relationship.Next() );

						 relationship.Properties( property );

						 assertTrue( property.Next() );
						 assertEquals( propToken, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("world") );

						 assertFalse( "should only find one property", property.Next() );
						 assertFalse( "should only find one relationship", relationship.Next() );
					}

					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertThat( graphDb.getRelationshipById( relationshipId ).getProperty( propKey ), equalTo( "world" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeRemovedPropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeRemovedPropertyInTransaction()
		 {
			  // Given
			  long relationshipId;
			  string propKey = "prop1";
			  int propToken;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					relationshipId = write.RelationshipCreate( write.NodeCreate(), tx.TokenWrite().relationshipTypeGetOrCreateForName("R"), write.NodeCreate() );
					propToken = tx.Token().propertyKeyGetOrCreateForName(propKey);
					assertEquals( write.RelationshipSetProperty( relationshipId, propToken, stringValue( "hello" ) ), NO_VALUE );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					assertEquals( tx.DataWrite().relationshipRemoveProperty(relationshipId, propToken), stringValue("hello") );
					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleRelationship(relationshipId, relationship);
						 assertTrue( "should access relationship", relationship.Next() );

						 relationship.Properties( property );
						 assertFalse( "should not find any properties", property.Next() );
						 assertFalse( "should only find one relationship", relationship.Next() );
					}

					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertFalse( graphDb.getRelationshipById( relationshipId ).hasProperty( propKey ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeRemovedThenAddedPropertyInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeRemovedThenAddedPropertyInTransaction()
		 {
			  // Given
			  long relationshipId;
			  string propKey = "prop1";
			  int propToken;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					relationshipId = write.RelationshipCreate( write.NodeCreate(), tx.TokenWrite().relationshipTypeGetOrCreateForName("R"), write.NodeCreate() );
					propToken = tx.Token().propertyKeyGetOrCreateForName(propKey);
					assertEquals( write.RelationshipSetProperty( relationshipId, propToken, stringValue( "hello" ) ), NO_VALUE );
					tx.Success();
			  }

			  // When/Then
			  using ( Transaction tx = beginTransaction() )
			  {
					assertEquals( tx.DataWrite().relationshipRemoveProperty(relationshipId, propToken), stringValue("hello") );
					assertEquals( tx.DataWrite().relationshipSetProperty(relationshipId, propToken, stringValue("world")), NO_VALUE );
					using ( RelationshipScanCursor relationship = tx.Cursors().allocateRelationshipScanCursor(), PropertyCursor property = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleRelationship(relationshipId, relationship);
						 assertTrue( "should access relationship", relationship.Next() );

						 relationship.Properties( property );
						 assertTrue( property.Next() );
						 assertEquals( propToken, property.PropertyKey() );
						 assertEquals( property.PropertyValue(), stringValue("world") );

						 assertFalse( "should not find any properties", property.Next() );
						 assertFalse( "should only find one relationship", relationship.Next() );
					}

					tx.Success();
			  }

			  using ( Neo4Net.Graphdb.Transaction ignored = graphDb.beginTx() )
			  {
					assertThat( graphDb.getRelationshipById( relationshipId ).getProperty( propKey ), equalTo( "world" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountFromTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCountFromTxState()
		 {
			  //dense outgoing
			  AssertCount(100, OUT, group =>
			  {
				assertEquals( 101, group.outgoingCount() );
				assertEquals( 0, group.incomingCount() );
				assertEquals( 0, group.loopCount() );
				assertEquals( 101, group.totalCount() );
			  });
			  //sparse outgoing
			  AssertCount(1, OUT, group =>
			  {
				assertEquals( 2, group.outgoingCount() );
				assertEquals( 0, group.incomingCount() );
				assertEquals( 0, group.loopCount() );
				assertEquals( 2, group.totalCount() );
			  });
			  //dense incoming
			  AssertCount(100, IN, group =>
			  {
				assertEquals( 0, group.outgoingCount() );
				assertEquals( 101, group.incomingCount() );
				assertEquals( 0, group.outgoingCount() );
				assertEquals( 101, group.totalCount() );
			  });
			  //sparse incoming
			  AssertCount(1, IN, group =>
			  {
				assertEquals( 0, group.outgoingCount() );
				assertEquals( 2, group.incomingCount() );
				assertEquals( 0, group.loopCount() );
				assertEquals( 2, group.totalCount() );
			  });

			  //dense loops
			  AssertCount(100, LOOP, group =>
			  {
				assertEquals( 0, group.incomingCount() );
				assertEquals( 0, group.outgoingCount() );
				assertEquals( 101, group.loopCount() );
				assertEquals( 101, group.totalCount() );
			  });
			  //sparse loops
			  AssertCount(1, LOOP, group =>
			  {
				assertEquals( 0, group.outgoingCount() );
				assertEquals( 0, group.incomingCount() );
				assertEquals( 2, group.loopCount() );
				assertEquals( 2, group.totalCount() );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupCursorShouldSeeNewTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GroupCursorShouldSeeNewTypes()
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					long start = write.NodeCreate();
					int outgoing = tx.TokenWrite().relationshipTypeGetOrCreateForName("OUT");
					int incoming = tx.TokenWrite().relationshipTypeGetOrCreateForName("IN");
					int looping = tx.TokenWrite().relationshipTypeGetOrCreateForName("LOOP");
					long @out = write.RelationshipCreate( start, outgoing, write.NodeCreate() );
					long in1 = write.RelationshipCreate( write.NodeCreate(), incoming, start );
					long in2 = write.RelationshipCreate( write.NodeCreate(), incoming, start );
					long loop = write.RelationshipCreate( start, looping, start );
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor traversal = tx.Cursors().allocateRelationshipTraversalCursor(), RelationshipGroupCursor group = tx.Cursors().allocateRelationshipGroupCursor() )
					{
						 Read read = tx.DataRead();
						 read.SingleNode( start, node );
						 assertTrue( node.Next() );
						 node.Relationships( group );

						 while ( group.next() )
						 {
							  int t = group.Type();
							  if ( t == outgoing )
							  {
									assertEquals( 1, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
									AssertRelationships( OUT, group, traversal, @out );
									AssertNoRelationships( IN, group, traversal );
									AssertNoRelationships( LOOP, group, traversal );
							  }
							  else if ( t == incoming )
							  {
									assertEquals( 0, group.OutgoingCount() );
									assertEquals( 2, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
									AssertRelationships( IN, group, traversal, in1, in2 );
									AssertNoRelationships( OUT, group, traversal );
									AssertNoRelationships( LOOP, group, traversal );
							  }
							  else if ( t == looping )
							  {
									assertEquals( 0, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 1, group.LoopCount() );
									AssertRelationships( LOOP, group, traversal, loop );
									AssertNoRelationships( OUT, group, traversal );
									AssertNoRelationships( IN, group, traversal );
							  }
							  else
							  {
									fail( t + "  is not the type you're looking for " );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupCursorShouldAddToCountFromTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GroupCursorShouldAddToCountFromTxState()
		 {
			  long start;
			  long existingRelationship;
			  int type;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					start = write.NodeCreate();
					type = tx.TokenWrite().relationshipTypeGetOrCreateForName("OUT");
					existingRelationship = write.RelationshipCreate( start, type, write.NodeCreate() );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					long newRelationship = write.RelationshipCreate( start, type, write.NodeCreate() );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor traversal = tx.Cursors().allocateRelationshipTraversalCursor(), RelationshipGroupCursor group = tx.Cursors().allocateRelationshipGroupCursor() )
					{
						 Read read = tx.DataRead();
						 read.SingleNode( start, node );
						 assertTrue( node.Next() );
						 node.Relationships( group );

						 assertTrue( group.next() );
						 assertEquals( type, group.Type() );
						 assertEquals( 2, group.OutgoingCount() );
						 assertEquals( 0, group.IncomingCount() );
						 assertEquals( 0, group.LoopCount() );
						 AssertRelationships( OUT, group, traversal, newRelationship, existingRelationship );

						 assertFalse( group.next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupCursorShouldSeeBothOldAndNewRelationshipsFromSparseNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GroupCursorShouldSeeBothOldAndNewRelationshipsFromSparseNode()
		 {
			  long start;
			  long existingRelationship;
			  int one;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					start = write.NodeCreate();
					one = tx.TokenWrite().relationshipTypeGetOrCreateForName("ONE");
					existingRelationship = write.RelationshipCreate( start, one, write.NodeCreate() );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					int two = tx.TokenWrite().relationshipTypeGetOrCreateForName("TWO");
					long newRelationship = write.RelationshipCreate( start, two, write.NodeCreate() );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor traversal = tx.Cursors().allocateRelationshipTraversalCursor(), RelationshipGroupCursor group = tx.Cursors().allocateRelationshipGroupCursor() )
					{
						 Read read = tx.DataRead();
						 read.SingleNode( start, node );
						 assertTrue( node.Next() );
						 assertFalse( node.Dense );
						 node.Relationships( group );

						 while ( group.next() )
						 {
							  int t = group.Type();
							  if ( t == one )
							  {
									assertEquals( 1, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
									AssertRelationships( OUT, group, traversal, existingRelationship );
							  }
							  else if ( t == two )
							  {
									assertEquals( 1, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
									AssertRelationships( OUT, group, traversal, newRelationship );

							  }
							  else
							  {
									fail( t + "  is not the type you're looking for " );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupCursorShouldSeeBothOldAndNewRelationshipsFromDenseNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GroupCursorShouldSeeBothOldAndNewRelationshipsFromDenseNode()
		 {
			  long start;
			  long existingRelationship;
			  int one, bulk;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					start = write.NodeCreate();
					one = tx.TokenWrite().relationshipTypeGetOrCreateForName("ONE");
					existingRelationship = write.RelationshipCreate( start, one, write.NodeCreate() );
					bulk = tx.TokenWrite().relationshipTypeGetOrCreateForName("BULK");
					for ( int i = 0; i < 100; i++ )
					{
						 write.RelationshipCreate( start, bulk, write.NodeCreate() );
					}
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					int two = tx.TokenWrite().relationshipTypeGetOrCreateForName("TWO");
					long newRelationship = write.RelationshipCreate( start, two, write.NodeCreate() );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor traversal = tx.Cursors().allocateRelationshipTraversalCursor(), RelationshipGroupCursor group = tx.Cursors().allocateRelationshipGroupCursor() )
					{
						 Read read = tx.DataRead();
						 read.SingleNode( start, node );
						 assertTrue( node.Next() );
						 assertTrue( node.Dense );
						 node.Relationships( group );

						 while ( group.next() )
						 {
							  int t = group.Type();
							  if ( t == one )
							  {
									assertEquals( 1, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
									AssertRelationships( OUT, group, traversal, existingRelationship );

							  }
							  else if ( t == two )
							  {
									assertEquals( 1, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
									AssertRelationships( OUT, group, traversal, newRelationship );
							  }
							  else if ( t == bulk )
							  {
									assertEquals( 100, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
							  }
							  else
							  {
									fail( t + "  is not the type you're looking for " );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupCursorShouldNewRelationshipBetweenAlreadyConnectedSparseNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GroupCursorShouldNewRelationshipBetweenAlreadyConnectedSparseNodes()
		 {
			  long start;
			  long end;
			  long existingRelationship;
			  int type;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					start = write.NodeCreate();
					end = write.NodeCreate();
					type = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					existingRelationship = write.RelationshipCreate( start, type, end );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					long newRelationship = write.RelationshipCreate( start, type, write.NodeCreate() );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor traversal = tx.Cursors().allocateRelationshipTraversalCursor(), RelationshipGroupCursor group = tx.Cursors().allocateRelationshipGroupCursor() )
					{
						 Read read = tx.DataRead();
						 read.SingleNode( start, node );
						 assertTrue( node.Next() );
						 assertFalse( node.Dense );
						 node.Relationships( group );

						 assertTrue( group.next() );
						 assertEquals( type, group.Type() );
						 assertEquals( 2, group.OutgoingCount() );
						 assertEquals( 0, group.IncomingCount() );
						 assertEquals( 0, group.LoopCount() );
						 AssertRelationships( OUT, group, traversal, newRelationship, existingRelationship );

						 assertFalse( group.next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void groupCursorShouldNewRelationshipBetweenAlreadyConnectedDenseNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GroupCursorShouldNewRelationshipBetweenAlreadyConnectedDenseNodes()
		 {
			  long start;
			  long end;
			  long existingRelationship;
			  int type, bulk;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					start = write.NodeCreate();
					end = write.NodeCreate();
					type = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					existingRelationship = write.RelationshipCreate( start, type, end );
					bulk = tx.TokenWrite().relationshipTypeGetOrCreateForName("BULK");
					for ( int i = 0; i < 100; i++ )
					{
						 write.RelationshipCreate( start, bulk, write.NodeCreate() );
					}
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					long newRelationship = write.RelationshipCreate( start, type, write.NodeCreate() );

					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor traversal = tx.Cursors().allocateRelationshipTraversalCursor(), RelationshipGroupCursor group = tx.Cursors().allocateRelationshipGroupCursor() )
					{
						 Read read = tx.DataRead();
						 read.SingleNode( start, node );
						 assertTrue( node.Next() );
						 assertTrue( node.Dense );
						 node.Relationships( group );

						 while ( group.next() )
						 {
							  int t = group.Type();
							  if ( t == type )
							  {
									assertEquals( 2, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
									AssertRelationships( OUT, group, traversal, existingRelationship, newRelationship );

							  }
							  else if ( t == bulk )
							  {
									assertEquals( bulk, group.Type() );
									assertEquals( 100, group.OutgoingCount() );
									assertEquals( 0, group.IncomingCount() );
									assertEquals( 0, group.LoopCount() );
							  }
							  else
							  {
									fail( t + "  is not the type you're looking for " );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountNewRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCountNewRelationships()
		 {
			  int relationship;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					relationship = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					write.RelationshipCreate( write.NodeCreate(), relationship, write.NodeCreate() );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					write.RelationshipCreate( write.NodeCreate(), relationship, write.NodeCreate() );

					long countsTxState = tx.DataRead().countsForRelationship(-1, relationship, -1);
					long countsNoTxState = tx.DataRead().countsForRelationshipWithoutTxState(-1, relationship, -1);

					assertEquals( 2, countsTxState );
					assertEquals( 1, countsNoTxState );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCountRemovedRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCountRemovedRelationships()
		 {
			  int relationshipId;
			  long relationship;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					relationshipId = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					relationship = write.RelationshipCreate( write.NodeCreate(), relationshipId, write.NodeCreate() );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					write.RelationshipDelete( relationship );

					long countsTxState = tx.DataRead().countsForRelationship(-1, relationshipId, -1);
					long countsNoTxState = tx.DataRead().countsForRelationshipWithoutTxState(-1, relationshipId, -1);

					assertEquals( 0, countsTxState );
					assertEquals( 1, countsNoTxState );
			  }
		 }

		 private void AssertRelationships( RelationshipDirection direction, RelationshipGroupCursor group, RelationshipTraversalCursor traversal, params long[] relationships )
		 {
			  switch ( direction )
			  {
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.Out:
					group.Outgoing( traversal );
					break;
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.In:
					group.Incoming( traversal );
					break;
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.Loop:
					group.Loops( traversal );
					break;
			  default:
					throw new AssertionError( "Where is your god now!" );
			  }

			  MutableLongSet set = LongHashSet.newSetWith( relationships );
			  foreach ( long relationship in relationships )
			  {
					assertTrue( traversal.next() );
					assertTrue( set.contains( traversal.RelationshipReference() ) );
					set.remove( traversal.RelationshipReference() );
			  }
			  assertTrue( set.Empty );
			  assertFalse( traversal.next() );
		 }

		 private void AssertNoRelationships( RelationshipDirection direction, RelationshipGroupCursor group, RelationshipTraversalCursor traversal )
		 {
			  switch ( direction )
			  {
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.Out:
					group.Outgoing( traversal );
					assertFalse( traversal.next() );
					break;
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.In:
					group.Incoming( traversal );
					assertFalse( traversal.next() );
					break;
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.Loop:
					group.Loops( traversal );
					assertFalse( traversal.next() );
					break;
			  default:
					throw new AssertionError( "Where is your god now!" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void traverseWithoutGroups(RelationshipTestSupport.StartNode start, boolean detached) throws Exception
		 private void TraverseWithoutGroups( RelationshipTestSupport.StartNode start, bool detached )
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					IDictionary<string, int> expectedCounts = ModifyStartNodeRelationships( start, tx );

					// given
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipTraversalCursor relationship = tx.Cursors().allocateRelationshipTraversalCursor() )
					{
						 // when
						 tx.DataRead().singleNode(start.Id, node);

						 assertTrue( "access node", node.Next() );
						 if ( detached )
						 {
							  tx.DataRead().relationships(start.Id, node.AllRelationshipsReference(), relationship);
						 }
						 else
						 {
							  node.AllRelationships( relationship );
						 }

						 IDictionary<string, int> counts = count( tx, relationship );

						 // then
						 assertCounts( expectedCounts, counts );
					}

					tx.Failure();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void traverseViaGroups(RelationshipTestSupport.StartNode start, boolean detached) throws Exception
		 private void TraverseViaGroups( RelationshipTestSupport.StartNode start, bool detached )
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					Read read = tx.DataRead();
					IDictionary<string, int> expectedCounts = ModifyStartNodeRelationships( start, tx );

					// given
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipGroupCursor group = tx.Cursors().allocateRelationshipGroupCursor(), RelationshipTraversalCursor relationship = tx.Cursors().allocateRelationshipTraversalCursor() )
					{
						 // when
						 read.SingleNode( start.Id, node );
						 assertTrue( "access node", node.Next() );
						 if ( detached )
						 {
							  read.RelationshipGroups( start.Id, node.RelationshipGroupReference(), group );
						 }
						 else
						 {
							  node.Relationships( group );
						 }

						 while ( group.next() )
						 {
							  // outgoing
							  if ( detached )
							  {
									read.Relationships( start.Id, group.OutgoingReference(), relationship );
							  }
							  else
							  {
									group.Outgoing( relationship );
							  }
							  // then
							  RelationshipTestSupport.AssertCount( tx, relationship, expectedCounts, group.Type(), OUTGOING );

							  // incoming
							  if ( detached )
							  {
									read.Relationships( start.Id, group.IncomingReference(), relationship );
							  }
							  else
							  {
									group.Incoming( relationship );
							  }
							  // then
							  RelationshipTestSupport.AssertCount( tx, relationship, expectedCounts, group.Type(), INCOMING );

							  // loops
							  if ( detached )
							  {
									read.Relationships( start.Id, group.LoopsReference(), relationship );
							  }
							  else
							  {
									group.Loops( relationship );
							  }
							  // then
							  RelationshipTestSupport.AssertCount( tx, relationship, expectedCounts, group.Type(), BOTH );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Map<String,int> modifyStartNodeRelationships(RelationshipTestSupport.StartNode start, Transaction tx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private IDictionary<string, int> ModifyStartNodeRelationships( RelationshipTestSupport.StartNode start, Transaction tx )
		 {
			  IDictionary<string, int> expectedCounts = new Dictionary<string, int>();
			  foreach ( KeyValuePair<string, IList<RelationshipTestSupport.StartRelationship>> kv in start.Relationships.SetOfKeyValuePairs() )
			  {
					IList<RelationshipTestSupport.StartRelationship> rs = kv.Value;
					RelationshipTestSupport.StartRelationship head = rs[0];
					int type = tx.Token().relationshipType(head.Type.name());
					switch ( head.Direction.innerEnumValue )
					{
					case Neo4Net.Graphdb.Direction.InnerEnum.INCOMING:
						 tx.DataWrite().relationshipCreate(tx.DataWrite().nodeCreate(), type, start.Id);
						 tx.DataWrite().relationshipCreate(tx.DataWrite().nodeCreate(), type, start.Id);
						 break;
					case Neo4Net.Graphdb.Direction.InnerEnum.OUTGOING:
						 tx.DataWrite().relationshipCreate(start.Id, type, tx.DataWrite().nodeCreate());
						 tx.DataWrite().relationshipCreate(start.Id, type, tx.DataWrite().nodeCreate());
						 break;
					case Neo4Net.Graphdb.Direction.InnerEnum.BOTH:
						 tx.DataWrite().relationshipCreate(start.Id, type, start.Id);
						 tx.DataWrite().relationshipCreate(start.Id, type, start.Id);
						 break;
					default:
						 throw new System.InvalidOperationException( "Oh ye be cursed, foul checkstyle!" );
					}
					tx.DataWrite().relationshipDelete(head.Id);
					expectedCounts[kv.Key] = rs.Count + 1;
			  }

			  string newTypeName = "NEW";
			  int newType = tx.Token().relationshipTypeGetOrCreateForName(newTypeName);
			  tx.DataWrite().relationshipCreate(tx.DataWrite().nodeCreate(), newType, start.Id);
			  tx.DataWrite().relationshipCreate(start.Id, newType, tx.DataWrite().nodeCreate());
			  tx.DataWrite().relationshipCreate(start.Id, newType, start.Id);

			  expectedCounts[computeKey( newTypeName, OUTGOING )] = 1;
			  expectedCounts[computeKey( newTypeName, INCOMING )] = 1;
			  expectedCounts[computeKey( newTypeName, BOTH )] = 1;

			  return expectedCounts;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hasPropertiesShouldSeeNewlyCreatedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HasPropertiesShouldSeeNewlyCreatedProperties()
		 {
			  // Given
			  long relationship;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					int token = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					relationship = write.RelationshipCreate( write.NodeCreate(), token, write.NodeCreate() );
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					using ( RelationshipScanCursor cursor = tx.Cursors().allocateRelationshipScanCursor() )
					{
						 tx.DataRead().singleRelationship(relationship, cursor);
						 assertTrue( cursor.Next() );
						 assertFalse( HasProperties( cursor, tx ) );
						 tx.DataWrite().relationshipSetProperty(relationship, tx.TokenWrite().propertyKeyGetOrCreateForName("prop"), stringValue("foo"));
						 assertTrue( HasProperties( cursor, tx ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hasPropertiesShouldSeeNewlyCreatedPropertiesOnNewlyCreatedRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HasPropertiesShouldSeeNewlyCreatedPropertiesOnNewlyCreatedRelationship()
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					int token = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					long relationship = write.RelationshipCreate( write.NodeCreate(), token, write.NodeCreate() );
					using ( RelationshipScanCursor cursor = tx.Cursors().allocateRelationshipScanCursor() )
					{
						 tx.DataRead().singleRelationship(relationship, cursor);
						 assertTrue( cursor.Next() );
						 assertFalse( HasProperties( cursor, tx ) );
						 tx.DataWrite().relationshipSetProperty(relationship, tx.TokenWrite().propertyKeyGetOrCreateForName("prop"), stringValue("foo"));
						 assertTrue( HasProperties( cursor, tx ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hasPropertiesShouldSeeNewlyRemovedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HasPropertiesShouldSeeNewlyRemovedProperties()
		 {
			  // Given
			  long relationship;
			  int prop1, prop2, prop3;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					int token = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					relationship = write.RelationshipCreate( write.NodeCreate(), token, write.NodeCreate() );
					prop1 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop1");
					prop2 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop2");
					prop3 = tx.TokenWrite().propertyKeyGetOrCreateForName("prop3");
					tx.DataWrite().relationshipSetProperty(relationship, prop1, longValue(1));
					tx.DataWrite().relationshipSetProperty(relationship, prop2, longValue(2));
					tx.DataWrite().relationshipSetProperty(relationship, prop3, longValue(3));
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					using ( RelationshipScanCursor cursor = tx.Cursors().allocateRelationshipScanCursor() )
					{
						 tx.DataRead().singleRelationship(relationship, cursor);
						 assertTrue( cursor.Next() );

						 assertTrue( HasProperties( cursor, tx ) );
						 tx.DataWrite().relationshipRemoveProperty(relationship, prop1);
						 assertTrue( HasProperties( cursor, tx ) );
						 tx.DataWrite().relationshipRemoveProperty(relationship, prop2);
						 assertTrue( HasProperties( cursor, tx ) );
						 tx.DataWrite().relationshipRemoveProperty(relationship, prop3);
						 assertFalse( HasProperties( cursor, tx ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void propertyTypeShouldBeTxStateAware() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PropertyTypeShouldBeTxStateAware()
		 {
			  // Given
			  long relationship;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					int token = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					relationship = write.RelationshipCreate( write.NodeCreate(), token, write.NodeCreate() );
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = beginTransaction() )
			  {
					using ( RelationshipScanCursor relationships = tx.Cursors().allocateRelationshipScanCursor(), PropertyCursor properties = tx.Cursors().allocatePropertyCursor() )
					{
						 tx.DataRead().singleRelationship(relationship, relationships);
						 assertTrue( relationships.Next() );
						 assertFalse( HasProperties( relationships, tx ) );
						 int prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
						 tx.DataWrite().relationshipSetProperty(relationship, prop, stringValue("foo"));
						 relationships.Properties( properties );

						 assertTrue( properties.Next() );
						 assertThat( properties.PropertyType(), equalTo(ValueGroup.TEXT) );
					}
			  }
		 }

		 private bool HasProperties( RelationshipScanCursor cursor, Transaction tx )
		 {
			  using ( PropertyCursor propertyCursor = tx.Cursors().allocatePropertyCursor() )
			  {
					cursor.Properties( propertyCursor );
					return propertyCursor.Next();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void relateNTimes(int nRelationshipsInStore, int type, long n1, long n2, Transaction tx) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void RelateNTimes( int nRelationshipsInStore, int type, long n1, long n2, Transaction tx )
		 {
			  for ( int i = 0; i < nRelationshipsInStore; i++ )
			  {
					tx.DataWrite().relationshipCreate(n1, type, n2);
			  }
		 }

		 private void AssertCountRelationships( RelationshipScanCursor relationship, int expectedCount, long sourceNode, int type, long targetNode )
		 {
			  int count = 0;
			  while ( relationship.Next() )
			  {
					assertEquals( sourceNode, relationship.SourceNodeReference() );
					assertEquals( type, relationship.Type() );
					assertEquals( targetNode, relationship.TargetNodeReference() );
					count++;
			  }
			  assertEquals( expectedCount, count );
		 }

		 internal enum RelationshipDirection
		 {
			  Out,
			  In,
			  Loop
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertCount(int count, RelationshipDirection direction, System.Action<RelationshipGroupCursor> asserter) throws Exception
		 private void AssertCount( int count, RelationshipDirection direction, System.Action<RelationshipGroupCursor> asserter )
		 {
			  long start;
			  int type;
			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					start = write.NodeCreate();
					type = tx.TokenWrite().relationshipTypeGetOrCreateForName("R");
					for ( int i = 0; i < count; i++ )
					{
						 CreateRelationship( direction, start, type, write );
					}
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					CreateRelationship( direction, start, type, write );
					using ( NodeCursor node = tx.Cursors().allocateNodeCursor(), RelationshipGroupCursor group = tx.Cursors().allocateRelationshipGroupCursor() )
					{
						 Read read = tx.DataRead();
						 read.SingleNode( start, node );
						 assertTrue( node.Next() );
						 node.Relationships( group );
						 assertTrue( group.next() );
						 asserter( group );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createRelationship(RelationshipDirection direction, long start, int type, Write write) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 private void CreateRelationship( RelationshipDirection direction, long start, int type, Write write )
		 {
			  switch ( direction )
			  {
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.Out:
					write.RelationshipCreate( start, type, write.NodeCreate() );
					break;
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.In:
					write.RelationshipCreate( write.NodeCreate(), type, start );
					break;
			  case Neo4Net.Internal.Kernel.Api.RelationshipTransactionStateTestBase.RelationshipDirection.Loop:
					write.RelationshipCreate( start, type, start );
					break;
			  default:
					throw new System.InvalidOperationException( "Checkstyle, you win again!" );
			  }
		 }
	}

}