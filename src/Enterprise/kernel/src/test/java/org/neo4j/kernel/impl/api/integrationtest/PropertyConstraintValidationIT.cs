/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Suite = org.junit.runners.Suite;
	using SuiteClasses = org.junit.runners.Suite.SuiteClasses;

	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using SchemaWrite = Neo4Net.@internal.Kernel.Api.SchemaWrite;
	using TokenWrite = Neo4Net.@internal.Kernel.Api.TokenWrite;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using Write = Neo4Net.@internal.Kernel.Api.Write;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using ConstraintViolationTransactionFailureException = Neo4Net.Kernel.Api.Exceptions.ConstraintViolationTransactionFailureException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forRelType;
	using static Neo4Net.Kernel.Impl.Api.integrationtest.PropertyConstraintValidationIT.NodeKeyConstraintValidationIT;
	using static Neo4Net.Kernel.Impl.Api.integrationtest.PropertyConstraintValidationIT.NodePropertyExistenceConstraintValidationIT;
	using static Neo4Net.Kernel.Impl.Api.integrationtest.PropertyConstraintValidationIT.RelationshipPropertyExistenceConstraintValidationIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Suite.class) @SuiteClasses({ NodePropertyExistenceConstraintValidationIT.class, RelationshipPropertyExistenceConstraintValidationIT.class, NodeKeyConstraintValidationIT.class }) public class PropertyConstraintValidationIT
	public class PropertyConstraintValidationIT
	{
		 public class NodeKeyConstraintValidationIT : NodePropertyExistenceConstraintValidationIT
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createConstraint(String key, String property) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override void CreateConstraint( string key, string property )
			  {
					TokenWrite tokenWrite = TokenWriteInNewTransaction();
					int label = tokenWrite.LabelGetOrCreateForName( key );
					int propertyKey = tokenWrite.PropertyKeyGetOrCreateForName( property );
					Commit();

					SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
					schemaWrite.NodeKeyConstraintCreate( forLabel( label, propertyKey ) );
					Commit();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requirePropertyFromMultipleNodeKeys()
			  public virtual void RequirePropertyFromMultipleNodeKeys()
			  {
					Label label = Label.label( "multiNodeKeyLabel" );
					SchemaHelper.createNodeKeyConstraint( Db, label, "property1", "property2" );
					SchemaHelper.createNodeKeyConstraint( Db, label, "property2", "property3" );
					SchemaHelper.createNodeKeyConstraint( Db, label, "property3", "property4" );

					assertException(() =>
					{
					 using ( Neo4Net.Graphdb.Transaction transaction = Db.beginTx() )
					 {
						  Node node = Db.createNode( label );
						  node.setProperty( "property1", "1" );
						  node.setProperty( "property2", "2" );
						  transaction.Success();
					 }
					}, typeof( ConstraintViolationException ), "Node(0) with label `multiNodeKeyLabel` must have the properties `property2, property3`");

					assertException(() =>
					{
					 using ( Neo4Net.Graphdb.Transaction transaction = Db.beginTx() )
					 {
						  Node node = Db.createNode( label );
						  node.setProperty( "property1", "1" );
						  node.setProperty( "property2", "2" );
						  node.setProperty( "property3", "3" );
						  transaction.Success();
					 }
					}, typeof( ConstraintViolationException ), "Node(1) with label `multiNodeKeyLabel` must have the properties `property3, property4`");
			  }
		 }

		 public class NodePropertyExistenceConstraintValidationIT : AbstractPropertyExistenceConstraintValidationIT
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowNoopLabelUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldAllowNoopLabelUpdate()
			  {
					// given
					long entityId = CreateConstraintAndEntity( "Label1", "key1", "value1" );

					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

					// when
					int label = transaction.TokenWrite().labelGetOrCreateForName("Label1");
					transaction.DataWrite().nodeAddLabel(entityId, label);

					// then should not throw exception
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createConstraint(String key, String property) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override void CreateConstraint( string key, string property )
			  {
					TokenWrite tokenWrite = TokenWriteInNewTransaction();
					int label = tokenWrite.LabelGetOrCreateForName( key );
					int propertyKey = tokenWrite.PropertyKeyGetOrCreateForName( property );
					Commit();

					SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
					schemaWrite.NodePropertyExistenceConstraintCreate( forLabel( label, propertyKey ) );
					Commit();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String type) throws Exception
			  internal override long CreateEntity( Transaction transaction, string type )
			  {
					long node = transaction.DataWrite().nodeCreate();
					int labelId = transaction.TokenWrite().labelGetOrCreateForName(type);
					transaction.DataWrite().nodeAddLabel(node, labelId);
					return node;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String property, String value) throws Exception
			  internal override long CreateEntity( Transaction transaction, string property, string value )
			  {
					long node = transaction.DataWrite().nodeCreate();
					int propertyKey = transaction.TokenWrite().propertyKeyGetOrCreateForName(property);
					transaction.DataWrite().nodeSetProperty(node, propertyKey, Values.of(value));
					return node;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String type, String property, String value) throws Exception
			  internal override long CreateEntity( Transaction transaction, string type, string property, string value )
			  {
					long node = CreateEntity( transaction, type );
					int propertyKey = transaction.TokenWrite().propertyKeyGetOrCreateForName(property);
					transaction.DataWrite().nodeSetProperty(node, propertyKey, Values.of(value));
					return node;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long createConstraintAndEntity(String type, String property, String value) throws Exception
			  internal override long CreateConstraintAndEntity( string type, string property, string value )
			  {
					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
					int label = transaction.TokenWrite().labelGetOrCreateForName(type);
					long node = transaction.DataWrite().nodeCreate();
					transaction.DataWrite().nodeAddLabel(node, label);
					int propertyKey = transaction.TokenWrite().propertyKeyGetOrCreateForName(property);
					transaction.DataWrite().nodeSetProperty(node, propertyKey, Values.of(value));
					Commit();

					CreateConstraint( type, property );

					return node;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setProperty(org.neo4j.internal.kernel.api.Write writeOps, long entityId, int propertyKeyId, org.neo4j.values.storable.Value value) throws Exception
			  internal override void SetProperty( Write writeOps, long entityId, int propertyKeyId, Value value )
			  {
					writeOps.NodeSetProperty( entityId, propertyKeyId, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void removeProperty(org.neo4j.internal.kernel.api.Write writeOps, long entityId, int propertyKey) throws Exception
			  internal override void RemoveProperty( Write writeOps, long entityId, int propertyKey )
			  {
					writeOps.NodeRemoveProperty( entityId, propertyKey );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int entityCount() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  internal override int EntityCount()
			  {
				  Transaction transaction = NewTransaction();
					int result = CountNodes( transaction );
					Rollback();
					return result;
			  }
		 }

		 public class RelationshipPropertyExistenceConstraintValidationIT : AbstractPropertyExistenceConstraintValidationIT
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createConstraint(String key, String property) throws org.neo4j.internal.kernel.api.exceptions.KernelException
			  internal override void CreateConstraint( string key, string property )
			  {
					TokenWrite tokenWrite = TokenWriteInNewTransaction();
					int relTypeId = tokenWrite.RelationshipTypeGetOrCreateForName( key );
					int propertyKeyId = tokenWrite.PropertyKeyGetOrCreateForName( property );
					Commit();

					SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
					schemaWrite.RelationshipPropertyExistenceConstraintCreate( forRelType( relTypeId, propertyKeyId ) );
					Commit();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String type) throws Exception
			  internal override long CreateEntity( Transaction transaction, string type )
			  {
					long start = transaction.DataWrite().nodeCreate();
					long end = transaction.DataWrite().nodeCreate();
					int relType = transaction.TokenWrite().relationshipTypeGetOrCreateForName(type);
					return transaction.DataWrite().relationshipCreate(start, relType, end);
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String property, String value) throws Exception
			  internal override long CreateEntity( Transaction transaction, string property, string value )
			  {
					long start = transaction.DataWrite().nodeCreate();
					long end = transaction.DataWrite().nodeCreate();
					string relationshipTypeName = System.Guid.randomUUID().ToString();
					int relType = transaction.TokenWrite().relationshipTypeGetOrCreateForName(relationshipTypeName);
					long relationship = transaction.DataWrite().relationshipCreate(start, relType, end);

					int propertyKey = transaction.TokenWrite().propertyKeyGetOrCreateForName(property);
					transaction.DataWrite().relationshipSetProperty(relationship, propertyKey, Values.of(value));
					return relationship;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String type, String property, String value) throws Exception
			  internal override long CreateEntity( Transaction transaction, string type, string property, string value )
			  {
					long relationship = CreateEntity( transaction, type );
					int propertyKey = transaction.TokenWrite().propertyKeyGetOrCreateForName(property);
					transaction.DataWrite().relationshipSetProperty(relationship, propertyKey, Values.of(value));
					return relationship;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long createConstraintAndEntity(String type, String property, String value) throws Exception
			  internal override long CreateConstraintAndEntity( string type, string property, string value )
			  {
					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
					int relType = transaction.TokenWrite().relationshipTypeGetOrCreateForName(type);
					long start = transaction.DataWrite().nodeCreate();
					long end = transaction.DataWrite().nodeCreate();
					long relationship = transaction.DataWrite().relationshipCreate(start, relType, end);
					int propertyKey = transaction.TokenWrite().propertyKeyGetOrCreateForName(property);
					transaction.DataWrite().relationshipSetProperty(relationship, propertyKey, Values.of(value));
					Commit();

					CreateConstraint( type, property );

					return relationship;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setProperty(org.neo4j.internal.kernel.api.Write writeOps, long entityId, int propertyKeyId, org.neo4j.values.storable.Value value) throws Exception
			  internal override void SetProperty( Write writeOps, long entityId, int propertyKeyId, Value value )
			  {
					writeOps.RelationshipSetProperty( entityId, propertyKeyId, value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void removeProperty(org.neo4j.internal.kernel.api.Write writeOps, long entityId, int propertyKey) throws Exception
			  internal override void RemoveProperty( Write writeOps, long entityId, int propertyKey )
			  {
					writeOps.RelationshipRemoveProperty( entityId, propertyKey );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int entityCount() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  internal override int EntityCount()
			  {
					Transaction transaction = NewTransaction();
					int result = CountRelationships( transaction );
					Rollback();
					return result;
			  }
		 }

		 public abstract class AbstractPropertyExistenceConstraintValidationIT : KernelIntegrationTest
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void createConstraint(String key, String property) throws org.neo4j.internal.kernel.api.exceptions.KernelException;
			  internal abstract void CreateConstraint( string key, string property );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String type) throws Exception;
			  internal abstract long CreateEntity( Transaction transaction, string type );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String property, String value) throws Exception;
			  internal abstract long CreateEntity( Transaction transaction, string property, string value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract long createEntity(org.neo4j.internal.kernel.api.Transaction transaction, String type, String property, String value) throws Exception;
			  internal abstract long CreateEntity( Transaction transaction, string type, string property, string value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract long createConstraintAndEntity(String type, String property, String value) throws Exception;
			  internal abstract long CreateConstraintAndEntity( string type, string property, string value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void setProperty(org.neo4j.internal.kernel.api.Write writeOps, long entityId, int propertyKeyId, org.neo4j.values.storable.Value value) throws Exception;
			  internal abstract void SetProperty( Write writeOps, long entityId, int propertyKeyId, Value value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void removeProperty(org.neo4j.internal.kernel.api.Write writeOps, long entityId, int propertyKey) throws Exception;
			  internal abstract void RemoveProperty( Write writeOps, long entityId, int propertyKey );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract int entityCount() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException;
			  internal abstract int EntityCount();

			  protected internal override GraphDatabaseService CreateGraphDatabase()
			  {
					return ( new TestEnterpriseGraphDatabaseFactory() ).setFileSystem(FileSystemRule.get()).newEmbeddedDatabaseBuilder(TestDir.storeDir()).newGraphDatabase();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforcePropertyExistenceConstraintWhenCreatingEntityWithoutProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldEnforcePropertyExistenceConstraintWhenCreatingEntityWithoutProperty()
			  {
					// given
					CreateConstraint( "Type1", "key1" );

					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

					// when
					CreateEntity( transaction, "Type1" );
					try
					{
						 Commit();
						 fail( "should have thrown exception" );
					}
					// then
					catch ( ConstraintViolationTransactionFailureException e )
					{
						 Status expected = Neo4Net.Kernel.Api.Exceptions.Status_Schema.ConstraintValidationFailed;
						 assertThat( e.Status(), @is(expected) );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceConstraintWhenRemoving() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldEnforceConstraintWhenRemoving()
			  {
					// given
					long entity = CreateConstraintAndEntity( "Type1", "key1", "value1" );
					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

					// when
					int key = transaction.TokenWrite().propertyKeyGetOrCreateForName("key1");
					RemoveProperty( transaction.DataWrite(), entity, key );
					try
					{
						 Commit();
						 fail( "should have thrown exception" );
					}
					// then
					catch ( ConstraintViolationTransactionFailureException e )
					{
						 Status expected = Neo4Net.Kernel.Api.Exceptions.Status_Schema.ConstraintValidationFailed;
						 assertThat( e.Status(), @is(expected) );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowTemporaryViolationsWithinTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldAllowTemporaryViolationsWithinTransactions()
			  {
					// given
					long entity = CreateConstraintAndEntity( "Type1", "key1", "value1" );
					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

					// when
					int key = transaction.TokenWrite().propertyKeyGetOrCreateForName("key1");
					//remove and put back
					RemoveProperty( transaction.DataWrite(), entity, key );
					SetProperty( transaction.DataWrite(), entity, key, Values.of("value2") );

					Commit();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowNoopPropertyUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldAllowNoopPropertyUpdate()
			  {
					// given
					long entity = CreateConstraintAndEntity( "Type1", "key1", "value1" );

					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

					// when
					int key = transaction.TokenWrite().propertyKeyGetOrCreateForName("key1");
					SetProperty( transaction.DataWrite(), entity, key, Values.of("value1") );

					// then should not throw exception
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowCreationOfNonConflictingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldAllowCreationOfNonConflictingData()
			  {
					// given
					CreateConstraintAndEntity( "Type1", "key1", "value1" );

					Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

					// when
					CreateEntity( transaction, "key1", "value1" );
					CreateEntity( transaction, "Type2" );
					CreateEntity( transaction, "Type1", "key1", "value2" );
					CreateEntity( transaction, "Type1", "key1", "value3" );

					Commit();

					// then
					assertEquals( 5, EntityCount() );
			  }
		 }
	}

}