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
namespace Org.Neo4j.Server.Security.Auth
{
	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using PasswordPolicy = Org.Neo4j.Kernel.api.security.PasswordPolicy;

	public class BasicPasswordPolicy : PasswordPolicy
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void validatePassword(byte[] password) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override void ValidatePassword( sbyte[] password )
		 {
			  if ( password == null || password.Length == 0 )
			  {
					throw new InvalidArgumentsException( "A password cannot be empty." );
			  }
		 }
	}

}