using System;

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
namespace Org.Neo4j.Kernel.impl.transaction.log.pruning
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class EntryTimespanThresholdTest
	{
		 private readonly File _file = mock( typeof( File ) );
		 private readonly LogFileInformation _source = mock( typeof( LogFileInformation ) );
		 private readonly long _version = 4;
		 private Clock _clock = Clock.@fixed( Instant.ofEpochMilli( 1000 ), ZoneOffset.UTC );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseWhenTimeIsEqualOrAfterTheLowerLimit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseWhenTimeIsEqualOrAfterTheLowerLimit()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EntryTimespanThreshold threshold = new EntryTimespanThreshold(clock, java.util.concurrent.TimeUnit.MILLISECONDS, 200);
			  EntryTimespanThreshold threshold = new EntryTimespanThreshold( _clock, TimeUnit.MILLISECONDS, 200 );

			  when( _source.getFirstStartRecordTimestamp( _version ) ).thenReturn( 800L );

			  // when
			  threshold.Init();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = threshold.reached(file, version, source);
			  bool result = threshold.Reached( _file, _version, _source );

			  // then
			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnReturnWhenTimeIsBeforeTheLowerLimit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnReturnWhenTimeIsBeforeTheLowerLimit()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EntryTimespanThreshold threshold = new EntryTimespanThreshold(clock, java.util.concurrent.TimeUnit.MILLISECONDS, 100);
			  EntryTimespanThreshold threshold = new EntryTimespanThreshold( _clock, TimeUnit.MILLISECONDS, 100 );

			  when( _source.getFirstStartRecordTimestamp( _version ) ).thenReturn( 800L );

			  // when
			  threshold.Init();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = threshold.reached(file, version, source);
			  bool result = threshold.Reached( _file, _version, _source );

			  // then
			  assertTrue( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfTheLogCannotBeRead() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfTheLogCannotBeRead()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EntryTimespanThreshold threshold = new EntryTimespanThreshold(clock, java.util.concurrent.TimeUnit.MILLISECONDS, 100);
			  EntryTimespanThreshold threshold = new EntryTimespanThreshold( _clock, TimeUnit.MILLISECONDS, 100 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.IOException ex = new java.io.IOException();
			  IOException ex = new IOException();
			  when( _source.getFirstStartRecordTimestamp( _version ) ).thenThrow( ex );

			  // when
			  threshold.Init();
			  try
			  {
					threshold.Reached( _file, _version, _source );
					fail( "should have thrown" );
			  }
			  catch ( Exception e )
			  {
					// then
					assertEquals( ex, e.InnerException );
			  }
		 }
	}

}