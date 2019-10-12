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
namespace Org.Neo4j.Graphdb.factory.module
{

	using Org.Neo4j.Function;
	using ProcedureGDBFacadeSPI = Org.Neo4j.Graphdb.facade.spi.ProcedureGDBFacadeSPI;
	using URLAccessValidationError = Org.Neo4j.Graphdb.security.URLAccessValidationError;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Context = Org.Neo4j.Kernel.api.proc.Context;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using CoreAPIAvailabilityGuard = Org.Neo4j.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;

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
			  KernelTransaction tx = context.GetOrElse( Org.Neo4j.Kernel.api.proc.Context_Fields.KernelTransaction, null );
			  SecurityContext securityContext;
			  if ( tx != null )
			  {
					securityContext = tx.SecurityContext();
			  }
			  else
			  {
					securityContext = context.Get( Org.Neo4j.Kernel.api.proc.Context_Fields.SecurityContext );
			  }
			  GraphDatabaseFacade facade = new GraphDatabaseFacade();
			  ProcedureGDBFacadeSPI procedureGDBFacadeSPI = new ProcedureGDBFacadeSPI( _dataSource, _dataSource.neoStoreDataSource.DependencyResolver, _availability, _urlValidator, securityContext, _bridge );
			  facade.Init( procedureGDBFacadeSPI, _bridge, _platform.config, _tokenHolders );
			  return facade;
		 }
	}

}