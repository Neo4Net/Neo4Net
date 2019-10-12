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
namespace Neo4Net.Kernel.recovery
{

	using ByteUnit = Neo4Net.Io.ByteUnit;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;

	/// <summary>
	/// Transaction log truncator used during recovery to truncate all the logs after some specified position, that
	/// recovery treats as corrupted or non-readable.
	/// Transaction log file specified by provided log position will be truncated to provided length, any
	/// subsequent files will be removed.
	/// Any removed or modified log content will be stored in separate corruption logs archive for further analysis and as
	/// an additional safety option to have the possibility to fully restore original logs in a faulty case.
	/// </summary>
	public class CorruptedLogsTruncator
	{
		 public static readonly string CorruptedTxLogsBaseName = "corrupted-" + TransactionLogFiles.DEFAULT_NAME;
		 private static readonly string _logFileArchivePattern = CorruptedTxLogsBaseName + "-%d-%d-%d.zip";

		 private readonly File _storeDir;
		 private readonly LogFiles _logFiles;
		 private readonly FileSystemAbstraction _fs;

		 public CorruptedLogsTruncator( File storeDir, LogFiles logFiles, FileSystemAbstraction fs )
		 {
			  this._storeDir = storeDir;
			  this._logFiles = logFiles;
			  this._fs = fs;
		 }

		 /// <summary>
		 /// Truncate all transaction logs after provided position. Log version specified in a position will be
		 /// truncated to provided byte offset, any subsequent log files will be deleted. Backup copy of removed data will
		 /// be stored in separate archive. </summary>
		 /// <param name="positionAfterLastRecoveredTransaction"> position after last recovered transaction </param>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(org.neo4j.kernel.impl.transaction.log.LogPosition positionAfterLastRecoveredTransaction) throws java.io.IOException
		 public virtual void Truncate( LogPosition positionAfterLastRecoveredTransaction )
		 {
			  long recoveredTransactionLogVersion = positionAfterLastRecoveredTransaction.LogVersion;
			  long recoveredTransactionOffset = positionAfterLastRecoveredTransaction.ByteOffset;
			  if ( IsRecoveredLogCorrupted( recoveredTransactionLogVersion, recoveredTransactionOffset ) || HaveMoreRecentLogFiles( recoveredTransactionLogVersion ) )
			  {
					BackupCorruptedContent( recoveredTransactionLogVersion, recoveredTransactionOffset );
					TruncateLogFiles( recoveredTransactionLogVersion, recoveredTransactionOffset );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void truncateLogFiles(long recoveredTransactionLogVersion, long recoveredTransactionOffset) throws java.io.IOException
		 private void TruncateLogFiles( long recoveredTransactionLogVersion, long recoveredTransactionOffset )
		 {
			  File lastRecoveredTransactionLog = _logFiles.getLogFileForVersion( recoveredTransactionLogVersion );
			  _fs.truncate( lastRecoveredTransactionLog, recoveredTransactionOffset );
			  ForEachSubsequentLogFile( recoveredTransactionLogVersion, fileIndex => _fs.deleteFile( _logFiles.getLogFileForVersion( fileIndex ) ) );
		 }

		 private void ForEachSubsequentLogFile( long recoveredTransactionLogVersion, System.Action<long> action )
		 {
			  long highestLogVersion = _logFiles.HighestLogVersion;
			  for ( long fileIndex = recoveredTransactionLogVersion + 1; fileIndex <= highestLogVersion; fileIndex++ )
			  {
					action( fileIndex );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void backupCorruptedContent(long recoveredTransactionLogVersion, long recoveredTransactionOffset) throws java.io.IOException
		 private void BackupCorruptedContent( long recoveredTransactionLogVersion, long recoveredTransactionOffset )
		 {
			  File corruptedLogArchive = GetArchiveFile( recoveredTransactionLogVersion, recoveredTransactionOffset );
			  using ( ZipOutputStream recoveryContent = new ZipOutputStream( new BufferedOutputStream( _fs.openAsOutputStream( corruptedLogArchive, false ) ) ) )
			  {
					ByteBuffer zipBuffer = ByteBuffer.allocate( ( int ) ByteUnit.mebiBytes( 1 ) );
					CopyTransactionLogContent( recoveredTransactionLogVersion, recoveredTransactionOffset, recoveryContent, zipBuffer );
					ForEachSubsequentLogFile(recoveredTransactionLogVersion, fileIndex =>
					{
					 try
					 {
						  CopyTransactionLogContent( fileIndex, 0, recoveryContent, zipBuffer );
					 }
					 catch ( IOException io )
					 {
						  throw new UncheckedIOException( io );
					 }
					});
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File getArchiveFile(long recoveredTransactionLogVersion, long recoveredTransactionOffset) throws java.io.IOException
		 private File GetArchiveFile( long recoveredTransactionLogVersion, long recoveredTransactionOffset )
		 {
			  File corruptedLogsFolder = new File( _storeDir, CorruptedTxLogsBaseName );
			  _fs.mkdirs( corruptedLogsFolder );
			  return new File( corruptedLogsFolder, format( _logFileArchivePattern, recoveredTransactionLogVersion, recoveredTransactionOffset, DateTimeHelper.CurrentUnixTimeMillis() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void copyTransactionLogContent(long logFileIndex, long logOffset, java.util.zip.ZipOutputStream destination, ByteBuffer byteBuffer) throws java.io.IOException
		 private void CopyTransactionLogContent( long logFileIndex, long logOffset, ZipOutputStream destination, ByteBuffer byteBuffer )
		 {
			  File logFile = _logFiles.getLogFileForVersion( logFileIndex );
			  if ( _fs.getFileSize( logFile ) == logOffset )
			  {
					// file was recovered fully, nothing to backup
					return;
			  }
			  ZipEntry zipEntry = new ZipEntry( logFile.Name );
			  destination.putNextEntry( zipEntry );
			  using ( StoreChannel transactionLogChannel = _fs.open( logFile, OpenMode.READ ) )
			  {
					transactionLogChannel.Position( logOffset );
					while ( transactionLogChannel.read( byteBuffer ) >= 0 )
					{
						 byteBuffer.flip();
						 destination.write( byteBuffer.array(), byteBuffer.position(), byteBuffer.remaining() );
						 byteBuffer.clear();
					}
			  }
			  destination.closeEntry();
		 }

		 private bool HaveMoreRecentLogFiles( long recoveredTransactionLogVersion )
		 {
			  return _logFiles.HighestLogVersion > recoveredTransactionLogVersion;
		 }

		 private bool IsRecoveredLogCorrupted( long recoveredTransactionLogVersion, long recoveredTransactionOffset )
		 {
			  return _logFiles.getLogFileForVersion( recoveredTransactionLogVersion ).length() > recoveredTransactionOffset;
		 }
	}

}