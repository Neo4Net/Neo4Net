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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{
	using Test = org.junit.Test;

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class FileSizeThresholdTest
	{

		 private FileSystemAbstraction _fs = mock( typeof( FileSystemAbstraction ) );
		 private readonly LogFileInformation _source = mock( typeof( LogFileInformation ) );
		 private readonly File _file = mock( typeof( File ) );
		 private readonly long _version = 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseWhenFileSizeIsLowerThanMaxSize()
		 public virtual void ShouldReturnFalseWhenFileSizeIsLowerThanMaxSize()
		 {
			  // given
			  const long maxSize = 10;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FileSizeThreshold threshold = new FileSizeThreshold(fs, maxSize);
			  FileSizeThreshold threshold = new FileSizeThreshold( _fs, maxSize );

			  when( _fs.getFileSize( _file ) ).thenReturn( 5L );

			  // when
			  threshold.Init();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = threshold.reached(file, version, source);
			  bool result = threshold.Reached( _file, _version, _source );

			  // then
			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTrueWhenASingleFileSizeIsGreaterOrEqualThanMaxSize()
		 public virtual void ShouldReturnTrueWhenASingleFileSizeIsGreaterOrEqualThanMaxSize()
		 {
			  // given
			  long sixteenGigabytes = 16L * 1024 * 1024 * 1024;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FileSizeThreshold threshold = new FileSizeThreshold(fs, sixteenGigabytes);
			  FileSizeThreshold threshold = new FileSizeThreshold( _fs, sixteenGigabytes );

			  when( _fs.getFileSize( _file ) ).thenReturn( sixteenGigabytes );

			  // when
			  threshold.Init();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = threshold.reached(file, version, source);
			  bool result = threshold.Reached( _file, _version, _source );

			  // then
			  assertTrue( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSumSizeWhenCalledMultipleTimes()
		 public virtual void ShouldSumSizeWhenCalledMultipleTimes()
		 {
			  // given
			  const long maxSize = 10;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FileSizeThreshold threshold = new FileSizeThreshold(fs, maxSize);
			  FileSizeThreshold threshold = new FileSizeThreshold( _fs, maxSize );

			  when( _fs.getFileSize( _file ) ).thenReturn( 5L );

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
//ORIGINAL LINE: @Test public void shouldForgetPreviousValuesAfterAInitCall()
		 public virtual void ShouldForgetPreviousValuesAfterAInitCall()
		 {
			  // given
			  const long maxSize = 10;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FileSizeThreshold threshold = new FileSizeThreshold(fs, maxSize);
			  FileSizeThreshold threshold = new FileSizeThreshold( _fs, maxSize );

			  when( _fs.getFileSize( _file ) ).thenReturn( 5L );

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