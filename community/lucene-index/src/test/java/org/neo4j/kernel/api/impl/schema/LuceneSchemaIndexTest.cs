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
	using Document = org.apache.lucene.document.Document;
	using Field = org.apache.lucene.document.Field;
	using StringField = org.apache.lucene.document.StringField;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using IOUtils = Org.Neo4j.Io.IOUtils;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using DefaultFileSystemExtension = Org.Neo4j.Test.extension.DefaultFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneSchemaIndexTest
	internal class LuceneSchemaIndexTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fs;
		 private DefaultFileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

		 private readonly DirectoryFactory _dirFactory = new Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory();
		 private SchemaIndex _index;
		 private readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.forLabel( 3, 5 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void closeIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseIndex()
		 {
			  IOUtils.closeAll( _index, _dirFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markAsOnline() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MarkAsOnline()
		 {
			  _index = CreateIndex();
			  _index.IndexWriter.addDocument( NewDocument() );
			  _index.markAsOnline();

			  assertTrue( _index.Online, "Should have had online status set" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markAsOnlineAndClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MarkAsOnlineAndClose()
		 {
			  _index = CreateIndex();
			  _index.IndexWriter.addDocument( NewDocument() );
			  _index.markAsOnline();

			  _index.close();

			  _index = OpenIndex();
			  assertTrue( _index.Online, "Should have had online status set" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markAsOnlineTwice() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MarkAsOnlineTwice()
		 {
			  _index = CreateIndex();
			  _index.markAsOnline();

			  _index.IndexWriter.addDocument( NewDocument() );
			  _index.markAsOnline();

			  assertTrue( _index.Online, "Should have had online status set" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markAsOnlineTwiceAndClose() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MarkAsOnlineTwiceAndClose()
		 {
			  _index = CreateIndex();
			  _index.markAsOnline();

			  _index.IndexWriter.addDocument( NewDocument() );
			  _index.markAsOnline();
			  _index.close();

			  _index = OpenIndex();
			  assertTrue( _index.Online, "Should have had online status set" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markAsOnlineIsRespectedByOtherWriter() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MarkAsOnlineIsRespectedByOtherWriter()
		 {
			  _index = CreateIndex();
			  _index.markAsOnline();
			  _index.close();

			  _index = OpenIndex();
			  _index.IndexWriter.addDocument( NewDocument() );
			  _index.close();

			  _index = OpenIndex();
			  assertTrue( _index.Online, "Should have had online status set" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SchemaIndex createIndex() throws java.io.IOException
		 private SchemaIndex CreateIndex()
		 {
			  SchemaIndex schemaIndex = NewSchemaIndex();
			  schemaIndex.create();
			  schemaIndex.open();
			  return schemaIndex;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SchemaIndex openIndex() throws java.io.IOException
		 private SchemaIndex OpenIndex()
		 {
			  SchemaIndex schemaIndex = NewSchemaIndex();
			  schemaIndex.open();
			  return schemaIndex;
		 }

		 private SchemaIndex NewSchemaIndex()
		 {
			  LuceneSchemaIndexBuilder builder = LuceneSchemaIndexBuilder.Create( _descriptor, Config.defaults() );
			  return builder.WithIndexRootFolder( new File( _testDir.directory( "index" ), "testIndex" ) ).withDirectoryFactory( _dirFactory ).withFileSystem( _fs ).build();
		 }

		 private static Document NewDocument()
		 {
			  Document doc = new Document();
			  doc.add( new StringField( "test", System.Guid.randomUUID().ToString(), Field.Store.YES ) );
			  return doc;
		 }
	}

}