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
namespace Org.Neo4j.Server.rest.repr
{

	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using HttpRequestContext = com.sun.jersey.api.core.HttpRequestContext;

	using Org.Neo4j.Server.database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Provider public final class OutputFormatProvider extends org.neo4j.server.database.InjectableProvider<OutputFormat>
	public sealed class OutputFormatProvider : InjectableProvider<OutputFormat>
	{
		 private readonly RepresentationFormatRepository _repository;

		 public OutputFormatProvider( RepresentationFormatRepository repository ) : base( typeof( OutputFormat ) )
		 {
			  this._repository = repository;
		 }

		 public override OutputFormat GetValue( HttpContext context )
		 {
			  try
			  {
					HttpRequestContext request = context.Request;
					return _repository.outputFormat( request.AcceptableMediaTypes, request.BaseUri, request.RequestHeaders );
			  }
			  catch ( MediaTypeNotSupportedException e )
			  {
					throw new WebApplicationException( Response.status( Response.Status.NOT_ACCEPTABLE ).entity( e.Message ).build() );
			  }
		 }
	}

}