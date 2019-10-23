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
namespace Neo4Net.GraphDb.factory.module
{

	using AbstractEditionModule = Neo4Net.GraphDb.factory.module.edition.AbstractEditionModule;
	using DatabaseEditionContext = Neo4Net.GraphDb.factory.module.edition.context.DatabaseEditionContext;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using InwardKernel = Neo4Net.Kernel.api.InwardKernel;
	using CoreAPIAvailabilityGuard = Neo4Net.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

	public class DataSourceModule
	{
		 public readonly NeoStoreDataSource NeoStoreDataSource;

		 public readonly System.Func<InwardKernel> KernelAPI;

		 public readonly System.Func<StoreId> StoreId;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public readonly CoreAPIAvailabilityGuard CoreAPIAvailabilityGuardConflict;

		 public DataSourceModule( string databaseName, PlatformModule platformModule, AbstractEditionModule editionModule, Procedures procedures, GraphDatabaseFacade graphDatabaseFacade )
		 {
			  platformModule.DiagnosticsManager.prependProvider( platformModule.Config );
			  DatabaseEditionContext editionContext = editionModule.CreateDatabaseContext( databaseName );
			  ModularDatabaseCreationContext context = new ModularDatabaseCreationContext( databaseName, platformModule, editionContext, procedures, graphDatabaseFacade );
			  NeoStoreDataSource = new NeoStoreDataSource( context );

			  this.CoreAPIAvailabilityGuardConflict = context.CoreAPIAvailabilityGuard;
			  this.StoreId = NeoStoreDataSource.getStoreId;
			  this.KernelAPI = NeoStoreDataSource.getKernel;

			  ProcedureGDSFactory gdsFactory = new ProcedureGDSFactory( platformModule, this, CoreAPIAvailabilityGuardConflict, context.TokenHolders, editionModule.ThreadToTransactionBridge );
			  procedures.RegisterComponent( typeof( IGraphDatabaseService ), gdsFactory.apply, true );
		 }

		 public virtual CoreAPIAvailabilityGuard CoreAPIAvailabilityGuard
		 {
			 get
			 {
				  return CoreAPIAvailabilityGuardConflict;
			 }
		 }
	}

}