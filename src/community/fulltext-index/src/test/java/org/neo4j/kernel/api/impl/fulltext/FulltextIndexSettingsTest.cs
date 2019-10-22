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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using MultiTokenSchemaDescriptor = Neo4Net.Kernel.api.schema.MultiTokenSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenRegistry = Neo4Net.Kernel.impl.core.TokenRegistry;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.schema.IndexProviderDescriptor.UNDECIDED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.fulltext.FulltextIndexSettings.readOrInitializeDescriptor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.storageengine.api.schema.IndexDescriptor.Type.GENERAL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, DefaultFileSystemExtension.class}) class FulltextIndexSettingsTest
	internal class FulltextIndexSettingsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory directory;
		 internal TestDirectory Directory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject DefaultFileSystemAbstraction fs;
		 internal DefaultFileSystemAbstraction Fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPersistFulltextIndexSettings() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPersistFulltextIndexSettings()
		 {
			  // Given
			  File indexFolder = Directory.directory( "indexFolder" );
			  string analyzerName = "simple";
			  string eventuallyConsistency = "true";
			  string defaultAnalyzer = "defaultAnalyzer";
			  int[] propertyIds = new int[] { 1, 2, 3 };
			  MultiTokenSchemaDescriptor schema = SchemaDescriptorFactory.multiToken( new int[]{ 1, 2 }, IEntityType.NODE, propertyIds );

			  // A fulltext index descriptor with configurations
			  Properties properties = properties( analyzerName, eventuallyConsistency );
			  FulltextSchemaDescriptor fulltextSchemaDescriptor = new FulltextSchemaDescriptor( schema, properties );
			  StoreIndexDescriptor storeIndexDescriptor = StoreIndexDescriptorFromSchema( fulltextSchemaDescriptor );
			  TokenRegistry tokenRegistry = SimpleTokenHolder.CreatePopulatedTokenRegistry( Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY, propertyIds );
			  SimpleTokenHolder tokenHolder = new SimpleTokenHolder( tokenRegistry );
			  FulltextIndexDescriptor fulltextIndexDescriptor = readOrInitializeDescriptor( storeIndexDescriptor, defaultAnalyzer, tokenHolder, indexFolder, Fs );
			  assertEquals( analyzerName, fulltextIndexDescriptor.AnalyzerName() );
			  assertEquals( bool.Parse( eventuallyConsistency ), fulltextIndexDescriptor.EventuallyConsistent );

			  // When persisting it
			  FulltextIndexSettings.SaveFulltextIndexSettings( fulltextIndexDescriptor, indexFolder, Fs );

			  // Then we should be able to load it back with settings being the same
			  StoreIndexDescriptor loadingIndexDescriptor = StoreIndexDescriptorFromSchema( schema );
			  FulltextIndexDescriptor loadedDescriptor = readOrInitializeDescriptor( loadingIndexDescriptor, defaultAnalyzer, tokenHolder, indexFolder, Fs );
			  assertEquals( fulltextIndexDescriptor.AnalyzerName(), loadedDescriptor.AnalyzerName() );
			  assertEquals( fulltextIndexDescriptor.EventuallyConsistent, loadedDescriptor.EventuallyConsistent );
		 }

		 private StoreIndexDescriptor StoreIndexDescriptorFromSchema( SchemaDescriptor schema )
		 {
			  return ( new IndexDescriptor( schema, GENERAL, "indexName", UNDECIDED ) ).withId( 1 );
		 }

		 private Properties Properties( string analyzerName, string eventuallyConsistency )
		 {
			  Properties properties = new Properties();
			  properties.putIfAbsent( FulltextIndexSettings.INDEX_CONFIG_ANALYZER, analyzerName );
			  properties.putIfAbsent( FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT, eventuallyConsistency );
			  return properties;
		 }
	}

}