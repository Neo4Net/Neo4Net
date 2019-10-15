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
namespace Neo4Net.Graphdb.factory.module.id
{

	using BufferedIdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using DefaultIdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.DefaultIdController;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using BufferingIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdReuseEligibility = Neo4Net.Kernel.impl.store.id.IdReuseEligibility;
	using IdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

	public class IdContextFactory
	{
		  private static readonly bool _idBufferingFlag = FeatureToggles.flag( typeof( IdContextFactory ), "safeIdBuffering", true );

		 private readonly IJobScheduler _jobScheduler;
		 private readonly System.Func<string, IdGeneratorFactory> _idFactoryProvider;
		 private readonly IdTypeConfigurationProvider _idTypeConfigurationProvider;
		 private readonly IdReuseEligibility _eligibleForIdReuse;
		 private readonly System.Func<IdGeneratorFactory, IdGeneratorFactory> _factoryWrapper;

		 internal IdContextFactory( IJobScheduler jobScheduler, System.Func<string, IdGeneratorFactory> idFactoryProvider, IdTypeConfigurationProvider idTypeConfigurationProvider, IdReuseEligibility eligibleForIdReuse, System.Func<IdGeneratorFactory, IdGeneratorFactory> factoryWrapper )
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

		 private DatabaseIdContext CreateDefaultIdContext<T1>( System.Func<T1> idGeneratorFactoryProvider, string databaseName ) where T1 : Neo4Net.Kernel.impl.store.id.IdGeneratorFactory
		 {
			  return CreateIdContext( idGeneratorFactoryProvider( databaseName ), CreateDefaultIdController() );
		 }

		 private DatabaseIdContext CreateBufferingIdContext<T1>( System.Func<T1> idGeneratorFactoryProvider, IJobScheduler jobScheduler, string databaseName ) where T1 : Neo4Net.Kernel.impl.store.id.IdGeneratorFactory
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

		 private static BufferedIdController CreateBufferedIdController( BufferingIdGeneratorFactory idGeneratorFactory, IJobScheduler scheduler )
		 {
			  return new BufferedIdController( idGeneratorFactory, scheduler );
		 }

		 private static DefaultIdController CreateDefaultIdController()
		 {
			  return new DefaultIdController();
		 }
	}

}