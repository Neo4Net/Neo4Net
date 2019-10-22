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
namespace Neo4Net.Kernel.impl.transaction.log.pruning
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
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
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

	public class ThresholdBasedPruneStrategyTest
	{
		 private readonly FileSystemAbstraction _fileSystem = mock( typeof( FileSystemAbstraction ) );
		 private readonly LogFiles _files = mock( typeof( TransactionLogFiles ) );
		 private readonly Threshold _threshold = mock( typeof( Threshold ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDeleteAnythingIfThresholdDoesNotAllow()
		 public virtual void ShouldNotDeleteAnythingIfThresholdDoesNotAllow()
		 {
			  // Given
			  File fileName0 = new File( "logical.log.v0" );
			  File fileName1 = new File( "logical.log.v1" );
			  File fileName2 = new File( "logical.log.v2" );
			  File fileName3 = new File( "logical.log.v3" );
			  File fileName4 = new File( "logical.log.v4" );
			  File fileName5 = new File( "logical.log.v5" );
			  File fileName6 = new File( "logical.log.v6" );

			  when( _files.getLogFileForVersion( 6 ) ).thenReturn( fileName6 );
			  when( _files.getLogFileForVersion( 5 ) ).thenReturn( fileName5 );
			  when( _files.getLogFileForVersion( 4 ) ).thenReturn( fileName4 );
			  when( _files.getLogFileForVersion( 3 ) ).thenReturn( fileName3 );
			  when( _files.getLogFileForVersion( 2 ) ).thenReturn( fileName2 );
			  when( _files.getLogFileForVersion( 1 ) ).thenReturn( fileName1 );
			  when( _files.getLogFileForVersion( 0 ) ).thenReturn( fileName0 );

			  when( _fileSystem.fileExists( fileName6 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName5 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName4 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName3 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName2 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName1 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName0 ) ).thenReturn( true );

			  when( _fileSystem.getFileSize( any() ) ).thenReturn(LOG_HEADER_SIZE + 1L);

			  when( _threshold.reached( any(), anyLong(), any() ) ).thenReturn(false);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ThresholdBasedPruneStrategy strategy = new ThresholdBasedPruneStrategy(fileSystem, files, threshold);
			  ThresholdBasedPruneStrategy strategy = new ThresholdBasedPruneStrategy( _fileSystem, _files, _threshold );

			  // When
			  strategy.FindLogVersionsToDelete( 7L ).forEachOrdered( v => _fileSystem.deleteFile( _files.getLogFileForVersion( v ) ) );

			  // Then
			  verify( _threshold, times( 1 ) ).init();
			  verify( _fileSystem, never() ).deleteFile(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteJustWhatTheThresholdSays()
		 public virtual void ShouldDeleteJustWhatTheThresholdSays()
		 {
			  // Given
			  when( _threshold.reached( any(), eq(6L), any() ) ).thenReturn(false);
			  when( _threshold.reached( any(), eq(5L), any() ) ).thenReturn(false);
			  when( _threshold.reached( any(), eq(4L), any() ) ).thenReturn(false);
			  when( _threshold.reached( any(), eq(3L), any() ) ).thenReturn(true);

			  File fileName1 = new File( "logical.log.v1" );
			  File fileName2 = new File( "logical.log.v2" );
			  File fileName3 = new File( "logical.log.v3" );
			  File fileName4 = new File( "logical.log.v4" );
			  File fileName5 = new File( "logical.log.v5" );
			  File fileName6 = new File( "logical.log.v6" );

			  when( _files.getLogFileForVersion( 6 ) ).thenReturn( fileName6 );
			  when( _files.getLogFileForVersion( 5 ) ).thenReturn( fileName5 );
			  when( _files.getLogFileForVersion( 4 ) ).thenReturn( fileName4 );
			  when( _files.getLogFileForVersion( 3 ) ).thenReturn( fileName3 );
			  when( _files.getLogFileForVersion( 2 ) ).thenReturn( fileName2 );
			  when( _files.getLogFileForVersion( 1 ) ).thenReturn( fileName1 );

			  when( _fileSystem.fileExists( fileName6 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName5 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName4 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName3 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName2 ) ).thenReturn( true );
			  when( _fileSystem.fileExists( fileName1 ) ).thenReturn( true );

			  when( _fileSystem.getFileSize( any() ) ).thenReturn(LOG_HEADER_SIZE + 1L);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ThresholdBasedPruneStrategy strategy = new ThresholdBasedPruneStrategy(fileSystem, files, threshold);
			  ThresholdBasedPruneStrategy strategy = new ThresholdBasedPruneStrategy( _fileSystem, _files, _threshold );

			  // When
			  strategy.FindLogVersionsToDelete( 7L ).forEachOrdered( v => _fileSystem.deleteFile( _files.getLogFileForVersion( v ) ) );

			  // Then
			  InOrder order = inOrder( _threshold, _fileSystem );
			  order.verify( _threshold, times( 1 ) ).init();
			  order.verify( _fileSystem, times( 1 ) ).deleteFile( fileName1 );
			  order.verify( _fileSystem, times( 1 ) ).deleteFile( fileName2 );
			  order.verify( _fileSystem, times( 1 ) ).deleteFile( fileName3 );
			  verify( _fileSystem, never() ).deleteFile(fileName4);
			  verify( _fileSystem, never() ).deleteFile(fileName5);
			  verify( _fileSystem, never() ).deleteFile(fileName6);
		 }
	}

}