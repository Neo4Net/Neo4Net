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
namespace Neo4Net.Kernel.impl.coreapi
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using LabelEntry = Neo4Net.Graphdb.@event.LabelEntry;
	using Neo4Net.Graphdb.@event;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using PropertyKeyIdNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.PropertyKeyIdNotFoundKernelException;
	using AccessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode;
	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using TxState = Neo4Net.Kernel.Impl.Api.state.TxState;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using NodeProxy = Neo4Net.Kernel.impl.core.NodeProxy;
	using RelationshipProxy = Neo4Net.Kernel.impl.core.RelationshipProxy;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using StubStorageCursors = Neo4Net.Storageengine.Api.StubStorageCursors;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.genericMap;

	public class TxStateTransactionDataViewTest
	{
		 private static readonly long[] _noLabels = new long[0];
		 private readonly ThreadToStatementContextBridge _bridge = mock( typeof( ThreadToStatementContextBridge ) );
		 private readonly Statement _stmt = mock( typeof( Statement ) );
		 private readonly StubStorageCursors _ops = new StubStorageCursors();
		 private readonly KernelTransaction _transaction = mock( typeof( KernelTransaction ) );
		 private readonly TokenRead _tokenRead = mock( typeof( TokenRead ) );

		 private readonly TransactionState _state = new TxState();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws org.neo4j.internal.kernel.api.exceptions.PropertyKeyIdNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  when( _transaction.tokenRead() ).thenReturn(_tokenRead);
			  when( _bridge.get() ).thenReturn(_stmt);
			  when( _tokenRead.propertyKeyName( anyInt() ) ).thenAnswer(invocationOnMock =>
			  {
				int id = invocationOnMock.getArgument( 0 );
				return _ops.propertyKeyTokenHolder().getTokenById(id).name();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void showsCreatedNodes()
		 public virtual void ShowsCreatedNodes()
		 {
			  // Given
			  _state.nodeDoCreate( 1 );
			  _state.nodeDoCreate( 2 );

			  // When & Then
			  assertThat( IdList( Snapshot().createdNodes() ), equalTo(asList(1L, 2L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void showsDeletedNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShowsDeletedNodes()
		 {
			  // Given
			  _state.nodeDoDelete( 1L );
			  _state.nodeDoDelete( 2L );

			  int labelId = 15;
			  when( _tokenRead.nodeLabelName( labelId ) ).thenReturn( "label" );

			  _ops.withNode( 1, new long[]{ labelId }, genericMap( "key", Values.of( "p" ) ) );
			  _ops.withNode( 2, _noLabels );

			  // When & Then
			  TxStateTransactionDataSnapshot snapshot = snapshot();
			  assertThat( IdList( snapshot.DeletedNodes() ), equalTo(asList(1L, 2L)) );
			  assertThat( single( snapshot.RemovedLabels() ).label().name(), equalTo("label") );
			  assertThat( single( snapshot.RemovedNodeProperties() ).key(), equalTo("key") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void showsAddedRelationships()
		 public virtual void ShowsAddedRelationships()
		 {
			  // Given
			  _state.relationshipDoCreate( 1, 1, 1L, 2L );
			  _state.relationshipDoCreate( 2, 1, 1L, 1L );

			  // When & Then
			  assertThat( IdList( Snapshot().createdRelationships() ), equalTo(asList(1L, 2L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void showsRemovedRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShowsRemovedRelationships()
		 {
			  // Given
			  _state.relationshipDoDelete( 1L, 1, 1L, 2L );
			  _state.relationshipDoDelete( 2L, 1, 1L, 1L );

			  _ops.withRelationship( 1, 1, 1, 2 );
			  _ops.withRelationship( 2, 1, 1, 1, genericMap( "key", Values.of( "p" ) ) );

			  // When & Then
			  TxStateTransactionDataSnapshot snapshot = snapshot();
			  assertThat( IdList( snapshot.DeletedRelationships() ), equalTo(asList(1L, 2L)) );
			  assertThat( single( snapshot.RemovedRelationshipProperties() ).key(), equalTo("key") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctlySaysNodeIsDeleted()
		 public virtual void CorrectlySaysNodeIsDeleted()
		 {
			  // Given
			  _state.nodeDoDelete( 1L );
			  Node node = mock( typeof( Node ) );
			  when( node.Id ).thenReturn( 1L );
			  _ops.withNode( 1 );

			  // When & Then
			  assertThat( Snapshot().isDeleted(node), equalTo(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctlySaysRelIsDeleted()
		 public virtual void CorrectlySaysRelIsDeleted()
		 {
			  // Given
			  _state.relationshipDoDelete( 1L, 1, 1L, 2L );

			  Relationship rel = mock( typeof( Relationship ) );
			  when( rel.Id ).thenReturn( 1L );
			  _ops.withRelationship( 1L, 1L, 1, 2L );

			  // When & Then
			  assertThat( Snapshot().isDeleted(rel), equalTo(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedNodePropertiesProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAddedNodePropertiesProperties()
		 {
			  // Given
			  int propertyKeyId = _ops.propertyKeyTokenHolder().getOrCreateId("theKey");
			  Value prevValue = Values.of( "prevValue" );
			  _state.nodeDoChangeProperty( 1L, propertyKeyId, Values.of( "newValue" ) );
			  _ops.withNode( 1, _noLabels, genericMap( "theKey", prevValue ) );

			  // When
			  IEnumerable<PropertyEntry<Node>> propertyEntries = Snapshot().assignedNodeProperties();

			  // Then
			  PropertyEntry<Node> entry = single( propertyEntries );
			  assertThat( entry.Key(), equalTo("theKey") );
			  assertThat( entry.Value(), equalTo("newValue") );
			  assertThat( entry.PreviouslyCommitedValue(), equalTo("prevValue") );
			  assertThat( entry.Entity().Id, equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRemovedNodeProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListRemovedNodeProperties()
		 {
			  // Given
			  int propertyKeyId = _ops.propertyKeyTokenHolder().getOrCreateId("theKey");
			  Value prevValue = Values.of( "prevValue" );
			  _state.nodeDoRemoveProperty( 1L, propertyKeyId );
			  _ops.withNode( 1, _noLabels, genericMap( "theKey", prevValue ) );

			  // When
			  IEnumerable<PropertyEntry<Node>> propertyEntries = Snapshot().removedNodeProperties();

			  // Then
			  PropertyEntry<Node> entry = single( propertyEntries );
			  assertThat( entry.Key(), equalTo("theKey") );
			  assertThat( entry.PreviouslyCommitedValue(), equalTo("prevValue") );
			  assertThat( entry.Entity().Id, equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRemovedRelationshipProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListRemovedRelationshipProperties()
		 {
			  // Given
			  int propertyKeyId = _ops.propertyKeyTokenHolder().getOrCreateId("theKey");
			  Value prevValue = Values.of( "prevValue" );
			  _state.relationshipDoRemoveProperty( 1L, propertyKeyId );
			  _ops.withRelationship( 1, 0, 0, 0, genericMap( "theKey", prevValue ) );

			  // When
			  IEnumerable<PropertyEntry<Relationship>> propertyEntries = Snapshot().removedRelationshipProperties();

			  // Then
			  PropertyEntry<Relationship> entry = single( propertyEntries );
			  assertThat( entry.Key(), equalTo("theKey") );
			  assertThat( entry.PreviouslyCommitedValue(), equalTo("prevValue") );
			  assertThat( entry.Entity().Id, equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedRelationshipProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAddedRelationshipProperties()
		 {
			  // Given
			  Value prevValue = Values.of( "prevValue" );
			  int propertyKeyId = _ops.propertyKeyTokenHolder().getOrCreateId("theKey");
			  _state.relationshipDoReplaceProperty( 1L, propertyKeyId, prevValue, Values.of( "newValue" ) );
			  _ops.withRelationship( 1, 0, 0, 0, genericMap( "theKey", prevValue ) );

			  // When
			  IEnumerable<PropertyEntry<Relationship>> propertyEntries = Snapshot().assignedRelationshipProperties();

			  // Then
			  PropertyEntry<Relationship> entry = single( propertyEntries );
			  assertThat( entry.Key(), equalTo("theKey") );
			  assertThat( entry.Value(), equalTo("newValue") );
			  assertThat( entry.PreviouslyCommitedValue(), equalTo("prevValue") );
			  assertThat( entry.Entity().Id, equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAddedLabels()
		 {
			  // Given
			  int labelId = 2;
			  when( _tokenRead.nodeLabelName( labelId ) ).thenReturn( "theLabel" );
			  _state.nodeDoAddLabel( labelId, 1L );

			  // When
			  IEnumerable<LabelEntry> labelEntries = Snapshot().assignedLabels();

			  // Then
			  LabelEntry entry = single( labelEntries );
			  assertThat( entry.Label().name(), equalTo("theLabel") );
			  assertThat( entry.Node().Id, equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRemovedLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListRemovedLabels()
		 {
			  // Given
			  int labelId = 2;
			  when( _tokenRead.nodeLabelName( labelId ) ).thenReturn( "theLabel" );
			  _state.nodeDoRemoveLabel( labelId, 1L );

			  // When
			  IEnumerable<LabelEntry> labelEntries = Snapshot().removedLabels();

			  // Then
			  LabelEntry entry = single( labelEntries );
			  assertThat( entry.Label().name(), equalTo("theLabel") );
			  assertThat( entry.Node().Id, equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void accessTransactionIdAndCommitTime()
		 public virtual void AccessTransactionIdAndCommitTime()
		 {
			  long committedTransactionId = 7L;
			  long commitTime = 10L;
			  when( _transaction.TransactionId ).thenReturn( committedTransactionId );
			  when( _transaction.CommitTime ).thenReturn( commitTime );

			  TxStateTransactionDataSnapshot transactionDataSnapshot = Snapshot();
			  assertEquals( committedTransactionId, transactionDataSnapshot.TransactionId );
			  assertEquals( commitTime, transactionDataSnapshot.CommitTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetEmptyUsernameForAnonymousContext()
		 public virtual void ShouldGetEmptyUsernameForAnonymousContext()
		 {
			  when( _transaction.securityContext() ).thenReturn(AnonymousContext.read().authorize(s => -1, GraphDatabaseSettings.DEFAULT_DATABASE_NAME));

			  TxStateTransactionDataSnapshot transactionDataSnapshot = Snapshot();
			  assertEquals( "", transactionDataSnapshot.Username() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessUsernameFromAuthSubject()
		 public virtual void ShouldAccessUsernameFromAuthSubject()
		 {
			  AuthSubject authSubject = mock( typeof( AuthSubject ) );
			  when( authSubject.Username() ).thenReturn("Christof");
			  when( _transaction.securityContext() ).thenReturn(new SecurityContext(authSubject, Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Full));

			  TxStateTransactionDataSnapshot transactionDataSnapshot = Snapshot();
			  assertEquals( "Christof", transactionDataSnapshot.Username() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessEmptyMetaData()
		 public virtual void ShouldAccessEmptyMetaData()
		 {
			  TxStateTransactionDataSnapshot transactionDataSnapshot = Snapshot();
			  assertEquals( 0, transactionDataSnapshot.MetaData().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessExampleMetaData()
		 public virtual void ShouldAccessExampleMetaData()
		 {
			  EmbeddedProxySPI spi = mock( typeof( EmbeddedProxySPI ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.api.KernelTransactionImplementation transaction = mock(org.neo4j.kernel.impl.api.KernelTransactionImplementation.class);
			  KernelTransactionImplementation transaction = mock( typeof( KernelTransactionImplementation ) );
			  when( transaction.MetaData ).thenReturn( genericMap( "username", "Igor" ) );
			  TxStateTransactionDataSnapshot transactionDataSnapshot = new TxStateTransactionDataSnapshot( _state, spi, _ops, transaction );
			  assertEquals( 1, transactionDataSnapshot.MetaData().Count );
			  assertThat( "Expected metadata map to contain defined username", transactionDataSnapshot.MetaData(), equalTo(genericMap("username", "Igor")) );
		 }

		 private IList<long> IdList<T1>( IEnumerable<T1> entities ) where T1 : Neo4Net.Graphdb.PropertyContainer
		 {
			  IList<long> @out = new List<long>();
			  foreach ( PropertyContainer entity in entities )
			  {
					@out.Add( entity is Node ? ( ( Node ) entity ).Id : ( ( Relationship ) entity ).Id );
			  }
			  return @out;
		 }

		 private TxStateTransactionDataSnapshot Snapshot()
		 {
			  EmbeddedProxySPI spi = mock( typeof( EmbeddedProxySPI ) );
			  when( spi.NewNodeProxy( anyLong() ) ).thenAnswer(invocation => new NodeProxy(spi, invocation.getArgument(0)));
			  when( spi.NewRelationshipProxy( anyLong() ) ).thenAnswer(invocation => new RelationshipProxy(spi, invocation.getArgument(0)));
			  when( spi.NewRelationshipProxy( anyLong(), anyLong(), anyInt(), anyLong() ) ).thenAnswer(invocation => new RelationshipProxy(spi, invocation.getArgument(0), invocation.getArgument(1), invocation.getArgument(2), invocation.getArgument(3)));
			  return new TxStateTransactionDataSnapshot( _state, spi, _ops, _transaction );
		 }
	}

}