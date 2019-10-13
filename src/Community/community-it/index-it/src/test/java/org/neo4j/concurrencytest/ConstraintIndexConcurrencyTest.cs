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
namespace Neo4Net.Concurrencytest
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using NodeValueIndexCursor = Neo4Net.@internal.Kernel.Api.NodeValueIndexCursor;
	using Read = Neo4Net.@internal.Kernel.Api.Read;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using ThreadingRule = Neo4Net.Test.rule.concurrent.ThreadingRule;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class ConstraintIndexConcurrencyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.ThreadingRule threads = new org.neo4j.test.rule.concurrent.ThreadingRule();
		 public readonly ThreadingRule Threads = new ThreadingRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowConcurrentViolationOfConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowConcurrentViolationOfConstraint()
		 {
			  // Given
			  GraphDatabaseAPI graphDb = Db.GraphDatabaseAPI;

			  System.Func<KernelTransaction> ktxSupplier = () => graphDb.DependencyResolver.resolveDependency(typeof(ThreadToStatementContextBridge)).getKernelTransactionBoundToThisThread(true);

			  Label label = label( "Foo" );
			  string propertyKey = "bar";
			  string conflictingValue = "baz";

			  // a constraint
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.Schema().constraintFor(label).assertPropertyIsUnique(propertyKey).create();
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					KernelTransaction ktx = ktxSupplier();
					int labelId = ktx.TokenRead().nodeLabel(label.Name());
					int propertyKeyId = ktx.TokenRead().propertyKey(propertyKey);
					IndexDescriptor index = TestIndexDescriptorFactory.uniqueForLabel( labelId, propertyKeyId );
					Read read = ktx.DataRead();
					using ( NodeValueIndexCursor cursor = ktx.Cursors().allocateNodeValueIndexCursor() )
					{
						 read.NodeIndexSeek( ktx.SchemaRead().index(labelId, propertyKeyId), cursor, IndexOrder.NONE, false, IndexQuery.exact(index.Schema().PropertyId, "The value is irrelevant, we just want to perform some sort of lookup against this " + "index") );
					}
					// then let another thread come in and create a node
					Threads.execute(Db =>
					{
					 using ( Transaction transaction = Db.beginTx() )
					 {
						  Db.createNode( label ).setProperty( propertyKey, conflictingValue );
						  transaction.success();
					 }
					 return null;
					}, graphDb).get();

					// before we create a node with the same property ourselves - using the same statement that we have
					// already used for lookup against that very same index
					long node = ktx.DataWrite().nodeCreate();
					ktx.DataWrite().nodeAddLabel(node, labelId);
					try
					{
						 ktx.DataWrite().nodeSetProperty(node, propertyKeyId, Values.of(conflictingValue));

						 fail( "exception expected" );
					}
					// Then
					catch ( UniquePropertyValueValidationException e )
					{
						 assertEquals( ConstraintDescriptorFactory.uniqueForLabel( labelId, propertyKeyId ), e.Constraint() );
						 IndexEntryConflictException conflict = Iterators.single( e.Conflicts().GetEnumerator() );
						 assertEquals( Values.stringValue( conflictingValue ), conflict.SinglePropertyValue );
					}

					tx.Success();
			  }
		 }
	}

}