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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IndexCreator = Neo4Net.Graphdb.schema.IndexCreator;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using NodeValueIndexCursor = Neo4Net.@internal.Kernel.Api.NodeValueIndexCursor;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using TestLabels = Neo4Net.Test.TestLabels;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class NativeStringIndexingIT
	{
		 private const Label LABEL = TestLabels.LABEL_ONE;
		 private const string KEY = "key";
		 private const string KEY2 = "key2";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider, org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName());
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(GraphDatabaseSettings.default_schema_provider, GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSizesCloseToTheLimit()
		 public virtual void ShouldHandleSizesCloseToTheLimit()
		 {
			  // given
			  CreateIndex( KEY );

			  // when
			  IDictionary<string, long> strings = new Dictionary<string, long>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < 1_000; i++ )
					{
						 string @string;
						 do
						 {
							  @string = Random.nextAlphaNumericString( 3_000, 4_000 );
						 } while ( strings.ContainsKey( @string ) );

						 Node node = Db.createNode( LABEL );
						 node.SetProperty( KEY, @string );
						 strings[@string] = node.Id;
					}
					tx.Success();
			  }

			  // then
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( string @string in strings.Keys )
					{
						 Node node = Db.findNode( LABEL, KEY, @string );
						 assertEquals( strings[@string], node.Id );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailBeforeCommitOnSizesLargerThanLimit()
		 public virtual void ShouldFailBeforeCommitOnSizesLargerThanLimit()
		 {
			  // given
			  CreateIndex( KEY );

			  // when a string slightly longer than the native string limit
			  int length = 5_000;
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( LABEL ).setProperty( KEY, Random.nextAlphaNumericString( length, length ) );
						 tx.Success();
					}
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// then good
					assertThat( e.Message, containsString( "Property value size is too large for index. Please see index documentation for limitations." ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleCompositeSizesCloseToTheLimit() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleCompositeSizesCloseToTheLimit()
		 {
			  // given
			  CreateIndex( KEY, KEY2 );

			  // when a string longer than native string limit, but within lucene limit
			  int length = 20_000;
			  string string1 = Random.nextAlphaNumericString( length, length );
			  string string2 = Random.nextAlphaNumericString( length, length );
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode( LABEL );
					node.SetProperty( KEY, string1 );
					node.SetProperty( KEY2, string2 );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
					int labelId = ktx.TokenRead().nodeLabel(LABEL.Name());
					int propertyKeyId1 = ktx.TokenRead().propertyKey(KEY);
					int propertyKeyId2 = ktx.TokenRead().propertyKey(KEY2);
					using ( NodeValueIndexCursor cursor = ktx.Cursors().allocateNodeValueIndexCursor() )
					{
						 ktx.DataRead().nodeIndexSeek(TestIndexDescriptorFactory.forLabel(labelId, propertyKeyId1, propertyKeyId2), cursor, IndexOrder.NONE, false, IndexQuery.exact(propertyKeyId1, string1), IndexQuery.exact(propertyKeyId2, string2));
						 assertTrue( cursor.Next() );
						 assertEquals( node.Id, cursor.NodeReference() );
						 assertFalse( cursor.Next() );
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailBeforeCommitOnCompositeSizesLargerThanLimit()
		 public virtual void ShouldFailBeforeCommitOnCompositeSizesLargerThanLimit()
		 {
			  // given
			  CreateIndex( KEY, KEY2 );

			  // when a string longer than lucene string limit
			  int length = 50_000;
			  try
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Node node = Db.createNode( LABEL );
						 node.SetProperty( KEY, Random.nextAlphaNumericString( length, length ) );
						 node.SetProperty( KEY2, Random.nextAlphaNumericString( length, length ) );
						 tx.Success();
					}
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException e )
			  {
					// then good
					assertThat( e.Message, containsString( "Property value size is too large for index. Please see index documentation for limitations." ) );
			  }
		 }

		 private void CreateIndex( params string[] keys )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IndexCreator indexCreator = Db.schema().indexFor(LABEL);
					foreach ( string key in keys )
					{
						 indexCreator = indexCreator.On( key );
					}
					indexCreator.Create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, SECONDS);
					tx.Success();
			  }
		 }
	}

}