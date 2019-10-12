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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using DefaultFileSystemExtension = Org.Neo4j.Test.extension.DefaultFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneSchemaIndexBuilderTest
	internal class LuceneSchemaIndexBuilderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fileSystemRule;
		 private DefaultFileSystemAbstraction _fileSystemRule;

		 private readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.forLabel( 0, 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readOnlyIndexCreation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ReadOnlyIndexCreation()
		 {
			  using ( SchemaIndex schemaIndex = LuceneSchemaIndexBuilder.Create( _descriptor, ReadOnlyConfig ).withFileSystem( _fileSystemRule ).withOperationalMode( OperationalMode.single ).withIndexRootFolder( _testDir.directory( "a" ) ).build() )
			  {
					assertTrue( schemaIndex.ReadOnly, "Builder should construct read only index." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writableIndexCreation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void WritableIndexCreation()
		 {
			  using ( SchemaIndex schemaIndex = LuceneSchemaIndexBuilder.Create( _descriptor, DefaultConfig ).withFileSystem( _fileSystemRule ).withOperationalMode( OperationalMode.single ).withIndexRootFolder( _testDir.directory( "b" ) ).build() )
			  {
					assertFalse( schemaIndex.ReadOnly, "Builder should construct writable index." );
			  }
		 }

		 private static Config DefaultConfig
		 {
			 get
			 {
				  return Config.defaults();
			 }
		 }

		 private static Config ReadOnlyConfig
		 {
			 get
			 {
				  return Config.defaults( GraphDatabaseSettings.read_only, Settings.TRUE );
			 }
		 }
	}

}