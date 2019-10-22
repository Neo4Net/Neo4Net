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
namespace Neo4Net.GraphDb.factory.module.id
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdReuseEligibility = Neo4Net.Kernel.impl.store.id.IdReuseEligibility;
	using CommunityIdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using IdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;


	public class IdContextFactoryBuilder
	{
		 private IdReuseEligibility _idReuseEligibility = Neo4Net.Kernel.impl.store.id.IdReuseEligibility_Fields.Always;
		 private FileSystemAbstraction _fileSystemAbstraction;
		 private IJobScheduler _jobScheduler;
		 private System.Func<string, IdGeneratorFactory> _idGeneratorFactoryProvider;
		 private IdTypeConfigurationProvider _idTypeConfigurationProvider;
		 private System.Func<IdGeneratorFactory, IdGeneratorFactory> _factoryWrapper;

		 private IdContextFactoryBuilder()
		 {
		 }

		 public static IdContextFactoryBuilder Of( IdTypeConfigurationProvider configurationProvider, IJobScheduler jobScheduler )
		 {
			  IdContextFactoryBuilder builder = new IdContextFactoryBuilder();
			  builder._idTypeConfigurationProvider = configurationProvider;
			  builder._jobScheduler = jobScheduler;
			  return builder;
		 }

		 public static IdContextFactoryBuilder Of( FileSystemAbstraction fileSystemAbstraction, IJobScheduler jobScheduler )
		 {
			  IdContextFactoryBuilder builder = new IdContextFactoryBuilder();
			  builder._fileSystemAbstraction = fileSystemAbstraction;
			  builder._jobScheduler = jobScheduler;
			  return builder;
		 }

		 public virtual IdContextFactoryBuilder WithFileSystem( FileSystemAbstraction fileSystem )
		 {
			  this._fileSystemAbstraction = fileSystem;
			  return this;
		 }

		 public virtual IdContextFactoryBuilder WithIdReuseEligibility( IdReuseEligibility eligibleForIdReuse )
		 {
			  this._idReuseEligibility = eligibleForIdReuse;
			  return this;
		 }

		 public virtual IdContextFactoryBuilder WithIdGenerationFactoryProvider( System.Func<string, IdGeneratorFactory> idGeneratorFactoryProvider )
		 {
			  this._idGeneratorFactoryProvider = idGeneratorFactoryProvider;
			  return this;
		 }

		 public virtual IdContextFactoryBuilder WithFactoryWrapper( System.Func<IdGeneratorFactory, IdGeneratorFactory> factoryWrapper )
		 {
			  this._factoryWrapper = factoryWrapper;
			  return this;
		 }

		 public virtual IdContextFactory Build()
		 {
			  if ( _idGeneratorFactoryProvider == null )
			  {
					requireNonNull( _fileSystemAbstraction, "File system is required to build id generator factory." );
					_idGeneratorFactoryProvider = databaseName => new DefaultIdGeneratorFactory( _fileSystemAbstraction, _idTypeConfigurationProvider );
			  }
			  if ( _idTypeConfigurationProvider == null )
			  {
					_idTypeConfigurationProvider = new CommunityIdTypeConfigurationProvider();
			  }
			  if ( _factoryWrapper == null )
			  {
					_factoryWrapper = identity();
			  }
			  return new IdContextFactory( _jobScheduler, _idGeneratorFactoryProvider, _idTypeConfigurationProvider, _idReuseEligibility, _factoryWrapper );
		 }
	}

}