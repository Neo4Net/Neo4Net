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
namespace Neo4Net.Kernel.impl.storemigration.participant
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using RecordStorageReader = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;
	using StoragePropertyCursor = Neo4Net.Storageengine.Api.StoragePropertyCursor;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using InputEntityVisitor = Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;

	public class StoreScanChunkIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void differentChunksHaveDifferentCursors()
		 public virtual void DifferentChunksHaveDifferentCursors()
		 {
			  GraphDatabaseAPI database = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
			  try
			  {
					RecordStorageEngine recordStorageEngine = database.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );
					NeoStores neoStores = recordStorageEngine.TestAccessNeoStores();
					RecordStorageReader storageReader = new RecordStorageReader( neoStores );
					TestStoreScanChunk scanChunk1 = new TestStoreScanChunk( this, storageReader, false );
					TestStoreScanChunk scanChunk2 = new TestStoreScanChunk( this, storageReader, false );
					assertNotSame( scanChunk1.Cursor, scanChunk2.Cursor );
					assertNotSame( scanChunk1.StorePropertyCursor, scanChunk2.StorePropertyCursor );
			  }
			  finally
			  {
					database.Shutdown();
			  }
		 }

		 private class TestStoreScanChunk : StoreScanChunk<StorageNodeCursor>
		 {
			 private readonly StoreScanChunkIT _outerInstance;

			  internal TestStoreScanChunk( StoreScanChunkIT outerInstance, RecordStorageReader storageReader, bool requiresPropertyMigration ) : base( storageReader.AllocateNodeCursor(), storageReader, requiresPropertyMigration )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void Read( StorageNodeCursor cursor, long id )
			  {
					cursor.Single( id );
			  }

			  internal override void VisitRecord( StorageNodeCursor record, InputEntityVisitor visitor )
			  {
					// empty
			  }

			  internal virtual StorageNodeCursor Cursor
			  {
				  get
				  {
						return Cursor;
				  }
			  }

			  internal virtual StoragePropertyCursor StorePropertyCursor
			  {
				  get
				  {
						return StorePropertyCursor;
				  }
			  }
		 }
	}

}