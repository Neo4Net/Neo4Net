using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.util
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.AutoCreatingHashMap.nested;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.AutoCreatingHashMap.values;

	public class AutoCreatingHashMapTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateValuesIfMissing()
		 public virtual void ShouldCreateValuesIfMissing()
		 {
			  // GIVEN
			  IDictionary<string, AtomicLong> map = new AutoCreatingHashMap<string, AtomicLong>( values( typeof( AtomicLong ) ) );
			  string key = "should be created";

			  // WHEN
			  map[key].incrementAndGet();

			  // THEN
			  assertEquals( 1, map[key].get() );
			  assertTrue( map.ContainsKey( key ) );
			  assertFalse( map.ContainsKey( "any other key" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateValuesEvenForNestedMaps()
		 public virtual void ShouldCreateValuesEvenForNestedMaps()
		 {
			  // GIVEN
			  IDictionary<string, IDictionary<string, IDictionary<string, AtomicLong>>> map = new AutoCreatingHashMap<string, IDictionary<string, IDictionary<string, AtomicLong>>>( nested( typeof( string ), nested( typeof( string ), values( typeof( AtomicLong ) ) ) ) );
			  string keyLevelOne = "first";
			  string keyLevelTwo = "second";
			  string keyLevelThree = "third";

			  // WHEN
			  map[keyLevelOne][keyLevelTwo][keyLevelThree].addAndGet( 10 );

			  // THEN
			  assertTrue( map.ContainsKey( keyLevelOne ) );
			  assertFalse( map.ContainsKey( keyLevelTwo ) ); // or any other value for that matter
			  IDictionary<string, IDictionary<string, AtomicLong>> levelOne = map[keyLevelOne];
			  assertTrue( levelOne.ContainsKey( keyLevelTwo ) );
			  assertFalse( levelOne.ContainsKey( keyLevelThree ) ); // or any other value for that matter
			  IDictionary<string, AtomicLong> levelTwo = levelOne[keyLevelTwo];
			  assertTrue( levelTwo.ContainsKey( keyLevelThree ) );
			  assertFalse( levelTwo.ContainsKey( keyLevelOne ) ); // or any other value for that matter
			  AtomicLong levelThree = levelTwo[keyLevelThree];
			  assertEquals( 10, levelThree.get() );
		 }
	}

}