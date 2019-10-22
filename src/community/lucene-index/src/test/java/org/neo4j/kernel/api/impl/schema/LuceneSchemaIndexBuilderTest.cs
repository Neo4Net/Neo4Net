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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneSchemaIndexBuilderTest
	internal class LuceneSchemaIndexBuilderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.io.fs.DefaultFileSystemAbstraction fileSystemRule;
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