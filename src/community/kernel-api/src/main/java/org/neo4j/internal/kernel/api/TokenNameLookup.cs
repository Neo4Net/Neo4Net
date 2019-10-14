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
namespace Neo4Net.Internal.Kernel.Api
{

	using EntityType = Neo4Net.Storageengine.Api.EntityType;

	/// <summary>
	/// Lookup of names from token ids. Tokens are mostly referred to by ids throughout several abstractions.
	/// Sometimes token names are required, this is a way to lookup names in those cases.
	/// </summary>
	public interface TokenNameLookup
	{
		 /// <param name="labelId"> id of label to get name for. </param>
		 /// <returns> name of label token with given id. </returns>
		 string LabelGetName( int labelId );

		 /// <param name="relationshipTypeId"> id of relationship type to get name for. </param>
		 /// <returns> name of relationship type token with given id. </returns>
		 string RelationshipTypeGetName( int relationshipTypeId );

		 /// <param name="propertyKeyId"> id of property key to get name for. </param>
		 /// <returns> name of property key token with given id. </returns>
		 string PropertyKeyGetName( int propertyKeyId );

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default String[] entityTokensGetNames(org.neo4j.storageengine.api.EntityType type, int[] entityTokenIds)
	//	 {
	//		  IntFunction<String> mapper;
	//		  switch (type)
	//		  {
	//		  case NODE:
	//				mapper = this::labelGetName;
	//				break;
	//		  case RELATIONSHIP:
	//				mapper = this::relationshipTypeGetName;
	//				break;
	//		  default:
	//				throw new IllegalArgumentException("Cannot lookup names for tokens of type: " + type);
	//		  }
	//		  String[] tokenNames = new String[entityTokenIds.length];
	//		  for (int i = 0; i < entityTokenIds.length; i++)
	//		  {
	//				tokenNames[i] = mapper.apply(entityTokenIds[i]);
	//		  }
	//		  return tokenNames;
	//	 }
	}

}