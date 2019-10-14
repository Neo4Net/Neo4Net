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

	public interface DatabaseEditionContext
	{
		 DatabaseIdContext IdContext { get; }

		 TokenHolders CreateTokenHolders();

		 System.Func<File, FileSystemWatcherService> WatcherServiceFactory { get; }

		 AccessCapability AccessCapability { get; }

		 IOLimiter IoLimiter { get; }

		 ConstraintSemantics ConstraintSemantics { get; }

		 CommitProcessFactory CommitProcessFactory { get; }

		 TransactionHeaderInformationFactory HeaderInformationFactory { get; }

		 SchemaWriteGuard SchemaWriteGuard { get; }

		 long TransactionStartTimeout { get; }

		 Locks CreateLocks();

		 StatementLocksFactory CreateStatementLocksFactory();

		 DatabaseTransactionStats CreateTransactionMonitor();

		 DatabaseAvailabilityGuard CreateDatabaseAvailabilityGuard( SystemNanoClock clock, LogService logService, Config config );
	}

}