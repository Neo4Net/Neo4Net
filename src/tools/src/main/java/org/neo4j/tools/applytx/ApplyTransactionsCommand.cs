using System;

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
namespace Neo4Net.tools.applytx
{

	using Neo4Net.Cursors;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Args = Neo4Net.Helpers.Args;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Neo4Net.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using TransactionRepresentationCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using ReadOnlyTransactionStore = Neo4Net.Kernel.impl.transaction.log.ReadOnlyTransactionStore;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using ArgsCommand = Neo4Net.tools.console.input.ArgsCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.progress.ProgressMonitorFactory.textual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode.EXTERNAL;

	public class ApplyTransactionsCommand : ArgsCommand
	{
		 private readonly File _from;
		 private readonly System.Func<GraphDatabaseAPI> _to;

		 public ApplyTransactionsCommand( File from, System.Func<GraphDatabaseAPI> to )
		 {
			  this._from = from;
			  this._to = to;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void run(Neo4Net.helpers.Args args, java.io.PrintStream out) throws Exception
		 protected internal override void Run( Args args, PrintStream @out )
		 {
			  DependencyResolver dependencyResolver = _to.get().DependencyResolver;
			  TransactionIdStore txIdStore = dependencyResolver.ResolveDependency( typeof( TransactionIdStore ) );
			  Config config = dependencyResolver.ResolveDependency( typeof( Config ) );
			  long fromTx = txIdStore.LastCommittedTransaction.transactionId();
			  long toTx;
			  if ( args.Orphans().Count == 0 )
			  {
					throw new System.ArgumentException( "No tx specified" );
			  }

			  string whereTo = args.Orphans()[0];
			  if ( whereTo.Equals( "next" ) )
			  {
					toTx = fromTx + 1;
			  }
			  else if ( whereTo.Equals( "last" ) )
			  {
					toTx = long.MaxValue;
			  }
			  else
			  {
					toTx = long.Parse( whereTo );
			  }

			  long lastApplied = ApplyTransactions( _from, _to.get(), config, fromTx, toTx, @out );
			  @out.println( "Applied transactions up to and including " + lastApplied );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long applyTransactions(java.io.File fromPath, Neo4Net.kernel.internal.GraphDatabaseAPI toDb, Neo4Net.kernel.configuration.Config toConfig, long fromTxExclusive, long toTxInclusive, java.io.PrintStream out) throws Exception
		 private long ApplyTransactions( File fromPath, GraphDatabaseAPI toDb, Config toConfig, long fromTxExclusive, long toTxInclusive, PrintStream @out )
		 {
			  DependencyResolver resolver = toDb.DependencyResolver;
			  TransactionRepresentationCommitProcess commitProcess = new TransactionRepresentationCommitProcess( resolver.ResolveDependency( typeof( TransactionAppender ) ), resolver.ResolveDependency( typeof( StorageEngine ) ) );
			  LifeSupport life = new LifeSupport();
			  try
			  {
					  using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), IJobScheduler jobScheduler = createInitializedScheduler(), PageCache pageCache = StandalonePageCacheFactory.createPageCache(fileSystem, jobScheduler) )
					  {
						LogicalTransactionStore source = life.Add( new ReadOnlyTransactionStore( pageCache, fileSystem, DatabaseLayout.of( fromPath ), Config.defaults(), new Monitors() ) );
						life.Start();
						long lastAppliedTx = fromTxExclusive;
						// Some progress if there are more than a couple of transactions to apply
						ProgressListener progress = toTxInclusive - fromTxExclusive >= 100 ? textual( @out ).singlePart( "Application progress", toTxInclusive - fromTxExclusive ) : Neo4Net.Helpers.progress.ProgressListener_Fields.None;
						using ( IOCursor<CommittedTransactionRepresentation> cursor = source.GetTransactions( fromTxExclusive + 1 ) )
						{
							 while ( cursor.next() )
							 {
								  CommittedTransactionRepresentation transaction = cursor.get();
								  TransactionRepresentation transactionRepresentation = transaction.TransactionRepresentation;
								  try
								  {
										commitProcess.Commit( new TransactionToApply( transactionRepresentation ), NULL, EXTERNAL );
										progress.Add( 1 );
								  }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final Throwable e)
								  catch ( Exception e )
								  {
										Console.Error.WriteLine( "ERROR applying transaction " + transaction.CommitEntry.TxId );
										throw e;
								  }
								  lastAppliedTx = transaction.CommitEntry.TxId;
								  if ( lastAppliedTx == toTxInclusive )
								  {
										break;
								  }
							 }
						}
						return lastAppliedTx;
					  }
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

		 public override string ToString()
		 {
			  return ArrayUtil.join( new string[] { "Applies transaction from the source onto the new db. Example:", "  apply last : applies transactions from the currently last applied and up to the last", "               transaction of source db", "  apply next : applies the next transaction onto the new db", "  apply 234  : applies up to and including tx 234 from the source db onto the new db" }, format( "%n" ) );
		 }
	}

}