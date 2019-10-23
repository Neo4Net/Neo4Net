using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.com.storecopy
{

	using Neo4Net.com;
	using Neo4Net.Functions;
	using Resource = Neo4Net.GraphDb.Resource;
	using Neo4Net.GraphDb;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using StoreCopyCheckPointMutex = Neo4Net.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using StoreFileMetadata = Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.RequestContext.anonymous;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.getCanonicalFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.relativePath;

	/// <summary>
	/// Is able to feed store files in a consistent way to a <seealso cref="Response"/> to be picked up by a
	/// <seealso cref="StoreCopyClient"/>, for example.
	/// </summary>
	/// <seealso cref= StoreCopyClient </seealso>
	public class StoreCopyServer
	{
		 public interface Monitor
		 {
			  void StartTryCheckPoint( string storeCopyIdentifier );

			  void FinishTryCheckPoint( string storeCopyIdentifier );

			  void StartStreamingStoreFile( File file, string storeCopyIdentifier );

			  void FinishStreamingStoreFile( File file, string storeCopyIdentifier );

			  void StartStreamingStoreFiles( string storeCopyIdentifier );

			  void FinishStreamingStoreFiles( string storeCopyIdentifier );

			  void StartStreamingTransactions( long startTxId, string storeCopyIdentifier );

			  void FinishStreamingTransactions( long endTxId, string storeCopyIdentifier );
		 }

		  public class Monitor_Adapter : Monitor
		  {
			  private readonly StoreCopyServer _outerInstance;

			  public Monitor_Adapter( StoreCopyServer outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

				public override void StartTryCheckPoint( string storeCopyIdentifier )
				{ // empty
				}

				public override void FinishTryCheckPoint( string storeCopyIdentifier )
				{ // empty
				}

				public override void StartStreamingStoreFile( File file, string storeCopyIdentifier )
				{ // empty
				}

				public override void FinishStreamingStoreFile( File file, string storeCopyIdentifier )
				{ // empty
				}

				public override void StartStreamingStoreFiles( string storeCopyIdentifier )
				{ // empty
				}

				public override void FinishStreamingStoreFiles( string storeCopyIdentifier )
				{ // empty
				}

				public override void StartStreamingTransactions( long startTxId, string storeCopyIdentifier )
				{ // empty
				}

				public override void FinishStreamingTransactions( long endTxId, string storeCopyIdentifier )
				{ // empty
				}
		  }

		 private readonly NeoStoreDataSource _dataSource;
		 private readonly CheckPointer _checkPointer;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly File _databaseDirectory;
		 private readonly Monitor _monitor;

		 public StoreCopyServer( NeoStoreDataSource dataSource, CheckPointer checkPointer, FileSystemAbstraction fileSystem, File databaseDirectory, Monitor monitor )
		 {
			  this._dataSource = dataSource;
			  this._checkPointer = checkPointer;
			  this._fileSystem = fileSystem;
			  this._databaseDirectory = getCanonicalFile( databaseDirectory );
			  this._monitor = monitor;
		 }

		 public virtual Monitor Monitor()
		 {
			  return _monitor;
		 }

		 /// <summary>
		 /// Trigger store flush (checkpoint) and write <seealso cref="NeoStoreDataSource.listStoreFiles(bool) store files"/> to the
		 /// given <seealso cref="StoreWriter"/>.
		 /// </summary>
		 /// <param name="triggerName"> name of the component asks for store files. </param>
		 /// <param name="writer"> store writer to write files to. </param>
		 /// <param name="includeLogs"> <code>true</code> if transaction logs should be copied, <code>false</code> otherwise. </param>
		 /// <returns> a <seealso cref="RequestContext"/> specifying at which point the store copy started. </returns>
		 public virtual RequestContext FlushStoresAndStreamStoreFiles( string triggerName, StoreWriter writer, bool includeLogs )
		 {
			  try
			  {
					string storeCopyIdentifier = Thread.CurrentThread.Name;
					ThrowingAction<IOException> checkPointAction = () =>
					{
					 _monitor.startTryCheckPoint( storeCopyIdentifier );
					 _checkPointer.tryCheckPoint( new SimpleTriggerInfo( triggerName ) );
					 _monitor.finishTryCheckPoint( storeCopyIdentifier );
					};

					// Copy the store files
					long lastAppliedTransaction;
					StoreCopyCheckPointMutex mutex = _dataSource.StoreCopyCheckPointMutex;
					try
					{
							using ( Resource @lock = mutex.StoreCopy( checkPointAction ), ResourceIterator<StoreFileMetadata> files = _dataSource.listStoreFiles( includeLogs ) )
							{
							 lastAppliedTransaction = _checkPointer.lastCheckPointedTransactionId();
							 _monitor.startStreamingStoreFiles( storeCopyIdentifier );
							 ByteBuffer temporaryBuffer = ByteBuffer.allocateDirect( ( int ) ByteUnit.mebiBytes( 1 ) );
							 while ( Files.MoveNext() )
							 {
								  StoreFileMetadata meta = Files.Current;
								  File file = meta.File();
								  bool isLogFile = meta.LogFile;
								  int recordSize = meta.RecordSize();
      
								  using ( ReadableByteChannel fileChannel = _fileSystem.open( file, OpenMode.READ ) )
								  {
										long fileSize = _fileSystem.getFileSize( file );
										DoWrite( writer, temporaryBuffer, file, recordSize, fileChannel, fileSize, storeCopyIdentifier, isLogFile );
								  }
							 }
							}
					}
					finally
					{
						 _monitor.finishStreamingStoreFiles( storeCopyIdentifier );
					}

					return anonymous( lastAppliedTransaction );
			  }
			  catch ( IOException e )
			  {
					throw new ServerFailureException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doWrite(StoreWriter writer, ByteBuffer temporaryBuffer, java.io.File file, int recordSize, java.nio.channels.ReadableByteChannel fileChannel, long fileSize, String storeCopyIdentifier, boolean isLogFile) throws java.io.IOException
		 private void DoWrite( StoreWriter writer, ByteBuffer temporaryBuffer, File file, int recordSize, ReadableByteChannel fileChannel, long fileSize, string storeCopyIdentifier, bool isLogFile )
		 {
			  _monitor.startStreamingStoreFile( file, storeCopyIdentifier );
			  string path = isLogFile ? file.Name : relativePath( _databaseDirectory, file );
			  writer.Write( path, fileChannel, temporaryBuffer, fileSize > 0, recordSize );
			  _monitor.finishStreamingStoreFile( file, storeCopyIdentifier );
		 }
	}

}