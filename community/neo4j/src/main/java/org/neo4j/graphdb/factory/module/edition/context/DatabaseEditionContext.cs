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