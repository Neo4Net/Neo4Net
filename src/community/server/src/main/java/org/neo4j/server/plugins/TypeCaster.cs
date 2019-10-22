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

	internal abstract class TypeCaster
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract Object get(org.Neo4Net.kernel.internal.GraphDatabaseAPI graphDb, ParameterList parameters, String name) throws org.Neo4Net.server.rest.repr.BadInputException;
		 internal abstract object Get( GraphDatabaseAPI graphDb, ParameterList parameters, string name );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object convert(Object[] result) throws org.Neo4Net.server.rest.repr.BadInputException
		 internal virtual object Convert( object[] result )
		 {
			  throw new BadInputException( "Cannot convert to primitive array: " + Arrays.ToString( result ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract Object[] getList(org.Neo4Net.kernel.internal.GraphDatabaseAPI graphDb, ParameterList parameters, String name) throws org.Neo4Net.server.rest.repr.BadInputException;
		 internal abstract object[] GetList( GraphDatabaseAPI graphDb, ParameterList parameters, string name );
	}

}