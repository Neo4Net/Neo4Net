using System;

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
namespace Org.Neo4j.causalclustering.catchup
{
	using Test = org.junit.Test;


	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class CatchUpChannelPoolTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReUseAChannelThatWasReleased() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReUseAChannelThatWasReleased()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  CatchUpChannelPool<TestChannel> pool = new CatchUpChannelPool<TestChannel>( TestChannel::new );

			  // when
			  TestChannel channelA = pool.Acquire( LocalAddress( 1 ) );
			  pool.Release( channelA );
			  TestChannel channelB = pool.Acquire( LocalAddress( 1 ) );

			  // then
			  assertSame( channelA, channelB );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateANewChannelIfFirstChannelIsDisposed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateANewChannelIfFirstChannelIsDisposed()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  CatchUpChannelPool<TestChannel> pool = new CatchUpChannelPool<TestChannel>( TestChannel::new );

			  // when
			  TestChannel channelA = pool.Acquire( LocalAddress( 1 ) );
			  pool.Dispose( channelA );
			  TestChannel channelB = pool.Acquire( LocalAddress( 1 ) );

			  // then
			  assertNotSame( channelA, channelB );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateANewChannelIfFirstChannelIsStillActive() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateANewChannelIfFirstChannelIsStillActive()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  CatchUpChannelPool<TestChannel> pool = new CatchUpChannelPool<TestChannel>( TestChannel::new );

			  // when
			  TestChannel channelA = pool.Acquire( LocalAddress( 1 ) );
			  TestChannel channelB = pool.Acquire( LocalAddress( 1 ) );

			  // then
			  assertNotSame( channelA, channelB );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCleanUpOnClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCleanUpOnClose()
		 {
			  // given
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  CatchUpChannelPool<TestChannel> pool = new CatchUpChannelPool<TestChannel>( TestChannel::new );

			  TestChannel channelA = pool.Acquire( LocalAddress( 1 ) );
			  TestChannel channelB = pool.Acquire( LocalAddress( 1 ) );
			  TestChannel channelC = pool.Acquire( LocalAddress( 1 ) );

			  pool.Release( channelA );
			  pool.Release( channelC );

			  TestChannel channelD = pool.Acquire( LocalAddress( 2 ) );
			  TestChannel channelE = pool.Acquire( LocalAddress( 2 ) );
			  TestChannel channelF = pool.Acquire( LocalAddress( 2 ) );

			  // when
			  pool.Close();

			  // then
			  assertTrue( channelA.Closed );
			  assertTrue( channelB.Closed );
			  assertTrue( channelC.Closed );
			  assertTrue( channelD.Closed );
			  assertTrue( channelE.Closed );
			  assertTrue( channelF.Closed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWithExceptionIsChannelIsNotActive()
		 public virtual void ShouldFailWithExceptionIsChannelIsNotActive()
		 {
			  CatchUpChannelPool<TestChannel> pool = new CatchUpChannelPool<TestChannel>( advertisedSocketAddress => new TestChannel( advertisedSocketAddress, false ) );

			  try
			  {
					pool.Acquire( LocalAddress( 1 ) );
			  }
			  catch ( Exception e )
			  {
					assertEquals( typeof( ConnectException ), e.GetType() );
					assertEquals( "Unable to connect to localhost:1", e.Message );
					return;
			  }
			  fail();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckConnectionOnIdleChannelFirst()
		 public virtual void ShouldCheckConnectionOnIdleChannelFirst()
		 {
			  // given
			  CatchUpChannelPool<TestChannel> pool = new CatchUpChannelPool<TestChannel>( new FuncAnonymousInnerClass( this ) );

			  TestChannel channel = null;
			  try
			  {
					channel = pool.Acquire( LocalAddress( 1 ) );
					assertNotNull( channel );
			  }
			  catch ( Exception )
			  {
					fail( "Not expected exception" );
			  }

			  // when channel loses connection in idle
			  channel.IsActive = false;
			  pool.Release( channel );

			  try
			  {
					// then
					pool.Acquire( LocalAddress( 1 ) );
			  }
			  catch ( Exception e )
			  {
					assertEquals( typeof( ConnectException ), e.GetType() );
					assertEquals( "Unable to connect to localhost:1", e.Message );
					return;
			  }
			  fail();
		 }

		 private class FuncAnonymousInnerClass : System.Func<AdvertisedSocketAddress, TestChannel>
		 {
			 private readonly CatchUpChannelPoolTest _outerInstance;

			 public FuncAnonymousInnerClass( CatchUpChannelPoolTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 firstIsActive = true;
			 }

			 internal bool firstIsActive;

			 public override TestChannel apply( AdvertisedSocketAddress address )
			 {
				  TestChannel testChannel = new TestChannel( address, firstIsActive );
				  firstIsActive = false;
				  return testChannel;
			 }
		 }

		 private class TestChannel : CatchUpChannelPool.Channel
		 {
			  internal readonly AdvertisedSocketAddress Address;
			  internal bool IsActive;
			  internal bool Closed;

			  internal TestChannel( AdvertisedSocketAddress address, bool isActive )
			  {

					this.Address = address;
					this.IsActive = isActive;
			  }

			  internal TestChannel( AdvertisedSocketAddress address ) : this( address, true )
			  {
			  }

			  public override AdvertisedSocketAddress Destination()
			  {
					return Address;
			  }

			  public override void Connect()
			  {
					// do nothing
			  }

			  public virtual bool Active
			  {
				  get
				  {
						return IsActive;
				  }
			  }

			  public override void Close()
			  {
					Closed = true;
			  }
		 }

		 private static AdvertisedSocketAddress LocalAddress( int port )
		 {
			  return new AdvertisedSocketAddress( "localhost", port );
		 }
	}

}