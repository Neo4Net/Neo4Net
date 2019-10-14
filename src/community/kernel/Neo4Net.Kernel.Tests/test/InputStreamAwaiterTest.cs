using System;
using System.Text;

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
namespace Neo4Net.Test
{
	using Test = org.junit.jupiter.api.Test;


	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;

	internal class InputStreamAwaiterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWaitForALineWithoutBlocking() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldWaitForALineWithoutBlocking()
		 {
			  // given
			  FakeClock clock = FakeClock;
			  Stream inputStream = spy( new MockInputStream( new Ticker( clock, 5, TimeUnit.MILLISECONDS ), Lines( "important message" ) ) );
			  InputStreamAwaiter awaiter = new InputStreamAwaiter( clock, inputStream );

			  // when
			  awaiter.AwaitLine( "important message", 5, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTimeoutWhenDifferentContentProvided()
		 internal virtual void ShouldTimeoutWhenDifferentContentProvided()
		 {
			  // given
			  FakeClock clock = FakeClock;
			  Stream inputStream = spy( new MockInputStream( new Ticker( clock, 1, TimeUnit.SECONDS ), Lines( "different content" ), Lines( "different message" ) ) );
			  InputStreamAwaiter awaiter = new InputStreamAwaiter( clock, inputStream );

			  // when
			  assertThrows( typeof( TimeoutException ), () => awaiter.awaitLine("important message", 5, TimeUnit.SECONDS) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTimeoutWhenNoContentProvided() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldTimeoutWhenNoContentProvided()
		 {
			  // given
			  FakeClock clock = FakeClock;
			  Stream inputStream = spy( new MockInputStream( new Ticker( clock, 1, TimeUnit.SECONDS ) ) );
			  InputStreamAwaiter awaiter = new InputStreamAwaiter( clock, inputStream );

			  // when
			  assertThrows( typeof( TimeoutException ), () => awaiter.awaitLine("important message", 5, TimeUnit.SECONDS) );
		 }

		 private static string Lines( params string[] lines )
		 {
			  StringBuilder result = new StringBuilder();
			  foreach ( string line in lines )
			  {
					result.Append( line ).Append( Environment.NewLine );
			  }
			  return result.ToString();
		 }

		 private static FakeClock FakeClock
		 {
			 get
			 {
				  return Clocks.fakeClock();
			 }
		 }

		 private class Ticker
		 {
			  internal readonly FakeClock Clock;
			  internal readonly long Duration;
			  internal readonly TimeUnit TimeUnit;

			  internal Ticker( FakeClock clock, long duration, TimeUnit timeUnit )
			  {
					this.Clock = clock;
					this.Duration = duration;
					this.TimeUnit = timeUnit;
			  }

			  internal virtual void Tick()
			  {
					Clock.forward( Duration, TimeUnit );
			  }
		 }

		 private class MockInputStream : Stream
		 {
			  internal readonly Ticker Ticker;
			  internal readonly sbyte[][] Chunks;
			  internal int Chunk;

			  internal MockInputStream( Ticker ticker, params string[] chunks )
			  {
					this.Ticker = ticker;
					this.Chunks = new sbyte[chunks.Length][];
					for ( int i = 0; i < chunks.Length; i++ )
					{
						 this.Chunks[i] = chunks[i].GetBytes();
					}
			  }

			  public override int Available()
			  {
					Ticker.tick();
					if ( Chunk >= Chunks.Length )
					{
						 return 0;
					}
					return Chunks[Chunk].Length;
			  }

			  public override int Read( sbyte[] target )
			  {
					if ( Chunk >= Chunks.Length )
					{
						 return 0;
					}
					sbyte[] source = Chunks[Chunk++];
					Array.Copy( source, 0, target, 0, source.Length );
					return source.Length;
			  }

			  public override int Read()
			  {
					throw new System.NotSupportedException();
			  }
		 }
	}

}