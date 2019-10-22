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
	using Test = org.junit.Test;

	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using SchemaWrite = Neo4Net.Internal.Kernel.Api.SchemaWrite;
	using TokenNameLookup = Neo4Net.Internal.Kernel.Api.TokenNameLookup;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using TokenWrite = Neo4Net.Internal.Kernel.Api.TokenWrite;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.IndexQuery.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;

	public class UniquenessConstraintValidationIT : KernelIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceOnSetProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnforceOnSetProperty()
		 {
			  // given
			  ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  long node = CreateLabeledNode( transaction, "Label1" );
			  try
			  {
					int propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName("key1");
					transaction.DataWrite().nodeSetProperty(node, propertyKeyId, Values.of("value1"));

					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( UniquePropertyValueValidationException e )
			  {
					assertThat( e.GetUserMessage( TokenLookup( transaction ) ), containsString( "`key1` = 'value1'" ) );
			  }
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundingErrorsFromLongToDoubleShouldNotPreventTxFromCommitting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RoundingErrorsFromLongToDoubleShouldNotPreventTxFromCommitting()
		 {
			  // Given
			  // a node with a constrained label and a long value
			  long propertyValue = 285414114323346805L;
			  long firstNode = ConstrainedNode( "label1", "key1", propertyValue );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  long node = CreateLabeledNode( transaction, "label1" );

			  assertNotEquals( firstNode, node );

			  // When
			  // a new node with the same constraint is added, with a value not equal but which would be mapped to the same double
			  propertyValue++;
			  // note how propertyValue is definitely not equal to propertyValue++ but they do equal if they are cast to double
			  int propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName("key1");
			  transaction.DataWrite().nodeSetProperty(node, propertyKeyId, Values.of(propertyValue));

			  // Then
			  // the commit should still succeed
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceUniquenessConstraintOnAddLabelForNumberPropertyOnNodeNotFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnforceUniquenessConstraintOnAddLabelForNumberPropertyOnNodeNotFromTransaction()
		 {
			  // given
			  ConstrainedNode( "Label1", "key1", 1 );

			  // when
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  long node = CreateNode( transaction, "key1", 1 );
			  Commit();

			  transaction = NewTransaction( AnonymousContext.writeToken() );
			  try
			  {
					int label = transaction.TokenWrite().labelGetOrCreateForName("Label1");
					transaction.DataWrite().nodeAddLabel(node, label);

					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( UniquePropertyValueValidationException e )
			  {
					assertThat( e.GetUserMessage( TokenLookup( transaction ) ), containsString( "`key1` = 1" ) );
			  }
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceUniquenessConstraintOnAddLabelForStringProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnforceUniquenessConstraintOnAddLabelForStringProperty()
		 {
			  // given
			  ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  long node = CreateNode( transaction, "key1", "value1" );
			  try
			  {
					int label = transaction.TokenWrite().labelGetOrCreateForName("Label1");
					transaction.DataWrite().nodeAddLabel(node, label);

					fail( "should have thrown exception" );
			  }
			  // then
			  catch ( UniquePropertyValueValidationException e )
			  {
					assertThat( e.GetUserMessage( TokenLookup( transaction ) ), containsString( "`key1` = 'value1'" ) );
			  }
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRemoveAndAddConflictingDataInOneTransaction_DeleteNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRemoveAndAddConflictingDataInOneTransactionDeleteNode()
		 {
			  // given
			  long node = ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  transaction.DataWrite().nodeDelete(node);
			  CreateLabeledNode( transaction, "Label1", "key1", "value1" );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRemoveAndAddConflictingDataInOneTransaction_RemoveLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRemoveAndAddConflictingDataInOneTransactionRemoveLabel()
		 {
			  // given
			  long node = ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  int label = transaction.TokenWrite().labelGetOrCreateForName("Label1");
			  transaction.DataWrite().nodeRemoveLabel(node, label);
			  CreateLabeledNode( transaction, "Label1", "key1", "value1" );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRemoveAndAddConflictingDataInOneTransaction_RemoveProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRemoveAndAddConflictingDataInOneTransactionRemoveProperty()
		 {
			  // given
			  long node = ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  int key = transaction.TokenRead().propertyKey("key1");
			  transaction.DataWrite().nodeRemoveProperty(node, key);
			  CreateLabeledNode( transaction, "Label1", "key1", "value1" );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRemoveAndAddConflictingDataInOneTransaction_ChangeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRemoveAndAddConflictingDataInOneTransactionChangeProperty()
		 {
			  // given
			  long node = ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  int propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName("key1");
			  transaction.DataWrite().nodeSetProperty(node, propertyKeyId, Values.of("value2"));
			  CreateLabeledNode( transaction, "Label1", "key1", "value1" );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPreventConflictingDataInSameTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPreventConflictingDataInSameTransaction()
		 {
			  // given
			  ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  CreateLabeledNode( transaction, "Label1", "key1", "value2" );
			  try
			  {
					CreateLabeledNode( transaction, "Label1", "key1", "value2" );

					fail( "expected exception" );
			  }
			  // then
			  catch ( UniquePropertyValueValidationException e )
			  {
					assertThat( e.GetUserMessage( TokenLookup( transaction ) ), containsString( "`key1` = 'value2'" ) );
			  }
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowNoopPropertyUpdate() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowNoopPropertyUpdate()
		 {
			  // given
			  long node = ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  int key = transaction.TokenWrite().propertyKeyGetOrCreateForName("key1");
			  transaction.DataWrite().nodeSetProperty(node, key, Values.of("value1"));

			  // then should not throw exception
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowNoopLabelUpdate() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowNoopLabelUpdate()
		 {
			  // given
			  long node = ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  int label = transaction.TokenWrite().labelGetOrCreateForName("Label1");
			  transaction.DataWrite().nodeAddLabel(node, label);

			  // then should not throw exception
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowCreationOfNonConflictingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowCreationOfNonConflictingData()
		 {
			  // given
			  ConstrainedNode( "Label1", "key1", "value1" );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  // when
			  CreateNode( transaction, "key1", "value1" );
			  CreateLabeledNode( transaction, "Label2", "key1", "value1" );
			  CreateLabeledNode( transaction, "Label1", "key1", "value2" );
			  CreateLabeledNode( transaction, "Label1", "key2", "value1" );

			  Commit();

			  // then
			  transaction = NewTransaction( AnonymousContext.writeToken() );
			  assertEquals( "number of nodes", 5, CountNodes( transaction ) );
			  Rollback();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unrelatedNodesWithSamePropertyShouldNotInterfereWithUniquenessCheck() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UnrelatedNodesWithSamePropertyShouldNotInterfereWithUniquenessCheck()
		 {
			  // given
			  CreateConstraint( "Person", "id" );

			  long ourNode;
			  {
					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
					ourNode = CreateLabeledNode( transaction, "Person", "id", 1 );
					CreateLabeledNode( transaction, "Item", "id", 2 );
					Commit();
			  }

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  TokenRead tokenRead = transaction.TokenRead();
			  int person = tokenRead.NodeLabel( "Person" );
			  int propId = tokenRead.PropertyKey( "id" );
			  IndexReference idx = transaction.SchemaRead().index(person, propId);

			  // when
			  CreateLabeledNode( transaction, "Item", "id", 2 );

			  // then I should find the original node
			  assertThat( transaction.DataRead().lockingNodeUniqueIndexSeek(idx, exact(propId, Values.of(1))), equalTo(ourNode) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingUniqueNodeWithUnrelatedValueShouldNotAffectLookup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddingUniqueNodeWithUnrelatedValueShouldNotAffectLookup()
		 {
			  // given
			  CreateConstraint( "Person", "id" );

			  long ourNode;
			  {
					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
					ourNode = CreateLabeledNode( transaction, "Person", "id", 1 );
					Commit();
			  }

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  TokenRead tokenRead = transaction.TokenRead();
			  int person = tokenRead.NodeLabel( "Person" );
			  int propId = tokenRead.PropertyKey( "id" );
			  IndexReference idx = transaction.SchemaRead().index(person, propId);

			  // when
			  CreateLabeledNode( transaction, "Person", "id", 2 );

			  // then I should find the original node
			  assertThat( transaction.DataRead().lockingNodeUniqueIndexSeek(idx, exact(propId, Values.of(1))), equalTo(ourNode) );
			  Commit();
		 }

		 private TokenNameLookup TokenLookup( Transaction transaction )
		 {
			  return new SilentTokenNameLookup( transaction.TokenRead() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createLabeledNode(org.Neo4Net.internal.kernel.api.Transaction transaction, String label) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private long CreateLabeledNode( Transaction transaction, string label )
		 {
			  long node = transaction.DataWrite().nodeCreate();
			  int labelId = transaction.TokenWrite().labelGetOrCreateForName(label);
			  transaction.DataWrite().nodeAddLabel(node, labelId);
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNode(org.Neo4Net.internal.kernel.api.Transaction transaction, String key, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private long CreateNode( Transaction transaction, string key, object value )
		 {
			  long node = transaction.DataWrite().nodeCreate();
			  int propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName(key);
			  transaction.DataWrite().nodeSetProperty(node, propertyKeyId, Values.of(value));
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createLabeledNode(org.Neo4Net.internal.kernel.api.Transaction transaction, String label, String key, Object value) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private long CreateLabeledNode( Transaction transaction, string label, string key, object value )
		 {
			  long node = CreateLabeledNode( transaction, label );
			  int propertyKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName(key);
			  transaction.DataWrite().nodeSetProperty(node, propertyKeyId, Values.of(value));
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long constrainedNode(String labelName, String propertyKey, Object propertyValue) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private long ConstrainedNode( string labelName, string propertyKey, object propertyValue )
		 {
			  long node;
			  {
					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
					int label = transaction.TokenWrite().labelGetOrCreateForName(labelName);
					node = transaction.DataWrite().nodeCreate();
					transaction.DataWrite().nodeAddLabel(node, label);
					int key = transaction.TokenWrite().propertyKeyGetOrCreateForName(propertyKey);
					transaction.DataWrite().nodeSetProperty(node, key, Values.of(propertyValue));
					Commit();
			  }
			  CreateConstraint( labelName, propertyKey );
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createConstraint(String label, String propertyKey) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private void CreateConstraint( string label, string propertyKey )
		 {
			  int labelId;
			  int propertyKeyId;
			  TokenWrite tokenWrite = TokenWriteInNewTransaction();
			  labelId = tokenWrite.LabelGetOrCreateForName( label );
			  propertyKeyId = tokenWrite.PropertyKeyGetOrCreateForName( propertyKey );
			  Commit();

			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
			  schemaWrite.UniquePropertyConstraintCreate( forLabel( labelId, propertyKeyId ) );
			  Commit();
		 }
	}

}