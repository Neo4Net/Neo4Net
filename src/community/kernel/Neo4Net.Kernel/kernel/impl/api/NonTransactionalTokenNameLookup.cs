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
namespace Neo4Net.Kernel.Impl.Api
{
	using TokenNameLookup = Neo4Net.Internal.Kernel.Api.TokenNameLookup;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using TokenNotFoundException = Neo4Net.Kernel.impl.core.TokenNotFoundException;

	/// <summary>
	/// A token name resolver that directly accesses the databases cached property and label tokens, bypassing
	/// the transactional and locking layers.
	/// </summary>
	public class NonTransactionalTokenNameLookup : TokenNameLookup
	{
		 private readonly TokenHolders _tokenHolders;
		 private readonly bool _includeId;

		 public NonTransactionalTokenNameLookup( TokenHolders tokenHolders ) : this( tokenHolders, false )
		 {
		 }

		 public NonTransactionalTokenNameLookup( TokenHolders tokenHolders, bool includeId )
		 {
			  this._tokenHolders = tokenHolders;
			  this._includeId = includeId;
		 }

		 public override string LabelGetName( int labelId )
		 {
			  return TokenById( _tokenHolders.labelTokens(), labelId, "label" );
		 }

		 public override string RelationshipTypeGetName( int relTypeId )
		 {
			  return TokenById( _tokenHolders.relationshipTypeTokens(), relTypeId, "relationshipType" );
		 }

		 public override string PropertyKeyGetName( int propertyKeyId )
		 {
			  return TokenById( _tokenHolders.propertyKeyTokens(), propertyKeyId, "property" );
		 }

		 private string TokenById( TokenHolder tokenHolder, int tokenId, string tokenType )
		 {
			  try
			  {
					string tokenName = tokenHolder.GetTokenById( tokenId ).name();
					return _includeId ? NameAndId( tokenId, tokenName ) : tokenName;
			  }
			  catch ( TokenNotFoundException )
			  {
					// Ignore errors from reading key
			  }
			  return NameAndId( tokenId, tokenType );
		 }

		 private static string NameAndId( int id, string name )
		 {
			  return format( "%s[%d]", name, id );
		 }
	}

}