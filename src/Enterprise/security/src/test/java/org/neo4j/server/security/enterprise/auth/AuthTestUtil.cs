using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Server.security.enterprise.auth
{

	internal class AuthTestUtil
	{
		 private AuthTestUtil()
		 {
		 }

		 public static T[] With<T>( Type clazz, T[] things, params T[] moreThings )
		 {
				 clazz = typeof( T );
			  return Stream.concat( Arrays.stream( things ), Arrays.stream( moreThings ) ).toArray( size => ( T[] ) Array.CreateInstance( clazz, size ) );
		 }

		 public static string[] With( string[] things, params string[] moreThings )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.concat( Arrays.stream( things ), Arrays.stream( moreThings ) ).toArray( string[]::new );
		 }

		 public static IList<T> ListOf<T>( params T[] values )
		 {
			  return Stream.of( values ).collect( Collectors.toList() );
		 }

	}

}