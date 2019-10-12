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
namespace Org.Neo4j.Graphdb.factory.module.edition
{
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using SecurityModule = Org.Neo4j.Kernel.api.security.SecurityModule;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using AccessCapability = Org.Neo4j.Kernel.impl.factory.AccessCapability;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using DependencySatisfier = Org.Neo4j.Kernel.impl.util.DependencySatisfier;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	internal class SecurityModuleDependenciesDependencies : SecurityModule.Dependencies
	{
		 private readonly PlatformModule _platformModule;
		 private readonly AbstractEditionModule _editionModule;
		 private readonly Procedures _procedures;

		 internal SecurityModuleDependenciesDependencies( PlatformModule platformModule, AbstractEditionModule editionModule, Procedures procedures )
		 {
			  this._platformModule = platformModule;
			  this._editionModule = editionModule;
			  this._procedures = procedures;
		 }

		 public override LogService LogService()
		 {
			  return _platformModule.logging;
		 }

		 public override Config Config()
		 {
			  return _platformModule.config;
		 }

		 public override Procedures Procedures()
		 {
			  return _procedures;
		 }

		 public override JobScheduler Scheduler()
		 {
			  return _platformModule.jobScheduler;
		 }

		 public override FileSystemAbstraction FileSystem()
		 {
			  return _platformModule.fileSystem;
		 }

		 public override DependencySatisfier DependencySatisfier()
		 {
			  return _platformModule.dependencies;
		 }

		 public override AccessCapability AccessCapability()
		 {
			  return _editionModule.accessCapability;
		 }
	}

}