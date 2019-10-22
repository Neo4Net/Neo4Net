using System;
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
namespace Neo4Net.Io.layout
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class DatabaseLayoutTest
	internal class DatabaseLayoutTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void databaseLayoutForAbsoluteFile()
		 internal virtual void DatabaseLayoutForAbsoluteFile()
		 {
			  File databaseDir = _testDirectory.databaseDir();
			  DatabaseLayout databaseLayout = DatabaseLayout.Of( databaseDir );
			  assertEquals( databaseLayout.DatabaseDirectory(), databaseDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void databaseLayoutResolvesLinks() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DatabaseLayoutResolvesLinks()
		 {
			  Path basePath = _testDirectory.directory().toPath();
			  File databaseDir = _testDirectory.databaseDir( "notAbsolute" );
			  Path linkPath = basePath.resolve( "link" );
			  Path symbolicLink = Files.createSymbolicLink( linkPath, databaseDir.toPath() );
			  DatabaseLayout databaseLayout = DatabaseLayout.Of( symbolicLink.toFile() );
			  assertEquals( databaseLayout.DatabaseDirectory(), databaseDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void databaseLayoutUseCanonicalRepresentation()
		 internal virtual void DatabaseLayoutUseCanonicalRepresentation()
		 {
			  File storeDir = _testDirectory.storeDir( "notCanonical" );
			  Path basePath = _testDirectory.databaseDir( storeDir ).toPath();
			  Path notCanonicalPath = basePath.resolve( "../anotherDatabase" );
			  DatabaseLayout databaseLayout = DatabaseLayout.Of( notCanonicalPath.toFile() );
			  File expectedDirectory = StoreLayout.Of( storeDir ).databaseLayout( "anotherDatabase" ).databaseDirectory();
			  assertEquals( expectedDirectory, databaseLayout.DatabaseDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void databaseLayoutForName()
		 internal virtual void DatabaseLayoutForName()
		 {
			  string databaseName = "testDatabase";
			  StoreLayout storeLayout = _testDirectory.storeLayout();
			  DatabaseLayout testDatabase = DatabaseLayout.Of( storeLayout, databaseName );
			  assertEquals( new File( storeLayout.StoreDirectory(), databaseName ), testDatabase.DatabaseDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void databaseLayoutForFolderAndName()
		 internal virtual void DatabaseLayoutForFolderAndName()
		 {
			  string database = "database";
			  DatabaseLayout databaseLayout = DatabaseLayout.Of( _testDirectory.storeDir(), database );
			  assertEquals( _testDirectory.databaseLayout( database ).databaseDirectory(), databaseLayout.DatabaseDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void databaseLayoutProvideCorrectDatabaseName()
		 internal virtual void DatabaseLayoutProvideCorrectDatabaseName()
		 {
			  assertEquals( "graph.db", _testDirectory.databaseLayout().DatabaseName );
			  assertEquals( "testDb", _testDirectory.databaseLayout( "testDb" ).DatabaseName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void storeFilesHaveExpectedNames()
		 internal virtual void StoreFilesHaveExpectedNames()
		 {
			  DatabaseLayout layout = _testDirectory.databaseLayout();
			  assertEquals( "neostore", layout.MetadataStore().Name );
			  assertEquals( "neostore.counts.db.a", layout.CountStoreA().Name );
			  assertEquals( "neostore.counts.db.b", layout.CountStoreB().Name );
			  assertEquals( "neostore.labelscanstore.db", layout.LabelScanStore().Name );
			  assertEquals( "neostore.labeltokenstore.db", layout.LabelTokenStore().Name );
			  assertEquals( "neostore.labeltokenstore.db.names", layout.LabelTokenNamesStore().Name );
			  assertEquals( "neostore.nodestore.db", layout.NodeStore().Name );
			  assertEquals( "neostore.nodestore.db.labels", layout.NodeLabelStore().Name );
			  assertEquals( "neostore.propertystore.db", layout.PropertyStore().Name );
			  assertEquals( "neostore.propertystore.db.arrays", layout.PropertyArrayStore().Name );
			  assertEquals( "neostore.propertystore.db.index", layout.PropertyKeyTokenStore().Name );
			  assertEquals( "neostore.propertystore.db.index.keys", layout.PropertyKeyTokenNamesStore().Name );
			  assertEquals( "neostore.propertystore.db.strings", layout.PropertyStringStore().Name );
			  assertEquals( "neostore.relationshipgroupstore.db", layout.RelationshipGroupStore().Name );
			  assertEquals( "neostore.relationshipstore.db", layout.RelationshipStore().Name );
			  assertEquals( "neostore.relationshiptypestore.db", layout.RelationshipTypeTokenStore().Name );
			  assertEquals( "neostore.relationshiptypestore.db.names", layout.RelationshipTypeTokenNamesStore().Name );
			  assertEquals( "neostore.schemastore.db", layout.SchemaStore().Name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void idFilesHaveExpectedNames()
		 internal virtual void IdFilesHaveExpectedNames()
		 {
			  DatabaseLayout layout = _testDirectory.databaseLayout();
			  assertEquals( "neostore.id", layout.IdMetadataStore().Name );
			  assertEquals( "neostore.labeltokenstore.db.id", layout.IdLabelTokenStore().Name );
			  assertEquals( "neostore.labeltokenstore.db.names.id", layout.IdLabelTokenNamesStore().Name );
			  assertEquals( "neostore.nodestore.db.id", layout.IdNodeStore().Name );
			  assertEquals( "neostore.nodestore.db.labels.id", layout.IdNodeLabelStore().Name );
			  assertEquals( "neostore.propertystore.db.arrays.id", layout.IdPropertyArrayStore().Name );
			  assertEquals( "neostore.propertystore.db.id", layout.IdPropertyStore().Name );
			  assertEquals( "neostore.propertystore.db.index.id", layout.IdPropertyKeyTokenStore().Name );
			  assertEquals( "neostore.propertystore.db.index.keys.id", layout.IdPropertyKeyTokenNamesStore().Name );
			  assertEquals( "neostore.propertystore.db.strings.id", layout.IdPropertyStringStore().Name );
			  assertEquals( "neostore.relationshipgroupstore.db.id", layout.IdRelationshipGroupStore().Name );
			  assertEquals( "neostore.relationshipstore.db.id", layout.IdRelationshipStore().Name );
			  assertEquals( "neostore.relationshiptypestore.db.id", layout.IdRelationshipTypeTokenStore().Name );
			  assertEquals( "neostore.relationshiptypestore.db.names.id", layout.IdRelationshipTypeTokenNamesStore().Name );
			  assertEquals( "neostore.schemastore.db.id", layout.IdSchemaStore().Name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allStoreFiles()
		 internal virtual void AllStoreFiles()
		 {
			  DatabaseLayout layout = _testDirectory.databaseLayout();
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<string> files = layout.StoreFiles().Select(File.getName).collect(toSet());
			  assertThat( files, hasItem( "neostore" ) );
			  assertThat( files, hasItem( "neostore.counts.db.a" ) );
			  assertThat( files, hasItem( "neostore.counts.db.b" ) );
			  assertThat( files, hasItem( "neostore.labelscanstore.db" ) );
			  assertThat( files, hasItem( "neostore.labeltokenstore.db" ) );
			  assertThat( files, hasItem( "neostore.labeltokenstore.db.names" ) );
			  assertThat( files, hasItem( "neostore.nodestore.db" ) );
			  assertThat( files, hasItem( "neostore.nodestore.db.labels" ) );
			  assertThat( files, hasItem( "neostore.propertystore.db" ) );
			  assertThat( files, hasItem( "neostore.propertystore.db.arrays" ) );
			  assertThat( files, hasItem( "neostore.propertystore.db.index" ) );
			  assertThat( files, hasItem( "neostore.propertystore.db.index.keys" ) );
			  assertThat( files, hasItem( "neostore.propertystore.db.strings" ) );
			  assertThat( files, hasItem( "neostore.relationshipgroupstore.db" ) );
			  assertThat( files, hasItem( "neostore.relationshipstore.db" ) );
			  assertThat( files, hasItem( "neostore.relationshiptypestore.db" ) );
			  assertThat( files, hasItem( "neostore.relationshiptypestore.db.names" ) );
			  assertThat( files, hasItem( "neostore.schemastore.db" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lookupFileByDatabaseFile()
		 internal virtual void LookupFileByDatabaseFile()
		 {
			  DatabaseLayout layout = _testDirectory.databaseLayout();
			  DatabaseFile[] databaseFiles = DatabaseFile.values();
			  foreach ( DatabaseFile databaseFile in databaseFiles )
			  {
					assertNotNull( layout.File( databaseFile ).findAny().orElseThrow(() => new Exception("Mapping was expected to be found")) );
			  }

			  File metadata = layout.File( DatabaseFile.MetadataStore ).findFirst().orElseThrow(() => new Exception("Mapping was expected to be found"));
			  assertEquals( "neostore", metadata.Name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lookupIdFileByDatabaseFile()
		 internal virtual void LookupIdFileByDatabaseFile()
		 {
			  DatabaseLayout layout = _testDirectory.databaseLayout();
			  DatabaseFile[] databaseFiles = DatabaseFile.values();
			  foreach ( DatabaseFile databaseFile in databaseFiles )
			  {
					Optional<File> idFile = layout.IdFile( databaseFile );
					assertEquals( databaseFile.hasIdFile(), idFile.Present );
			  }

			  File metadataId = layout.IdFile( DatabaseFile.MetadataStore ).orElseThrow( () => new Exception("Mapping was expected to be found") );
			  assertEquals( "neostore.id", metadataId.Name );
		 }
	}

}