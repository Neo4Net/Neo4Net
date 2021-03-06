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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using MutableObjectIntMap = org.eclipse.collections.api.map.primitive.MutableObjectIntMap;
	using ObjectIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Node = Org.Neo4j.Graphdb.Node;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using AddNodeCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddNodeCommand;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using IndexDefineCommand = Org.Neo4j.Kernel.impl.index.IndexDefineCommand;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using EphemeralFileSystemExtension = Org.Neo4j.Test.extension.EphemeralFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.impl.lucene.@explicit.LuceneIndexImplementation.EXACT_CONFIG;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({EphemeralFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneCommandApplierTest
	internal class LuceneCommandApplierTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fs;
		 private EphemeralFileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMultipleIdSpaces() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleMultipleIdSpaces()
		 {
			  // GIVEN
			  string indexName = "name";
			  string key = "key";
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();
			  IndexConfigStore configStore = new IndexConfigStore( databaseLayout, _fs );
			  configStore.Set( typeof( Node ), indexName, EXACT_CONFIG );
			  using ( Lifespan lifespan = new Lifespan() )
			  {
					Config dataSourceConfig = Config.defaults( LuceneDataSource.Configuration.Ephemeral, Settings.TRUE );
					LuceneDataSource originalDataSource = new LuceneDataSource( databaseLayout, dataSourceConfig, configStore, _fs, OperationalMode.single );
					LuceneDataSource dataSource = lifespan.Add( spy( originalDataSource ) );

					using ( LuceneCommandApplier applier = new LuceneCommandApplier( dataSource, false ) )
					{
						 // WHEN issuing a command where the index name is mapped to a certain id
						 IndexDefineCommand definitions = definitions( ObjectIntHashMap.newWithKeysValues( indexName, 0 ), ObjectIntHashMap.newWithKeysValues( key, 0 ) );
						 applier.VisitIndexDefineCommand( definitions );
						 applier.VisitIndexAddNodeCommand( AddNodeToIndex( definitions, indexName, 0L ) );
						 // and then later issuing a command for that same index, but in another transaction where
						 // the local index name id is a different one
						 definitions = definitions( ObjectIntHashMap.newWithKeysValues( indexName, 1 ), ObjectIntHashMap.newWithKeysValues( key, 0 ) );
						 applier.VisitIndexDefineCommand( definitions );
						 applier.VisitIndexAddNodeCommand( AddNodeToIndex( definitions, indexName, 1L ) );
					}

					// THEN both those updates should have been directed to the same index
					verify( dataSource, times( 1 ) ).getIndexSearcher( any( typeof( IndexIdentifier ) ) );
			  }
		 }

		 private static AddNodeCommand AddNodeToIndex( IndexDefineCommand definitions, string indexName, long nodeId )
		 {
			  AddNodeCommand command = new AddNodeCommand();
			  command.Init( definitions.GetOrAssignIndexNameId( indexName ), nodeId, ( sbyte ) 0, "some value" );
			  return command;
		 }

		 private static IndexDefineCommand Definitions( MutableObjectIntMap<string> names, MutableObjectIntMap<string> keys )
		 {
			  IndexDefineCommand definitions = new IndexDefineCommand();
			  definitions.Init( names, keys );
			  return definitions;
		 }
	}

}