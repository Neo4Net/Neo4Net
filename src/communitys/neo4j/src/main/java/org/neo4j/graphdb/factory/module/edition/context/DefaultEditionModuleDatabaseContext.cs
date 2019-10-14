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
namespace Neo4Net.Graphdb.factory.module.edition.context
{

	using DatabaseIdContext = Neo4Net.Graphdb.factory.module.id.DatabaseIdContext;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CommitProcessFactory = Neo4Net.Kernel.Impl.Api.CommitProcessFactory;
	using SchemaWriteGuard = Neo4Net.Kernel.Impl.Api.SchemaWriteGuard;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using DatabaseTransactionStats = Neo4Net.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using FileSystemWatcherService = Neo4Net.Kernel.impl.util.watcher.FileSystemWatcherService;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	public class DefaultEditionModuleDatabaseContext : DatabaseEditionContext
	{
		 private readonly System.Func<File, FileSystemWatcherService> _watcherServiceFactory;
		 private readonly string _databaseName;
		 private readonly AccessCapability _accessCapability;
		 private readonly IOLimiter _ioLimiter;
		 private readonly ConstraintSemantics _constraintSemantics;
		 private readonly CommitProcessFactory _commitProcessFactory;
		 private readonly TransactionHeaderInformationFactory _headerInformationFactory;
		 private readonly SchemaWriteGuard _schemaWriteGuard;
		 private readonly long _transactionStartTimeout;
		 private readonly TokenHolders _tokenHolders;
		 private readonly Locks _locks;
		 private readonly DatabaseTransactionStats _transactionMonitor;
		 private readonly AbstractEditionModule _editionModule;
		 private readonly DatabaseIdContext _idContext;
		 private readonly StatementLocksFactory _statementLocksFactory;

		 public DefaultEditionModuleDatabaseContext( DefaultEditionModule editionModule, string databaseName )
		 {
			  this._databaseName = databaseName;
			  this._transactionStartTimeout = editionModule.TransactionStartTimeout;
			  this._schemaWriteGuard = editionModule.SchemaWriteGuard;
			  this._headerInformationFactory = editionModule.HeaderInformationFactory;
			  this._commitProcessFactory = editionModule.CommitProcessFactory;
			  this._constraintSemantics = editionModule.ConstraintSemantics;
			  this._ioLimiter = editionModule.IoLimiter;
			  this._accessCapability = editionModule.AccessCapability;
			  this._watcherServiceFactory = editionModule.WatcherServiceFactory;
			  this._idContext = editionModule.IdContextFactory.createIdContext( databaseName );
			  this._tokenHolders = editionModule.TokenHoldersProvider.apply( databaseName );
			  this._locks = editionModule.LocksSupplier.get();
			  this._statementLocksFactory = editionModule.StatementLocksFactoryProvider.apply( _locks );
			  this._transactionMonitor = editionModule.CreateTransactionMonitor();
			  this._editionModule = editionModule;
		 }

		 public virtual DatabaseIdContext IdContext
		 {
			 get
			 {
				  return _idContext;
			 }
		 }

		 public override TokenHolders CreateTokenHolders()
		 {
			  return _tokenHolders;
		 }

		 public virtual System.Func<File, FileSystemWatcherService> WatcherServiceFactory
		 {
			 get
			 {
				  return _watcherServiceFactory;
			 }
		 }

		 public virtual AccessCapability AccessCapability
		 {
			 get
			 {
				  return _accessCapability;
			 }
		 }

		 public virtual IOLimiter IoLimiter
		 {
			 get
			 {
				  return _ioLimiter;
			 }
		 }

		 public virtual ConstraintSemantics ConstraintSemantics
		 {
			 get
			 {
				  return _constraintSemantics;
			 }
		 }

		 public virtual CommitProcessFactory CommitProcessFactory
		 {
			 get
			 {
				  return _commitProcessFactory;
			 }
		 }

		 public virtual TransactionHeaderInformationFactory HeaderInformationFactory
		 {
			 get
			 {
				  return _headerInformationFactory;
			 }
		 }

		 public virtual SchemaWriteGuard SchemaWriteGuard
		 {
			 get
			 {
				  return _schemaWriteGuard;
			 }
		 }

		 public virtual long TransactionStartTimeout
		 {
			 get
			 {
				  return _transactionStartTimeout;
			 }
		 }

		 public override Locks CreateLocks()
		 {
			  return _locks;
		 }

		 public override StatementLocksFactory CreateStatementLocksFactory()
		 {
			  return _statementLocksFactory;
		 }

		 public override DatabaseTransactionStats CreateTransactionMonitor()
		 {
			  return _transactionMonitor;
		 }

		 public override DatabaseAvailabilityGuard CreateDatabaseAvailabilityGuard( SystemNanoClock clock, LogService logService, Config config )
		 {
			  return _editionModule.createDatabaseAvailabilityGuard( _databaseName, clock, logService, config );
		 }
	}

}