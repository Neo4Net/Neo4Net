﻿/*
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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Org.Neo4j.Graphdb.Label;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using NumberIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.NumberIndexProvider;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using DateValue = Org.Neo4j.Values.Storable.DateValue;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.NativeLuceneFusionIndexProviderFactory20.subProviderDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;

	public class FusionIndexIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider, org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName());
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