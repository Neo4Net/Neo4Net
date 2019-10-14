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
namespace Neo4Net.Internal.Kernel.Api.security
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;

	/// <summary>
	/// Controls the capabilities of a KernelTransaction, including the authenticated user and authorization data.
	/// 
	/// Must extend LoginContext to handle procedures creating internal transactions, periodic commit and the parallel cypher prototype.
	/// </summary>
	public class SecurityContext : LoginContext
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly AuthSubject SubjectConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly AccessMode ModeConflict;

		 public SecurityContext( AuthSubject subject, AccessMode mode )
		 {
			  this.SubjectConflict = subject;
			  this.ModeConflict = mode;
		 }

		 /// <summary>
		 /// Get the authorization data of the user. This is immutable.
		 /// </summary>
		 public virtual AccessMode Mode()
		 {
			  return ModeConflict;
		 }

		 /// <summary>
		 /// Check whether the user is an admin.
		 /// </summary>
		 public virtual bool Admin
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public override AuthSubject Subject()
		 {
			  return SubjectConflict;
		 }

		 public override SecurityContext Authorize( System.Func<string, int> propertyIdLookup, string dbName )
		 {
			  return this;
		 }

		 /// <summary>
		 /// Create a copy of this SecurityContext with the provided mode.
		 /// </summary>
		 public virtual SecurityContext WithMode( AccessMode mode )
		 {
			  return new SecurityContext( SubjectConflict, mode );
		 }

		 public virtual void AssertCredentialsNotExpired()
		 {
			  if ( Subject().AuthenticationResult.Equals(AuthenticationResult.PasswordChangeRequired) )
			  {
					throw Mode().onViolation(PERMISSION_DENIED);
			  }
		 }

		 public virtual string Description()
		 {
			  return string.Format( "user '{0}' with {1}", Subject().username(), Mode().name() );
		 }

		 protected internal virtual string DefaultString( string name )
		 {
			  return string.Format( "{0}{{ username={1}, accessMode={2} }}", name, Subject().username(), Mode() );
		 }

		 /// <summary>
		 /// Allows all operations. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("StaticInitializerReferencesSubClass") public static final SecurityContext AUTH_DISABLED = new AuthDisabled(AccessMode_Static.FULL);
		 public static readonly SecurityContext AuthDisabled = new AuthDisabled( AccessMode_Static.Full );

		 private class AuthDisabled : SecurityContext
		 {
			  internal AuthDisabled( AccessMode mode ) : base( AuthSubject.AUTH_DISABLED, mode )
			  {
			  }

			  public override SecurityContext WithMode( AccessMode mode )
			  {
					return new AuthDisabled( mode );
			  }

			  public override string Description()
			  {
					return "AUTH_DISABLED with " + Mode().name();
			  }

			  public override string ToString()
			  {
					return DefaultString( "auth-disabled" );
			  }
		 }
	}

}