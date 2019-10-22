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
namespace Neo4Net.Kernel.Api.Impl.Index.storage
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using IndexFolderLayout = Neo4Net.Kernel.Api.Impl.Index.storage.layout.IndexFolderLayout;
	using EphemeralFileSystemExtension = Neo4Net.Test.extension.EphemeralFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(EphemeralFileSystemExtension.class) class FailureStorageTest
	internal class FailureStorageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.graphdb.mockfs.EphemeralFileSystemAbstraction fs;
		 private EphemeralFileSystemAbstraction _fs;
		 private IndexFolderLayout _indexFolderLayout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void before()
		 internal virtual void Before()
		 {
			  File rootDirectory = new File( "dir" );
			  _fs.mkdirs( rootDirectory );
			  _indexFolderLayout = new IndexFolderLayout( rootDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReserveFailureFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReserveFailureFile()
		 {
			  // GIVEN
			  FailureStorage storage = new FailureStorage( _fs, _indexFolderLayout );

			  // WHEN
			  storage.ReserveForIndex();

			  // THEN
			  File failureFile = storage.FailureFile();
			  assertTrue( _fs.fileExists( failureFile ) );
			  assertTrue( _fs.getFileSize( failureFile ) > 100 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStoreFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldStoreFailure()
		 {
			  // GIVEN
			  FailureStorage storage = new FailureStorage( _fs, _indexFolderLayout );
			  storage.ReserveForIndex();
			  string failure = format( "A failure message%nspanning%nmultiple lines." );

			  // WHEN
			  storage.StoreIndexFailure( failure );

			  // THEN
			  File failureFile = storage.FailureFile();
			  assertTrue( _fs.fileExists( failureFile ) );
			  assertTrue( _fs.getFileSize( failureFile ) > 100 );
			  assertEquals( failure, storage.LoadIndexFailure() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldClearFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldClearFailure()
		 {
			  // GIVEN
			  FailureStorage storage = new FailureStorage( _fs, _indexFolderLayout );
			  storage.ReserveForIndex();
			  string failure = format( "A failure message%nspanning%nmultiple lines." );
			  storage.StoreIndexFailure( failure );
			  File failureFile = storage.FailureFile();
			  assertTrue( _fs.fileExists( failureFile ) );
			  assertTrue( _fs.getFileSize( failureFile ) > 100 );

			  // WHEN
			  storage.ClearForIndex();

			  // THEN
			  assertFalse( _fs.fileExists( failureFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAppendFailureIfAlreadyExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldAppendFailureIfAlreadyExists()
		 {
			  // GIVEN
			  FailureStorage storage = new FailureStorage( _fs, _indexFolderLayout );
			  storage.ReserveForIndex();
			  string failure1 = "Once upon a time there was a first failure";
			  string failure2 = "Then there was another";
			  storage.StoreIndexFailure( failure1 );

			  // WHEN
			  storage.StoreIndexFailure( failure2 );

			  // THEN
			  string allFailures = storage.LoadIndexFailure();
			  assertThat( allFailures, containsString( failure1 ) );
			  assertThat( allFailures, containsString( failure2 ) );
		 }
	}

}