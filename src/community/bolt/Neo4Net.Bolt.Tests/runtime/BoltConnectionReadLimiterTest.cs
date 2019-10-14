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
namespace Neo4Net.Bolt.runtime
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using Job = Neo4Net.Bolt.v1.runtime.Job;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.NullResponseHandler.nullResponseHandler;

	public class BoltConnectionReadLimiterTest
	{
		 private static readonly Job _job = machine => machine.process( new InitMessage( "INIT", emptyMap() ), nullResponseHandler() );
		 private BoltConnection _connection;
		 private EmbeddedChannel _channel;
		 private Log _log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _channel = new EmbeddedChannel();
			  _log = mock( typeof( Log ) );

			  _connection = mock( typeof( BoltConnection ) );
			  when( _connection.id() ).thenReturn(_channel.id().asLongText());
			  when( _connection.channel() ).thenReturn(_channel);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _channel.finishAndReleaseAll();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDisableAutoReadBelowHighWatermark()
		 public virtual void ShouldNotDisableAutoReadBelowHighWatermark()
		 {
			  BoltConnectionReadLimiter limiter = NewLimiter( 1, 2 );

			  assertTrue( _channel.config().AutoRead );

			  limiter.Enqueued( _connection, _job );

			  assertTrue( _channel.config().AutoRead );
			  verify( _log, never() ).warn(anyString(), any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisableAutoReadWhenAtHighWatermark()
		 public virtual void ShouldDisableAutoReadWhenAtHighWatermark()
		 {
			  BoltConnectionReadLimiter limiter = NewLimiter( 1, 2 );

			  assertTrue( _channel.config().AutoRead );

			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );

			  assertFalse( _channel.config().AutoRead );
			  verify( _log ).warn( contains( "disabled" ), eq( _channel.remoteAddress() ), eq(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisableAutoReadOnlyOnceWhenAboveHighWatermark()
		 public virtual void ShouldDisableAutoReadOnlyOnceWhenAboveHighWatermark()
		 {
			  BoltConnectionReadLimiter limiter = NewLimiter( 1, 2 );

			  assertTrue( _channel.config().AutoRead );

			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );

			  assertFalse( _channel.config().AutoRead );
			  verify( _log, times( 1 ) ).warn( contains( "disabled" ), eq( _channel.remoteAddress() ), eq(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnableAutoReadWhenAtLowWatermark()
		 public virtual void ShouldEnableAutoReadWhenAtLowWatermark()
		 {
			  BoltConnectionReadLimiter limiter = NewLimiter( 1, 2 );

			  assertTrue( _channel.config().AutoRead );

			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );
			  limiter.Drained( _connection, Arrays.asList( _job, _job ) );

			  assertTrue( _channel.config().AutoRead );
			  verify( _log, times( 1 ) ).warn( contains( "disabled" ), eq( _channel.remoteAddress() ), eq(3) );
			  verify( _log, times( 1 ) ).warn( contains( "enabled" ), eq( _channel.remoteAddress() ), eq(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnableAutoReadOnlyOnceWhenBelowLowWatermark()
		 public virtual void ShouldEnableAutoReadOnlyOnceWhenBelowLowWatermark()
		 {
			  BoltConnectionReadLimiter limiter = NewLimiter( 1, 2 );

			  assertTrue( _channel.config().AutoRead );

			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );
			  limiter.Enqueued( _connection, _job );
			  limiter.Drained( _connection, Arrays.asList( _job, _job, _job ) );

			  assertTrue( _channel.config().AutoRead );
			  verify( _log, times( 1 ) ).warn( contains( "disabled" ), eq( _channel.remoteAddress() ), eq(3) );
			  verify( _log, times( 1 ) ).warn( contains( "enabled" ), eq( _channel.remoteAddress() ), eq(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisableAndEnableAutoRead()
		 public virtual void ShouldDisableAndEnableAutoRead()
		 {
			  int lowWatermark = 3;
			  int highWatermark = 5;
			  BoltConnectionReadLimiter limiter = NewLimiter( lowWatermark, highWatermark );

			  assertTrue( _channel.config().AutoRead );

			  for ( int i = 0; i < highWatermark + 1; i++ )
			  {
					limiter.Enqueued( _connection, _job );
			  }
			  assertFalse( _channel.config().AutoRead );

			  limiter.Drained( _connection, singleton( _job ) );
			  assertFalse( _channel.config().AutoRead );
			  limiter.Drained( _connection, singleton( _job ) );
			  assertFalse( _channel.config().AutoRead );

			  limiter.Drained( _connection, singleton( _job ) );
			  assertTrue( _channel.config().AutoRead );

			  for ( int i = 0; i < 3; i++ )
			  {
					limiter.Enqueued( _connection, _job );
			  }
			  assertFalse( _channel.config().AutoRead );

			  limiter.Drained( _connection, Arrays.asList( _job, _job, _job, _job, _job, _job ) );
			  assertTrue( _channel.config().AutoRead );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptNegativeLowWatermark()
		 public virtual void ShouldNotAcceptNegativeLowWatermark()
		 {
			  try
			  {
					NewLimiter( -1, 5 );
					fail( "exception expected" );
			  }
			  catch ( System.ArgumentException exc )
			  {
					assertThat( exc.Message, startsWith( "invalid lowWatermark value" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptLowWatermarkEqualToHighWatermark()
		 public virtual void ShouldNotAcceptLowWatermarkEqualToHighWatermark()
		 {
			  try
			  {
					NewLimiter( 5, 5 );
					fail( "exception expected" );
			  }
			  catch ( System.ArgumentException exc )
			  {
					assertThat( exc.Message, startsWith( "invalid lowWatermark value" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptLowWatermarkLargerThanHighWatermark()
		 public virtual void ShouldNotAcceptLowWatermarkLargerThanHighWatermark()
		 {
			  try
			  {
					NewLimiter( 6, 5 );
					fail( "exception expected" );
			  }
			  catch ( System.ArgumentException exc )
			  {
					assertThat( exc.Message, startsWith( "invalid lowWatermark value" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptZeroHighWatermark()
		 public virtual void ShouldNotAcceptZeroHighWatermark()
		 {
			  try
			  {
					NewLimiter( 1, 0 );
					fail( "exception expected" );
			  }
			  catch ( System.ArgumentException exc )
			  {
					assertThat( exc.Message, startsWith( "invalid highWatermark value" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptNegativeHighWatermark()
		 public virtual void ShouldNotAcceptNegativeHighWatermark()
		 {
			  try
			  {
					NewLimiter( 1, -1 );
					fail( "exception expected" );
			  }
			  catch ( System.ArgumentException exc )
			  {
					assertThat( exc.Message, startsWith( "invalid highWatermark value" ) );
			  }
		 }

		 private BoltConnectionReadLimiter NewLimiter( int low, int high )
		 {
			  LogService logService = mock( typeof( LogService ) );
			  when( logService.GetInternalLog( typeof( BoltConnectionReadLimiter ) ) ).thenReturn( _log );
			  return new BoltConnectionReadLimiter( logService, low, high );
		 }
	}

}