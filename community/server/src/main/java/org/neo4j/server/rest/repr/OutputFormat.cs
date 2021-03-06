﻿using System;

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

	using NodeNotFoundException = Org.Neo4j.Server.rest.web.NodeNotFoundException;
	using RelationshipNotFoundException = Org.Neo4j.Server.rest.web.RelationshipNotFoundException;
	using HttpHeaderUtils = Org.Neo4j.Server.web.HttpHeaderUtils;
	using UTF8 = Org.Neo4j.@string.UTF8;

	public class OutputFormat
	{
		 private readonly RepresentationFormat _format;
		 private readonly ExtensionInjector _extensions;
		 private readonly URI _baseUri;

		 private RepresentationWriteHandler _representationWriteHandler = RepresentationWriteHandler.DO_NOTHING;

		 public OutputFormat( RepresentationFormat format, URI baseUri, ExtensionInjector extensions )
		 {
			  this._format = format;
			  this._baseUri = baseUri;
			  this._extensions = extensions;
		 }

		 public virtual RepresentationWriteHandler RepresentationWriteHandler
		 {
			 set
			 {
				  this._representationWriteHandler = value;
			 }
			 get
			 {
				  return this._representationWriteHandler;
			 }
		 }


		 public Response Ok( Representation representation )
		 {
			  if ( representation.Empty )
			  {
					return NoContent();
			  }
			  return Response( Response.ok(), representation );
		 }

		 public Response OkIncludeLocation<REPR>( REPR representation ) where REPR : Representation, EntityRepresentation
		 {
			  if ( representation.Empty )
			  {
					return NoContent();
			  }
			  return Response( Response.ok().header(HttpHeaders.LOCATION, Uri(representation)), representation );
		 }

		 public Response Created<REPR>( REPR representation ) where REPR : Representation, EntityRepresentation
		 {
			  return Response( Response.created( Uri( representation ) ), representation );
		 }

		 public Response Response( Response.StatusType status, Representation representation )
		 {
			  return Response( Response.status( status ), representation );
		 }

		 public virtual Response BadRequest( Exception exception )
		 {
			  return Response( Response.status( BAD_REQUEST ), new ExceptionRepresentation( exception ) );
		 }

		 public virtual Response NotFound( Exception exception )
		 {
			  return Response( Response.status( Response.Status.NOT_FOUND ), new ExceptionRepresentation( exception ) );
		 }

		 public virtual Response NotFound()
		 {
			  _representationWriteHandler.onRepresentationFinal();
			  return Response.status( Response.Status.NOT_FOUND ).build();
		 }

		 public virtual Response SeeOther( URI uri )
		 {
			  return Response.seeOther( _baseUri.resolve( uri ) ).build();
		 }

		 public virtual Response Conflict( Exception exception )
		 {
			  return Response( Response.status( Response.Status.CONFLICT ), new ExceptionRepresentation( exception ) );
		 }

		 public Response Conflict<REPR>( REPR representation ) where REPR : Representation, EntityRepresentation
		 {
			  return Response( Response.status( Response.Status.CONFLICT ), representation );
		 }

		 /// <summary>
		 /// Server error with stack trace included as needed. </summary>
		 /// <param name="exception"> the error </param>
		 /// <returns> the internal server error response </returns>
		 public virtual Response ServerErrorWithoutLegacyStacktrace( Exception exception )
		 {
			  return Response( Response.status( Response.Status.INTERNAL_SERVER_ERROR ), new ExceptionRepresentation( exception, false ) );
		 }

		 public virtual Response ServerError( Exception exception )
		 {
			  return Response( Response.status( Response.Status.INTERNAL_SERVER_ERROR ), new ExceptionRepresentation( exception ) );
		 }

		 private URI Uri( EntityRepresentation representation )
		 {
			  return URI.create( Assemble( representation.SelfUri() ) );
		 }

		 protected internal virtual Response Response( Response.ResponseBuilder response, Representation representation )
		 {
			  return FormatRepresentation( response, representation ).type( HttpHeaderUtils.mediaTypeWithCharsetUtf8( MediaType ) ).build();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.ws.rs.core.Response.ResponseBuilder formatRepresentation(javax.ws.rs.core.Response.ResponseBuilder response, final Representation representation)
		 private Response.ResponseBuilder FormatRepresentation( Response.ResponseBuilder response, Representation representation )
		 {
			  _representationWriteHandler.onRepresentationStartWriting();

			  bool mustFail = representation is ExceptionRepresentation;

			  if ( _format is StreamingFormat )
			  {
					return response.entity( Stream( representation, ( StreamingFormat ) _format, mustFail ) );
			  }
			  else
			  {
					return response.entity( ToBytes( Assemble( representation ), mustFail ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Object stream(final Representation representation, final StreamingFormat streamingFormat, final boolean mustFail)
		 private object Stream( Representation representation, StreamingFormat streamingFormat, bool mustFail )
		 {
			  return ( StreamingOutput ) output =>
			  {
				RepresentationFormat outputStreamFormat = streamingFormat.WriteTo( output );
				try
				{
					 representation.Serialize( outputStreamFormat, _baseUri, _extensions );

					 if ( !mustFail )
					 {
						  _representationWriteHandler.onRepresentationWritten();
					 }
				}
				catch ( Exception e )
				{
					 if ( e is NodeNotFoundException || e is RelationshipNotFoundException )
					 {
						  throw new WebApplicationException( NotFound( e ) );
					 }
					 if ( e is BadInputException )
					 {
						  throw new WebApplicationException( BadRequest( e ) );
					 }
					 throw new WebApplicationException( e, ServerError( e ) );
				}
				finally
				{
					 _representationWriteHandler.onRepresentationFinal();
				}
			  };
		 }

		 public static void Write( Representation representation, RepresentationFormat format, URI baseUri )
		 {
			  representation.Serialize( format, baseUri, null );
		 }

		 private sbyte[] ToBytes( string entity, bool mustFail )
		 {
			  sbyte[] entityAsBytes = UTF8.encode( entity );
			  if ( !mustFail )
			  {
					_representationWriteHandler.onRepresentationWritten();
			  }
			  _representationWriteHandler.onRepresentationFinal();
			  return entityAsBytes;
		 }

		 public virtual MediaType MediaType
		 {
			 get
			 {
				  return _format.mediaType;
			 }
		 }

		 public virtual string Assemble( Representation representation )
		 {
			  return representation.Serialize( _format, _baseUri, _extensions );
		 }

		 public virtual Response NoContent()
		 {
			  _representationWriteHandler.onRepresentationStartWriting();
			  _representationWriteHandler.onRepresentationWritten();
			  _representationWriteHandler.onRepresentationFinal();
			  return Response.status( Response.Status.NO_CONTENT ).build();
		 }

		 public virtual Response MethodNotAllowed( System.NotSupportedException e )
		 {
			  return Response( Response.status( 405 ), new ExceptionRepresentation( e ) );
		 }

		 public virtual Response Ok()
		 {
			  _representationWriteHandler.onRepresentationStartWriting();
			  _representationWriteHandler.onRepresentationWritten();
			  _representationWriteHandler.onRepresentationFinal();
			  return Response.ok().build();
		 }

		 public virtual Response BadRequest( MediaType mediaType, string entity )
		 {
			  _representationWriteHandler.onRepresentationStartWriting();
			  _representationWriteHandler.onRepresentationFinal();
			  return Response.status( BAD_REQUEST ).type( mediaType ).entity( entity ).build();
		 }
	}

}