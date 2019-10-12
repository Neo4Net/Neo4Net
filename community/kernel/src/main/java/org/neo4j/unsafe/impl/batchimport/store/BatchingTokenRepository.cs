using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.store
{

	using Loaders = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.Loaders;
	using Org.Neo4j.Kernel.impl.store;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using TokenRecord = Org.Neo4j.Kernel.impl.store.record.TokenRecord;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using Org.Neo4j.@unsafe.Batchinsert.@internal;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// Batching version of a <seealso cref="TokenStore"/> where tokens can be created and retrieved, but only persisted
	/// to storage as part of <seealso cref="close() closing"/>. Instances of this class are thread safe
	/// to call <seealso cref="getOrCreateId(string)"/> methods on.
	/// </summary>
	public abstract class BatchingTokenRepository<RECORD> : System.Func<object, int>, System.IDisposable where RECORD : Org.Neo4j.Kernel.impl.store.record.TokenRecord
	{
		 private readonly IDictionary<string, int> _tokens = new Dictionary<string, int>();
		 private readonly TokenStore<RECORD> _store;
		 private readonly RecordAccess_Loader<RECORD, Void> _loader;
		 private int _highId;
		 private int _highestCreatedId;

		 internal BatchingTokenRepository( TokenStore<RECORD> store, RecordAccess_Loader<RECORD, Void> loader )
		 {
			  this._store = store;
			  this._loader = loader;
			  this._highId = ( int )store.HighId;
			  this._highestCreatedId = _highId - 1;
		 }

		 /// <summary>
		 /// Returns the id for token with the specified {@code name}, potentially creating that token and
		 /// assigning a new id as part of this call.
		 /// </summary>
		 /// <param name="name"> token name. </param>
		 /// <returns> the id (created or existing) for the token by this name. </returns>
		 public virtual int GetOrCreateId( string name )
		 {
			  Debug.Assert( !string.ReferenceEquals( name, null ) );
			  int? id = _tokens[name];
			  if ( id == null )
			  {
					lock ( _tokens )
					{
						 id = _tokens.computeIfAbsent( name, k => _highId++ );
					}
			  }
			  return id.Value;
		 }

		 /// <summary>
		 /// Returns the id for token with the specified {@code key}, which can be a <seealso cref="string"/> if representing
		 /// a user-defined name or an <seealso cref="Integer"/> if representing an existing type from an external source,
		 /// which wants to preserve its name --> id tokens. Also see <seealso cref="getOrCreateId(string)"/> for more details.
		 /// </summary>
		 /// <param name="key"> name or id of this token. </param>
		 /// <returns> the id (created or existing) for the token key. </returns>
		 public virtual int GetOrCreateId( object key )
		 {
			  if ( key is string )
			  {
					// A name was supplied, get or create a token id for it
					return GetOrCreateId( ( string ) key );
			  }
			  else if ( key is int? )
			  {
					// A raw token id was supplied, just use it
					return ( int? ) key.Value;
			  }
			  throw new System.ArgumentException( "Expected either a String or Integer for property key, but was '" + key + "'" + ", " + key.GetType() );
		 }

		 public override int ApplyAsInt( object key )
		 {
			  return GetOrCreateId( key );
		 }

		 public virtual long[] GetOrCreateIds( string[] names )
		 {
			  return GetOrCreateIds( names, names.Length );
		 }

		 /// <summary>
		 /// Returns or creates multiple tokens for given token names.
		 /// </summary>
		 /// <param name="names"> token names to lookup or create token ids for. </param>
		 /// <param name="length"> length of the names array to consider, the array itself may be longer. </param>
		 /// <returns> {@code long[]} containing the label ids. </returns>
		 public virtual long[] GetOrCreateIds( string[] names, int length )
		 {
			  long[] result = new long[length];
			  int from;
			  int to;
			  for ( from = 0, to = 0; from < length; from++ )
			  {
					int id = GetOrCreateId( names[from] );
					if ( !Contains( result, id, to ) )
					{
						 result[to++] = id;
					}
			  }
			  if ( to < from )
			  {
					result = Arrays.copyOf( result, to );
			  }
			  Arrays.sort( result );
			  return result;
		 }

		 private bool Contains( long[] array, long id, int arrayLength )
		 {
			  for ( int i = 0; i < arrayLength; i++ )
			  {
					if ( array[i] == id )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public virtual int HighId
		 {
			 get
			 {
				  return _highId;
			 }
		 }

		 /// <summary>
		 /// Closes this repository and writes all created tokens to the underlying store.
		 /// </summary>
		 public override void Close()
		 {
			  Flush();
		 }

		 public virtual void Flush()
		 {
			  // Batch-friendly record access
			  RecordAccess<RECORD, Void> recordAccess = new DirectRecordAccess<RECORD, Void>( _store, _loader );

			  // Create the tokens
			  TokenCreator<RECORD> creator = new TokenCreator<RECORD>( _store );
			  int highest = _highestCreatedId;
			  foreach ( KeyValuePair<int, string> tokenToCreate in SortCreatedTokensById() )
			  {
					if ( tokenToCreate.Key > _highestCreatedId )
					{
						 creator.CreateToken( tokenToCreate.Value, tokenToCreate.Key, recordAccess );
						 highest = Math.Max( highest, tokenToCreate.Key );
					}
			  }
			  // Store them
			  int highestId = max( toIntExact( _store.HighestPossibleIdInUse ), highest );
			  recordAccess.Close();
			  _store.HighestPossibleIdInUse = highestId;
			  _highestCreatedId = highestId;
		 }

		 private IEnumerable<KeyValuePair<int, string>> SortCreatedTokensById()
		 {
			  IDictionary<int, string> sorted = new SortedDictionary<int, string>();
			  foreach ( KeyValuePair<string, int> entry in _tokens.SetOfKeyValuePairs() )
			  {
					sorted[entry.Value] = entry.Key;
			  }
			  return sorted.SetOfKeyValuePairs();
		 }

		 public class BatchingPropertyKeyTokenRepository : BatchingTokenRepository<PropertyKeyTokenRecord>
		 {
			  internal BatchingPropertyKeyTokenRepository( TokenStore<PropertyKeyTokenRecord> store ) : base( store, Loaders.propertyKeyTokenLoader( store ) )
			  {
			  }
		 }

		 public class BatchingLabelTokenRepository : BatchingTokenRepository<LabelTokenRecord>
		 {
			  internal BatchingLabelTokenRepository( TokenStore<LabelTokenRecord> store ) : base( store, Loaders.labelTokenLoader( store ) )
			  {
			  }
		 }

		 public class BatchingRelationshipTypeTokenRepository : BatchingTokenRepository<RelationshipTypeTokenRecord>
		 {
			  internal BatchingRelationshipTypeTokenRepository( TokenStore<RelationshipTypeTokenRecord> store ) : base( store, Loaders.relationshipTypeTokenLoader( store ) )
			  {
			  }
		 }
	}

}