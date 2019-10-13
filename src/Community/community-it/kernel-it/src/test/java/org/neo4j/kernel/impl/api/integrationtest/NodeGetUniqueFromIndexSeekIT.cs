using System;
using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using Read = Neo4Net.@internal.Kernel.Api.Read;
	using TokenWrite = Neo4Net.@internal.Kernel.Api.TokenWrite;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using Write = Neo4Net.@internal.Kernel.Api.Write;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexQuery.exact;

	public class NodeGetUniqueFromIndexSeekIT : KernelIntegrationTest
	{
		 private int _labelId;
		 private int _propertyId1;
		 private int _propertyId2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateKeys()
		 {
			  TokenWrite tokenWrite = TokenWriteInNewTransaction();
			  this._labelId = tokenWrite.LabelGetOrCreateForName( "Person" );
			  this._propertyId1 = tokenWrite.PropertyKeyGetOrCreateForName( "foo" );
			  this._propertyId2 = tokenWrite.PropertyKeyGetOrCreateForName( "bar" );
			  Commit();
		 }

		 // nodeGetUniqueWithLabelAndProperty(statement, :Person, foo=val)
		 //
		 // Given we have a unique constraint on :Person(foo)
		 // (If not, throw)
		 //
		 // If there is a node n with n:Person and n.foo == val, return it
		 // If there is no such node, return ?
		 //
		 // Ensure that if that method is called again with the same argument from some other transaction,
		 // that transaction blocks until this transaction has finished
		 //

		 // [X] must return node from the unique index with the given property
		 // [X] must return NO_SUCH_NODE if it is not in the index for the given property
		 //
		 // must block other transactions that try to call it with the same arguments

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindMatchingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindMatchingNode()
		 {
			  // given
			  IndexReference index = CreateUniquenessConstraint( _labelId, _propertyId1 );
			  Value value = Values.of( "value" );
			  long nodeId = CreateNodeWithValue( value );

			  // when looking for it
			  Read read = NewTransaction().dataRead();
			  int propertyId = index.Properties()[0];
			  long foundId = read.LockingNodeUniqueIndexSeek( index, exact( propertyId, value ) );
			  Commit();

			  // then
			  assertEquals( "Created node was not found", nodeId, foundId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindNonMatchingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNonMatchingNode()
		 {
			  // given
			  IndexReference index = CreateUniquenessConstraint( _labelId, _propertyId1 );
			  Value value = Values.of( "value" );
			  CreateNodeWithValue( Values.of( "other_" + value ) );

			  // when looking for it
			  Transaction transaction = NewTransaction();
			  long foundId = transaction.DataRead().lockingNodeUniqueIndexSeek(index, exact(_propertyId1, value));
			  Commit();

			  // then
			  assertTrue( "Non-matching created node was found", IsNoSuchNode( foundId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompositeFindMatchingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompositeFindMatchingNode()
		 {
			  // given
			  IndexReference index = CreateUniquenessConstraint( _labelId, _propertyId1, _propertyId2 );
			  Value value1 = Values.of( "value1" );
			  Value value2 = Values.of( "value2" );
			  long nodeId = CreateNodeWithValues( value1, value2 );

			  // when looking for it
			  Transaction transaction = NewTransaction();
			  long foundId = transaction.DataRead().lockingNodeUniqueIndexSeek(index, exact(_propertyId1, value1), exact(_propertyId2, value2));
			  Commit();

			  // then
			  assertEquals( "Created node was not found", nodeId, foundId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCompositeFindNonMatchingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCompositeFindNonMatchingNode()
		 {
			  // given
			  IndexReference index = CreateUniquenessConstraint( _labelId, _propertyId1, _propertyId2 );
			  Value value1 = Values.of( "value1" );
			  Value value2 = Values.of( "value2" );
			  CreateNodeWithValues( Values.of( "other_" + value1 ), Values.of( "other_" + value2 ) );

			  // when looking for it
			  Transaction transaction = NewTransaction();
			  long foundId = transaction.DataRead().lockingNodeUniqueIndexSeek(index, exact(_propertyId1, value1), exact(_propertyId2, value2));
			  Commit();

			  // then
			  assertTrue( "Non-matching created node was found", IsNoSuchNode( foundId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void shouldBlockUniqueIndexSeekFromCompetingTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBlockUniqueIndexSeekFromCompetingTransaction()
		 {
			  // This is the interleaving that we are trying to verify works correctly:
			  // ----------------------------------------------------------------------
			  // Thread1 (main)        : Thread2
			  // create unique node    :
			  // lookup(node)          :
			  // open start latch ----->
			  //    |                  : lookup(node)
			  // wait for T2 to block  :      |
			  //                       :    *block*
			  // commit --------------->   *unblock*
			  // wait for T2 end latch :      |
			  //                       : finish transaction
			  //                       : open end latch
			  // *unblock* <-------------‘
			  // assert that we complete before timeout
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch latch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch latch = new DoubleLatch();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.IndexReference index = createUniquenessConstraint(labelId, propertyId1);
			  IndexReference index = CreateUniquenessConstraint( _labelId, _propertyId1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.Value value = org.neo4j.values.storable.Values.of("value");
			  Value value = Values.of( "value" );

			  Write write = DataWriteInNewTransaction();
			  long nodeId = write.NodeCreate();
			  write.NodeAddLabel( nodeId, _labelId );

			  // This adds the node to the unique index and should take an index write lock
			  write.NodeSetProperty( nodeId, _propertyId1, value );

			  ThreadStart runnableForThread2 = () =>
			  {
				latch.WaitForAllToStart();
				try
				{
					using ( Transaction tx = Kernel.beginTransaction( Transaction.Type.@implicit, LoginContext.AUTH_DISABLED ) )
					{
						 tx.dataRead().lockingNodeUniqueIndexSeek(index, exact(_propertyId1, value));
						 tx.success();
					}
				}
				catch ( KernelException e )
				{
					 throw new Exception( e );
				}
				finally
				{
					 latch.Finish();
				}
			  };
			  Thread thread2 = new Thread( runnableForThread2, "Transaction Thread 2" );
			  thread2.Start();
			  latch.StartAndWaitForAllToStart();

			  while ( ( thread2.State != Thread.State.TIMED_WAITING ) && ( thread2.State != Thread.State.WAITING ) )
			  {
					Thread.yield();
			  }

			  Commit();
			  latch.WaitForAllToFinish();
		 }

		 private bool IsNoSuchNode( long foundId )
		 {
			  return StatementConstants.NO_SUCH_NODE == foundId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNodeWithValue(org.neo4j.values.storable.Value value) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreateNodeWithValue( Value value )
		 {
			  Write write = DataWriteInNewTransaction();
			  long nodeId = write.NodeCreate();
			  write.NodeAddLabel( nodeId, _labelId );
			  write.NodeSetProperty( nodeId, _propertyId1, value );
			  Commit();
			  return nodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNodeWithValues(org.neo4j.values.storable.Value value1, org.neo4j.values.storable.Value value2) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreateNodeWithValues( Value value1, Value value2 )
		 {
			  Write write = DataWriteInNewTransaction();
			  long nodeId = write.NodeCreate();
			  write.NodeAddLabel( nodeId, _labelId );
			  write.NodeSetProperty( nodeId, _propertyId1, value1 );
			  write.NodeSetProperty( nodeId, _propertyId2, value2 );
			  Commit();
			  return nodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference createUniquenessConstraint(int labelId, int... propertyIds) throws Exception
		 private IndexReference CreateUniquenessConstraint( int labelId, params int[] propertyIds )
		 {
			  Transaction transaction = NewTransaction( LoginContext.AUTH_DISABLED );
			  LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( labelId, propertyIds );
			  transaction.SchemaWrite().uniquePropertyConstraintCreate(descriptor);
			  IndexReference result = transaction.SchemaRead().index(descriptor.LabelId, descriptor.PropertyIds);
			  Commit();
			  return result;
		 }
	}

}