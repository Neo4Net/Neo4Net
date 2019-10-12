using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{

	using NamedToken = Org.Neo4j.@internal.Kernel.Api.NamedToken;
	using AbstractTokenHolderBase = Org.Neo4j.Kernel.impl.core.AbstractTokenHolderBase;
	using TokenRegistry = Org.Neo4j.Kernel.impl.core.TokenRegistry;

	internal class SimpleTokenHolder : AbstractTokenHolderBase
	{
		 private int _nextId;

		 internal SimpleTokenHolder( TokenRegistry tokenRegistry ) : base( tokenRegistry )
		 {
		 }

		 protected internal override int CreateToken( string tokenName )
		 {
			  return _nextId++;
		 }

		 public override void GetOrCreateIds( string[] names, int[] ids )
		 {
			  for ( int i = 0; i < names.Length; i++ )
			  {
					ids[i] = GetOrCreateId( names[i] );
			  }
		 }

		 internal static TokenRegistry CreatePopulatedTokenRegistry( string tokenType, int[] tokenIds )
		 {
			  TokenRegistry tokenRegistry = new TokenRegistry( tokenType );
			  IList<NamedToken> tokens = new List<NamedToken>();
			  foreach ( int propertyId in tokenIds )
			  {
					tokens.Add( new NamedToken( tokenType + propertyId, propertyId ) );
			  }
			  tokenRegistry.InitialTokens = tokens;
			  return tokenRegistry;
		 }
	}

}