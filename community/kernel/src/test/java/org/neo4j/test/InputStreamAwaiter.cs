using System;
using System.Text;
using System.Threading;

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
namespace Org.Neo4j.Test
{

	using Clocks = Org.Neo4j.Time.Clocks;


	public class InputStreamAwaiter
	{
		 private readonly Stream _input;
		 private readonly sbyte[] _bytes = new sbyte[1024];
		 private readonly Clock _clock;

		 public InputStreamAwaiter( Stream input ) : this( Clocks.systemClock(), input )
		 {
		 }

		 public InputStreamAwaiter( Clock clock, Stream input )
		 {
			  this._clock = clock;
			  this._input = input;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitLine(String expectedLine, long timeout, java.util.concurrent.TimeUnit unit) throws java.io.IOException, java.util.concurrent.TimeoutException, InterruptedException
		 public virtual void AwaitLine( string expectedLine, long timeout, TimeUnit unit )
		 {
			  long deadline = _clock.millis() + unit.toMillis(timeout);
			  StringBuilder buffer = new StringBuilder();
			  do
			  {
					while ( _input.available() > 0 )
					{
						 buffer.Append( StringHelper.NewString( _bytes, 0, _input.Read( _bytes, 0, _bytes.Length ) ) );
					}

					string[] lines = buffer.ToString().Split(Environment.NewLine, true);
					foreach ( string line in lines )
					{
						 if ( expectedLine.Equals( line ) )
						 {
							  return;
						 }
					}

					Thread.Sleep( 10 );
			  } while ( _clock.millis() < deadline );

			  throw new TimeoutException( "Timed out waiting to read line: [" + expectedLine + "]. Seen input:\n\t" + buffer.ToString().replaceAll("\n", "\n\t") );
		 }
	}

}