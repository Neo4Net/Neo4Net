using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.enterprise.api.security
{

	using AccessMode = Org.Neo4j.@internal.Kernel.Api.security.AccessMode;
	using AuthSubject = Org.Neo4j.@internal.Kernel.Api.security.AuthSubject;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;

	/// <summary>
	/// A logged in and authorized user.
	/// </summary>
	public class EnterpriseSecurityContext : SecurityContext
	{
		 private readonly ISet<string> _roles;
		 private readonly bool _isAdmin;

		 public EnterpriseSecurityContext( AuthSubject subject, AccessMode mode, ISet<string> roles, bool isAdmin ) : base( subject, mode )
		 {
			  this._roles = roles;
			  this._isAdmin = isAdmin;
		 }

		 public override bool Admin
		 {
			 get
			 {
				  return _isAdmin;
			 }
		 }

		 public override EnterpriseSecurityContext Authorize( System.Func<string, int> propertyIdLookup, string dbName )
		 {
			  return this;
		 }

		 public override EnterpriseSecurityContext WithMode( AccessMode mode )
		 {
			  return new EnterpriseSecurityContext( SubjectConflict, mode, _roles, _isAdmin );
		 }

		 /// <summary>
		 /// Get the roles of the authenticated user.
		 /// </summary>
		 public virtual ISet<string> Roles()
		 {
			  return _roles;
		 }

		 /// <summary>
		 /// Allows all operations. </summary>
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public new static readonly EnterpriseSecurityContext AuthDisabledConflict = AuthDisabled( Org.Neo4j.@internal.Kernel.Api.security.AccessMode_Static.Full );

		 private static EnterpriseSecurityContext AuthDisabled( AccessMode mode )
		 {
			  return new EnterpriseSecurityContextAnonymousInnerClass( AuthSubject.AUTH_DISABLED, mode, Collections.emptySet() );
		 }

		 private class EnterpriseSecurityContextAnonymousInnerClass : EnterpriseSecurityContext
		 {
			 private new AccessMode _mode;

			 public EnterpriseSecurityContextAnonymousInnerClass( UnknownType authDisabled, AccessMode mode, UnknownType emptySet ) : base( authDisabled, mode, emptySet, true )
			 {
				 this._mode = mode;
			 }


			 public override EnterpriseSecurityContext withMode( AccessMode mode )
			 {
				  return AuthDisabled( mode );
			 }

			 public override string description()
			 {
				  return "AUTH_DISABLED with " + outerInstance.mode().name();
			 }

			 public override string ToString()
			 {
				  return outerInstance.defaultString( "enterprise-auth-disabled" );
			 }
		 }
	}

}