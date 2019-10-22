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
namespace Neo4Net.GraphDb.factory.module.edition
{
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using SecurityModule = Neo4Net.Kernel.api.security.SecurityModule;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using DependencySatisfier = Neo4Net.Kernel.impl.util.DependencySatisfier;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

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

		 public override IJobScheduler Scheduler()
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