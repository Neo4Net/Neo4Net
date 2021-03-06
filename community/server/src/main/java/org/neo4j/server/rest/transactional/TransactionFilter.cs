﻿/*
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
namespace Org.Neo4j.Server.rest.transactional
{
	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using ResourceMethodDispatchAdapter = com.sun.jersey.spi.container.ResourceMethodDispatchAdapter;
	using ResourceMethodDispatchProvider = com.sun.jersey.spi.container.ResourceMethodDispatchProvider;
	using RequestDispatcher = com.sun.jersey.spi.dispatch.RequestDispatcher;

	using Database = Org.Neo4j.Server.database.Database;
	using Org.Neo4j.Server.database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Provider public class TransactionFilter extends org.neo4j.server.database.InjectableProvider<Void> implements com.sun.jersey.spi.container.ResourceMethodDispatchAdapter
	public class TransactionFilter : InjectableProvider<Void>, ResourceMethodDispatchAdapter
	{
		 private Database _database;

		 public TransactionFilter( Database database ) : base( typeof( Void ) )
		 {
			  this._database = database;
		 }

		 public override Void GetValue( HttpContext httpContext )
		 {
			  throw new System.InvalidOperationException( "This _really_ should never happen" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public com.sun.jersey.spi.container.ResourceMethodDispatchProvider adapt(final com.sun.jersey.spi.container.ResourceMethodDispatchProvider resourceMethodDispatchProvider)
		 public override ResourceMethodDispatchProvider Adapt( ResourceMethodDispatchProvider resourceMethodDispatchProvider )
		 {
			  return abstractResourceMethod =>
			  {
				RequestDispatcher requestDispatcher = resourceMethodDispatchProvider.create( abstractResourceMethod );

				return new TransactionalRequestDispatcher( _database, requestDispatcher );
			  };
		 }
	}

}