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
namespace Neo4Net.CommandLine.Admin
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using SystemExitRule = Neo4Net.Test.rule.system.SystemExitRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class RealOutsideWorldTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.system.SystemExitRule systemExitRule = Neo4Net.test.rule.system.SystemExitRule.none();
		 public SystemExitRule SystemExitRule = SystemExitRule.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeFileSystemOnClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseFileSystemOnClose()
		 {
			  RealOutsideWorld outsideWorld = new RealOutsideWorld();
			  FileSystemAbstraction fileSystemMock = mock( typeof( FileSystemAbstraction ) );
			  outsideWorld.FileSystemAbstraction = fileSystemMock;

			  outsideWorld.Dispose();

			  verify( fileSystemMock ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeFilesystemOnExit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseFilesystemOnExit()
		 {
			  RealOutsideWorld outsideWorld = new RealOutsideWorld();
			  FileSystemAbstraction fileSystemMock = mock( typeof( FileSystemAbstraction ) );
			  outsideWorld.FileSystemAbstraction = fileSystemMock;

			  SystemExitRule.expectExit( 0 );

			  outsideWorld.Exit( 0 );

			  verify( fileSystemMock ).close();
		 }
	}

}