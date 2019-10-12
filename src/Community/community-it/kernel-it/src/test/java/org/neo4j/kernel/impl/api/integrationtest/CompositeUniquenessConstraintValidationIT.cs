using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertException;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CompositeUniquenessConstraintValidationIT
	public class CompositeUniquenessConstraintValidationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public readonly TestName TestName = new TestName();
		 private readonly int _numberOfProps;
		 private readonly object[] _aValues;
		 private readonly object[] _bValues;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{index}: {0}") public static Iterable<TestParams> parameterValues()
		 public static IEnumerable<TestParams> ParameterValues()
		 {
			  return Arrays.asList( Param( Values( 10 ), Values( 10d ) ), Param( Values( 10, 20 ), Values( 10, 20 ) ), Param( Values( 10L, 20L ), Values( 10, 20 ) ), Param( Values( 10, 20 ), Values( 10L, 20L ) ), Param( Values( 10, 20 ), Values( 10.0, 20.0 ) ), Param( Values( 10, 20 ), Values( 10.0, 20.0 ) ), Param( Values( new int[]{ 1, 2 }, "v2" ), Values( new int[]{ 1, 2 }, "v2" ) ), Param( Values( "a", "b", "c" ), Values( "a", "b", "c" ) ), Param( Values( 285414114323346805L ), Values( 285414114323346805L ) ), Param( Values( 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ), Values( 1d, 2d, 3d, 4d, 5d, 6d, 7d, 8d, 9d, 10d ) ) );
		 }

		 private static TestParams Param( object[] l, object[] r )
		 {
			  return new TestParams( l, r );
		 }

		 private static object[] Values( params object[] values )
		 {
			  return values;
		 }

		 private const int LABEL = 1;

		 public CompositeUniquenessConstraintValidationIT( TestParams @params )
		 {
			  Debug.Assert( @params.Lhs.Length == @params.Rhs.Length );
			  _aValues = @params.Lhs;
			  _bValues = @params.Rhs;
			  _numberOfProps = _aValues.Length;
		 }

		 private Transaction _transaction;
		 private GraphDatabaseAPI _graphDatabaseAPI;
		 protected internal Kernel Kernel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _graphDatabaseAPI = DbRule.GraphDatabaseAPI;
			  Kernel = _graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( Kernel ) );

			  NewTransaction();
			  _transaction.schemaWrite().uniquePropertyConstraintCreate(forLabel(LABEL, PropertyIds()));
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void clean() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Clean()
		 {
			  if ( _transaction != null )
			  {
					_transaction.close();
			  }

			  NewTransaction();
			  _transaction.schemaWrite().constraintDrop(ConstraintDescriptorFactory.uniqueForLabel(LABEL, PropertyIds()));
			  Commit();

			  using ( Transaction tx = Kernel.beginTransaction( Neo4Net.@internal.Kernel.Api.Transaction_Type.Implicit, LoginContext.AUTH_DISABLED ), NodeCursor node = tx.Cursors().allocateNodeCursor() )
			  {
					tx.DataRead().allNodesScan(node);
					while ( node.Next() )
					{
						 tx.DataWrite().nodeDelete(node.NodeReference());
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRemoveAndAddConflictingDataInOneTransaction_DeleteNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRemoveAndAddConflictingDataInOneTransactionDeleteNode()
		 {
			  // given
			  long node = CreateNodeWithLabelAndProps( LABEL, _aValues );

			  // when
			  NewTransaction();
			  _transaction.dataWrite().nodeDelete(node);
			  long newNode = CreateLabeledNode( LABEL );
			  SetProperties( newNode, _aValues );

			  // then does not fail
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRemoveAndAddConflictingDataInOneTransaction_RemoveLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRemoveAndAddConflictingDataInOneTransactionRemoveLabel()
		 {
			  // given
			  long node = CreateNodeWithLabelAndProps( LABEL, _aValues );

			  // when
			  NewTransaction();
			  _transaction.dataWrite().nodeRemoveLabel(node, LABEL);
			  long newNode = CreateLabeledNode( LABEL );
			  SetProperties( newNode, _aValues );

			  // then does not fail
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRemoveAndAddConflictingDataInOneTransaction_RemoveProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRemoveAndAddConflictingDataInOneTransactionRemoveProperty()
		 {
			  // given
			  long node = CreateNodeWithLabelAndProps( LABEL, _aValues );

			  // when
			  NewTransaction();
			  _transaction.dataWrite().nodeRemoveProperty(node, 0);
			  long newNode = CreateLabeledNode( LABEL );
			  SetProperties( newNode, _aValues );

			  // then does not fail
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRemoveAndAddConflictingDataInOneTransaction_ChangeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRemoveAndAddConflictingDataInOneTransactionChangeProperty()
		 {
			  // given
			  long node = CreateNodeWithLabelAndProps( LABEL, _aValues );

			  // when
			  NewTransaction();
			  _transaction.dataWrite().nodeSetProperty(node, 0, Values.of("Alive!"));
			  long newNode = CreateLabeledNode( LABEL );
			  SetProperties( newNode, _aValues );

			  // then does not fail
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPreventConflictingDataInTx() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPreventConflictingDataInTx()
		 {
			  // Given

			  // When
			  NewTransaction();
			  long n1 = CreateLabeledNode( LABEL );
			  long n2 = CreateLabeledNode( LABEL );
			  SetProperties( n1, _aValues );
			  int lastPropertyOffset = _numberOfProps - 1;
			  for ( int prop = 0; prop < lastPropertyOffset; prop++ )
			  {
					SetProperty( n2, prop, _aValues[prop] ); // still ok
			  }

			  assertException(() =>
			  {
				SetProperty( n2, lastPropertyOffset, _aValues[lastPropertyOffset] ); // boom!

			  }, typeof( UniquePropertyValueValidationException ));

			  // Then should fail
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceOnSetProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnforceOnSetProperty()
		 {
			  // given
			  CreateNodeWithLabelAndProps( LABEL, this._aValues );

			  // when
			  NewTransaction();
			  long node = CreateLabeledNode( LABEL );

			  int lastPropertyOffset = _numberOfProps - 1;
			  for ( int prop = 0; prop < lastPropertyOffset; prop++ )
			  {
					SetProperty( node, prop, _aValues[prop] ); // still ok
			  }

			  assertException(() =>
			  {
				SetProperty( node, lastPropertyOffset, _aValues[lastPropertyOffset] ); // boom!

			  }, typeof( UniquePropertyValueValidationException ));
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceOnSetLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnforceOnSetLabel()
		 {
			  // given
			  CreateNodeWithLabelAndProps( LABEL, this._aValues );

			  // when
			  NewTransaction();
			  long node = CreateNode();
			  SetProperties( node, _bValues ); // ok because no label is set

			  assertException(() =>
			  {
				AddLabel( node, LABEL ); // boom!

			  }, typeof( UniquePropertyValueValidationException ));
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceOnSetPropertyInTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnforceOnSetPropertyInTx()
		 {
			  // when
			  NewTransaction();
			  long aNode = CreateLabeledNode( LABEL );
			  SetProperties( aNode, _aValues );

			  long nodeB = CreateLabeledNode( LABEL );
			  int lastPropertyOffset = _numberOfProps - 1;
			  for ( int prop = 0; prop < lastPropertyOffset; prop++ )
			  {
					SetProperty( nodeB, prop, _bValues[prop] ); // still ok
			  }

			  assertException(() =>
			  {
				SetProperty( nodeB, lastPropertyOffset, _bValues[lastPropertyOffset] ); // boom!
			  }, typeof( UniquePropertyValueValidationException ));
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceOnSetLabelInTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnforceOnSetLabelInTx()
		 {
			  // given
			  CreateNodeWithLabelAndProps( LABEL, _aValues );

			  // when
			  NewTransaction();
			  long nodeB = CreateNode();
			  SetProperties( nodeB, _bValues );

			  assertException(() =>
			  {
				AddLabel( nodeB, LABEL ); // boom!

			  }, typeof( UniquePropertyValueValidationException ));
			  Commit();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void newTransaction() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void NewTransaction()
		 {
			  if ( _transaction != null )
			  {
					fail( "tx already opened" );
			  }
			  _transaction = Kernel.beginTransaction( KernelTransaction.Type.@implicit, LoginContext.AUTH_DISABLED );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void commit() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual void Commit()
		 {
			  _transaction.success();
			  try
			  {
					_transaction.close();
			  }
			  finally
			  {
					_transaction = null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createLabeledNode(int labelId) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreateLabeledNode( int labelId )
		 {
			  long node = _transaction.dataWrite().nodeCreate();
			  _transaction.dataWrite().nodeAddLabel(node, labelId);
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addLabel(long nodeId, int labelId) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void AddLabel( long nodeId, int labelId )
		 {
			  _transaction.dataWrite().nodeAddLabel(nodeId, labelId);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setProperty(long nodeId, int propertyId, Object value) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void SetProperty( long nodeId, int propertyId, object value )
		 {
			  _transaction.dataWrite().nodeSetProperty(nodeId, propertyId, Values.of(value));
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNode() throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreateNode()
		 {
			  return _transaction.dataWrite().nodeCreate();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNodeWithLabelAndProps(int labelId, Object[] propertyValues) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreateNodeWithLabelAndProps( int labelId, object[] propertyValues )
		 {
			  NewTransaction();
			  long nodeId = CreateNode();
			  AddLabel( nodeId, labelId );
			  for ( int prop = 0; prop < _numberOfProps; prop++ )
			  {
					SetProperty( nodeId, prop, propertyValues[prop] );
			  }
			  Commit();
			  return nodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setProperties(long nodeId, Object[] propertyValues) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void SetProperties( long nodeId, object[] propertyValues )
		 {
			  for ( int prop = 0; prop < propertyValues.Length; prop++ )
			  {
					SetProperty( nodeId, prop, propertyValues[prop] );
			  }
		 }

		 private int[] PropertyIds()
		 {
			  int[] props = new int[_numberOfProps];
			  for ( int i = 0; i < _numberOfProps; i++ )
			  {
					props[i] = i;
			  }
			  return props;
		 }

		 internal class TestParams // Only here to be able to produce readable output
		 {
			  internal readonly object[] Lhs;
			  internal readonly object[] Rhs;

			  internal TestParams( object[] lhs, object[] rhs )
			  {
					this.Lhs = lhs;
					this.Rhs = rhs;
			  }

			  public override string ToString()
			  {
					return string.Format( "lhs={0}, rhs={1}", ArrayUtils.ToString( Lhs ), ArrayUtils.ToString( Rhs ) );
			  }
		 }
	}

}