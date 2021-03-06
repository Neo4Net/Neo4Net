﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.dbms
{

	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using UserManager = Org.Neo4j.Kernel.api.security.UserManager;
	using UserManagerSupplier = Org.Neo4j.Kernel.api.security.UserManagerSupplier;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using AuthorizationRepresentation = Org.Neo4j.Server.rest.repr.AuthorizationRepresentation;
	using BadInputException = Org.Neo4j.Server.rest.repr.BadInputException;
	using ExceptionRepresentation = Org.Neo4j.Server.rest.repr.ExceptionRepresentation;
	using InputFormat = Org.Neo4j.Server.rest.repr.InputFormat;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;
	using Neo4jError = Org.Neo4j.Server.rest.transactional.error.Neo4jError;
	using UTF8 = Org.Neo4j.@string.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.dbms.AuthorizedRequestWrapper.getLoginContextFromUserPrincipal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.web.CustomStatusType.UNPROCESSABLE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/user") public class UserService
	public class UserService
	{
		 public const string PASSWORD = "password";

		 private readonly UserManagerSupplier _userManagerSupplier;
		 private readonly InputFormat _input;
		 private readonly OutputFormat _output;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public UserService(@Context UserManagerSupplier userManagerSupplier, @Context InputFormat input, @Context OutputFormat output)
		 public UserService( UserManagerSupplier userManagerSupplier, InputFormat input, OutputFormat output )
		 {
			  this._userManagerSupplier = userManagerSupplier;
			  this._input = input;
			  this._output = output;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path("/{username}") public javax.ws.rs.core.Response getUser(@PathParam("username") String username, @Context HttpServletRequest req)
		 public virtual Response GetUser( string username, HttpServletRequest req )
		 {
			  Principal principal = req.UserPrincipal;
			  if ( principal == null || !principal.Name.Equals( username ) )
			  {
					return _output.notFound();
			  }

			  LoginContext loginContext = getLoginContextFromUserPrincipal( principal );
			  UserManager userManager = _userManagerSupplier.getUserManager( loginContext.Subject(), false );

			  try
			  {
					User user = userManager.GetUser( username );
					return _output.ok( new AuthorizationRepresentation( user ) );
			  }
			  catch ( InvalidArgumentsException )
			  {
					return _output.notFound();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path("/{username}/password") public javax.ws.rs.core.Response setPassword(@PathParam("username") String username, @Context HttpServletRequest req, String payload)
		 public virtual Response SetPassword( string username, HttpServletRequest req, string payload )
		 {
			  Principal principal = req.UserPrincipal;
			  if ( principal == null || !principal.Name.Equals( username ) )
			  {
					return _output.notFound();
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, Object> deserialized;
			  IDictionary<string, object> deserialized;
			  try
			  {
					deserialized = _input.readMap( payload );
			  }
			  catch ( BadInputException e )
			  {
					return _output.response( BAD_REQUEST, new ExceptionRepresentation( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, e.Message ) ) );
			  }

			  object o = deserialized[PASSWORD];
			  if ( o == null )
			  {
					return _output.response( UNPROCESSABLE, new ExceptionRepresentation( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Required parameter '{0}' is missing.", PASSWORD ) ) ) );
			  }
			  if ( !( o is string ) )
			  {
					return _output.response( UNPROCESSABLE, new ExceptionRepresentation( new Neo4jError( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.InvalidFormat, string.Format( "Expected '{0}' to be a string.", PASSWORD ) ) ) );
			  }
			  string newPassword = ( string ) o;

			  try
			  {
					LoginContext loginContext = getLoginContextFromUserPrincipal( principal );
					if ( loginContext == null )
					{
						 return _output.notFound();
					}
					else
					{
						 UserManager userManager = _userManagerSupplier.getUserManager( loginContext.Subject(), false );
						 userManager.SetUserPassword( username, UTF8.encode( newPassword ), false );
					}
			  }
			  catch ( IOException e )
			  {
					return _output.serverErrorWithoutLegacyStacktrace( e );
			  }
			  catch ( InvalidArgumentsException e )
			  {
					return _output.response( UNPROCESSABLE, new ExceptionRepresentation( new Neo4jError( e.Status(), e.Message ) ) );
			  }
			  return _output.ok();
		 }
	}

}