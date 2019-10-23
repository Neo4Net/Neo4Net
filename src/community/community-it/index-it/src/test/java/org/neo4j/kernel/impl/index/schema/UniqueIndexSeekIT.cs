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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using CombinableMatcher = org.hamcrest.core.CombinableMatcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using TrackingIndexExtensionFactory = Neo4Net.Kernel.Impl.Index.Schema.tracking.TrackingIndexExtensionFactory;
	using TrackingReadersIndexAccessor = Neo4Net.Kernel.Impl.Index.Schema.tracking.TrackingReadersIndexAccessor;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.both;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.NativeLuceneFusionIndexProviderFactory20.DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.tracking.TrackingReadersIndexAccessor.numberOfClosedReaders;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.tracking.TrackingReadersIndexAccessor.numberOfOpenReaders;

	public class UniqueIndexSeekIT
	{
		private bool InstanceFieldsInitialized = false;

		public UniqueIndexSeekIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Directory = TestDirectory.testDirectory( Fs );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.DefaultFileSystemRule fs = new org.Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule Fs = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory(fs);
		 public TestDirectory Directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uniqueIndexSeekDoNotLeakIndexReaders() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UniqueIndexSeekDoNotLeakIndexReaders()
		 {
			  TrackingIndexExtensionFactory indexExtensionFactory = new TrackingIndexExtensionFactory();
			  GraphDatabaseAPI database = CreateDatabase( indexExtensionFactory );
			  try
			  {

					Label label = label( "spaceship" );
					string nameProperty = "name";
					CreateUniqueConstraint( database, label, nameProperty );

					GenerateRandomData( database, label, nameProperty );

					assertNotNull( indexExtensionFactory.IndexProvider );
					assertThat( numberOfClosedReaders(), greaterThan(0L) );
					assertThat( numberOfOpenReaders(), greaterThan(0L) );
					assertThat( numberOfClosedReaders(), CloseTo(numberOfOpenReaders(), 1) );

					LockNodeUsingUniqueIndexSeek( database, label, nameProperty );

					assertThat( numberOfClosedReaders(), CloseTo(numberOfOpenReaders(), 1) );
			  }
			  finally
			  {
					database.Shutdown();
			  }
		 }

		 private static CombinableMatcher<long> CloseTo( long from, long delta )
		 {
			  return both( greaterThanOrEqualTo( from - delta ) ).and( lessThanOrEqualTo( from + delta ) );
		 }

		 private GraphDatabaseAPI CreateDatabase( TrackingIndexExtensionFactory indexExtensionFactory )
		 {
			  return ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).setKernelExtensions(singletonList(indexExtensionFactory)).newEmbeddedDatabaseBuilder(Directory.databaseDir()).setConfig(default_schema_provider, DESCRIPTOR.name()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void lockNodeUsingUniqueIndexSeek(org.Neo4Net.kernel.internal.GraphDatabaseAPI database, org.Neo4Net.graphdb.Label label, String nameProperty) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 private static void LockNodeUsingUniqueIndexSeek( GraphDatabaseAPI database, Label label, string nameProperty )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					ThreadToStatementContextBridge contextBridge = database.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
					KernelTransaction kernelTransaction = contextBridge.GetKernelTransactionBoundToThisThread( true );
					TokenRead tokenRead = kernelTransaction.TokenRead();
					Read dataRead = kernelTransaction.DataRead();

					int labelId = tokenRead.NodeLabel( label.Name() );
					int propertyId = tokenRead.PropertyKey( nameProperty );
					IndexReference indexReference = kernelTransaction.SchemaRead().index(labelId, propertyId);
					dataRead.LockingNodeUniqueIndexSeek( indexReference, IndexQuery.ExactPredicate.exact( propertyId, "value" ) );
					transaction.Success();
			  }
		 }

		 private static void GenerateRandomData( GraphDatabaseAPI database, Label label, string nameProperty )
		 {
			  for ( int i = 0; i < 1000; i++ )
			  {
					using ( Transaction transaction = database.BeginTx() )
					{
						 Node node = database.CreateNode( label );
						 node.SetProperty( nameProperty, "PlanetExpress" + i );
						 transaction.Success();
					}
			  }
		 }

		 private static void CreateUniqueConstraint( GraphDatabaseAPI database, Label label, string nameProperty )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					database.Schema().constraintFor(label).assertPropertyIsUnique(nameProperty).create();
					transaction.Success();
			  }
			  using ( Transaction transaction = database.BeginTx() )
			  {
					database.Schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					transaction.Success();
			  }
		 }
	}

}