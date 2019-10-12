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
namespace Org.Neo4j.Bolt.runtime
{
	/// <summary>
	/// Metadata that becomes available as soon as a statement is started, and is sent to the client before the result
	/// stream is sent.
	/// </summary>
	public interface StatementMetadata
	{
		 string[] FieldNames();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 StatementMetadata EMPTY = new StatementMetadata()
	//	 {
	//		  private final String[] emptyFields = new String[0];
	//
	//		  @@Override public String[] fieldNames()
	//		  {
	//				return emptyFields;
	//		  }
	//	 };
	}

}