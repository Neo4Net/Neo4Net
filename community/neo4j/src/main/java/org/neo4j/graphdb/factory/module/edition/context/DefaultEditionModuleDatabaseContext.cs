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
namespace Org.Neo4j.Graphdb.factory.module.edition.context
{

	using DatabaseIdContext = Org.Neo4j.Graphdb.factory.module.id.DatabaseIdContext;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using AccessCapability = Org.Neo4j.Kernel.impl.factory.AccessCapability;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using DatabaseTransactionStats = Org.Neo4j.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using FileSystemWatcherService = Org.Neo4j.Kernel.impl.util.watcher.FileSystemWatcherService;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

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