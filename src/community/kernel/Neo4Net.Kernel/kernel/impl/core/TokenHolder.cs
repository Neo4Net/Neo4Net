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
namespace Neo4Net.Kernel.impl.core
{

	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;

	public interface TokenHolder
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void setInitialTokens(java.util.List<org.neo4j.internal.kernel.api.NamedToken> tokens) throws NonUniqueTokenException;
		 IList<NamedToken> InitialTokens { set; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addToken(org.neo4j.internal.kernel.api.NamedToken token) throws NonUniqueTokenException;
		 void AddToken( NamedToken token );

		 /// <summary>
		 /// Get the id of the token by the given name, or create a new id for the token if it does not have one already,
		 /// and then return that id.
		 /// <para>
		 /// This method is thread-safe, and will ensure that distinct tokens will not have multiple ids allocated for them.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="name"> The name of the token to get the id for. </param>
		 /// <returns> The (possibly newly created) id of the given token. </returns>
		 int GetOrCreateId( string name );

		 /// <summary>
		 /// Resolve the ids of the given token {@code names} into the array for {@code ids}.
		 /// <para>
		 /// Any tokens that don't already have an id will have one created for it.
		 /// </para>
		 /// </summary>
		 void GetOrCreateIds( string[] names, int[] ids );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.internal.kernel.api.NamedToken getTokenById(int id) throws TokenNotFoundException;
		 NamedToken GetTokenById( int id );

		 /// <summary>
		 /// Returns the id, or <seealso cref="TokenRead.NO_TOKEN"/> if no token with this name exists.
		 /// </summary>
		 int GetIdByName( string name );

		 /// <summary>
		 /// Resolve the ids of the given token {@code names} into the array for {@code ids}.
		 /// <para>
		 /// Any tokens that don't already have an id will not be resolved, and the corrosponding entry in the {@code ids}
		 /// array will be left untouched. If you wish for those unresolved id entries to end up with the <seealso cref="TokenRead.NO_TOKEN"/>
		 /// value, you must first fill the array with that value before calling this method.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if some of the token names could not be resolved, {@code false} otherwise. </returns>
		 bool GetIdsByNames( string[] names, int[] ids );

		 IEnumerable<NamedToken> AllTokens { get; }

		 int Size();
	}

	public static class TokenHolder_Fields
	{
		 public const string TYPE_PROPERTY_KEY = "PropertyKey";
		 public const string TYPE_RELATIONSHIP_TYPE = "RelationshipType";
		 public const string TYPE_LABEL = "Label";
	}

}