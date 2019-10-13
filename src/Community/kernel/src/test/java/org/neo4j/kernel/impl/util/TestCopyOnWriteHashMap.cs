using System.Collections;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.util
{
	using Test = org.junit.Test;


	using Iterators = Neo4Net.Helpers.Collections.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class TestCopyOnWriteHashMap
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void keySetUnaffectedByChanges()
		 public virtual void KeySetUnaffectedByChanges()
		 {
			  IDictionary<int, string> map = new CopyOnWriteHashMap<int, string>();
			  map[0] = "0";
			  map[1] = "1";
			  map[2] = "2";

			  assertThat( map, hasKey( 0 ) );
			  assertThat( map, hasKey( 1 ) );
			  assertThat( map, hasKey( 2 ) );

			  IEnumerator<int> keys = map.Keys.GetEnumerator();
			  map.Remove( 1 );
			  IList<int> keysBeforeDeletion = Iterators.asList( keys );
			  assertThat( keysBeforeDeletion, contains( 0, 1, 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void entrySetUnaffectedByChanges()
		 public virtual void EntrySetUnaffectedByChanges()
		 {
			  IDictionary<int, string> map = new CopyOnWriteHashMap<int, string>();
			  map[0] = "0";
			  map[1] = "1";
			  map[2] = "2";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map.Entry<int, String>[] allEntries = map.entrySet().toArray(new java.util.Map.Entry[0]);
			  KeyValuePair<int, string>[] allEntries = map.SetOfKeyValuePairs().toArray(new DictionaryEntry[0]);

			  assertThat( map.SetOfKeyValuePairs(), containsInAnyOrder(allEntries) );

			  IEnumerator<KeyValuePair<int, string>> entries = map.SetOfKeyValuePairs().GetEnumerator();
			  map.Remove( 1 );
			  IList<KeyValuePair<int, string>> entriesBeforeRemoval = Iterators.asList( entries );
			  assertThat( entriesBeforeRemoval, containsInAnyOrder( allEntries ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotShouldKeepData()
		 public virtual void SnapshotShouldKeepData()
		 {
			  CopyOnWriteHashMap<int, string> map = new CopyOnWriteHashMap<int, string>();
			  map[0] = "0";
			  IDictionary<int, string> snapshot = map.Snapshot();
			  assertThat( snapshot[0], @is( "0" ) );
			  assertThat( map.Remove( 0 ), @is( "0" ) );
			  assertThat( snapshot[0], @is( "0" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = UnsupportedOperationException.class) public void snapshotMustBeUnmodifiable()
		 public virtual void SnapshotMustBeUnmodifiable()
		 {
			  ( new CopyOnWriteHashMap<>() ).Snapshot()[0] = "0";
		 }
	}

}