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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NumberIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.NumberIndexProvider;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.NativeLuceneFusionIndexProviderFactory20.subProviderDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.pointValue;

	public class FusionIndexIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.DatabaseRule db = new org.Neo4Net.test.rule.EmbeddedDatabaseRule().withSetting(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider, org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName());
		 public DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(GraphDatabaseSettings.default_schema_provider, GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName());

		 private DatabaseLayout _databaseLayout;
		 private readonly Label _label = Label.label( "label" );
		 private readonly string _propKey = "propKey";
		 private FileSystemAbstraction _fs;
		 private int _numberValue = 1;
		 private string _stringValue = "string";
		 private PointValue _spatialValue = pointValue( CoordinateReferenceSystem.WGS84, 0.5, 0.5 );
		 private DateValue _temporalValue = DateValue.date( 2018, 3, 19 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _databaseLayout = Db.databaseLayout();
			  _fs = Db.DependencyResolver.resolveDependency( typeof( FileSystemAbstraction ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRebuildFusionIndexIfNativePartIsMissing() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustRebuildFusionIndexIfNativePartIsMissing()
		 {
			  // given
			  InitializeIndexWithDataAndShutdown();

			  // when
			  IndexProviderDescriptor descriptor = NumberIndexProvider.NATIVE_PROVIDER_DESCRIPTOR;
			  DeleteIndexFilesFor( descriptor );

			  // then
			  // ... should rebuild
			  VerifyContent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRebuildFusionIndexIfLucenePartIsMissing() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustRebuildFusionIndexIfLucenePartIsMissing()
		 {
			  // given
			  InitializeIndexWithDataAndShutdown();

			  // when
			  IndexProviderDescriptor descriptor = LuceneIndexProviderFactory.ProviderDescriptor;
			  DeleteIndexFilesFor( descriptor );

			  // then
			  // ... should rebuild
			  VerifyContent();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRebuildFusionIndexIfCompletelyMissing() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustRebuildFusionIndexIfCompletelyMissing()
		 {
			  // given
			  InitializeIndexWithDataAndShutdown();

			  // when
			  IndexProviderDescriptor luceneDescriptor = LuceneIndexProviderFactory.ProviderDescriptor;
			  IndexProviderDescriptor nativeDescriptor = NumberIndexProvider.NATIVE_PROVIDER_DESCRIPTOR;
			  DeleteIndexFilesFor( luceneDescriptor );
			  DeleteIndexFilesFor( nativeDescriptor );

			  // then
			  // ... should rebuild
			  VerifyContent();
		 }

		 private void VerifyContent()
		 {
			  GraphDatabaseAPI newDb = Db.GraphDatabaseAPI;
			  using ( Transaction tx = newDb.BeginTx() )
			  {
					assertEquals( 1L, Iterators.stream( newDb.Schema().getIndexes(_label).GetEnumerator() ).count() );
					assertNotNull( newDb.FindNode( _label, _propKey, _numberValue ) );
					assertNotNull( newDb.FindNode( _label, _propKey, _stringValue ) );
					assertNotNull( newDb.FindNode( _label, _propKey, _spatialValue ) );
					assertNotNull( newDb.FindNode( _label, _propKey, _temporalValue ) );
					tx.Success();
			  }
		 }

		 private void DeleteIndexFilesFor( IndexProviderDescriptor descriptor )
		 {
			  File databaseDirectory = this._databaseLayout.databaseDirectory();
			  File rootDirectory = subProviderDirectoryStructure( databaseDirectory ).forProvider( descriptor ).rootDirectory();
			  File[] files = _fs.listFiles( rootDirectory );
			  foreach ( File indexFile in files )
			  {
					_fs.deleteFile( indexFile );
			  }
		 }

		 private void InitializeIndexWithDataAndShutdown()
		 {
			  CreateIndex();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( _label ).setProperty( _propKey, _numberValue );
					Db.createNode( _label ).setProperty( _propKey, _stringValue );
					Db.createNode( _label ).setProperty( _propKey, _spatialValue );
					Db.createNode( _label ).setProperty( _propKey, _temporalValue );
					tx.Success();
			  }
			  Db.shutdown();
		 }

		 private void CreateIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(_label).on(_propKey).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }
	}

}