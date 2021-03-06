﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;


	using AuthToken = Org.Neo4j.Kernel.api.security.AuthToken;
	using InvalidAuthTokenException = Org.Neo4j.Kernel.api.security.exception.InvalidAuthTokenException;

	public class ShiroAuthToken : AuthenticationToken
	{
		 private const string VALUE_DELIMITER = "'";
		 private const string PAIR_DELIMITER = ", ";
		 private const string KEY_VALUE_DELIMITER = "=";

		 private readonly IDictionary<string, object> _authToken;

		 public ShiroAuthToken( IDictionary<string, object> authToken )
		 {
			  this._authToken = authToken;
		 }

		 public override object Principal
		 {
			 get
			 {
				  return _authToken[Org.Neo4j.Kernel.api.security.AuthToken_Fields.PRINCIPAL];
			 }
		 }

		 public override object Credentials
		 {
			 get
			 {
				  return _authToken[Org.Neo4j.Kernel.api.security.AuthToken_Fields.CREDENTIALS];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getScheme() throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 public virtual string Scheme
		 {
			 get
			 {
				  return AuthToken.safeCast( Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, _authToken );
			 }
		 }

		 public virtual string SchemeSilently
		 {
			 get
			 {
				  object scheme = _authToken[Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY];
				  return scheme == null ? null : scheme.ToString();
			 }
		 }

		 public virtual IDictionary<string, object> AuthTokenMap
		 {
			 get
			 {
				  return _authToken;
			 }
		 }

		 /// <summary>
		 /// returns true if token map does not specify a realm, or if it specifies the requested realm </summary>
		 public virtual bool SupportsRealm( string realm )
		 {
			  object providedRealm = _authToken[Org.Neo4j.Kernel.api.security.AuthToken_Fields.REALM_KEY];

			  return providedRealm == null || providedRealm.Equals( "*" ) || providedRealm.Equals( realm ) || providedRealm.ToString().Length == 0;
		 }

		 public override string ToString()
		 {
			  if ( _authToken.Count == 0 )
			  {
					return "{}";
			  }

			  IList<string> keys = new List<string>( _authToken.Keys );
			  int schemeIndex = keys.IndexOf( Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY );
			  if ( schemeIndex > 0 )
			  {
					keys[schemeIndex] = keys[0];
					keys[0] = Org.Neo4j.Kernel.api.security.AuthToken_Fields.SCHEME_KEY;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return keys.Select( this.keyValueString ).collect( joining( PAIR_DELIMITER, "{ ", " }" ) );
		 }

		 private string KeyValueString( string key )
		 {
			  string valueString = key.Equals( Org.Neo4j.Kernel.api.security.AuthToken_Fields.CREDENTIALS ) ? "******" : _authToken[key].ToString();
			  return key + KEY_VALUE_DELIMITER + VALUE_DELIMITER + valueString + VALUE_DELIMITER;
		 }
	}

}