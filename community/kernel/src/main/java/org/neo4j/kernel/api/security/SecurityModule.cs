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
namespace Org.Neo4j.Kernel.api.security
{

	using Service = Org.Neo4j.Helpers.Service;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using SecurityProvider = Org.Neo4j.Kernel.api.security.provider.SecurityProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using AccessCapability = Org.Neo4j.Kernel.impl.factory.AccessCapability;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using DependencySatisfier = Org.Neo4j.Kernel.impl.util.DependencySatisfier;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	public abstract class SecurityModule : Service, Lifecycle, SecurityProvider
	{
		public abstract Org.Neo4j.Kernel.api.security.UserManagerSupplier UserManagerSupplier();
		public abstract Org.Neo4j.Kernel.api.security.AuthManager AuthManager();
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