using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Server.database
{

	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using URLAccessRule = Org.Neo4j.Graphdb.security.URLAccessRule;
	using Org.Neo4j.Helpers.Collection;
	using AvailabilityGuardInstaller = Org.Neo4j.Kernel.availability.AvailabilityGuardInstaller;
	using Org.Neo4j.Kernel.extension;
	using QueryEngineProvider = Org.Neo4j.Kernel.impl.query.QueryEngineProvider;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using DeferredExecutor = Org.Neo4j.Scheduler.DeferredExecutor;
	using Group = Org.Neo4j.Scheduler.Group;

	internal sealed class AvailabiltyGuardCapturingDependencies : GraphDatabaseFacadeFactory.Dependencies
	{
		 private readonly AvailabilityGuardInstaller _guardInstaller;
		 private readonly GraphDatabaseFacadeFactory.Dependencies _wrapped;

		 internal AvailabiltyGuardCapturingDependencies( AvailabilityGuardInstaller guardInstaller, GraphDatabaseFacadeFactory.Dependencies wrapped )
		 {
			  this._guardInstaller = guardInstaller;
			  this._wrapped = wrapped;
		 }

		 public override Monitors Monitors()
		 {
			  return _wrapped.monitors();
		 }

		 public override LogProvider UserLogProvider()
		 {
			  return _wrapped.userLogProvider();
		 }

		 public override IEnumerable<Type> SettingsClasses()
		 {
			  return _wrapped.settingsClasses();
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions()
		 public override IEnumerable<KernelExtensionFactory<object>> KernelExtensions()
		 {
			  return _wrapped.kernelExtensions();
		 }

		 public override IDictionary<string, URLAccessRule> UrlAccessRules()
		 {
			  return _wrapped.urlAccessRules();
		 }

		 public override IEnumerable<QueryEngineProvider> ExecutionEngines()
		 {
			  return _wrapped.executionEngines();
		 }

		 public override IEnumerable<Pair<DeferredExecutor, Group>> DeferredExecutors()
		 {
			  return _wrapped.deferredExecutors();
		 }

		 public override AvailabilityGuardInstaller AvailabilityGuardInstaller()
		 {
			  return _guardInstaller;
		 }
	}

}