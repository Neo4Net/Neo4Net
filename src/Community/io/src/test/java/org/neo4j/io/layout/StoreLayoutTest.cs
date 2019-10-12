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
namespace Neo4Net.Io.layout
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class StoreLayoutTest
	internal class StoreLayoutTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void storeLayoutForAbsoluteFile()
		 internal virtual void StoreLayoutForAbsoluteFile()
		 {
			  File storeDir = _testDirectory.storeDir();
			  StoreLayout storeLayout = StoreLayout.Of( storeDir );
			  assertEquals( storeDir, storeLayout.StoreDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void storeLayoutResolvesLinks() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StoreLayoutResolvesLinks()
		 {
			  Path basePath = _testDirectory.directory().toPath();
			  File storeDir = _testDirectory.storeDir( "notAbsolute" );
			  Path linkPath = basePath.resolve( "link" );
			  Path symbolicLink = Files.createSymbolicLink( linkPath, storeDir.toPath() );
			  StoreLayout storeLayout = StoreLayout.Of( symbolicLink.toFile() );
			  assertEquals( storeDir, storeLayout.StoreDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void storeLayoutUseCanonicalRepresentation()
		 internal virtual void StoreLayoutUseCanonicalRepresentation()
		 {
			  Path basePath = _testDirectory.storeDir( "notCanonical" ).toPath();
			  Path notCanonicalPath = basePath.resolve( "../anotherLocation" );
			  StoreLayout storeLayout = StoreLayout.Of( notCanonicalPath.toFile() );
			  assertEquals( _testDirectory.directory( "anotherLocation" ), storeLayout.StoreDirectory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void storeLockFileLocation()
		 internal virtual void StoreLockFileLocation()
		 {
			  StoreLayout storeLayout = _testDirectory.storeLayout();
			  File storeLockFile = storeLayout.StoreLockFile();
			  assertEquals( "store_lock", storeLockFile.Name );
			  assertEquals( storeLayout.StoreDirectory(), storeLockFile.ParentFile );
		 }
	}

}