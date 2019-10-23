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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using Registers = Neo4Net.Register.Registers;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;

	public class IndexSamplingIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private readonly Label _label = Label.label( "Person" );
		 private readonly string _property = "name";
		 private readonly long _nodes = 1000;
		 private readonly string[] _names = new string[] { "Neo4Net", "Neo", "Graph", "Apa" };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleNotUniqueIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSampleNotUniqueIndex()
		 {
			  IGraphDatabaseService db = null;
			  long deletedNodes = 0;
			  try
			  {
					// Given
					db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
					IndexDefinition indexDefinition;
					using ( Transaction tx = Db.beginTx() )
					{
						 indexDefinition = Db.schema().indexFor(_label).on(_property).create();
						 tx.Success();
					}

					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().awaitIndexOnline(indexDefinition, 10, TimeUnit.SECONDS);
						 tx.Success();
					}

					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int i = 0; i < _nodes; i++ )
						 {
							  Db.createNode( _label ).setProperty( _property, _names[i % _names.Length] );
							  tx.Success();
						 }

					}

					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int i = 0; i < ( _nodes / 10 ) ; i++ )
						 {
							  using ( ResourceIterator<Node> nodes = Db.findNodes( _label, _property, _names[i % _names.Length] ) )
							  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									nodes.next().delete();
							  }
							  deletedNodes++;
							  tx.Success();
						 }
					}
			  }
			  finally
			  {
					if ( db != null )
					{
						 Db.shutdown();
					}
			  }

			  // When
			  TriggerIndexResamplingOnNextStartup();

			  // Then

			  // lucene will consider also the delete nodes, native won't
			  Register_DoubleLongRegister register = FetchIndexSamplingValues( db );
			  assertEquals( _names.Length, register.ReadFirst() );
			  assertThat( register.ReadSecond(), allOf(greaterThanOrEqualTo(nodes - deletedNodes), lessThanOrEqualTo(nodes)) );

			  // but regardless, the deleted nodes should not be considered in the index size value
			  Register_DoubleLongRegister indexSizeRegister = FetchIndexSizeValues( db );
			  assertEquals( 0, indexSizeRegister.ReadFirst() );
			  assertEquals( nodes - deletedNodes, indexSizeRegister.ReadSecond() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleUniqueIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSampleUniqueIndex()
		 {
			  IGraphDatabaseService db = null;
			  long deletedNodes = 0;
			  try
			  {
					// Given
					db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().constraintFor(_label).assertPropertyIsUnique(_property).create();
						 tx.Success();
					}

					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int i = 0; i < _nodes; i++ )
						 {
							  Db.createNode( _label ).setProperty( _property, "" + i );
							  tx.Success();
						 }
					}

					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int i = 0; i < _nodes; i++ )
						 {
							  if ( i % 10 == 0 )
							  {
									deletedNodes++;
									Db.findNode( _label, _property, "" + i ).delete();
									tx.Success();
							  }
						 }
					}
			  }
			  finally
			  {
					if ( db != null )
					{
						 Db.shutdown();
					}
			  }

			  // When
			  TriggerIndexResamplingOnNextStartup();

			  // Then
			  Register_DoubleLongRegister indexSampleRegister = FetchIndexSamplingValues( db );
			  assertEquals( _nodes - deletedNodes, indexSampleRegister.ReadFirst() );
			  assertEquals( _nodes - deletedNodes, indexSampleRegister.ReadSecond() );

			  Register_DoubleLongRegister indexSizeRegister = FetchIndexSizeValues( db );
			  assertEquals( 0, indexSizeRegister.ReadFirst() );
			  assertEquals( _nodes - deletedNodes, indexSizeRegister.ReadSecond() );
		 }

		 private IndexReference IndexId( Neo4Net.Kernel.Api.Internal.Transaction tx )
		 {
			  int labelId = tx.TokenRead().nodeLabel(_label.name());
			  int propertyKeyId = tx.TokenRead().propertyKey(_property);
			  return tx.SchemaRead().index(labelId, propertyKeyId);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.register.Register_DoubleLongRegister fetchIndexSamplingValues(org.Neo4Net.graphdb.GraphDatabaseService db) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private Register_DoubleLongRegister FetchIndexSamplingValues( IGraphDatabaseService db )
		 {
			  try
			  {
					// Then
					db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") org.Neo4Net.kernel.internal.GraphDatabaseAPI api = (org.Neo4Net.kernel.internal.GraphDatabaseAPI) db;
					GraphDatabaseAPI api = ( GraphDatabaseAPI ) db;
					using ( Neo4Net.Kernel.Api.Internal.Transaction tx = api.DependencyResolver.resolveDependency( typeof( Kernel ) ).BeginTransaction( @explicit, AUTH_DISABLED ) )
					{
						 return tx.SchemaRead().indexSample(IndexId(tx), Registers.newDoubleLongRegister());
					}
			  }
			  finally
			  {
					if ( db != null )
					{
						 Db.shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.register.Register_DoubleLongRegister fetchIndexSizeValues(org.Neo4Net.graphdb.GraphDatabaseService db) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 private Register_DoubleLongRegister FetchIndexSizeValues( IGraphDatabaseService db )
		 {
			  try
			  {
					// Then
					db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") org.Neo4Net.kernel.internal.GraphDatabaseAPI api = (org.Neo4Net.kernel.internal.GraphDatabaseAPI) db;
					GraphDatabaseAPI api = ( GraphDatabaseAPI ) db;
					using ( Neo4Net.Kernel.Api.Internal.Transaction tx = api.DependencyResolver.resolveDependency( typeof( Kernel ) ).BeginTransaction( @explicit, AUTH_DISABLED ) )
					{
						 return tx.SchemaRead().indexUpdatesAndSize(IndexId(tx), Registers.newDoubleLongRegister());
					}
			  }
			  finally
			  {
					if ( db != null )
					{
						 Db.shutdown();
					}
			  }
		 }

		 private void TriggerIndexResamplingOnNextStartup()
		 {
			  // Trigger index resampling on next at startup
			  FileUtils.deleteFile( TestDirectory.databaseLayout().countStoreA() );
			  FileUtils.deleteFile( TestDirectory.databaseLayout().countStoreB() );
		 }
	}

}