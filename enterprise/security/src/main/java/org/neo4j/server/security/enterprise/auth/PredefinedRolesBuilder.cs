using System.Collections.Concurrent;
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
namespace Org.Neo4j.Server.security.enterprise.auth
{
	using SimpleRole = org.apache.shiro.authz.SimpleRole;
	using RolePermissionResolver = org.apache.shiro.authz.permission.RolePermissionResolver;
	using WildcardPermission = org.apache.shiro.authz.permission.WildcardPermission;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.ADMIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.ARCHITECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.EDITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.PUBLISHER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.enterprise.auth.plugin.api.PredefinedRoles.READER;


	public class PredefinedRolesBuilder : RolesBuilder
	{
		 private static readonly WildcardPermission _schema = new WildcardPermission( "schema:*" );
		 private static readonly WildcardPermission _full = new WildcardPermission( "*" );
		 private static readonly WildcardPermission _token = new WildcardPermission( "token:*" );
		 private static readonly WildcardPermission _readWrite = new WildcardPermission( "data:*" );
		 private static readonly WildcardPermission _read = new WildcardPermission( "data:read" );

		 private static readonly IDictionary<string, SimpleRole> _innerRoles = StaticBuildRoles();
		 public static readonly IDictionary<string, SimpleRole> Roles = Collections.unmodifiableMap( _innerRoles );

		 private static IDictionary<string, SimpleRole> StaticBuildRoles()
		 {
			  IDictionary<string, SimpleRole> roles = new ConcurrentDictionary<string, SimpleRole>( 4 );

			  SimpleRole admin = new SimpleRole( ADMIN );
			  admin.add( _full );
			  roles[ADMIN] = admin;

			  SimpleRole architect = new SimpleRole( ARCHITECT );
			  architect.add( _schema );
			  architect.add( _readWrite );
			  architect.add( _token );
			  roles[ARCHITECT] = architect;

			  SimpleRole publisher = new SimpleRole( PUBLISHER );
			  publisher.add( _readWrite );
			  publisher.add( _token );
			  roles[PUBLISHER] = publisher;

			  SimpleRole editor = new SimpleRole( EDITOR );
			  editor.add( _readWrite );
			  roles[EDITOR] = editor;

			  SimpleRole reader = new SimpleRole( READER );
			  reader.add( _read );
			  roles[READER] = reader;

			  return roles;
		 }

		 public static readonly RolePermissionResolver RolePermissionResolver = roleString =>
		 {
		  if ( roleString == null )
		  {
				return java.util.Collections.emptyList();
		  }
		  SimpleRole role = Roles[roleString];
		  if ( role != null )
		  {
				return role.Permissions;
		  }
		  else
		  {
				return java.util.Collections.emptyList();
		  }
		 };

		 public override IDictionary<string, SimpleRole> BuildRoles()
		 {
			  return Roles;
		 }
	}

}