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
namespace Neo4Net.Kernel.impl.store
{
	using Test = org.junit.Test;

	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class StoreTypeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeTypeOfValidStoreFile()
		 public virtual void StoreTypeOfValidStoreFile()
		 {
			  StoreType matchedType = StoreType.typeOf( DatabaseFile.NODE_STORE ).orElseThrow( () => new System.InvalidOperationException("Store type not found") );
			  assertEquals( StoreType.Node, matchedType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeTypeOfMetaDataStoreFile()
		 public virtual void StoreTypeOfMetaDataStoreFile()
		 {
			  StoreType matchedType = StoreType.typeOf( DatabaseFile.METADATA_STORE ).orElseThrow( () => new System.InvalidOperationException("Store type not found") );
			  assertEquals( StoreType.MetaData, matchedType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeTypeofSomeInvalidFile()
		 public virtual void StoreTypeofSomeInvalidFile()
		 {
			  assertThat( StoreType.typeOf( DatabaseFile.LABEL_SCAN_STORE ), @is( null ) );
		 }
	}

}