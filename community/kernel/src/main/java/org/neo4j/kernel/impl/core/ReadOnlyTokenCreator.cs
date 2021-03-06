﻿/*
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
namespace Org.Neo4j.Kernel.impl.core
{

	using ReadOnlyDbException = Org.Neo4j.Kernel.Api.Exceptions.ReadOnlyDbException;

	/// <summary>
	/// When the database is marked as read-only, then no tokens can be created.
	/// </summary>
	public class ReadOnlyTokenCreator : TokenCreator
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int createToken(String name) throws org.neo4j.kernel.api.exceptions.ReadOnlyDbException
		 public override int CreateToken( string name )
		 {
			  throw new ReadOnlyDbException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void createTokens(String[] names, int[] ids, System.Func<int, boolean> filter) throws org.neo4j.kernel.api.exceptions.ReadOnlyDbException
		 public override void CreateTokens( string[] names, int[] ids, System.Func<int, bool> filter )
		 {
			  throw new ReadOnlyDbException();
		 }
	}

}