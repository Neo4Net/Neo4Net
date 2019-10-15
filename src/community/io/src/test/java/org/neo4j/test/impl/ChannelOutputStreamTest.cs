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
namespace Neo4Net.Test.impl
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class ChannelOutputStreamTest
	internal class ChannelOutputStreamTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory tmpDir;
		 internal TestDirectory TmpDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStoreAByteAtBoundary() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldStoreAByteAtBoundary()
		 {
			  using ( EphemeralFileSystemAbstraction fs = new EphemeralFileSystemAbstraction() )
			  {
					File workFile = TmpDir.file( "a" );
					fs.Mkdirs( TmpDir.directory() );
					Stream @out = fs.OpenAsOutputStream( workFile, false );

					// When I write a byte[] that is larger than the internal buffer in
					// ChannelOutputStream..
					sbyte[] b = new sbyte[8097];
					b[b.Length - 1] = 7;
					@out.Write( b, 0, b.Length );
					@out.Flush();

					// Then it should get cleanly written and be readable
					Stream @in = fs.OpenAsInputStream( workFile );
					@in.skip( 8096 );
					assertEquals( 7, @in.Read() );
			  }
		 }
	}

}