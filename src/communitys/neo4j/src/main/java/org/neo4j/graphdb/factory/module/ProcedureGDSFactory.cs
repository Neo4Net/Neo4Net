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
namespace Neo4Net.Graphdb.factory.module
{

	using Neo4Net.Functions;
	using ProcedureGDBFacadeSPI = Neo4Net.Graphdb.facade.spi.ProcedureGDBFacadeSPI;
	using URLAccessValidationError = Neo4Net.Graphdb.security.URLAccessValidationError;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using CoreAPIAvailabilityGuard = Neo4Net.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;

	public class ProcedureGDSFactory : ThrowingFunction<Context, GraphDatabaseService, ProcedureException>
	{
		 private readonly PlatformModule _platform;
		 private readonly DataSourceModule _dataSource;
		 private readonly CoreAPIAvailabilityGuard _availability;
		 private readonly ThrowingFunction<URL, URL, URLAccessValidationError> _urlValidator;
		 private readonly TokenHolders _tokenHolders;
		 private readonly ThreadToStatementContextBridge _bridge;

		 internal ProcedureGDSFactory( PlatformModule platform, DataSourceModule dataSource, CoreAPIAvailabilityGuard coreAPIAvailabilityGuard, TokenHolders tokenHolders, ThreadToStatementContextBridge bridge )
		 {
			  this._platform = platform;
			  this._dataSource = dataSource;
			  this._availability = coreAPIAvailabilityGuard;
			  this._urlValidator = url => platform.UrlAccessRule.validate( platform.Config, url );
			  this._tokenHolders = tokenHolders;
			  this._bridge = bridge;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.GraphDatabaseService apply(org.neo4j.kernel.api.proc.Context context) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override GraphDatabaseService Apply( Context context )
		 {
			  KernelTransaction tx = context.GetOrElse( Neo4Net.Kernel.api.proc.Context_Fields.KernelTransaction, null );
			  SecurityContext securityContext;
			  if ( tx != null )
			  {
					securityContext = tx.SecurityContext();
			  }
			  else
			  {
					securityContext = context.Get( Neo4Net.Kernel.api.proc.Context_Fields.SecurityContext );
			  }
			  GraphDatabaseFacade facade = new GraphDatabaseFacade();
			  ProcedureGDBFacadeSPI procedureGDBFacadeSPI = new ProcedureGDBFacadeSPI( _dataSource, _dataSource.neoStoreDataSource.DependencyResolver, _availability, _urlValidator, securityContext, _bridge );
			  facade.Init( procedureGDBFacadeSPI, _bridge, _platform.config, _tokenHolders );
			  return facade;
		 }
	}

}