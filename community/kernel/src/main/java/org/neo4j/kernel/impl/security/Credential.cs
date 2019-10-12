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
namespace Org.Neo4j.Kernel.impl.security
{
	public interface Credential
	{
		 bool MatchesPassword( sbyte[] password );

		 /// <summary>
		 /// For testing purposes only!
		 /// Use method that takes byte[]
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default boolean matchesPassword(String password)
	//	 {
	//		  throw new UnsupportedOperationException("Use `boolean matchesPassword( byte[] password )` instead");
	//	 }

		 string Serialize();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 Credential INACCESSIBLE = new Credential()
	//	 {
	//		  @@Override public boolean matchesPassword(byte[] password)
	//		  {
	//				return false;
	//		  }
	//
	//		  @@Override public String serialize()
	//		  {
	//				return ",,,";
	//		  }
	//	 };
	}

}