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
namespace Neo4Net.Kernel
{
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using CheckPointerImpl = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointerImpl;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using SynchronizedArrayIdOrderingQueue = Neo4Net.Kernel.impl.util.SynchronizedArrayIdOrderingQueue;

	internal class NeoStoreTransactionLogModule
	{
		 private readonly LogicalTransactionStore _logicalTransactionStore;
		 private readonly LogFiles _logFiles;
		 private readonly LogRotation _logRotation;
		 private readonly CheckPointerImpl _checkPointer;
		 private readonly TransactionAppender _appender;
		 private readonly SynchronizedArrayIdOrderingQueue _explicitIndexTransactionOrdering;

		 internal NeoStoreTransactionLogModule( LogicalTransactionStore logicalTransactionStore, LogFiles logFiles, LogRotation logRotation, CheckPointerImpl checkPointer, TransactionAppender appender, SynchronizedArrayIdOrderingQueue explicitIndexTransactionOrdering )
		 {
			  this._logicalTransactionStore = logicalTransactionStore;
			  this._logFiles = logFiles;
			  this._logRotation = logRotation;
			  this._checkPointer = checkPointer;
			  this._appender = appender;
			  this._explicitIndexTransactionOrdering = explicitIndexTransactionOrdering;
		 }

		 public virtual LogicalTransactionStore LogicalTransactionStore()
		 {
			  return _logicalTransactionStore;
		 }

		 internal virtual CheckPointer CheckPointing()
		 {
			  return _checkPointer;
		 }

		 internal virtual TransactionAppender TransactionAppender()
		 {
			  return _appender;
		 }

		 public virtual void SatisfyDependencies( Dependencies dependencies )
		 {
			  dependencies.SatisfyDependencies( _checkPointer, _logFiles, _logFiles.LogFileInformation, _explicitIndexTransactionOrdering, _logicalTransactionStore, _logRotation, _appender );
		 }
	}

}