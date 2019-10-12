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
namespace Neo4Net.Test.extension
{
	using Nested = org.junit.jupiter.api.Nested;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(DefaultFileSystemExtension.class) class DefaultFileSystemExtensionTest
	internal class DefaultFileSystemExtensionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject DefaultFileSystemAbstraction rootFileSystem;
		 internal DefaultFileSystemAbstraction RootFileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileSystemInjectionCreateFileSystem()
		 internal virtual void FileSystemInjectionCreateFileSystem()
		 {
			  assertNotNull( RootFileSystem );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nested class NestedFileSystemTest
		 internal class NestedFileSystemTest
		 {
			 private readonly DefaultFileSystemExtensionTest _outerInstance;

			 public NestedFileSystemTest( DefaultFileSystemExtensionTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject DefaultFileSystemAbstraction nestedFileSystem;
			  internal DefaultFileSystemAbstraction NestedFileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nestedFileSystemInjection()
			  internal virtual void NestedFileSystemInjection()
			  {
					assertNotNull( NestedFileSystem );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rootFileSystemAvailable()
			  internal virtual void RootFileSystemAvailable()
			  {
					assertNotNull( outerInstance.RootFileSystem );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nestedAndRootFileSystemsAreTheSame()
			  internal virtual void NestedAndRootFileSystemsAreTheSame()
			  {
					assertSame( NestedFileSystem, outerInstance.RootFileSystem );
			  }
		 }
	}

}