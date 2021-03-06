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
namespace Org.Neo4j.Consistency.repair
{
	using Test = org.junit.jupiter.api.Test;

	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class RecordSetTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void toStringShouldPlaceEachRecordOnItsOwnLine()
		 internal virtual void ToStringShouldPlaceEachRecordOnItsOwnLine()
		 {
			  // given
			  NodeRecord record1 = new NodeRecord( 1, false, 1, 1 );
			  NodeRecord record2 = new NodeRecord( 2, false, 2, 2 );
			  RecordSet<NodeRecord> set = new RecordSet<NodeRecord>();
			  set.Add( record1 );
			  set.Add( record2 );

			  // when
			  string @string = set.ToString();

			  // then
			  string[] lines = @string.Split( "\n", true );
			  assertEquals( 4, lines.Length );
			  assertEquals( "[", lines[0] );
			  assertEquals( record1.ToString() + ",", lines[1] );
			  assertEquals( record2.ToString() + ",", lines[2] );
			  assertEquals( "]", lines[3] );
		 }
	}

}