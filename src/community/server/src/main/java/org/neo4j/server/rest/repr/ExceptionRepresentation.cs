using System;
using System.Collections.Generic;

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

	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Neo4Net.Collections.Helpers;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Neo4NetError = Neo4Net.Server.rest.transactional.error.Neo4NetError;

	public class ExceptionRepresentation : MappingRepresentation
	{
		 private readonly IList<Neo4NetError> _errors = new LinkedList<Neo4NetError>();
		 private bool _includeLegacyRepresentation;

		 public ExceptionRepresentation( Exception exception ) : this( exception, true )
		 {
		 }
		 public ExceptionRepresentation( Exception exception, bool includeLegacyRepresentation ) : base( RepresentationType.Exception )
		 {
			  this._errors.Add( new Neo4NetError( StatusCode( exception ), exception ) );
			  this._includeLegacyRepresentation = includeLegacyRepresentation;
		 }

		 public ExceptionRepresentation( params Neo4NetError[] errors ) : base( RepresentationType.Exception )
		 {
			  ( ( IList<Neo4NetError> )this._errors ).AddRange( Arrays.asList( errors ) );
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  // For legacy reasons, this actually serializes into two separate formats - the old format, which simply
			  // serializes a single exception, and the new format which serializes multiple errors and provides simple
			  // status codes.
			  if ( _includeLegacyRepresentation )
			  {
					RenderWithLegacyFormat( _errors[0].cause(), serializer );
			  }

			  RenderWithStatusCodeFormat( serializer );
		 }

		 private void RenderWithStatusCodeFormat( MappingSerializer serializer )
		 {
			  serializer.PutList( "errors", ErrorEntryRepresentation.List( _errors ) );
		 }

		 private void RenderWithLegacyFormat( Exception exception, MappingSerializer serializer )
		 {
			  string message = exception.Message;
			  if ( !string.ReferenceEquals( message, null ) )
			  {
					serializer.PutString( "message", message );
			  }
			  serializer.PutString( "exception", exception.GetType().Name );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  serializer.PutString( "fullname", exception.GetType().FullName );
			  StackTraceElement[] trace = exception.StackTrace;
			  if ( trace != null )
			  {
					ICollection<string> lines = new List<string>( trace.Length );
					foreach ( StackTraceElement element in trace )
					{
						 if ( element.ToString().matches(".*(jetty|jersey|sun\\.reflect|mortbay|javax\\.servlet).*") )
						 {
							  continue;
						 }
						 lines.Add( element.ToString() );
					}
					serializer.PutList( "stackTrace", ListRepresentation.String( lines ) );
			  }

			  Exception cause = exception.InnerException;
			  if ( cause != null )
			  {
					serializer.PutMapping( "cause", new ExceptionRepresentation( cause ) );
			  }
		 }

		 private class ErrorEntryRepresentation : MappingRepresentation
		 {
			  internal readonly Neo4NetError Error;

			  internal ErrorEntryRepresentation( Neo4NetError error ) : base( "error-entry" )
			  {
					this.Error = error;
			  }

			  protected internal override void Serialize( MappingSerializer serializer )
			  {
					serializer.PutString( "code", Error.status().code().serialize() );
					serializer.PutString( "message", Error.Message );
					if ( Error.shouldSerializeStackTrace() )
					{
						 serializer.PutString( "stackTrace", Error.StackTraceAsString );
					}
			  }

			  public static ListRepresentation List( ICollection<Neo4NetError> errors )
			  {
					return new ListRepresentation( "error-list", new IterableWrapperAnonymousInnerClass( errors ) );
			  }

			  private class IterableWrapperAnonymousInnerClass : IterableWrapper<ErrorEntryRepresentation, Neo4NetError>
			  {
				  public IterableWrapperAnonymousInnerClass( ICollection<Neo4NetError> errors ) : base( errors )
				  {
				  }

				  protected internal override ErrorEntryRepresentation underlyingObjectToObject( Neo4NetError error )
				  {
						return new ErrorEntryRepresentation( error );
				  }
			  }
		 }

		 private static Status StatusCode( Exception current )
		 {
			  while ( current != null )
			  {
					if ( current is Neo4Net.Kernel.Api.Exceptions.Status_HasStatus )
					{
						 return ( ( Neo4Net.Kernel.Api.Exceptions.Status_HasStatus ) current ).Status();
					}
					if ( current is ConstraintViolationException )
					{
						 return Neo4Net.Kernel.Api.Exceptions.Status_Schema.ConstraintValidationFailed;
					}
					current = current.InnerException;
			  }
			  return Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError;
		 }
	}

}