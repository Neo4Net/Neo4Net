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
namespace Neo4Net.@internal.Collector
{
	internal interface Anonymizer
	{
		 string PropertyKey( string name, int id );

		 string Label( string name, int id );

		 string RelationshipType( string name, int id );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Anonymizer PLAIN_TEXT = new Anonymizer()
	//	 {
	//		  @@Override public String propertyKey(String name, int id)
	//		  {
	//				return name;
	//		  }
	//
	//		  @@Override public String label(String name, int id)
	//		  {
	//				return name;
	//		  }
	//
	//		  @@Override public String relationshipType(String name, int id)
	//		  {
	//				return name;
	//		  }
	//	 };

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Anonymizer IDS = new Anonymizer()
	//	 {
	//		  @@Override public String propertyKey(String name, int id)
	//		  {
	//				return "p" + id;
	//		  }
	//
	//		  @@Override public String label(String name, int id)
	//		  {
	//				return "L" + id;
	//		  }
	//
	//		  @@Override public String relationshipType(String name, int id)
	//		  {
	//				return "R" + id;
	//		  }
	//	 };
	}

}