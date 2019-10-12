using System.Collections.Generic;
using System.Text;

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


	public class MediaTypeNotSupportedException : WebApplicationException
	{
		 private const long SERIAL_VERSION_UID = 5159216782240337940L;

		 public MediaTypeNotSupportedException( Response.Status status, ICollection<MediaType> supported, params MediaType[] requested ) : base( CreateResponse( status, Message( supported, requested ) ) )
		 {
		 }

		 private static Response CreateResponse( Response.Status status, string message )
		 {
			  return Response.status( status ).entity( message ).build();
		 }

		 private static string Message( ICollection<MediaType> supported, MediaType[] requested )
		 {
			  StringBuilder message = new StringBuilder( "No matching representation format found.\n" );
			  if ( requested.Length == 0 )
			  {
					message.Append( "No requested representation format supplied." );
			  }
			  else if ( requested.Length == 1 )
			  {
					message.Append( "Request format: " ).Append( requested[0] ).Append( "\n" );
			  }
			  else
			  {
					message.Append( "Requested formats:\n" );
					for ( int i = 0; i < requested.Length; i++ )
					{
						 message.Append( " " ).Append( i ).Append( ". " );
						 message.Append( requested[i] ).Append( "\n" );
					}
			  }
			  message.Append( "Supported representation formats:" );
			  if ( supported.Count == 0 )
			  {
					message.Append( " none" );
			  }
			  else
			  {
					foreach ( MediaType type in supported )
					{
						 message.Append( "\n * " ).Append( type );
					}
			  }
			  return message.ToString();
		 }
	}

}