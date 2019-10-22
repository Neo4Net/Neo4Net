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
namespace Neo4Net.Io.fs
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.DefaultFileSystemAbstraction.UNABLE_TO_CREATE_DIRECTORY_FORMAT;

	public class DefaultFileSystemAbstractionTest : FileSystemAbstractionTest
	{
		 protected internal override FileSystemAbstraction BuildFileSystemAbstraction()
		 {
			  return new DefaultFileSystemAbstraction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailGracefullyWhenPathCannotBeCreated()
		 internal virtual void ShouldFailGracefullyWhenPathCannotBeCreated()
		 {
			  Path = new FileAnonymousInnerClass( this, TestDirectory.directory(), System.Guid.randomUUID().ToString() );

			  IOException exception = assertThrows( typeof( IOException ), () => Fsa.mkdirs(Path) );
			  assertFalse( Fsa.fileExists( Path ) );
			  string expectedMessage = format( UNABLE_TO_CREATE_DIRECTORY_FORMAT, Path );
			  assertThat( exception.Message, @is( expectedMessage ) );
		 }

		 private class FileAnonymousInnerClass : File
		 {
			 private readonly DefaultFileSystemAbstractionTest _outerInstance;

			 public FileAnonymousInnerClass( DefaultFileSystemAbstractionTest outerInstance, File directory, UnknownType valueOf ) : base( directory, valueOf )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool mkdirs()
			 {
				  return false;
			 }
		 }
	}

}