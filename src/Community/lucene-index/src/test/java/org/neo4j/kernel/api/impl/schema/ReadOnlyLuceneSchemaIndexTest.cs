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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using ReadOnlyIndexPartitionFactory = Neo4Net.Kernel.Api.Impl.Index.partition.ReadOnlyIndexPartitionFactory;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class ReadOnlyLuceneSchemaIndexTest
	internal class ReadOnlyLuceneSchemaIndexTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;

		 private ReadOnlyDatabaseSchemaIndex _luceneSchemaIndex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  PartitionedIndexStorage indexStorage = new PartitionedIndexStorage( DirectoryFactory.PERSISTENT, _fileSystem, _testDirectory.directory() );
			  Config config = Config.defaults();
			  IndexSamplingConfig samplingConfig = new IndexSamplingConfig( config );
			  _luceneSchemaIndex = new ReadOnlyDatabaseSchemaIndex( indexStorage, TestIndexDescriptorFactory.forLabel( 0, 0 ), samplingConfig, new ReadOnlyIndexPartitionFactory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TearDown()
		 {
			  _luceneSchemaIndex.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void indexDeletionIndReadOnlyModeIsNotSupported()
		 internal virtual void IndexDeletionIndReadOnlyModeIsNotSupported()
		 {
			  assertThrows( typeof( System.NotSupportedException ), () => _luceneSchemaIndex.drop() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void indexCreationInReadOnlyModeIsNotSupported()
		 internal virtual void IndexCreationInReadOnlyModeIsNotSupported()
		 {
			  assertThrows( typeof( System.NotSupportedException ), () => _luceneSchemaIndex.create() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readOnlyIndexMarkingIsNotSupported()
		 internal virtual void ReadOnlyIndexMarkingIsNotSupported()
		 {
			  assertThrows( typeof( System.NotSupportedException ), () => _luceneSchemaIndex.markAsOnline() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readOnlyIndexMode()
		 internal virtual void ReadOnlyIndexMode()
		 {
			  assertTrue( _luceneSchemaIndex.ReadOnly );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writerIsNotAccessibleInReadOnlyMode()
		 internal virtual void WriterIsNotAccessibleInReadOnlyMode()
		 {
			  assertThrows( typeof( System.NotSupportedException ), () => _luceneSchemaIndex.IndexWriter );
		 }
	}

}