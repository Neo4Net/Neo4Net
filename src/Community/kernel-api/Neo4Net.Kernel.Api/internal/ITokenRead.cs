using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.Internal
{

	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using LabelNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.LabelNotFoundKernelException;
	using PropertyKeyIdNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.PropertyKeyIdNotFoundKernelException;

	public interface ITokenRead
	{
		 /// <summary>
		 /// Value indicating the a token does not exist in the graph.
		 /// </summary>

		 /// <summary>
		 /// Return the id of the provided label, or NO_TOKEN if the label isn't known to the graph.
		 /// </summary>
		 /// <param name="name"> The label name. </param>
		 /// <returns> the label id, or NO_TOKEN </returns>
		 int NodeLabel( string name );

		 /// <summary>
		 /// Returns the name of a label given its label id
		 /// </summary>
		 /// <param name="labelId"> The label id </param>
		 /// <returns> The name of the label </returns>
		 /// <exception cref="LabelNotFoundKernelException"> if no label is associates with this id </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String nodeLabelName(int labelId) throws Neo4Net.Kernel.Api.Internal.Exceptions.LabelNotFoundKernelException;
		 string NodeLabelName( int labelId );

		 /// <summary>
		 /// Return the id of the provided relationship type, or NO_TOKEN if the type isn't known to the graph.
		 /// </summary>
		 /// <param name="name"> The relationship type name. </param>
		 /// <returns> the relationship type id, or NO_TOKEN </returns>
		 int RelationshipType( string name );

		 /// <summary>
		 /// Returns the name of a relationship type given its id
		 /// </summary>
		 /// <param name="relationshipTypeId"> The id of the relationship type </param>
		 /// <returns> The name of the relationship type </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String relationshipTypeName(int relationshipTypeId) throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
		 string RelationshipTypeName( int relationshipTypeId );

		 /// <summary>
		 /// Return the id of the provided property key, or NO_TOKEN if the property isn't known to the graph.
		 /// </summary>
		 /// <param name="name"> The property key name. </param>
		 /// <returns> the property key id, or NO_TOKEN </returns>
		 int PropertyKey( string name );

		 /// <summary>
		 /// Returns the name of a property given its property key id
		 /// </summary>
		 /// <param name="propertyKeyId"> The id of the property </param>
		 /// <returns> The name of the key </returns>
		 /// <exception cref="PropertyKeyIdNotFoundKernelException"> if no key is associated with the id </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String propertyKeyName(int propertyKeyId) throws Neo4Net.Kernel.Api.Internal.Exceptions.PropertyKeyIdNotFoundKernelException;
		 string PropertyKeyName( int propertyKeyId );

		 /// <summary>
		 /// Returns all label tokens </summary>
		 /// <returns> an iterator over all label tokens in the database </returns>
		 IEnumerator<NamedToken> LabelsGetAllTokens();

		 /// <summary>
		 /// Returns all property tokens </summary>
		 /// <returns> an iterator over all property tokens in the database </returns>
		 IEnumerator<NamedToken> PropertyKeyGetAllTokens();

		 /// <summary>
		 /// Returns all relationship type tokens </summary>
		 /// <returns> an iterator over all relationship type tokens in the database </returns>
		 IEnumerator<NamedToken> RelationshipTypesGetAllTokens();

		 /// <summary>
		 /// Returns the number of labels in the database </summary>
		 /// <returns> the number of labels in the database </returns>
		 int LabelCount();

		 /// <summary>
		 /// Returns the number of properties in the database </summary>
		 /// <returns> the number of properties in the database </returns>
		 int PropertyKeyCount();

		 /// <summary>
		 /// Returns the number of relationship types in the database </summary>
		 /// <returns> the number of relationship types in the database </returns>
		 int RelationshipTypeCount();
	}

	public static class TokenRead_Fields
	{
		 public const int NO_TOKEN = -1;
	}

}