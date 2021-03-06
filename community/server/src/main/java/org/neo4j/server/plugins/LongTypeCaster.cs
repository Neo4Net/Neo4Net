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
namespace Org.Neo4j.Server.plugins
{
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using BadInputException = Org.Neo4j.Server.rest.repr.BadInputException;

	internal class LongTypeCaster : TypeCaster
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object get(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, ParameterList parameters, String name) throws org.neo4j.server.rest.repr.BadInputException
		 internal override object Get( GraphDatabaseAPI graphDb, ParameterList parameters, string name )
		 {
			  return parameters.GetLong( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object[] getList(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, ParameterList parameters, String name) throws org.neo4j.server.rest.repr.BadInputException
		 internal override object[] GetList( GraphDatabaseAPI graphDb, ParameterList parameters, string name )
		 {
			  return parameters.GetLongList( name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") long[] convert(Object[] data)
		 internal override long[] Convert( object[] data )
		 {
			  long?[] incoming = ( long?[] ) data;
			  long[] result = new long[incoming.Length];
			  for ( int i = 0; i < result.Length; i++ )
			  {
					result[i] = incoming[i].Value;
			  }
			  return result;
		 }
	}

}