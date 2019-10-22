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
	using ValueContext = Neo4Net.Index.lucene.ValueContext;

	public class ExactTxData : TxData
	{
		 private IDictionary<string, IDictionary<object, ISet<EntityId>>> _data;
		 private bool _hasOrphans;

		 internal ExactTxData( LuceneExplicitIndex index ) : base( index )
		 {
		 }

		 internal override void Add( TxDataHolder holder, IEntityId IEntityId, string key, object value )
		 {
			  IdCollection( key, value, true ).Add( IEntityId );
		 }

		 private ISet<EntityId> IdCollection( string key, object value, bool create )
		 {
			  IDictionary<object, ISet<EntityId>> keyMap = keyMap( key, create );
			  if ( keyMap == null )
			  {
					return null;
			  }

			  ISet<EntityId> ids = keyMap[value];
			  if ( ids == null && create )
			  {
					ids = new HashSet<EntityId>();
					keyMap[value] = ids;
					if ( value == null )
					{
						 _hasOrphans = true;
					}
			  }
			  return ids;
		 }

		 private IDictionary<object, ISet<EntityId>> KeyMap( string key, bool create )
		 {
			  if ( _data == null )
			  {
					if ( create )
					{
						 _data = new Dictionary<string, IDictionary<object, ISet<EntityId>>>();
					}
					else
					{
						 return null;
					}
			  }

			  IDictionary<object, ISet<EntityId>> inner = _data[key];
			  if ( inner == null && create )
			  {
					inner = new Dictionary<object, ISet<EntityId>>();
					_data[key] = inner;
					if ( string.ReferenceEquals( key, null ) )
					{
						 _hasOrphans = true;
					}
			  }
			  return inner;
		 }

		 private TxData ToFullTxData()
		 {
			  FullTxData data = new FullTxData( Index );
			  if ( this._data != null )
			  {
					foreach ( KeyValuePair<string, IDictionary<object, ISet<EntityId>>> entry in this._data.SetOfKeyValuePairs() )
					{
						 string key = entry.Key;
						 foreach ( KeyValuePair<object, ISet<EntityId>> valueEntry in entry.Value.entrySet() )
						 {
							  object value = valueEntry.Key;
							  foreach ( IEntityId id in valueEntry.Value )
							  {
									data.Add( null, id, key, value );
							  }
						 }
					}
			  }
			  return data;
		 }

		 internal override void Close()
		 {
		 }

		 internal override ICollection<EntityId> Query( TxDataHolder holder, Query query, QueryContext contextOrNull )
		 {
			  if ( contextOrNull != null && contextOrNull.TradeCorrectnessForSpeed )
			  {
					return Collections.emptyList();
			  }

			  TxData fullTxData = ToFullTxData();
			  holder.Set( fullTxData );
			  return fullTxData.Query( holder, query, contextOrNull );
		 }

		 internal override void Remove( TxDataHolder holder, IEntityId IEntityId, string key, object value )
		 {
			  if ( _data == null )
			  {
					return;
			  }

			  if ( string.ReferenceEquals( key, null ) || value == null )
			  {
					TxData fullData = ToFullTxData();
					fullData.Remove( holder, IEntityId, key, value );
					holder.Set( fullData );
			  }
			  else
			  {
					ICollection<EntityId> ids = IdCollection( key, value, false );
					if ( ids != null )
					{
						 ids.remove( IEntityId );
					}
			  }
		 }

		 internal override ICollection<EntityId> Get( TxDataHolder holder, string key, object value )
		 {
			  value = value is ValueContext ? ( ( ValueContext ) value ).CorrectValue : value.ToString();
			  ISet<EntityId> ids = IdCollection( key, value, false );
			  if ( ids == null || ids.Count == 0 )
			  {
					return Collections.emptySet();
			  }
			  return ids;
		 }

		 internal override ICollection<EntityId> GetOrphans( string key )
		 {
			  if ( !_hasOrphans )
			  {
					return null;
			  }

			  ISet<EntityId> orphans = IdCollection( null, null, false );
			  ISet<EntityId> keyOrphans = IdCollection( key, null, false );
			  return LuceneTransactionState.Merge( orphans, keyOrphans );
		 }

		 internal override IndexSearcher AsSearcher( TxDataHolder holder, QueryContext context )
		 {
			  if ( context != null && context.TradeCorrectnessForSpeed )
			  {
					return null;
			  }
			  TxData fullTxData = ToFullTxData();
			  holder.Set( fullTxData );
			  return fullTxData.AsSearcher( holder, context );
		 }
	}

}