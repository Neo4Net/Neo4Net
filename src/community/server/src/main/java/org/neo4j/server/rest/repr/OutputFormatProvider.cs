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
namespace Neo4Net.Server.rest.repr
{

	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using HttpRequestContext = com.sun.jersey.api.core.HttpRequestContext;

	using Neo4Net.Server.database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Provider public final class OutputFormatProvider extends Neo4Net.server.database.InjectableProvider<OutputFormat>
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