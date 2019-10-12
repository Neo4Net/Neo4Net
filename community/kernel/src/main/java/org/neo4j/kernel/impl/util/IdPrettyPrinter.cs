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
namespace Org.Neo4j.Kernel.impl.util
{
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;

	public class IdPrettyPrinter
	{
		 private IdPrettyPrinter()
		 {
		 }

		 public static string Label( int id )
		 {
			  return id == Org.Neo4j.@internal.Kernel.Api.Read_Fields.ANY_LABEL ? "" : ( ":label=" + id );
		 }

		 public static string PropertyKey( int id )
		 {
			  return id == Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN ? "" : ( ":propertyKey=" + id );
		 }

		 public static string RelationshipType( int id )
		 {
			  return id == Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN ? "" : ( "[:type=" + id + "]" );
		 }
	}

}