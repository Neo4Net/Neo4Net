using System;
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
	using IntSet = org.eclipse.collections.api.set.primitive.IntSet;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntIntHashMap;
	using ObjectIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using ReadOnlyDbException = Neo4Net.Kernel.Api.Exceptions.ReadOnlyDbException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.function.Predicates.ALWAYS_TRUE_INT;

	/// <summary>
	/// Keeps a registry of tokens using <seealso cref="TokenRegistry"/>.
	/// When asked for a token that isn't in the registry, delegates to a <seealso cref="TokenCreator"/> to create the token,
	/// then stores it in the registry.
	/// </summary>
	public class DelegatingTokenHolder : AbstractTokenHolderBase
	{
		 private readonly TokenCreator _tokenCreator;

		 public DelegatingTokenHolder( TokenCreator tokenCreator, string tokenType ) : base( new TokenRegistry( tokenType ) )
		 {
			  this._tokenCreator = tokenCreator;
		 }

		 /// <summary>
		 /// Create and put new token in cache.
		 /// </summary>
		 /// <param name="name"> token name </param>
		 /// <returns> newly created token id </returns>
		 /// <exception cref="KernelException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected synchronized int createToken(String name) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 protected internal override int CreateToken( string name )
		 {
			 lock ( this )
			 {
				  int? id = TokenRegistry.getId( name );
				  if ( id != null )
				  {
						return id.Value;
				  }
      
				  id = _tokenCreator.createToken( name );
				  try
				  {
						TokenRegistry.put( new NamedToken( name, id.Value ) );
				  }
				  catch ( NonUniqueTokenException e )
				  {
						throw new System.InvalidOperationException( "Newly created token should be unique.", e );
				  }
				  return id.Value;
			 }
		 }

		 public override void GetOrCreateIds( string[] names, int[] ids )
		 {
			  if ( names.Length != ids.Length )
			  {
					throw new System.ArgumentException( "Name and id arrays must have the same length." );
			  }
			  // Assume all tokens exist and try to resolve them. Break out on the first missing token.
			  bool hasUnresolvedTokens = ResolveIds( names, ids, ALWAYS_TRUE_INT );

			  if ( hasUnresolvedTokens )
			  {
					CreateMissingTokens( names, ids );
			  }
		 }

		 private void CreateMissingTokens( string[] names, int[] ids )
		 {
			 lock ( this )
			 {
				  // We redo the resolving under the lock, to make sure that these ids are really missing, and won't be
				  // created concurrently with us.
				  MutableIntSet unresolvedIndexes = new IntHashSet();
				  ResolveIds( names, ids, i => !unresolvedIndexes.add( i ) );
				  if ( !unresolvedIndexes.Empty )
				  {
						// We still have unresolved ids to create.
						ObjectIntHashMap<string> createdTokens = CreateUnresolvedTokens( unresolvedIndexes, names, ids );
						IList<NamedToken> createdTokensList = new List<NamedToken>( createdTokens.size() );
						createdTokens.forEachKeyValue( ( name, index ) => createdTokensList.Add( new NamedToken( name, ids[index] ) ) );
      
						TokenRegistry.putAll( createdTokensList );
				  }
			 }
		 }

		 private ObjectIntHashMap<string> CreateUnresolvedTokens( IntSet unresolvedIndexes, string[] names, int[] ids )
		 {
			  try
			  {
					// First, we need to filter out all of the tokens that are already resolved, so we only create tokens for
					// indexes that are in the unresolvedIndexes set.
					// However, we also need to deal with duplicate token names. For any token index we decide needs to have a
					// token created, we will add a mapping from the token name, to the ids-index into which the token id will
					// be written. This is the 'createdTokens' map. It maps token names to indexes into the 'ids' array.
					// If we find that the 'created'Tokens' map already has an entry for a given name, then that name is a
					// duplicate, and we will need to "remap" it later, by reading the token id from the correct index in the
					// 'ids' array, and storing it at the indexes of the duplicates. This is what the 'remappingIndexes' map is
					// for. This is a map from 'a' to 'b', where both 'a' and 'b' are indexes into the 'ids' array, and where
					// the corresponding name for 'a' is a duplicate of the name for 'b', and where we have already decided
					// that we will create a token id for index 'b'. After the token ids have been created, we go through the
					// 'remappingIndexes' map, and for every '(a,b)' entry, we store the token id created for 'b' and 'ids'
					// index 'a'.
					ObjectIntHashMap<string> createdTokens = new ObjectIntHashMap<string>();
					IntIntHashMap remappingIndexes = new IntIntHashMap();
					System.Func<int, bool> tokenCreateFilter = index =>
					{
					 bool needsCreate = unresolvedIndexes.contains( index );
					 if ( needsCreate )
					 {
						  // The name at this index is unresolved.
						  string name = names[index];
						  int creatingIndex = createdTokens.getIfAbsentPut( name, index );
						  if ( creatingIndex != index )
						  {
								// This entry has a duplicate name, so we need to remap this entry instead of creating a token
								// for it.
								remappingIndexes.put( index, creatingIndex );
								needsCreate = false;
						  }
					 }
					 return needsCreate;
					};

					// Create tokens for all the indexes that we don't filter out.
					_tokenCreator.createTokens( names, ids, tokenCreateFilter );

					// Remap duplicate tokens to the token id we created for the first instance of any duplicate token name.
					if ( remappingIndexes.notEmpty() )
					{
						 remappingIndexes.forEachKeyValue( ( index, creatingIndex ) => ids[index] = ids[creatingIndex] );
					}

					return createdTokens;
			  }
			  catch ( ReadOnlyDbException e )
			  {
					throw new TransactionFailureException( e.Message, e );
			  }
			  catch ( Exception e )
			  {
					throw new TransactionFailureException( "Could not create tokens.", e );
			  }
		 }
	}

}