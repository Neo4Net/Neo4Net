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
namespace Neo4Net.Kernel.api.security
{

	using AccessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode;
	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;

	/// <summary>
	/// Controls the capabilities of a KernelTransaction. </summary>
	public class AnonymousContext : LoginContext
	{
		 private readonly AccessMode _accessMode;

		 private AnonymousContext( AccessMode accessMode )
		 {
			  this._accessMode = accessMode;
		 }

		 public static AnonymousContext None()
		 {
			  return new AnonymousContext( Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.None );
		 }

		 public static AnonymousContext Read()
		 {
			  return new AnonymousContext( Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Read );
		 }

		 public static AnonymousContext Write()
		 {
			  return new AnonymousContext( Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Write );
		 }

		 public static AnonymousContext WriteToken()
		 {
			  return new AnonymousContext( Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.TokenWrite );
		 }

		 public static AnonymousContext WriteOnly()
		 {
			  return new AnonymousContext( Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.WriteOnly );
		 }

		 public static AnonymousContext Full()
		 {
			  return new AnonymousContext( Neo4Net.@internal.Kernel.Api.security.AccessMode_Static.Full );
		 }

		 public override AuthSubject Subject()
		 {
			  return AuthSubject.ANONYMOUS;
		 }

		 public override SecurityContext Authorize( System.Func<string, int> propertyIdLookup, string dbName )
		 {
			  return new SecurityContext( Subject(), _accessMode );
		 }
	}

}