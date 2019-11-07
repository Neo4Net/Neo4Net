using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.database
{

	using GraphDatabaseFacadeFactory = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory;
	using URLAccessRule = Neo4Net.GraphDb.security.URLAccessRule;
	using Neo4Net.Collections.Helpers;
	using AvailabilityGuardInstaller = Neo4Net.Kernel.availability.AvailabilityGuardInstaller;
	using Neo4Net.Kernel.extension;
	using QueryEngineProvider = Neo4Net.Kernel.impl.query.QueryEngineProvider;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using DeferredExecutor = Neo4Net.Scheduler.DeferredExecutor;
	using Group = Neo4Net.Scheduler.Group;

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
//ORIGINAL LINE: public Iterable<Neo4Net.kernel.extension.KernelExtensionFactory<?>> kernelExtensions()
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