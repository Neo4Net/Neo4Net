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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using TransactionMetadata = Neo4Net.Kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	/// <summary>
	/// Used for reading transactions off of file.
	/// </summary>
	public class ReadOnlyTransactionStore : Lifecycle, LogicalTransactionStore
	{
		 private readonly LifeSupport _life = new LifeSupport();
		 private readonly LogicalTransactionStore _physicalStore;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ReadOnlyTransactionStore(org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.layout.DatabaseLayout fromDatabaseLayout, org.neo4j.kernel.configuration.Config config, org.neo4j.kernel.monitoring.Monitors monitors) throws java.io.IOException
		 public ReadOnlyTransactionStore( PageCache pageCache, FileSystemAbstraction fs, DatabaseLayout fromDatabaseLayout, Config config, Monitors monitors )
		 {
			  TransactionMetadataCache transactionMetadataCache = new TransactionMetadataCache();
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( fromDatabaseLayout, fs, pageCache ).withLogEntryReader( logEntryReader ).withConfig( config ).build();
			  _physicalStore = new PhysicalLogicalTransactionStore( logFiles, transactionMetadataCache, logEntryReader, monitors, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionCursor getTransactions(long transactionIdToStartFrom) throws java.io.IOException
		 public override TransactionCursor GetTransactions( long transactionIdToStartFrom )
		 {
			  return _physicalStore.getTransactions( transactionIdToStartFrom );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionCursor getTransactions(LogPosition position) throws java.io.IOException
		 public override TransactionCursor GetTransactions( LogPosition position )
		 {
			  return _physicalStore.getTransactions( position );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionCursor getTransactionsInReverseOrder(LogPosition backToPosition) throws java.io.IOException
		 public override TransactionCursor GetTransactionsInReverseOrder( LogPosition backToPosition )
		 {
			  return _physicalStore.getTransactionsInReverseOrder( backToPosition );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata getMetadataFor(long transactionId) throws java.io.IOException
		 public override TransactionMetadata GetMetadataFor( long transactionId )
		 {
			  return _physicalStore.getMetadataFor( transactionId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean existsOnDisk(long transactionId) throws java.io.IOException
		 public override bool ExistsOnDisk( long transactionId )
		 {
			  return _physicalStore.existsOnDisk( transactionId );
		 }

		 public override void Init()
		 {
			  _life.init();
		 }

		 public override void Start()
		 {
			  _life.start();
		 }

		 public override void Stop()
		 {
			  _life.stop();
		 }

		 public override void Shutdown()
		 {
			  _life.shutdown();
		 }
	}

}