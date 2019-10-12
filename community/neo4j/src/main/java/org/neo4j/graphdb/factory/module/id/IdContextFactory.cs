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
namespace Org.Neo4j.Graphdb.factory.module.id
{

	using BufferedIdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using DefaultIdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.DefaultIdController;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using BufferingIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdReuseEligibility = Org.Neo4j.Kernel.impl.store.id.IdReuseEligibility;
	using IdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using FeatureToggles = Org.Neo4j.Util.FeatureToggles;

	public class IdContextFactory
	{
		  private static readonly bool _idBufferingFlag = FeatureToggles.flag( typeof( IdContextFactory ), "safeIdBuffering", true );

		 private readonly JobScheduler _jobScheduler;
		 private readonly System.Func<string, IdGeneratorFactory> _idFactoryProvider;
		 private readonly IdTypeConfigurationProvider _idTypeConfigurationProvider;
		 private readonly IdReuseEligibility _eligibleForIdReuse;
		 private readonly System.Func<IdGeneratorFactory, IdGeneratorFactory> _factoryWrapper;

		 internal IdContextFactory( JobScheduler jobScheduler, System.Func<string, IdGeneratorFactory> idFactoryProvider, IdTypeConfigurationProvider idTypeConfigurationProvider, IdReuseEligibility eligibleForIdReuse, System.Func<IdGeneratorFactory, IdGeneratorFactory> factoryWrapper )
		 {
			  this._jobScheduler = jobScheduler;
			  this._idFactoryProvider = idFactoryProvider;
			  this._idTypeConfigurationProvider = idTypeConfigurationProvider;
			  this._eligibleForIdReuse = eligibleForIdReuse;
			  this._factoryWrapper = factoryWrapper;
		 }

		 public virtual DatabaseIdContext CreateIdContext( string databaseName )
		 {
			  return _idBufferingFlag ? CreateBufferingIdContext( _idFactoryProvider, _jobScheduler, databaseName ) : CreateDefaultIdContext( _idFactoryProvider, databaseName );
		 }

		 private DatabaseIdContext CreateDefaultIdContext<T1>( System.Func<T1> idGeneratorFactoryProvider, string databaseName ) where T1 : Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory
		 {
			  return CreateIdContext( idGeneratorFactoryProvider( databaseName ), CreateDefaultIdController() );
		 }

		 private DatabaseIdContext CreateBufferingIdContext<T1>( System.Func<T1> idGeneratorFactoryProvider, JobScheduler jobScheduler, string databaseName ) where T1 : Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory
		 {
			  IdGeneratorFactory idGeneratorFactory = idGeneratorFactoryProvider( databaseName );
			  BufferingIdGeneratorFactory bufferingIdGeneratorFactory = new BufferingIdGeneratorFactory( idGeneratorFactory, _eligibleForIdReuse, _idTypeConfigurationProvider );
			  BufferedIdController bufferingController = CreateBufferedIdController( bufferingIdGeneratorFactory, jobScheduler );
			  return CreateIdContext( bufferingIdGeneratorFactory, bufferingController );
		 }

		 private DatabaseIdContext CreateIdContext( IdGeneratorFactory idGeneratorFactory, IdController idController )
		 {
			  return new DatabaseIdContext( _factoryWrapper.apply( idGeneratorFactory ), idController );
		 }

		 private static BufferedIdController CreateBufferedIdController( BufferingIdGeneratorFactory idGeneratorFactory, JobScheduler scheduler )
		 {
			  return new BufferedIdController( idGeneratorFactory, scheduler );
		 }

		 private static DefaultIdController CreateDefaultIdController()
		 {
			  return new DefaultIdController();
		 }
	}

}