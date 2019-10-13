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
namespace Neo4Net.Kernel.Api.Impl.Index.storage.layout
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class IndexFolderLayoutTest
	{
		 private readonly File _indexRoot = new File( "indexRoot" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testIndexFolder()
		 internal virtual void TestIndexFolder()
		 {
			  IndexFolderLayout indexLayout = CreateTestIndex();
			  File indexFolder = indexLayout.IndexFolder;

			  assertEquals( _indexRoot, indexFolder );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testIndexPartitionFolder()
		 internal virtual void TestIndexPartitionFolder()
		 {
			  IndexFolderLayout indexLayout = CreateTestIndex();

			  File indexFolder = indexLayout.IndexFolder;
			  File partitionFolder1 = indexLayout.GetPartitionFolder( 1 );
			  File partitionFolder3 = indexLayout.GetPartitionFolder( 3 );

			  assertEquals( partitionFolder1.ParentFile, partitionFolder3.ParentFile );
			  assertEquals( indexFolder, partitionFolder1.ParentFile );
			  assertEquals( "1", partitionFolder1.Name );
			  assertEquals( "3", partitionFolder3.Name );
		 }

		 private IndexFolderLayout CreateTestIndex()
		 {
			  return new IndexFolderLayout( _indexRoot );
		 }
	}

}