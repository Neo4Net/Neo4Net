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
namespace Neo4Net.Server.plugins
{
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;

	internal class BooleanTypeCaster : TypeCaster
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object get(Neo4Net.kernel.internal.GraphDatabaseAPI graphDb, ParameterList parameters, String name) throws Neo4Net.server.rest.repr.BadInputException
		 internal override object Get( GraphDatabaseAPI graphDb, ParameterList parameters, string name )
		 {
			  return parameters.GetBoolean( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object[] getList(Neo4Net.kernel.internal.GraphDatabaseAPI graphDb, ParameterList parameters, String name) throws Neo4Net.server.rest.repr.BadInputException
		 internal override object[] GetList( GraphDatabaseAPI graphDb, ParameterList parameters, string name )
		 {
			  return parameters.GetBooleanList( name );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") boolean[] convert(Object[] data)
		 internal override bool[] Convert( object[] data )
		 {
			  bool?[] incoming = ( bool?[] ) data;
			  bool[] result = new bool[incoming.Length];
			  for ( int i = 0; i < result.Length; i++ )
			  {
					result[i] = incoming[i].Value;
			  }
			  return result;
		 }
	}

}