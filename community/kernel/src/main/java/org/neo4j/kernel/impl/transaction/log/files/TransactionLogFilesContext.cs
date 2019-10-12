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
namespace Org.Neo4j.Kernel.impl.transaction.log.files
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;

	internal class TransactionLogFilesContext
	{
		 private readonly AtomicLong _rotationThreshold;
		 private readonly LogEntryReader _logEntryReader;
		 private readonly System.Func<long> _lastCommittedTransactionIdSupplier;
		 private readonly System.Func<long> _committingTransactionIdSupplier;
		 private readonly System.Func<LogVersionRepository> _logVersionRepositorySupplier;
		 private readonly LogFileCreationMonitor _logFileCreationMonitor;
		 private readonly FileSystemAbstraction _fileSystem;

		 internal TransactionLogFilesContext( AtomicLong rotationThreshold, LogEntryReader logEntryReader, System.Func<long> lastCommittedTransactionIdSupplier, System.Func<long> committingTransactionIdSupplier, LogFileCreationMonitor logFileCreationMonitor, System.Func<LogVersionRepository> logVersionRepositorySupplier, FileSystemAbstraction fileSystem )
		 {
			  this._rotationThreshold = rotationThreshold;
			  this._logEntryReader = logEntryReader;
			  this._lastCommittedTransactionIdSupplier = lastCommittedTransactionIdSupplier;
			  this._committingTransactionIdSupplier = committingTransactionIdSupplier;
			  this._logVersionRepositorySupplier = logVersionRepositorySupplier;
			  this._logFileCreationMonitor = logFileCreationMonitor;
			  this._fileSystem = fileSystem;
		 }

		 internal virtual AtomicLong RotationThreshold
		 {
			 get
			 {
				  return _rotationThreshold;
			 }
		 }

		 internal virtual LogEntryReader LogEntryReader
		 {
			 get
			 {
				  return _logEntryReader;
			 }
		 }

		 internal virtual LogVersionRepository LogVersionRepository
		 {
			 get
			 {
				  return _logVersionRepositorySupplier.get();
			 }
		 }

		 internal virtual long LastCommittedTransactionId
		 {
			 get
			 {
				  return _lastCommittedTransactionIdSupplier.AsLong;
			 }
		 }

		 internal virtual long CommittingTransactionId()
		 {
			  return _committingTransactionIdSupplier.AsLong;
		 }

		 internal virtual LogFileCreationMonitor LogFileCreationMonitor
		 {
			 get
			 {
				  return _logFileCreationMonitor;
			 }
		 }

		 internal virtual FileSystemAbstraction FileSystem
		 {
			 get
			 {
				  return _fileSystem;
			 }
		 }
	}

}