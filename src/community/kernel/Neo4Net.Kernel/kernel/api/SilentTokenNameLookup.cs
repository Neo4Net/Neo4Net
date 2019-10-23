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
namespace Neo4Net.Kernel.api
{
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using LabelNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.LabelNotFoundKernelException;
	using PropertyKeyIdNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.PropertyKeyIdNotFoundKernelException;

	/// <summary>
	/// Instances allow looking up ids back to their names.
	/// 
	/// </summary>
	public sealed class SilentTokenNameLookup : TokenNameLookup
	{
		 private readonly TokenRead _tokenRead;

		 public SilentTokenNameLookup( TokenRead tokenRead )
		 {
			  this._tokenRead = tokenRead;
		 }

		 /// <summary>
		 /// Returns the a label name given its id. In case of downstream failure, returns [labelId].
		 /// </summary>
		 public override string LabelGetName( int labelId )
		 {
			  try
			  {
					return _tokenRead.nodeLabelName( labelId );
			  }
			  catch ( LabelNotFoundKernelException )
			  {
					return "[" + labelId + "]";
			  }
		 }

		 /// <summary>
		 /// Returns the name of a relationship type given its id. In case of downstream failure, returns [relationshipTypeId].
		 /// </summary>
		 public override string RelationshipTypeGetName( int relTypeId )
		 {
			  try
			  {
					return _tokenRead.relationshipTypeName( relTypeId );
			  }
			  catch ( KernelException )
			  {
					return "[" + relTypeId + "]";
			  }
		 }

		 /// <summary>
		 /// Returns the name of a property given its id. In case of downstream failure, returns [propertyId].
		 /// </summary>
		 public override string PropertyKeyGetName( int propertyKeyId )
		 {
			  try
			  {
					return _tokenRead.propertyKeyName( propertyKeyId );
			  }
			  catch ( PropertyKeyIdNotFoundKernelException )
			  {
					return "[" + propertyKeyId + "]";
			  }
		 }
	}

}