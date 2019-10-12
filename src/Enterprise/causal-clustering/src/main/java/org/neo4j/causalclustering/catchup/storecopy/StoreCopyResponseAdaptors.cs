using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.storecopy
{

	using Neo4Net.causalclustering.catchup;
	using Log = Neo4Net.Logging.Log;

	public abstract class StoreCopyResponseAdaptors<T> : CatchUpResponseAdaptor<T>
	{
		 internal static StoreCopyResponseAdaptors<StoreCopyFinishedResponse> FilesCopyAdaptor( StoreFileStreamProvider storeFileStreamProvider, Log log )
		 {
			  return new StoreFilesCopyResponseAdaptors( storeFileStreamProvider, log );
		 }

		 internal static StoreCopyResponseAdaptors<PrepareStoreCopyResponse> PrepareStoreCopyAdaptor( StoreFileStreamProvider storeFileStreamProvider, Log log )
		 {
			  return new PrepareStoreCopyResponseAdaptors( storeFileStreamProvider, log );
		 }

		 private readonly StoreFileStreamProvider _storeFileStreamProvider;
		 private readonly Log _log;
		 private StoreFileStream _storeFileStream;

		 private StoreCopyResponseAdaptors( StoreFileStreamProvider storeFileStreamProvider, Log log )
		 {
			  this._storeFileStreamProvider = storeFileStreamProvider;
			  this._log = log;
		 }

		 /// <summary>
		 /// Files will be sent in order but multiple files may be sent during one response.
		 /// </summary>
		 /// <param name="requestOutcomeSignal"> signal </param>
		 /// <param name="fileHeader"> header for most resent file being sent </param>
		 public override void OnFileHeader( CompletableFuture<T> requestOutcomeSignal, FileHeader fileHeader )
		 {
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StoreFileStream fileStream = storeFileStreamProvider.acquire(fileHeader.fileName(), fileHeader.requiredAlignment());
					StoreFileStream fileStream = _storeFileStreamProvider.acquire( fileHeader.FileName(), fileHeader.RequiredAlignment() );
					// Make sure that each stream closes on complete but only the latest is written to
					requestOutcomeSignal.whenComplete( new CloseFileStreamOnComplete<>( this, fileStream, fileHeader.FileName() ) );
					this._storeFileStream = fileStream;
			  }
			  catch ( Exception e )
			  {
					requestOutcomeSignal.completeExceptionally( e );
			  }
		 }

		 public override bool OnFileContent( CompletableFuture<T> signal, FileChunk fileChunk )
		 {
			  try
			  {
					_storeFileStream.write( fileChunk.Bytes() );
			  }
			  catch ( Exception e )
			  {
					signal.completeExceptionally( e );
			  }
			  return fileChunk.Last;
		 }

		 private class PrepareStoreCopyResponseAdaptors : StoreCopyResponseAdaptors<PrepareStoreCopyResponse>
		 {
			  internal PrepareStoreCopyResponseAdaptors( StoreFileStreamProvider storeFileStreamProvider, Log log ) : base( storeFileStreamProvider, log )
			  {
			  }

			  public override void OnStoreListingResponse( CompletableFuture<PrepareStoreCopyResponse> signal, PrepareStoreCopyResponse response )
			  {
					signal.complete( response );
			  }
		 }

		 private class StoreFilesCopyResponseAdaptors : StoreCopyResponseAdaptors<StoreCopyFinishedResponse>
		 {
			  internal StoreFilesCopyResponseAdaptors( StoreFileStreamProvider storeFileStreamProvider, Log log ) : base( storeFileStreamProvider, log )
			  {
			  }

			  public override void OnFileStreamingComplete( CompletableFuture<StoreCopyFinishedResponse> signal, StoreCopyFinishedResponse response )
			  {
					signal.complete( response );
			  }
		 }

		 private class CloseFileStreamOnComplete<RESPONSE> : System.Action<RESPONSE, Exception>
		 {
			 private readonly StoreCopyResponseAdaptors<T> _outerInstance;

			  internal readonly StoreFileStream FileStream;
			  internal string FileName;

			  internal CloseFileStreamOnComplete( StoreCopyResponseAdaptors<T> outerInstance, StoreFileStream fileStream, string fileName )
			  {
				  this._outerInstance = outerInstance;
					this.FileStream = fileStream;
					this.FileName = fileName;
			  }

			  public override void Accept( RESPONSE response, Exception throwable )
			  {
					try
					{
						 FileStream.close();
					}
					catch ( Exception e )
					{
						 outerInstance.log.Error( format( "Unable to close store file stream for file '%s'", FileName ), e );
					}
			  }
		 }
	}

}