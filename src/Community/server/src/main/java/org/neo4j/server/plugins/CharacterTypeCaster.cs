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
namespace Neo4Net.Server.plugins
{
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;

	internal class CharacterTypeCaster : TypeCaster
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object get(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, ParameterList parameters, String name) throws org.neo4j.server.rest.repr.BadInputException
		 internal override object Get( GraphDatabaseAPI graphDb, ParameterList parameters, string name )
		 {
			  return parameters.GetCharacter( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object[] getList(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, ParameterList parameters, String name) throws org.neo4j.server.rest.repr.BadInputException
		 internal override object[] GetList( GraphDatabaseAPI graphDb, ParameterList parameters, string name )
		 {
			  return parameters.GetCharacterList( name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") char[] convert(Object[] data)
		 internal override char[] Convert( object[] data )
		 {
			  char?[] incoming = ( char?[] ) data;
			  char[] result = new char[incoming.Length];
			  for ( int i = 0; i < result.Length; i++ )
			  {
					result[i] = incoming[i].Value;
			  }
			  return result;
		 }
	}

}