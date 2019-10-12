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
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class FileCountThresholdTest
	{
		 private readonly File _file = mock( typeof( File ) );
		 private readonly long _version = 1L;
		 private readonly LogFileInformation _source = mock( typeof( LogFileInformation ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseWhenTheMaxNonEmptyLogCountIsNotReached()
		 public virtual void ShouldReturnFalseWhenTheMaxNonEmptyLogCountIsNotReached()
		 {
			  // given
			  const int maxNonEmptyLogCount = 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FileCountThreshold threshold = new FileCountThreshold(maxNonEmptyLogCount);
			  FileCountThreshold threshold = new FileCountThreshold( maxNonEmptyLogCount );

			  // when
			  threshold.Init();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = threshold.reached(file, version, source);
			  bool result = threshold.Reached( _file, _version, _source );

			  // then
			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTrueWhenTheMaxNonEmptyLogCountIsReached()
		 public virtual void ShouldReturnTrueWhenTheMaxNonEmptyLogCountIsReached()
		 {
			  // given
			  const int maxNonEmptyLogCount = 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FileCountThreshold threshold = new FileCountThreshold(maxNonEmptyLogCount);
			  FileCountThreshold threshold = new FileCountThreshold( maxNonEmptyLogCount );

			  // when
			  threshold.Init();
			  threshold.Reached( _file, _version, _source );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = threshold.reached(file, version, source);
			  bool result = threshold.Reached( _file, _version, _source );

			  // then
			  assertTrue( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetTheCounterWhenInitIsCalled()
		 public virtual void ShouldResetTheCounterWhenInitIsCalled()
		 {
			  // given
			  const int maxNonEmptyLogCount = 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FileCountThreshold threshold = new FileCountThreshold(maxNonEmptyLogCount);
			  FileCountThreshold threshold = new FileCountThreshold( maxNonEmptyLogCount );

			  // when
			  threshold.Init();
			  threshold.Reached( _file, _version, _source );
			  threshold.Reached( _file, _version, _source );
			  threshold.Init();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = threshold.reached(file, version, source);
			  bool result = threshold.Reached( _file, _version, _source );

			  // then
			  assertFalse( result );
		 }

	}

}