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
namespace Neo4Net.Kernel.impl.util
{
	using Read = Neo4Net.@internal.Kernel.Api.Read;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;

	public class IdPrettyPrinter
	{
		 private IdPrettyPrinter()
		 {
		 }

		 public static string Label( int id )
		 {
			  return id == Neo4Net.@internal.Kernel.Api.Read_Fields.ANY_LABEL ? "" : ( ":label=" + id );
		 }

		 public static string PropertyKey( int id )
		 {
			  return id == Neo4Net.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN ? "" : ( ":propertyKey=" + id );
		 }

		 public static string RelationshipType( int id )
		 {
			  return id == Neo4Net.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN ? "" : ( "[:type=" + id + "]" );
		 }
	}

}