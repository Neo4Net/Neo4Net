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

	using Org.Neo4j.Server.database;

	using HttpContext = com.sun.jersey.api.core.HttpContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Provider public final class InputFormatProvider extends org.neo4j.server.database.InjectableProvider<InputFormat>
	public sealed class InputFormatProvider : InjectableProvider<InputFormat>
	{
		 private readonly RepresentationFormatRepository _repository;

		 public InputFormatProvider( RepresentationFormatRepository repository ) : base( typeof( InputFormat ) )
		 {
			  this._repository = repository;
		 }

		 public override InputFormat GetValue( HttpContext context )
		 {
			  try
			  {
					return _repository.inputFormat( context.Request.MediaType );
			  }
			  catch ( MediaTypeNotSupportedException e )
			  {
					throw new WebApplicationException( Response.status( Response.Status.UNSUPPORTED_MEDIA_TYPE ).entity( e.Message ).build() );
			  }
		 }
	}

}