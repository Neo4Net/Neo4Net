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
namespace Org.Neo4j.Kernel.impl.storemigration
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using StandardV2_3 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_0 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_0;
	using Legacy23Store = Org.Neo4j.Kernel.impl.storemigration.legacystore.v23.Legacy23Store;
	using Legacy30Store = Org.Neo4j.Kernel.impl.storemigration.legacystore.v30.Legacy30Store;
	using LogPosition = Org.Neo4j.Kernel.impl.transaction.log.LogPosition;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogTailScanner = Org.Neo4j.Kernel.recovery.LogTailScanner;
	using UTF8 = Org.Neo4j.@string.UTF8;
	using Unzip = Org.Neo4j.Test.Unzip;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IoPrimitiveUtils.readAndFlip;

	public class MigrationTestUtils
	{
		 private MigrationTestUtils()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void changeVersionNumber(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File storeFile, String versionString) throws java.io.IOException
		 internal static void ChangeVersionNumber( FileSystemAbstraction fileSystem, File storeFile, string versionString )
		 {
			  sbyte[] versionBytes = UTF8.encode( versionString );
			  using ( StoreChannel fileChannel = fileSystem.Open( storeFile, OpenMode.READ_WRITE ) )
			  {
					fileChannel.Position( fileSystem.GetFileSize( storeFile ) - versionBytes.Length );
					fileChannel.write( ByteBuffer.wrap( versionBytes ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void prepareSampleLegacyDatabase(String version, org.neo4j.io.fs.FileSystemAbstraction workingFs, java.io.File workingDirectory, java.io.File prepareDirectory) throws java.io.IOException
		 public static void PrepareSampleLegacyDatabase( string version, FileSystemAbstraction workingFs, File workingDirectory, File prepareDirectory )
		 {
			  if ( !prepareDirectory.exists() )
			  {
					throw new System.ArgumentException( "bad prepare directory" );
			  }
			  File resourceDirectory = FindFormatStoreDirectoryForVersion( version, prepareDirectory );
			  workingFs.DeleteRecursively( workingDirectory );
			  workingFs.Mkdirs( workingDirectory );
			  workingFs.CopyRecursively( resourceDirectory, workingDirectory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.io.File findFormatStoreDirectoryForVersion(String version, java.io.File targetDir) throws java.io.IOException
		 internal static File FindFormatStoreDirectoryForVersion( string version, File targetDir )
		 {
			  if ( StandardV2_3.STORE_VERSION.Equals( version ) )
			  {
					return Find23FormatStoreDirectory( targetDir );
			  }
			  else if ( StandardV3_0.STORE_VERSION.Equals( version ) )
			  {
					return Find30FormatStoreDirectory( targetDir );
			  }
			  else
			  {
					throw new System.ArgumentException( "Unknown version" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File find30FormatStoreDirectory(java.io.File targetDir) throws java.io.IOException
		 private static File Find30FormatStoreDirectory( File targetDir )
		 {
			  return Unzip.unzip( typeof( Legacy30Store ), "upgradeTest30Db.zip", targetDir );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File find23FormatStoreDirectory(java.io.File targetDir) throws java.io.IOException
		 public static File Find23FormatStoreDirectory( File targetDir )
		 {
			  return Unzip.unzip( typeof( Legacy23Store ), "upgradeTest23Db.zip", targetDir );
		 }

		 public static bool CheckNeoStoreHasDefaultFormatVersion( StoreVersionCheck check, DatabaseLayout databaseLayout )
		 {
			  File metadataStore = databaseLayout.MetadataStore();
			  return check.HasVersion( metadataStore, RecordFormatSelector.defaultFormat().storeVersion() ).Outcome.Successful;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void verifyFilesHaveSameContent(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File original, java.io.File other) throws java.io.IOException
		 public static void VerifyFilesHaveSameContent( FileSystemAbstraction fileSystem, File original, File other )
		 {
			  const int bufferBatchSize = 32 * 1024;
			  File[] files = fileSystem.ListFiles( original );
			  foreach ( File originalFile in files )
			  {
					File otherFile = new File( other, originalFile.Name );
					if ( !fileSystem.IsDirectory( originalFile ) )
					{
						 using ( StoreChannel originalChannel = fileSystem.Open( originalFile, OpenMode.READ ), StoreChannel otherChannel = fileSystem.Open( otherFile, OpenMode.READ ) )
						 {
							  ByteBuffer buffer = ByteBuffer.allocate( bufferBatchSize );
							  while ( true )
							  {
									if ( !readAndFlip( originalChannel, buffer, bufferBatchSize ) )
									{
										 break;
									}
									sbyte[] originalBytes = new sbyte[buffer.limit()];
									buffer.get( originalBytes );

									if ( !readAndFlip( otherChannel, buffer, bufferBatchSize ) )
									{
										 fail( "Files have different sizes" );
									}

									sbyte[] otherBytes = new sbyte[buffer.limit()];
									buffer.get( otherBytes );

									assertArrayEquals( "Different content in " + originalFile, originalBytes, otherBytes );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void removeCheckPointFromTxLog(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File databaseDirectory) throws java.io.IOException
		 public static void RemoveCheckPointFromTxLog( FileSystemAbstraction fileSystem, File databaseDirectory )
		 {
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( databaseDirectory, fileSystem ).build();
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  LogTailScanner tailScanner = new LogTailScanner( logFiles, logEntryReader, new Monitors() );
			  LogTailScanner.LogTailInformation logTailInformation = tailScanner.TailInformation;

			  if ( logTailInformation.CommitsAfterLastCheckpoint() )
			  {
					// done already
					return;
			  }

			  // let's assume there is at least a checkpoint
			  assertNotNull( logTailInformation.LastCheckPoint );

			  LogPosition logPosition = logTailInformation.LastCheckPoint.LogPosition;
			  File logFile = logFiles.GetLogFileForVersion( logPosition.LogVersion );
			  fileSystem.Truncate( logFile, logPosition.ByteOffset );
		 }
	}

}