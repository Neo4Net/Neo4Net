using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class InFlightLogEntriesCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCacheUntilEnabled()
		 public virtual void ShouldNotCacheUntilEnabled()
		 {
			  InFlightMap<object> cache = new InFlightMap<object>();
			  object entry = new object();

			  cache.Put( 1L, entry );
			  assertNull( cache.Get( 1L ) );

			  cache.Enable();
			  cache.Put( 1L, entry );
			  assertEquals( entry, cache.Get( 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRegisterAndUnregisterValues()
		 public virtual void ShouldRegisterAndUnregisterValues()
		 {
			  InFlightMap<object> entries = new InFlightMap<object>();
			  entries.Enable();

			  IDictionary<long, object> logEntryList = new Dictionary<long, object>();
			  logEntryList[1L] = new object();

			  foreach ( KeyValuePair<long, object> entry in logEntryList.SetOfKeyValuePairs() )
			  {
					entries.Put( entry.Key, entry.Value );
			  }

			  foreach ( KeyValuePair<long, object> entry in logEntryList.SetOfKeyValuePairs() )
			  {
					object retrieved = entries.Get( entry.Key );
					assertEquals( entry.Value, retrieved );
			  }

			  long? unexpected = 2L;
			  object shouldBeNull = entries.Get( unexpected );
			  assertNull( shouldBeNull );

			  foreach ( KeyValuePair<long, object> entry in logEntryList.SetOfKeyValuePairs() )
			  {
					bool wasThere = entries.Remove( entry.Key );
					assertTrue( wasThere );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldNotReinsertValues()
		 public virtual void ShouldNotReinsertValues()
		 {
			  InFlightMap<object> entries = new InFlightMap<object>();
			  entries.Enable();
			  object addedObject = new object();
			  entries.Put( 1L, addedObject );
			  entries.Put( 1L, addedObject );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReplaceRegisteredValues()
		 public virtual void ShouldNotReplaceRegisteredValues()
		 {
			  InFlightMap<object> cache = new InFlightMap<object>();
			  cache.Enable();
			  object first = new object();
			  object second = new object();

			  try
			  {
					cache.Put( 1L, first );
					cache.Put( 1L, second );
					fail( "Should not allow silent replacement of values" );
			  }
			  catch ( System.ArgumentException )
			  {
					assertEquals( first, cache.Get( 1L ) );
			  }
		 }
	}

}