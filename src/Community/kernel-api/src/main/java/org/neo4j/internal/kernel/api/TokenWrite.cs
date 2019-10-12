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
namespace Neo4Net.@internal.Kernel.Api
{
	using IllegalTokenNameException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using TooManyLabelsException = Neo4Net.@internal.Kernel.Api.exceptions.schema.TooManyLabelsException;

	public interface TokenWrite
	{
		 /// <summary>
		 /// Returns a label id for a label name. If the label doesn't exist prior to
		 /// this call it gets created.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int labelGetOrCreateForName(String labelName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException;
		 int LabelGetOrCreateForName( string labelName );

		 /// <summary>
		 /// Get or create the label token ids for each of the given {@code labelNames}, and store them at the corresponding
		 /// index in the given {@code labelIds} array.
		 /// 
		 /// This is effectively a batching version of <seealso cref="labelGetOrCreateForName(string)"/>.
		 /// </summary>
		 /// <param name="labelNames"> The array of label names for which to resolve or create their id. </param>
		 /// <param name="labelIds"> The array into which the resulting token ids will be stored. </param>
		 /// <exception cref="TooManyLabelsException"> if too many labels would bve created by this call, compared to the token id space
		 /// available. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void labelGetOrCreateForNames(String[] labelNames, int[] labelIds) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException;
		 void LabelGetOrCreateForNames( string[] labelNames, int[] labelIds );

		 /// <summary>
		 /// Creates a label with the given name.
		 /// </summary>
		 /// <param name="labelName"> the name of the label. </param>
		 /// <returns> id of the created label. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int labelCreateForName(String labelName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException, org.neo4j.internal.kernel.api.exceptions.schema.TooManyLabelsException;
		 int LabelCreateForName( string labelName );

		 /// <summary>
		 /// Creates a property token with the given name.
		 /// </summary>
		 /// <param name="propertyKeyName"> the name of the property. </param>
		 /// <returns> id of the created property key. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int propertyKeyCreateForName(String propertyKeyName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException;
		 int PropertyKeyCreateForName( string propertyKeyName );

		 /// <summary>
		 /// Creates a relationship type with the given name. </summary>
		 /// <param name="relationshipTypeName"> the name of the relationship. </param>
		 /// <returns> id of the created relationship type. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int relationshipTypeCreateForName(String relationshipTypeName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException;
		 int RelationshipTypeCreateForName( string relationshipTypeName );

		 /// <summary>
		 /// Returns a property key id for a property key. If the key doesn't exist prior to
		 /// this call it gets created.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int propertyKeyGetOrCreateForName(String propertyKeyName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException;
		 int PropertyKeyGetOrCreateForName( string propertyKeyName );

		 /// <summary>
		 /// Get or create the property token ids for each of the given {@code propertyKeys}, and store them at the
		 /// corresponding index in the given {@code ids} array.
		 /// 
		 /// This is effectively a batching version of <seealso cref="propertyKeyGetOrCreateForName(string)"/>.
		 /// </summary>
		 /// <param name="propertyKeys"> The array of property names for which to resolve or create their id. </param>
		 /// <param name="ids"> The array into which the resulting token ids will be stored. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void propertyKeyGetOrCreateForNames(String[] propertyKeys, int[] ids) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException;
		 void PropertyKeyGetOrCreateForNames( string[] propertyKeys, int[] ids );

		 /// <summary>
		 /// Returns the id associated with the relationship type or creates a new one. </summary>
		 /// <param name="relationshipTypeName"> the name of the relationship </param>
		 /// <returns> the id associated with the name </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int relationshipTypeGetOrCreateForName(String relationshipTypeName) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException;
		 int RelationshipTypeGetOrCreateForName( string relationshipTypeName );

		 /// <summary>
		 /// Get or create the relationship type token ids for each of the given {@code relationshipTypes}, and store them at
		 /// the corresponding index in the given {@code ids} array.
		 /// 
		 /// This is effectively a batching version of <seealso cref="relationshipTypeGetOrCreateForName(string)"/>.
		 /// </summary>
		 /// <param name="relationshipTypes"> The array of relationship type names for which to resolve or create their id. </param>
		 /// <param name="ids"> The array into which the resulting token ids will be stored. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void relationshipTypeGetOrCreateForNames(String[] relationshipTypes, int[] ids) throws org.neo4j.internal.kernel.api.exceptions.schema.IllegalTokenNameException;
		 void RelationshipTypeGetOrCreateForNames( string[] relationshipTypes, int[] ids );
	}

}