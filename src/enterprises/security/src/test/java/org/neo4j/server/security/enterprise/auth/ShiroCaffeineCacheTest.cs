using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Server.security.enterprise.auth
{
	using FakeTicker = com.google.common.testing.FakeTicker;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ShiroCaffeineCacheTest
	{
		 private ShiroCaffeineCache<int, string> _cache;
		 private FakeTicker _fakeTicker;
		 private long _ttl = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _fakeTicker = new FakeTicker();
			  _cache = new ShiroCaffeineCache<int, string>( _fakeTicker.read, ThreadStart.run, _ttl, 5, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateAuthCacheForTTLZeroIfUsingTLL()
		 public virtual void ShouldFailToCreateAuthCacheForTTLZeroIfUsingTLL()
		 {
			  new ShiroCaffeineCache<>( _fakeTicker.read, ThreadStart.run, 0, 5, false );
			  try
			  {
					new ShiroCaffeineCache<>( _fakeTicker.read, ThreadStart.run, 0, 5, true );
					fail( "Expected IllegalArgumentException for a TTL of 0" );
			  }
			  catch ( System.ArgumentException e )
			  {
					assertThat( e.Message, containsString( "TTL must be larger than zero." ) );
			  }
			  catch ( Exception )
			  {
					fail( "Expected IllegalArgumentException for a TTL of 0" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetNonExistentValue()
		 public virtual void ShouldNotGetNonExistentValue()
		 {
			  assertThat( _cache.get( 1 ), equalTo( null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPutAndGet()
		 public virtual void ShouldPutAndGet()
		 {
			  _cache.put( 1, "1" );
			  assertThat( _cache.get( 1 ), equalTo( "1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnExpiredValueThroughPut()
		 public virtual void ShouldNotReturnExpiredValueThroughPut()
		 {
			  assertNull( _cache.put( 1, "first" ) );
			  assertThat( _cache.put( 1, "second" ), equalTo( "first" ) );
			  _fakeTicker.advance( _ttl + 1, MILLISECONDS );
			  assertNull( _cache.put( 1, "third" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemove()
		 public virtual void ShouldRemove()
		 {
			  assertNull( _cache.remove( 1 ) );
			  _cache.put( 1, "1" );
			  assertThat( _cache.remove( 1 ), equalTo( "1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClear()
		 public virtual void ShouldClear()
		 {
			  _cache.put( 1, "1" );
			  _cache.put( 2, "2" );
			  assertThat( _cache.size(), equalTo(2) );
			  _cache.clear();
			  assertThat( _cache.size(), equalTo(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetKeys()
		 public virtual void ShouldGetKeys()
		 {
			  _cache.put( 1, "1" );
			  _cache.put( 2, "1" );
			  _cache.put( 3, "1" );
			  assertThat( _cache.keys(), containsInAnyOrder(1, 2, 3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetValues()
		 public virtual void ShouldGetValues()
		 {
			  _cache.put( 1, "1" );
			  _cache.put( 2, "1" );
			  _cache.put( 3, "1" );
			  assertThat( _cache.values(), containsInAnyOrder("1", "1", "1") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotListExpiredValues()
		 public virtual void ShouldNotListExpiredValues()
		 {
			  _cache.put( 1, "1" );
			  _fakeTicker.advance( _ttl + 1, MILLISECONDS );
			  _cache.put( 2, "foo" );

			  assertThat( _cache.values(), containsInAnyOrder("foo") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetExpiredValues()
		 public virtual void ShouldNotGetExpiredValues()
		 {
			  _cache.put( 1, "1" );
			  _fakeTicker.advance( _ttl + 1, MILLISECONDS );
			  _cache.put( 2, "foo" );

			  assertThat( _cache.get( 1 ), equalTo( null ) );
			  assertThat( _cache.get( 2 ), equalTo( "foo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetKeysForExpiredValues()
		 public virtual void ShouldNotGetKeysForExpiredValues()
		 {
			  _cache.put( 1, "1" );
			  _fakeTicker.advance( _ttl + 1, MILLISECONDS );
			  _cache.put( 2, "foo" );

			  assertThat( _cache.keys(), containsInAnyOrder(2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveIfExceededCapacity()
		 public virtual void ShouldRemoveIfExceededCapacity()
		 {
			  _cache.put( 1, "one" );
			  _cache.put( 2, "two" );
			  _cache.put( 3, "three" );
			  _cache.put( 4, "four" );
			  _cache.put( 5, "five" );
			  _cache.put( 6, "six" );

			  assertThat( _cache.size(), equalTo(5) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetValueAfterTimePassed()
		 public virtual void ShouldGetValueAfterTimePassed()
		 {
			  _cache.put( 1, "foo" );
			  _fakeTicker.advance( _ttl - 1, MILLISECONDS );
			  assertThat( _cache.get( 1 ), equalTo( "foo" ) );
		 }
	}

}