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
namespace Neo4Net.Kernel.impl.transaction.log.stresstest.workload
{

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using DatabasePanicEventGenerator = Neo4Net.Kernel.impl.core.DatabasePanicEventGenerator;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using LogRotationImpl = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotationImpl;
	using IdOrderingQueue = Neo4Net.Kernel.impl.util.IdOrderingQueue;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using KernelEventHandlers = Neo4Net.Kernel.Internal.KernelEventHandlers;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;

	public class Runner : Callable<long>
	{
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly System.Func<bool> _condition;
		 private readonly int _threads;

		 public Runner( DatabaseLayout databaseLayout, System.Func<bool> condition, int threads )
		 {
			  this._databaseLayout = databaseLayout;
			  this._condition = condition;
			  this._threads = threads;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<long> call() throws Exception
		 public override long? Call()
		 {
			  long lastCommittedTransactionId;

			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), Lifespan life = new Lifespan() )
			  {
					TransactionIdStore transactionIdStore = new SimpleTransactionIdStore();
					TransactionMetadataCache transactionMetadataCache = new TransactionMetadataCache();
					LogFiles logFiles = life.Add( CreateLogFiles( transactionIdStore, fileSystem ) );

					TransactionAppender transactionAppender = life.Add( CreateBatchingTransactionAppender( transactionIdStore, transactionMetadataCache, logFiles ) );

					ExecutorService executorService = Executors.newFixedThreadPool( _threads );
					try
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?>[] handlers = new java.util.concurrent.Future[threads];
						 Future<object>[] handlers = new Future[_threads];
						 for ( int i = 0; i < _threads; i++ )
						 {
							  TransactionRepresentationFactory factory = new TransactionRepresentationFactory();
							  Worker task = new Worker( transactionAppender, factory, _condition );
							  handlers[i] = executorService.submit( task );
						 }

						 // wait for all the workers to complete
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> handle : handlers)
						 foreach ( Future<object> handle in handlers )
						 {
							  handle.get();
						 }
					}
					finally
					{
						 executorService.shutdown();
					}

					lastCommittedTransactionId = transactionIdStore.LastCommittedTransactionId;
			  }

			  return lastCommittedTransactionId;
		 }

		 private static BatchingTransactionAppender CreateBatchingTransactionAppender( TransactionIdStore transactionIdStore, TransactionMetadataCache transactionMetadataCache, LogFiles logFiles )
		 {
			  Log log = NullLog.Instance;
			  KernelEventHandlers kernelEventHandlers = new KernelEventHandlers( log );
			  DatabasePanicEventGenerator panicEventGenerator = new DatabasePanicEventGenerator( kernelEventHandlers );
			  DatabaseHealth databaseHealth = new DatabaseHealth( panicEventGenerator, log );
			  LogRotationImpl logRotation = new LogRotationImpl( NOOP_LOGROTATION_MONITOR, logFiles, databaseHealth );
			  return new BatchingTransactionAppender( logFiles, logRotation, transactionMetadataCache, transactionIdStore, IdOrderingQueue.BYPASS, databaseHealth );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.kernel.impl.transaction.log.files.LogFiles createLogFiles(org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore transactionIdStore, org.Neo4Net.io.fs.FileSystemAbstraction fileSystemAbstraction) throws java.io.IOException
		 private LogFiles CreateLogFiles( TransactionIdStore transactionIdStore, FileSystemAbstraction fileSystemAbstraction )
		 {
			  SimpleLogVersionRepository logVersionRepository = new SimpleLogVersionRepository();
			  return LogFilesBuilder.builder( _databaseLayout, fileSystemAbstraction ).withTransactionIdStore( transactionIdStore ).withLogVersionRepository( logVersionRepository ).build();
		 }

		 private static readonly Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation_Monitor NOOP_LOGROTATION_MONITOR = new LogRotation_MonitorAnonymousInnerClass();

		 private class LogRotation_MonitorAnonymousInnerClass : Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation_Monitor
		 {
			 public void startedRotating( long currentVersion )
			 {

			 }

			 public void finishedRotating( long currentVersion )
			 {

			 }
		 }

	}

}