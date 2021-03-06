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
namespace Org.Neo4j.Commandline.dbms
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.isSameOrChildFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.isSameOrChildPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.neo4jVersion;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class UtilTest
	internal class UtilTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory directory;
		 private TestDirectory _directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void canonicalPath()
		 internal virtual void CanonicalPath()
		 {
			  assertNotNull( Util.canonicalPath( "foo" ).Parent );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void returnsAVersion()
		 internal virtual void ReturnsAVersion()
		 {
			  assertNotNull( neo4jVersion(), "A version should be returned" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void correctlyIdentifySameOrChildFile()
		 internal virtual void CorrectlyIdentifySameOrChildFile()
		 {
			  assertTrue( isSameOrChildFile( _directory.directory(), _directory.directory("a") ) );
			  assertTrue( isSameOrChildFile( _directory.directory(), _directory.directory() ) );
			  assertTrue( isSameOrChildFile( _directory.directory( "/a/./b" ), _directory.directory( "a/b" ) ) );
			  assertTrue( isSameOrChildFile( _directory.directory( "a/b" ), _directory.directory( "/a/./b" ) ) );

			  assertFalse( isSameOrChildFile( _directory.directory( "a" ), _directory.directory( "b" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void correctlyIdentifySameOrChildPath()
		 internal virtual void CorrectlyIdentifySameOrChildPath()
		 {
			  assertTrue( isSameOrChildPath( _directory.directory().toPath(), _directory.directory("a").toPath() ) );
			  assertTrue( isSameOrChildPath( _directory.directory().toPath(), _directory.directory().toPath() ) );
			  assertTrue( isSameOrChildPath( _directory.directory( "/a/./b" ).toPath(), _directory.directory("a/b").toPath() ) );
			  assertTrue( isSameOrChildPath( _directory.directory( "a/b" ).toPath(), _directory.directory("/a/./b").toPath() ) );

			  assertFalse( isSameOrChildPath( _directory.directory( "a" ).toPath(), _directory.directory("b").toPath() ) );
		 }
	}

}