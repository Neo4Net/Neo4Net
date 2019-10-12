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
namespace Neo4Net.Kernel.api.security
{

	using Service = Neo4Net.Helpers.Service;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using SecurityProvider = Neo4Net.Kernel.api.security.provider.SecurityProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using DependencySatisfier = Neo4Net.Kernel.impl.util.DependencySatisfier;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	public abstract class SecurityModule : Service, Lifecycle, SecurityProvider
	{
		public abstract Neo4Net.Kernel.api.security.UserManagerSupplier UserManagerSupplier();
		public abstract Neo4Net.Kernel.api.security.AuthManager AuthManager();
		 protected internal readonly LifeSupport Life = new LifeSupport();

		 public SecurityModule( string key, params string[] altKeys ) : base( key, altKeys )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void setup(Dependencies dependencies) throws org.neo4j.internal.kernel.api.exceptions.KernelException, java.io.IOException;
		 public abstract void Setup( Dependencies dependencies );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {
			  Life.init();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  Life.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  Life.stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  Life.shutdown();
		 }

		 public interface Dependencies
		 {
			  LogService LogService();

			  Config Config();

			  Procedures Procedures();

			  JobScheduler Scheduler();

			  FileSystemAbstraction FileSystem();

			  DependencySatisfier DependencySatisfier();

			  AccessCapability AccessCapability();
		 }
	}

}