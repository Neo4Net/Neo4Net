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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;


	using QueryContext = Neo4Net.Index.lucene.QueryContext;

	internal class LuceneTransactionState : System.IDisposable
	{
		 private readonly IDictionary<IndexIdentifier, TxDataBoth> _txData = new Dictionary<IndexIdentifier, TxDataBoth>();

		 internal virtual void Add( LuceneExplicitIndex index, IEntityId IEntity, string key, object value )
		 {
			  TxDataBoth data = GetTxData( index, true );
			  Insert( IEntity, key, value, data.Added( true ), data.Removed( false ) );
		 }

		 internal virtual TxDataBoth GetTxData( LuceneExplicitIndex index, bool createIfNotExists )
		 {
			  IndexIdentifier identifier = index.Identifier;
			  TxDataBoth data = _txData[identifier];
			  if ( data == null && createIfNotExists )
			  {
					data = new TxDataBoth( index );
					_txData[identifier] = data;
			  }
			  return data;
		 }

		 internal virtual void Remove( LuceneExplicitIndex index, IEntityId IEntity, string key, object value )
		 {
			  TxDataBoth data = GetTxData( index, true );
			  Insert( IEntity, key, value, data.Removed( true ), data.Added( false ) );
		 }

		 internal virtual void Remove( LuceneExplicitIndex index, IEntityId IEntity, string key )
		 {
			  TxDataBoth data = GetTxData( index, true );
			  Insert( IEntity, key, null, data.Removed( true ), data.Added( false ) );
		 }

		 internal virtual void Remove( LuceneExplicitIndex index, IEntityId IEntity )
		 {
			  TxDataBoth data = GetTxData( index, true );
			  Insert( IEntity, null, null, data.Removed( true ), data.Added( false ) );
		 }

		 internal virtual void Delete( LuceneExplicitIndex index )
		 {
			  IndexIdentifier identifier = index.Identifier;
			  _txData[identifier] = new DeletedTxDataBoth( this, index );
		 }

		 private void Insert( IEntityId IEntity, string key, object value, TxDataHolder insertInto, TxDataHolder removeFrom )
		 {
			  if ( removeFrom != null )
			  {
					removeFrom.Remove( IEntity, key, value );
			  }
			  insertInto.Add( IEntity, key, value );
		 }

		 internal virtual ICollection<EntityId> GetRemovedIds( LuceneExplicitIndex index, Query query )
		 {
			  TxDataHolder removed = RemovedTxDataOrNull( index );
			  if ( removed == null )
			  {
					return Collections.emptySet();
			  }
			  ICollection<EntityId> ids = removed.Query( query, null );
			  return ids != null ? ids : Collections.emptySet();
		 }

		 internal virtual ICollection<EntityId> GetRemovedIds( LuceneExplicitIndex index, string key, object value )
		 {
			  TxDataHolder removed = RemovedTxDataOrNull( index );
			  if ( removed == null )
			  {
					return Collections.emptySet();
			  }
			  ICollection<EntityId> ids = removed.Get( key, value );
			  ICollection<EntityId> orphanIds = removed.GetOrphans( key );
			  return Merge( ids, orphanIds );
		 }

		 internal static ICollection<EntityId> Merge( ICollection<EntityId> c1, ICollection<EntityId> c2 )
		 {
			  if ( c1 == null && c2 == null )
			  {
					return Collections.emptySet();
			  }
			  else if ( c1 != null && c2 != null )
			  {
					if ( c1.Count == 0 )
					{
						 return c2;
					}
					if ( c2.Count == 0 )
					{
						 return c1;
					}
					ICollection<EntityId> result = new HashSet<EntityId>( c1.Count + c2.Count, 1 );
					result.addAll( c1 );
					result.addAll( c2 );
					return result;
			  }
			  else
			  {
					return c1 != null ? c1 : c2;
			  }
		 }

		 internal virtual ICollection<EntityId> GetAddedIds( LuceneExplicitIndex index, string key, object value )
		 {
			  TxDataHolder added = AddedTxDataOrNull( index );
			  if ( added == null )
			  {
					return Collections.emptySet();
			  }
			  ICollection<EntityId> ids = added.Get( key, value );
			  return ids != null ? ids : Collections.emptySet();
		 }

		 internal virtual TxDataHolder AddedTxDataOrNull( LuceneExplicitIndex index )
		 {
			  TxDataBoth data = GetTxData( index, false );
			  return data != null ? data.Added( false ) : null;
		 }

		 internal virtual TxDataHolder RemovedTxDataOrNull( LuceneExplicitIndex index )
		 {
			  TxDataBoth data = GetTxData( index, false );
			  return data != null ? data.Removed( false ) : null;
		 }

		 public override void Close()
		 {
			  foreach ( TxDataBoth data in this._txData.Values )
			  {
					data.Close();
			  }
			  this._txData.Clear();
		 }

		 // Bad name
		 private class TxDataBoth
		 {
			  internal TxDataHolder Add;
			  internal TxDataHolder Remove;
			  internal readonly LuceneExplicitIndex Index;

			  internal TxDataBoth( LuceneExplicitIndex index )
			  {
					this.Index = index;
			  }

			  internal virtual TxDataHolder Added( bool createIfNotExists )
			  {
					if ( this.Add == null && createIfNotExists )
					{
						 this.Add = new TxDataHolder( Index, Index.type.newTxData( Index ) );
					}
					return this.Add;
			  }

			  internal virtual TxDataHolder Removed( bool createIfNotExists )
			  {
					if ( this.Remove == null && createIfNotExists )
					{
						 this.Remove = new TxDataHolder( Index, Index.type.newTxData( Index ) );
					}
					return this.Remove;
			  }

			  internal virtual void Close()
			  {
					SafeClose( Add );
					SafeClose( Remove );
			  }

			  internal virtual void SafeClose( TxDataHolder data )
			  {
					if ( data != null )
					{
						 data.Dispose();
					}
			  }
		 }

		 private class DeletedTxDataBoth : TxDataBoth
		 {
			 private readonly LuceneTransactionState _outerInstance;

			  internal DeletedTxDataBoth( LuceneTransactionState outerInstance, LuceneExplicitIndex index ) : base( index )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal override TxDataHolder Added( bool createIfNotExists )
			  {
					throw IllegalStateException();
			  }

			  internal override TxDataHolder Removed( bool createIfNotExists )
			  {
					throw IllegalStateException();
			  }

			  internal virtual System.InvalidOperationException IllegalStateException()
			  {
					throw new System.InvalidOperationException( "This index (" + Index.Identifier + ") has been marked as deleted in this transaction" );
			  }
		 }

		 internal virtual IndexSearcher GetAdditionsAsSearcher( LuceneExplicitIndex index, QueryContext context )
		 {
			  TxDataHolder data = AddedTxDataOrNull( index );
			  return data != null ? data.AsSearcher( context ) : null;
		 }
	}

}